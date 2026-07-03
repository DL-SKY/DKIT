using Modules.State.Scripts.Implementation.Adventure.Actions;
using Modules.State.Scripts.Implementation.Adventure.Logic;
using Zenject;

namespace Modules.RPG.Scripts.Adventure.Choice.Executors
{
    public class GoToSceneChoiceActionExecutor : IChoiceActionExecutor
    {
        [Inject] private readonly AdventureStateLogic _stateLogic;

        private readonly string _sceneId;

        public GoToSceneChoiceActionExecutor(string sceneId)
        {
            _sceneId = sceneId;
        }

        public void Execute()
        {
            _stateLogic.ProcessAction(new SetCurrentAdventureSceneIdStateAction(_sceneId));
        }
    }
}
