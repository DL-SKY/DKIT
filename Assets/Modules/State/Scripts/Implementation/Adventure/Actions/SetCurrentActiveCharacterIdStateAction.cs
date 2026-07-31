using Modules.State.Scripts.Actions.Core;
using Modules.State.Scripts.Actions.Models;
using Modules.State.Scripts.Implementation.Adventure.StateDatas;

namespace Modules.State.Scripts.Implementation.Adventure.Actions
{
    public class SetCurrentActiveCharacterIdStateAction : StateActionBase<StateData>
    {
        public override StateChangeSource Source => StateChangeSource.SetCurrentActiveCharacterId;

        private readonly int _characterId;

        public SetCurrentActiveCharacterIdStateAction(int characterId)
        {
            _characterId = characterId;
        }

        public override StateActionValidationResult Validate(StateData state)
        {
            if (state?.Characters?.Characters == null)
                return StateActionValidationResult.Fail("Characters dictionary is null.", 153);

            if (_characterId <= 0)
                return StateActionValidationResult.Fail("Character id must be greater than zero.", 154);

            if (!state.Characters.Characters.TryGetValue(_characterId, out CharacterStateData character) || character == null)
                return StateActionValidationResult.Fail("Character is not found by id.", 155);

            return StateActionValidationResult.Ok;
        }

        public override void Execute(StateData state)
        {
            state.Characters.CurrentActiveCharacterId = _characterId;
        }
    }
}
