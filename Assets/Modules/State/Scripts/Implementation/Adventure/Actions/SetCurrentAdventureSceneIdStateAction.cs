using Modules.State.Scripts.Actions.Core;
using Modules.State.Scripts.Actions.Models;

namespace Modules.State.Scripts.Implementation.Adventure.Actions
{
    public class SetCurrentAdventureSceneIdStateAction : StateActionBase<StateData>
    {
        public override StateChangeSource Source => StateChangeSource.SetCurrentAdventureSceneId;

        private readonly string _sceneId;

        public SetCurrentAdventureSceneIdStateAction(string sceneId)
        {
            _sceneId = sceneId;
        }

        public override StateActionValidationResult Validate(StateData state)
        {
            if (state?.Adventures == null)
                return StateActionValidationResult.Fail("Adventures state is null.", 42);

            if (string.IsNullOrWhiteSpace(_sceneId))
                return StateActionValidationResult.Fail("Current adventure scene id is null or empty.", 43);

            return StateActionValidationResult.Ok;
        }

        public override void Execute(StateData state)
        {
            state.Adventures.CurrentAdventureSceneId = _sceneId;
        }
    }
}
