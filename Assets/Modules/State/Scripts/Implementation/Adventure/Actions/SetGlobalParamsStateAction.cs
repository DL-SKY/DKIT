using Modules.RPG.Scripts.Adventure.Choice;
using Modules.State.Scripts.Actions.Core;
using Modules.State.Scripts.Actions.Models;
using Modules.State.Scripts.Implementation.Adventure.StateDatas;

namespace Modules.State.Scripts.Implementation.Adventure.Actions
{
    public class SetGlobalParamsStateAction : StateActionBase<StateData>
    {
        public override StateChangeSource Source => StateChangeSource.SetGlobalParams;

        private readonly ChoiceActionParamsData _params;

        public SetGlobalParamsStateAction(ChoiceActionParamsData actionParams)
        {
            _params = actionParams;
        }

        public override StateActionValidationResult Validate(StateData state)
        {
            if (state?.Adventures == null)
                return StateActionValidationResult.Fail("Adventures state is null.", 49);

            if (_params == null)
                return StateActionValidationResult.Fail("Action params are null.", 50);

            return StateActionValidationResult.Ok;
        }

        public override void Execute(StateData state)
        {
            state.Adventures.Global ??= new GlobalStateData();
            state.Adventures.Global.Parameters ??= AdventureStateParamsOperator.CreateEmpty();

            AdventureStateParamsOperator.Merge(state.Adventures.Global.Parameters, _params);
        }
    }
}
