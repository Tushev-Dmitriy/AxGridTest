using AxGrid;
using AxGrid.FSM;
using UnityEngine;

namespace ExampleFSM
{
    [State("ExInit")]
    public class ExInitState : FSMState
    {
        [Enter]
        private void EnterThis()
        {
            Log.Debug($"{Parent.CurrentStateName} ENTER");
            Parent.Change("ExReady");
        }

        [Exit]
        private void ExitThis()
        {
            Log.Debug($"{Parent.CurrentStateName} EXIT");
        }
    }
}
