using Modules.RPG.Scripts.Adventure;

namespace Modules.RPG.Scripts.Adventure.Choice.Executors
{
    public class GoToNodeChoiceActionExecutor : IChoiceActionExecutor
    {
        private readonly IAdventureFlowController _adventureFlowController;
        private readonly string _nodeId;

        public GoToNodeChoiceActionExecutor(
            IAdventureFlowController adventureFlowController,
            string nodeId)
        {
            _adventureFlowController = adventureFlowController;
            _nodeId = nodeId;
        }

        public void Execute()
        {
            _adventureFlowController.GoToNode(_nodeId);
        }
    }
}
