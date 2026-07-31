using Modules.State.Scripts.Actions.Core;
using Modules.State.Scripts.Actions.Models;
using System.Collections.Generic;

namespace Modules.State.Scripts.Implementation.Adventure.Actions
{
    public class RemoveCharacterFromActivePartyStateAction : StateActionBase<StateData>
    {
        public override StateChangeSource Source => StateChangeSource.RemoveCharacterFromActiveParty;

        private readonly int _characterId;

        public RemoveCharacterFromActivePartyStateAction(int characterId)
        {
            _characterId = characterId;
        }

        public override StateActionValidationResult Validate(StateData state)
        {
            if (state?.Characters == null)
                return StateActionValidationResult.Fail("Characters state is null.", 161);

            if (_characterId <= 0)
                return StateActionValidationResult.Fail("Character id must be greater than zero.", 162);

            List<int> partyIds = state.Characters.ActivePartyCharacterIds;
            if (partyIds == null || !partyIds.Contains(_characterId))
                return StateActionValidationResult.Fail("Character is not in the active party.", 163);

            return StateActionValidationResult.Ok;
        }

        public override void Execute(StateData state)
        {
            state.Characters.ActivePartyCharacterIds.Remove(_characterId);

            if (state.Characters.CurrentActiveCharacterId == _characterId)
                state.Characters.CurrentActiveCharacterId = 0;
        }
    }
}
