using Modules.RPG.Scripts.Adventure;
using System;

namespace Modules.RPG.Scripts.Adventure.Choice.Executors
{
    [Obsolete]
    public class GoToSceneChoiceActionExecutor : IChoiceActionExecutor
    {
        private readonly IAdventureFlowController _adventureFlowController;
        private readonly string _sceneId;

        public GoToSceneChoiceActionExecutor(
            IAdventureFlowController adventureFlowController,
            string sceneId)
        {
            _adventureFlowController = adventureFlowController;
            _sceneId = sceneId;
        }

        public void Execute()
        {
            _adventureFlowController.GoToScene(_sceneId);
        }
    }
}
