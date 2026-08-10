using Modules.State.Scripts.Actions.Core;
using Modules.State.Scripts.Actions.Models;
using Modules.State.Scripts.Implementation.Adventure.StateDatas;
using System.Collections.Generic;

namespace Modules.State.Scripts.Implementation.Adventure.Actions
{
    public class SetCharacterParameterStateAction : StateActionBase<StateData>
    {
        public override StateChangeSource Source => StateChangeSource.Characters;

        private readonly string _key;
        private readonly int _value;

        public SetCharacterParameterStateAction(string key, int value)
        {
            _key = key;
            _value = value;
        }

        public override StateActionValidationResult Validate(StateData state)
        {
            if (state?.Characters?.Characters == null)
                return StateActionValidationResult.Fail("Characters dictionary is null.", 201);

            if (state.Characters.CurrentActiveCharacterId <= 0)
                return StateActionValidationResult.Fail("Current active character id must be greater than zero.", 202);

            if (string.IsNullOrWhiteSpace(_key))
                return StateActionValidationResult.Fail("Character parameter key is null or empty.", 203);

            int characterId = state.Characters.CurrentActiveCharacterId;
            if (!state.Characters.Characters.TryGetValue(characterId, out CharacterStateData character) || character == null)
                return StateActionValidationResult.Fail("Current active character is not found by id.", 204);

            return StateActionValidationResult.Ok;
        }

        public override void Execute(StateData state)
        {
            int characterId = state.Characters.CurrentActiveCharacterId;
            CharacterStateData character = state.Characters.Characters[characterId];
            character.Parameters ??= new Dictionary<string, int>();
            character.Parameters[_key] = _value;
        }
    }
}
