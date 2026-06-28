using Modules.State.Scripts.Actions.Core;
using Modules.State.Scripts.Actions.Models;

namespace Modules.State.Scripts.Implementation.Adventure.Actions
{
    public class SetCurrentAdventureIdStateAction : StateActionBase<StateData>
    {
        public override StateChangeSource Source => StateChangeSource.SetCurrentAdventureId;

        private readonly string _adventureId;

        public SetCurrentAdventureIdStateAction(string adventureId)
        {
            _adventureId = adventureId;
        }

        public override StateActionValidationResult Validate(StateData state)
        {
            if (state?.Adventures == null)
                return StateActionValidationResult.Fail("Adventures state is null.", 40);

            if (string.IsNullOrWhiteSpace(_adventureId))
                return StateActionValidationResult.Fail("Current adventure id is null or empty.", 41);

            return StateActionValidationResult.Ok;
        }

        public override void Execute(StateData state)
        {
            state.Adventures.CurrentAdventureId = _adventureId;
        }
    }
}
