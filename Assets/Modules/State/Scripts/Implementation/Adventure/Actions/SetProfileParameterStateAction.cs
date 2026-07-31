using Modules.State.Scripts.Actions.Core;
using Modules.State.Scripts.Actions.Models;
using System.Collections.Generic;

namespace Modules.State.Scripts.Implementation.Adventure.Actions
{
    public class SetProfileParameterStateAction : StateActionBase<StateData>
    {
        public override StateChangeSource Source => StateChangeSource.Profile;

        private readonly string _key;
        private readonly int _value;

        public SetProfileParameterStateAction(string key, int value)
        {
            _key = key;
            _value = value;
        }

        public override StateActionValidationResult Validate(StateData state)
        {
            if (state?.Profile == null)
                return StateActionValidationResult.Fail("Profile state is null.", 173);

            if (string.IsNullOrWhiteSpace(_key))
                return StateActionValidationResult.Fail("Profile parameter key is null or empty.", 174);

            if (_value < 0)
                return StateActionValidationResult.Fail("Profile parameter value can not be negative.", 175);

            return StateActionValidationResult.Ok;
        }

        public override void Execute(StateData state)
        {
            state.Profile.Parameters ??= new Dictionary<string, int>();
            state.Profile.Parameters[_key] = _value;
        }
    }
}
