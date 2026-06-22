using Modules.State.Scripts.Actions.Core;
using Modules.State.Scripts.Actions.Models;

namespace Modules.State.Scripts.Implementation.Match3.Actions
{
    public class SetProfileUpdateTimeStateAction : StateActionBase<StateData>
    {
        public override StateChangeSource Source => StateChangeSource.SetProfileUpdateTime;

        private readonly long _updateTime;

        public SetProfileUpdateTimeStateAction(long updateTime)
        {
            _updateTime = updateTime;
        }

        public override StateActionValidationResult Validate(StateData state)
        {
            if (state.Profile == null)
                return StateActionValidationResult.Fail("Profile state is null.", 20);

            if (_updateTime <= 0)
                return StateActionValidationResult.Fail("Update time must be greater than zero.", 21);

            return StateActionValidationResult.Ok;
        }

        public override void Execute(StateData state)
        {
            state.Profile.UpdateTime = _updateTime;
        }
    }
}
