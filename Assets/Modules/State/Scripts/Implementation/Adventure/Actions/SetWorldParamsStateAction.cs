using Modules.RPG.Scripts.Adventure.Choice;
using Modules.State.Scripts.Actions.Core;
using Modules.State.Scripts.Actions.Models;
using Modules.State.Scripts.Implementation.Adventure.StateDatas;

namespace Modules.State.Scripts.Implementation.Adventure.Actions
{
    public class SetWorldParamsStateAction : StateActionBase<StateData>
    {
        public override StateChangeSource Source => StateChangeSource.SetWorldParams;

        private readonly ChoiceActionParamsData _params;

        public SetWorldParamsStateAction(ChoiceActionParamsData actionParams)
        {
            _params = actionParams;
        }

        public override StateActionValidationResult Validate(StateData state)
        {
            if (state?.Adventures == null)
                return StateActionValidationResult.Fail("Adventures state is null.", 44);

            if (_params == null)
                return StateActionValidationResult.Fail("Action params are null.", 45);

            return StateActionValidationResult.Ok;
        }

        public override void Execute(StateData state)
        {
            state.Adventures.World ??= new WorldStateData();
            state.Adventures.World.Parameters ??= AdventureStateParamsOperator.CreateEmpty();

            AdventureStateParamsOperator.Merge(state.Adventures.World.Parameters, _params);
        }
    }
}
