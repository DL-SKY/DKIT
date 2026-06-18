using Modules.State.Scripts.Actions.Core;
using Modules.State.Scripts.Actions.Models;

namespace Modules.State.Scripts.Implementation.Adventure.Actions
{
    public class ModifyAdventureProgressIntStateAction : StateActionBase<StateData>
    {
        private readonly string _key;
        private readonly int _delta;
        private readonly string _adventureId;

        public ModifyAdventureProgressIntStateAction(string key, int delta, string adventureId = null)
        {
            _key = key;
            _delta = delta;
            _adventureId = adventureId;
        }

        public override StateActionValidationResult Validate(StateData state)
        {
            var resolveResult = AdventureProgressStateActionHelper.TryResolveIntParameters(
                state,
                _key,
                _adventureId,
                out var ints);

            if (!resolveResult.IsValid)
                return resolveResult;

            ints.TryGetValue(_key, out var currentValue);

            if (currentValue + _delta < 0)
                return StateActionValidationResult.Fail("Progress int value can not be negative.", 34);

            return StateActionValidationResult.Ok;
        }

        public override void Execute(StateData state)
        {
            AdventureProgressStateActionHelper.TryResolveIntParameters(
                state,
                _key,
                _adventureId,
                out var ints);

            ints.TryGetValue(_key, out var currentValue);
            ints[_key] = currentValue + _delta;
        }
    }
}
