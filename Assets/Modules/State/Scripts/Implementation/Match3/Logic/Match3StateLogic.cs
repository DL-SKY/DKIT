using Modules.State.Scripts.Actions.Core;
using Zenject;

namespace Modules.State.Scripts.Implementation.Match3.Logic
{
    public class Match3StateLogic : StateLogic<StateData>
    {
        private const int DefaultBatchSize = 10;

        [Inject]
        public Match3StateLogic(Match3StateManager stateManager) : base(stateManager, stateManager.SaveState, DefaultBatchSize)
        {
        }
    }
}
