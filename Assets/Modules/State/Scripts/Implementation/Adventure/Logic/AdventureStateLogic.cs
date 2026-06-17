using Modules.State.Scripts.Actions.Core;
using Zenject;

namespace Modules.State.Scripts.Implementation.Adventure.Logic
{
    public class AdventureStateLogic : StateLogic<StateData>
    {
        private const int DefaultBatchSize = 10;

        [Inject]
        public AdventureStateLogic(AdventureStateManager stateManager) : base(stateManager, stateManager.SaveState, DefaultBatchSize)
        {
            UnityEngine.Debug.Log($"[AdventureStateLogic] Initialization completed. Batch size: {DefaultBatchSize}");
        }
    }
}
