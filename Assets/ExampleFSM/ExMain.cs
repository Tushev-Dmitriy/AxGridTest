using AxGrid;
using AxGrid.Base;
using AxGrid.FSM;
using AxGrid.Model;
using UnityEngine;

namespace ExampleFSM
{
    public class ExMain : MonoBehaviourExtBind
    {
        [OnAwake]
        private void AwakeThis()
        {
            Log.Debug("ExMain Awake");
        }
        
        [OnStart]
        private void StartThis()
        {
            Log.Debug("ExMain Start");
            Settings.Fsm = new FSM();
            Settings.Fsm.Add(new ExInitState());
            Settings.Fsm.Add(new ExReadyState());
            Settings.Fsm.Add(new ExStateOneState());
            Settings.Fsm.Add(new ExStateTwoState());
            
            Settings.Fsm.Start("ExInit");
        }

        [OnUpdate]
        private void UpdateThis()
        {
            //Log.Debug("ExMain Update");
            
            Settings.Fsm.Update(Time.deltaTime);
        }
        
        [OnRefresh(2f)]
        private void RefreshThis()
        {
            Log.Debug("ExMain Refresh 2 sec");
            Settings.Fsm.Invoke("MonoToFsmEvent");
        }

        [OnDelay(1f)]
        private void DelayTwoSec()
        {
            Log.Debug("ExMain Delay 1 sec");
        }
        
        [Bind("OnOneStateLoopCountChanged")]
        private void BindEventOne()
        {
            Log.Debug($"ExMain FsmToMonoEvent, COUNT loop one state = {Settings.Model.GetInt("OneStateLoopCount")}");
            
        }
        
        [Bind("FsmToMonoEvent")]
        private void BindEventTwo()
        {
            Log.Debug("ExMain FsmToMonoEvent");
        }
        

        
    }
}
