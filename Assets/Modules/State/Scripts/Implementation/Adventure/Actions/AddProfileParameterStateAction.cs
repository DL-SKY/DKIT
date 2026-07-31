using Modules.State.Scripts.Actions.Core;
using Modules.State.Scripts.Actions.Models;
using System.Collections.Generic;

namespace Modules.State.Scripts.Implementation.Adventure.Actions
{
    public class AddProfileParameterStateAction : StateActionBase<StateData>
    {
        public override StateChangeSource Source => StateChangeSource.AddProfileParameter;

        private readonly string _key;
        private readonly int _delta;

        public AddProfileParameterStateAction(string key, int delta)
        {
            _key = key;
            _delta = delta;
        }

        public override StateActionValidationResult Validate(StateData state)
        {
            if (state?.Profile == null)
                return StateActionValidationResult.Fail("Profile state is null.", 170);

            if (string.IsNullOrWhiteSpace(_key))
                return StateActionValidationResult.Fail("Profile parameter key is null or empty.", 171);

            var currentAmount = 0;
            if (state.Profile.Parameters != null)
                state.Profile.Parameters.TryGetValue(_key, out currentAmount);

            if (currentAmount + _delta < 0)
                return StateActionValidationResult.Fail("Profile parameter value can not be negative.", 172);

            return StateActionValidationResult.Ok;
        }

        public override void Execute(StateData state)
        {
            state.Profile.Parameters ??= new Dictionary<string, int>();

            var currentAmount = 0;
            state.Profile.Parameters.TryGetValue(_key, out currentAmount);
            state.Profile.Parameters[_key] = currentAmount + _delta;
        }
    }
}
