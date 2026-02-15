using System;
using AxGrid.Base;
using SmartFormat;
using TMPro;
using UnityEngine;

namespace AxGrid.Tools.Binders
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class UITextMPDataBind:Binder
    {
        [Header("События")]
        [Tooltip("Поля при изменении которых будет срабатывать собятие.")]
        public string[] fieldNames = new string[0];
        [Tooltip("Изменение люиого поля модели")]
        public bool modelChanged = true;
        
        [Header("Форматироване")]
        [Tooltip("Smart.Format(format, model)")]
        public string format = "{Balance.Game}";

        [Tooltip("Взять формат перед выводом")]
        public bool isFormatField = false;
        public bool applyModelForFromatField = true;
        protected TextMeshProUGUI uiText;

        [OnAwake]
        protected void AwakeThis()
        {
            try
            {
                uiText = GetComponent<TextMeshProUGUI>();
            }
            catch (Exception e)
            {
                Log.Error($"Error get Component:{e.Message}");
            }
        }
        
        [OnStart]
        protected void StartThis()
        {
            try
            {
                if (isFormatField)
                    if (applyModelForFromatField)
                        format = Smart.Format(Text.Text.Get(format), Model);
                    else
                        format = Text.Text.Get(format);
                if (modelChanged)
                   Model.EventManager.AddAction("ModelChanged", Changed);
                else
                    foreach (var fieldName in fieldNames)
                       Model.EventManager.AddAction($"On{fieldName}Changed", Changed);
                Changed();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        [OnDestroy]
        protected void DestroyThis()
        {
            try
            {
                if (modelChanged)
                    Model.EventManager.RemoveAction("ModelChanged", Changed);
                else
                    foreach (var fieldName in fieldNames)
                        Model.EventManager.RemoveAction($"On{fieldName}Changed", Changed);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        protected virtual void Changed()
        {
            uiText.text = Text.Text.Get(format, Model);
        }
    }
}