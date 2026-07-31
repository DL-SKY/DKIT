using Modules.State.Scripts.Actions.Core;
using Modules.State.Scripts.Actions.Models;

namespace Modules.State.Scripts.Implementation.Adventure.Actions
{
    public class ChangeHeroPointsStateAction : StateActionBase<StateData>
    {
        public override StateChangeSource Source => StateChangeSource.Characters;

        private readonly int _deltaPoints;

        public ChangeHeroPointsStateAction(int deltaPoints)
        {
            _deltaPoints = deltaPoints;
        }

        public override StateActionValidationResult Validate(StateData state)
        {
            if (state?.Characters == null)
                return StateActionValidationResult.Fail("Characters state is null.", 150);

            if (state.Characters.HeroPoints + _deltaPoints < 0)
                return StateActionValidationResult.Fail("Hero points can not be negative.", 151);

            return StateActionValidationResult.Ok;
        }

        public override void Execute(StateData state)
        {
            state.Characters.HeroPoints += _deltaPoints;
        }
    }
}
