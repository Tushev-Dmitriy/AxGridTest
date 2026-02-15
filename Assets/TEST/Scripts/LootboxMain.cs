using AxGrid;
using AxGrid.Base;
using UnityEngine;

namespace TestUnityWork.Lootbox
{
    public class LootboxMain : MonoBehaviourExt
    {
        [OnStart]
        private void StartThis()
        {
            LootboxStateMachine.CreateAndStart();
        }

        [OnUpdate]
        private void UpdateThis()
        {
            Settings.Fsm?.Update(Time.deltaTime);
        }
    }
}
