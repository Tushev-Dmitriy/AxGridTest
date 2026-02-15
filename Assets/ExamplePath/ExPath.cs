using AxGrid.Base;
using AxGrid.Path;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ExamplePath
{
    
    
    
    public class ExPath : MonoBehaviourExt
    {
        [SerializeField] private float timeAnim = 1f;
        [SerializeField] private float minValue = 0f;
        [SerializeField] private float maxValue = 100f;

        [SerializeField] private TMP_Text debug;
        [SerializeField] private Button repeatBtn;
        [SerializeField] private Button resetBtn;
        
        private float repeatCount = 0;
        private string cacheString = null;

        
        [OnStart]
        private void StartThis()
        {
            repeatBtn.onClick.AddListener(StartPath);
            resetBtn.onClick.AddListener(OnClickReset);
            ResetThis(false);
        }
        
        [OnDelay(1f)]
        private void StartPath()
        {
            ResetThis(false);
            repeatCount++;
            Path.Action(ActionPrintLog)
                .Wait(0.5f)
                .Action(() =>
                {
                    debug.text += "\n\nSTART EasingLinear";
                    cacheString = debug.text;
                })
                .EasingLinear(timeAnim, minValue, maxValue, value =>
                {
                    PrintToText($"\n{value:##0.00}", cacheString);
                })
                .Action(() => PrintToText("\nEND EasingLinear\n"))
                .Wait(1f)
                .Action(() =>
                {
                    
                    PrintToText("\nSTART EasingCircEaseIn");
                    cacheString = debug.text;
                })
                .EasingCircEaseIn(timeAnim, minValue, maxValue, value =>
                {
                    PrintToText($"\n{value:##0.00}", cacheString);
                })
                .Action(() =>
                {
                    PrintToText("\nEND EasingCircEaseIn");
                    repeatBtn.interactable = true;
                });
        }
        
        [OnDestroy]
        private void DestroyThis()
        {
            resetBtn.onClick.RemoveListener(OnClickReset);
            repeatBtn.onClick.RemoveListener(StartPath);
        }

        private void OnClickReset()
        {
            ResetThis(true);
        }
        
        private void ResetThis(bool enableRepeatBtn)
        {
            repeatBtn.interactable = enableRepeatBtn;
            Path = new CPath();
            debug.text = "";
        }

        private void ActionPrintLog()
        {
            PrintToText($"\nAction = {repeatCount}");
        }

        private void PrintToText(string text, string cache = null)
        {
            debug.text = (string.IsNullOrEmpty(cache) ? debug.text : cache) + text;
        }
    }
}
