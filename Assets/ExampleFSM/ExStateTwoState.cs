using AxGrid;
using AxGrid.FSM;
using AxGrid.Model;

namespace ExampleFSM
{
    [State("ExStateTwo")]
    public class ExStateTwoState : FSMState
    {
        [Enter]
        private void EnterThis()
        {
            Log.Debug($"{Parent.CurrentStateName} ENTER");
        }

        [One(5f)]
        private void ChangeState()
        {
            Parent.Change("ExReady");
        }

        [Loop(2f)]
        private void LoopThis()
        {
            Log.Debug($"{Parent.CurrentStateName} LOOP");
        }

        [Bind("MonoToFsmEvent")]
        private void BindEvent()
        {
            Log.Debug($"{Parent.CurrentStateName} MonoToFsmEvent");
        }
        
        [Exit]
        private void ExitThis()
        {
            Log.Debug($"{Parent.CurrentStateName} EXIT");
        }
    }
}
