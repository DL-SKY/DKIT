using Modules.RPG.Scripts.Adventure.Choice;
using Modules.State.Scripts.Actions.Core;
using Modules.State.Scripts.Actions.Models;
using Modules.State.Scripts.Implementation.Adventure.StateDatas;
using System.Collections.Generic;

namespace Modules.State.Scripts.Implementation.Adventure.Actions
{
    public class SetAdventureParamsStateAction : StateActionBase<StateData>
    {
        public override StateChangeSource Source => StateChangeSource.AdventuresParams;

        private readonly ChoiceActionParamsData _params;

        public SetAdventureParamsStateAction(ChoiceActionParamsData actionParams)
        {
            _params = actionParams;
        }

        public override StateActionValidationResult Validate(StateData state)
        {
            if (state?.Adventures == null)
                return StateActionValidationResult.Fail("Adventures state is null.", 46);

            if (_params == null)
                return StateActionValidationResult.Fail("Action params are null.", 47);

            if (string.IsNullOrWhiteSpace(state.Adventures.CurrentAdventureId))
                return StateActionValidationResult.Fail("Current adventure id is null or empty.", 48);

            return StateActionValidationResult.Ok;
        }

        public override void Execute(StateData state)
        {
            AdventuresStateData adventuresState = state.Adventures;
            adventuresState.Adventures ??= new Dictionary<string, AdventureStateData>();

            string adventureId = adventuresState.CurrentAdventureId;
            if (!adventuresState.Adventures.TryGetValue(adventureId, out AdventureStateData adventureState) || adventureState == null)
            {
                adventureState = new AdventureStateData
                {
                    AdventureId = adventureId,
                    Parameters = AdventureStateParamsOperator.CreateEmpty(),
                };
                adventuresState.Adventures[adventureId] = adventureState;
            }

            adventureState.Parameters ??= AdventureStateParamsOperator.CreateEmpty();
            AdventureStateParamsOperator.Merge(adventureState.Parameters, _params);
        }
    }
}
