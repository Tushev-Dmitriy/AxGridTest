using AxGrid;
using AxGrid.FSM;

namespace ExampleFSM
{
    [State("ExStateOne")]
    public class ExStateOneState : FSMState
    {
        [Enter]
        private void EnterThis()
        {
            Log.Debug($"{Parent.CurrentStateName} ENTER");
            Settings.Model.EventManager.Invoke("FsmToMonoEvent");
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
            
            // При установке нового значения отправляется событие On{name}Changed в соответствующую модель
            Settings.Model.Set("OneStateLoopCount", Settings.Model.GetInt("OneStateLoopCount", 0)+1); 
            
            // Тоже самое действие можно сделать через метод Model.Inc
        }

        [Exit]
        private void ExitThis()
        {
            Log.Debug($"{Parent.CurrentStateName} EXIT");
        }
    }
}
