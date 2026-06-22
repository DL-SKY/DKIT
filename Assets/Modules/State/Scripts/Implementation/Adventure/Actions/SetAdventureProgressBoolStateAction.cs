using Modules.State.Scripts.Actions.Core;
using Modules.State.Scripts.Actions.Models;

namespace Modules.State.Scripts.Implementation.Adventure.Actions
{
    public class SetAdventureProgressBoolStateAction : StateActionBase<StateData>
    {
        public override StateChangeSource Source => StateChangeSource.SetAdventureProgressBool;

        private readonly string _key;
        private readonly bool _value;
        private readonly string _adventureId;

        public SetAdventureProgressBoolStateAction(string key, bool value, string adventureId = null)
        {
            _key = key;
            _value = value;
            _adventureId = adventureId;
        }

        public override StateActionValidationResult Validate(StateData state)
        {
            return AdventureProgressStateActionHelper.TryResolveBoolParameters(
                state,
                _key,
                _adventureId,
                out _);
        }

        public override void Execute(StateData state)
        {
            AdventureProgressStateActionHelper.TryResolveBoolParameters(
                state,
                _key,
                _adventureId,
                out var bools);

            bools[_key] = _value;
        }
    }
}
