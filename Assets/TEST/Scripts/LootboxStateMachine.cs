using AxGrid;
using AxGrid.FSM;
using AxGrid.Model;

public static class LootboxStateMachine
    {
        private const string BootStateName = "__lb_boot";
        private const string IdleStateName = "__lb_idle";
        private const string AccelerationStateName = "__lb_accelerating";
        private const string RunningStateName = "__lb_running";
        private const string StoppingStateName = "__lb_stopping";

        public static void CreateAndStart()
        {
            Settings.Fsm = new FSM();
            Settings.Fsm.Add(new BootState());
            Settings.Fsm.Add(new IdleState());
            Settings.Fsm.Add(new AccelerationState());
            Settings.Fsm.Add(new RunningState());
            Settings.Fsm.Add(new StoppingState());
            Settings.Fsm.Start(BootStateName);
        }

        private static void SetButtons(bool startEnabled, bool stopEnabled)
        {
            Settings.Model.Set(LootboxSignals.StartButtonEnabledField, startEnabled);
            Settings.Model.Set(LootboxSignals.StopButtonEnabledField, stopEnabled);
        }

        private static void Send(string signal)
        {
            Settings.Invoke(signal);
        }

        [State(BootStateName)]
        private class BootState : FSMState
        {
            [Enter]
            private void EnterThis()
            {
                SetButtons(true, false);
                Parent.Change(IdleStateName);
            }
        }

        [State(IdleStateName)]
        private class IdleState : FSMState
        {
            [Enter]
            private void EnterThis()
            {
                SetButtons(true, false);
            }

            [Bind(LootboxSignals.OnButtonEvent)]
            private void OnButton(string buttonName)
            {
                if (buttonName == LootboxSignals.StartButtonName)
                {
                    Parent.Change(AccelerationStateName);
                }
            }
        }

        [State(AccelerationStateName)]
        private class AccelerationState : FSMState
        {
            [Enter]
            private void EnterThis()
            {
                SetButtons(false, false);
                Send(LootboxSignals.SpinStartRequestedEvent);
            }

            [One(3f)]
            private void AllowStopAfterDelay()
            {
                Parent.Change(RunningStateName);
            }
        }

        [State(RunningStateName)]
        private class RunningState : FSMState
        {
            [Enter]
            private void EnterThis()
            {
                SetButtons(false, true);
            }

            [Bind(LootboxSignals.OnButtonEvent)]
            private void OnButton(string buttonName)
            {
                if (buttonName == LootboxSignals.StopButtonName)
                {
                    Parent.Change(StoppingStateName);
                }
            }
        }

        [State(StoppingStateName)]
        private class StoppingState : FSMState
        {
            [Enter]
            private void EnterThis()
            {
                SetButtons(false, false);
                Send(LootboxSignals.SpinStopRequestedEvent);
            }

            [Bind(LootboxSignals.SpinStoppedEvent)]
            private void OnSpinStopped()
            {
                Parent.Change(IdleStateName);
            }
        }
    }