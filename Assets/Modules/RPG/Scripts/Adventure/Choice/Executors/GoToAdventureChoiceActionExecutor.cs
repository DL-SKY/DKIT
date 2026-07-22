using Modules.State.Scripts.Implementation.Adventure.Actions;
using Modules.State.Scripts.Implementation.Adventure.Logic;
using Zenject;

namespace Modules.RPG.Scripts.Adventure.Choice.Executors
{
    public class GoToAdventureChoiceActionExecutor : IChoiceActionExecutor
    {
        [Inject] private readonly AdventureStateLogic _stateLogic;

        private readonly string _adventureId;

        public GoToAdventureChoiceActionExecutor(string adventureId)
        {
            _adventureId = adventureId;
        }

        public void Execute()
        {
            // Clears CurrentAdventureSceneId; RuntimeSceneData then picks a StartScenes entry.
            _stateLogic.ProcessAction(new SetCurrentAdventureIdStateAction(_adventureId));
        }
    }
}
