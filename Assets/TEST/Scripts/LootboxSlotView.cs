using System;
using System.Collections.Generic;
using AxGrid;
using AxGrid.Base;
using AxGrid.Model;
using UnityEngine;
using UnityEngine.UI;

public class LootboxSlotView : MonoBehaviourExtBind
    {
        private const float MaxSpinSpeed = 1500f;
        private const float Acceleration = 900f;
        private const float Deceleration = 2000f;
        private const float StopSpeedThreshold = 45f;
        private const float AlignDuration = 0.32f;
        private const float AlignEpsilon = 0.5f;

        [SerializeField] private RectTransform viewport;
        [SerializeField] private RectTransform content;
        [SerializeField] private ParticleSystem stopParticles;

        private readonly List<RectTransform> _itemRects = new List<RectTransform>();

        private float _itemHeight;
        private float _itemStep;
        private float _centerLineY;
        private float _wrapBottomY;

        private float _currentSpeed;
        private float _targetSpeed;

        private bool _spinning;
        private bool _stopRequested;
        private bool _aligning;

        private float _alignElapsed;
        private float _alignOffsetTotal;
        private float _alignOffsetApplied;

        [OnAwake]
        private void AwakeThis()
        {
            ResolveReferences();
            CollectItems();
        }

        [Bind(LootboxSignals.SpinStartRequestedEvent)]
        private void OnSpinStartRequested()
        {
            _stopRequested = false;
            _aligning = false;
            _spinning = true;
            _targetSpeed = MaxSpinSpeed;

            StopParticlesClear();
        }

        [Bind(LootboxSignals.SpinStopRequestedEvent)]
        private void OnSpinStopRequested()
        {
            if (!_spinning)
            {
                NotifyStopped();
                return;
            }

            _stopRequested = true;
            _targetSpeed = 0f;
        }

        [OnUpdate]
        private void UpdateThis()
        {
            if (!IsReady())
            {
                return;
            }

            if (_aligning)
            {
                UpdateAlignment();
                return;
            }

            UpdateSpin();
        }

        private bool IsReady()
        {
            return content != null && _itemRects.Count > 0;
        }

        private void UpdateSpin()
        {
            if (!_spinning && _currentSpeed <= 0f)
            {
                return;
            }

            var accel = _targetSpeed > _currentSpeed ? Acceleration : Deceleration;
            _currentSpeed = Mathf.MoveTowards(_currentSpeed, _targetSpeed, accel * Time.deltaTime);

            if (_currentSpeed > 0f)
            {
                MoveItems(_currentSpeed * Time.deltaTime);
            }

            if (_stopRequested && _currentSpeed <= StopSpeedThreshold)
            {
                BeginAlignment();
            }
        }

        private void ResolveReferences()
        {
            var slotPanel = transform.Find("SlotPanel");
            if (slotPanel == null)
            {
                return;
            }

            if (viewport == null)
            {
                viewport = slotPanel.Find("Viewport") as RectTransform;
            }

            if (content == null && viewport != null)
            {
                content = viewport.Find("Content") as RectTransform;
            }

            if (stopParticles == null)
            {
                stopParticles = slotPanel.Find("StopParticles")?.GetComponent<ParticleSystem>();
            }
        }

        private void CollectItems()
        {
            _itemRects.Clear();

            if (content == null)
            {
                return;
            }

            foreach (Transform child in content)
            {
                if (!child.name.StartsWith("Item_", StringComparison.Ordinal))
                {
                    continue;
                }

                if (child is RectTransform rect)
                {
                    _itemRects.Add(rect);
                }
            }

            _itemRects.Sort((a, b) => b.anchoredPosition.y.CompareTo(a.anchoredPosition.y));

            if (_itemRects.Count == 0)
            {
                return;
            }

            _itemHeight = Mathf.Max(1f, _itemRects[0].rect.height);
            _itemStep = ComputeAverageStep();
            if (_itemStep <= 1f)
            {
                _itemStep = _itemHeight;
            }

            UpdateCenterLine();
        }

        private float ComputeAverageStep()
        {
            if (_itemRects.Count < 2)
            {
                return 0f;
            }

            var sum = 0f;
            for (var i = 1; i < _itemRects.Count; i++)
            {
                sum += Mathf.Abs(_itemRects[i - 1].anchoredPosition.y - _itemRects[i].anchoredPosition.y);
            }

            return sum / (_itemRects.Count - 1);
        }

        private void UpdateCenterLine()
        {
            if (viewport == null || content == null)
            {
                _centerLineY = -_itemHeight * 1.5f;
                _wrapBottomY = _centerLineY - (_itemHeight * 2f);
                return;
            }

            var centerWorld = viewport.TransformPoint(viewport.rect.center);
            var centerLocal = content.InverseTransformPoint(centerWorld);
            _centerLineY = centerLocal.y;

            var bottomWorld = viewport.TransformPoint(new Vector3(0f, viewport.rect.yMin, 0f));
            var bottomLocal = content.InverseTransformPoint(bottomWorld);
            _wrapBottomY = bottomLocal.y - _itemHeight;
        }

        private void MoveItems(float deltaY)
        {
            var topY = float.MinValue;

            for (var i = 0; i < _itemRects.Count; i++)
            {
                var pos = _itemRects[i].anchoredPosition;
                pos.y -= deltaY;
                _itemRects[i].anchoredPosition = pos;

                if (pos.y > topY)
                {
                    topY = pos.y;
                }
            }

            for (var i = 0; i < _itemRects.Count; i++)
            {
                var pos = _itemRects[i].anchoredPosition;
                if (pos.y >= _wrapBottomY)
                {
                    continue;
                }

                pos.y = topY + _itemStep;
                _itemRects[i].anchoredPosition = pos;
                topY = pos.y;
            }
        }

        private void BeginAlignment()
        {
            _stopRequested = false;
            _aligning = true;

            var nearestItem = GetItemClosestToCenter();
            var nearestCenterY = nearestItem.anchoredPosition.y - (_itemHeight * 0.5f);
            _alignOffsetTotal = _centerLineY - nearestCenterY;
            _alignOffsetApplied = 0f;
            _alignElapsed = 0f;

            if (Mathf.Abs(_alignOffsetTotal) <= AlignEpsilon)
            {
                FinishStop();
            }
        }

        private RectTransform GetItemClosestToCenter()
        {
            var best = _itemRects[0];
            var bestDistance = float.MaxValue;

            for (var i = 0; i < _itemRects.Count; i++)
            {
                var itemCenter = _itemRects[i].anchoredPosition.y - (_itemHeight * 0.5f);
                var distance = Mathf.Abs(_centerLineY - itemCenter);
                if (distance < bestDistance)
                {
                    best = _itemRects[i];
                    bestDistance = distance;
                }
            }

            return best;
        }

        private void UpdateAlignment()
        {
            _alignElapsed += Time.deltaTime;
            var t = Mathf.Clamp01(_alignElapsed / AlignDuration);
            var eased = 1f - Mathf.Pow(1f - t, 3f);
            var targetApplied = _alignOffsetTotal * eased;
            var delta = targetApplied - _alignOffsetApplied;
            _alignOffsetApplied = targetApplied;

            if (Mathf.Abs(delta) > 0f)
            {
                for (var i = 0; i < _itemRects.Count; i++)
                {
                    var pos = _itemRects[i].anchoredPosition;
                    pos.y += delta;
                    _itemRects[i].anchoredPosition = pos;
                }
            }

            if (t >= 1f)
            {
                FinishStop();
            }
        }

        private void FinishStop()
        {
            _aligning = false;
            _spinning = false;
            _stopRequested = false;
            _currentSpeed = 0f;
            _targetSpeed = 0f;

            PlayStopParticles();
            NotifyStopped();
        }

        private void StopParticlesClear()
        {
            if (stopParticles == null)
            {
                return;
            }

            stopParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        private void PlayStopParticles()
        {
            if (stopParticles == null)
            {
                return;
            }

            stopParticles.Clear(true);
            stopParticles.Play(true);
        }

        private void NotifyStopped()
        {
            Settings.Invoke(LootboxSignals.SpinStoppedEvent);
        }
    }