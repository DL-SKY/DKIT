using Modules.RPG.Scripts.Adventure.Choice;
using Modules.State.Scripts.Implementation.Adventure.Actions;
using Modules.State.Scripts.Implementation.Adventure.Logic;
using Zenject;

namespace Modules.RPG.Scripts.Adventure.Choice.Executors
{
    public class SetGlobalParamsChoiceActionExecutor : IChoiceActionExecutor
    {
        [Inject] private readonly AdventureStateLogic _stateLogic;

        private readonly ChoiceActionParamsData _params;

        public SetGlobalParamsChoiceActionExecutor(ChoiceActionParamsData actionParams)
        {
            _params = actionParams;
        }

        public void Execute()
        {
            _stateLogic.ProcessAction(new SetGlobalParamsStateAction(_params));
        }
    }
}
