using System;
using System.Collections.Generic;
using AxGrid;
using AxGrid.Base;
using AxGrid.Model;
using UnityEngine;
using UnityEngine.UI;

namespace TestUnityWork.Lootbox
{
    public class LootboxSlotView : MonoBehaviourExtBind
    {
        private const float MaxSpinSpeed = 1500f;
        private const float Acceleration = 900f;
        private const float Deceleration = 2000f;
        private const float StopSpeedThreshold = 45f;
        private const float AlignDuration = 0.32f;

        [SerializeField] private RectTransform viewport;
        [SerializeField] private RectTransform content;
        [SerializeField] private ParticleSystem stopParticles;

        private readonly List<RectTransform> itemRects = new List<RectTransform>();

        private float itemHeight;
        private float itemStep;
        private float centerLineY;
        private float wrapBottomY;

        private float currentSpeed;
        private float targetSpeed;

        private bool spinning;
        private bool stopRequested;
        private bool aligning;

        private float alignElapsed;
        private float alignOffsetTotal;
        private float alignOffsetApplied;

        [OnAwake]
        private void AwakeThis()
        {
            ResolveReferences();
            CollectItems();
        }

        [Bind(LootboxSignals.SpinStartRequestedEvent)]
        private void OnSpinStartRequested()
        {
            stopRequested = false;
            aligning = false;
            spinning = true;
            targetSpeed = MaxSpinSpeed;

            if (stopParticles != null)
            {
                stopParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        [Bind(LootboxSignals.SpinStopRequestedEvent)]
        private void OnSpinStopRequested()
        {
            if (!spinning)
            {
                Settings.Invoke(LootboxSignals.SpinStoppedEvent);
                return;
            }

            stopRequested = true;
            targetSpeed = 0f;
        }

        [OnUpdate]
        private void UpdateThis()
        {
            if (content == null || itemRects.Count == 0)
            {
                return;
            }

            if (aligning)
            {
                UpdateAlignment();
                return;
            }

            if (!spinning && currentSpeed <= 0f)
            {
                return;
            }

            var speedDelta = (targetSpeed > currentSpeed ? Acceleration : Deceleration) * Time.deltaTime;
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, speedDelta);

            if (currentSpeed > 0f)
            {
                MoveItems(currentSpeed * Time.deltaTime);
            }

            if (stopRequested && currentSpeed <= StopSpeedThreshold)
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
            itemRects.Clear();

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

                if (!(child is RectTransform rect))
                {
                    continue;
                }

                itemRects.Add(rect);
            }

            itemRects.Sort((a, b) => b.anchoredPosition.y.CompareTo(a.anchoredPosition.y));

            if (itemRects.Count == 0)
            {
                return;
            }

            itemHeight = Mathf.Max(1f, itemRects[0].rect.height);
            itemStep = ComputeAverageStep();
            if (itemStep <= 1f)
            {
                itemStep = itemHeight;
            }

            UpdateCenterLine();
        }

        private float ComputeAverageStep()
        {
            if (itemRects.Count < 2)
            {
                return 0f;
            }

            var sum = 0f;
            for (var i = 1; i < itemRects.Count; i++)
            {
                sum += Mathf.Abs(itemRects[i - 1].anchoredPosition.y - itemRects[i].anchoredPosition.y);
            }

            return sum / (itemRects.Count - 1);
        }

        private void UpdateCenterLine()
        {
            if (viewport == null || content == null)
            {
                centerLineY = -itemHeight * 1.5f;
                wrapBottomY = centerLineY - (itemHeight * 2f);
                return;
            }

            var centerWorld = viewport.TransformPoint(viewport.rect.center);
            var centerLocal = content.InverseTransformPoint(centerWorld);
            centerLineY = centerLocal.y;

            var bottomWorld = viewport.TransformPoint(new Vector3(0f, viewport.rect.yMin, 0f));
            var bottomLocal = content.InverseTransformPoint(bottomWorld);
            wrapBottomY = bottomLocal.y - itemHeight;
        }

        private void MoveItems(float deltaY)
        {
            var topY = float.MinValue;

            for (var i = 0; i < itemRects.Count; i++)
            {
                var pos = itemRects[i].anchoredPosition;
                pos.y -= deltaY;
                itemRects[i].anchoredPosition = pos;

                if (pos.y > topY)
                {
                    topY = pos.y;
                }
            }

            for (var i = 0; i < itemRects.Count; i++)
            {
                var pos = itemRects[i].anchoredPosition;
                if (pos.y >= wrapBottomY)
                {
                    continue;
                }

                pos.y = topY + itemStep;
                itemRects[i].anchoredPosition = pos;
                topY = pos.y;
            }
        }

        private void BeginAlignment()
        {
            stopRequested = false;
            aligning = true;

            var nearestItem = GetItemClosestToCenter();
            var nearestCenterY = nearestItem.anchoredPosition.y - (itemHeight * 0.5f);
            alignOffsetTotal = centerLineY - nearestCenterY;
            alignOffsetApplied = 0f;
            alignElapsed = 0f;

            if (Mathf.Abs(alignOffsetTotal) <= 0.5f)
            {
                FinishStop();
            }
        }

        private RectTransform GetItemClosestToCenter()
        {
            var best = itemRects[0];
            var bestDistance = float.MaxValue;

            for (var i = 0; i < itemRects.Count; i++)
            {
                var itemCenter = itemRects[i].anchoredPosition.y - (itemHeight * 0.5f);
                var distance = Mathf.Abs(centerLineY - itemCenter);
                if (distance < bestDistance)
                {
                    best = itemRects[i];
                    bestDistance = distance;
                }
            }

            return best;
        }

        private void UpdateAlignment()
        {
            alignElapsed += Time.deltaTime;
            var t = Mathf.Clamp01(alignElapsed / AlignDuration);
            var eased = 1f - Mathf.Pow(1f - t, 3f);
            var targetApplied = alignOffsetTotal * eased;
            var delta = targetApplied - alignOffsetApplied;
            alignOffsetApplied = targetApplied;

            if (Mathf.Abs(delta) > 0f)
            {
                for (var i = 0; i < itemRects.Count; i++)
                {
                    var pos = itemRects[i].anchoredPosition;
                    pos.y += delta;
                    itemRects[i].anchoredPosition = pos;
                }
            }

            if (t >= 1f)
            {
                FinishStop();
            }
        }

        private void FinishStop()
        {
            aligning = false;
            spinning = false;
            stopRequested = false;
            currentSpeed = 0f;
            targetSpeed = 0f;

            if (stopParticles != null)
            {
                stopParticles.Clear(true);
                stopParticles.Play(true);
            }

            Settings.Invoke(LootboxSignals.SpinStoppedEvent);
        }

    }
}
