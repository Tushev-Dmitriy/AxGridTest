using AxGrid;
using AxGrid.FSM;
using UnityEngine;

namespace ExampleFSM
{
    [State("ExReady")]
    public class ExReadyState : FSMState
    {
        [Enter]
        private void EnterThis()
        {
            Log.Debug($"{Parent.CurrentStateName} ENTER");
        }

        [One(1f)]
        private void ChangeState()
        {
            Parent.Change(Random.Range(0, 2) == 0 ? "ExStateOne" : "ExStateTwo");
        }

        [Exit]
        private void ExitThis()
        {
            Log.Debug($"{Parent.CurrentStateName} EXIT");
        }
    }
}
