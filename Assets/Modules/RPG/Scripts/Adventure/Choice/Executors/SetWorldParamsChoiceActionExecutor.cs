using Modules.RPG.Scripts.Adventure.Choice;
using Modules.State.Scripts.Implementation.Adventure.Actions;
using Modules.State.Scripts.Implementation.Adventure.Logic;
using Zenject;

namespace Modules.RPG.Scripts.Adventure.Choice.Executors
{
    public class SetWorldParamsChoiceActionExecutor : IChoiceActionExecutor
    {
        [Inject] private readonly AdventureStateLogic _stateLogic;

        private readonly ChoiceActionParamsData _params;

        public SetWorldParamsChoiceActionExecutor(ChoiceActionParamsData actionParams)
        {
            _params = actionParams;
        }

        public void Execute()
        {
            _stateLogic.ProcessAction(new SetWorldParamsStateAction(_params));
        }
    }
}
