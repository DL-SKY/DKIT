using Modules.Definitions.Scripts.Implementation.Adventures.Constants;
using Modules.State.Scripts.Actions.Core;
using Modules.State.Scripts.Actions.Models;
using Modules.State.Scripts.Implementation.Adventure.StateDatas;
using System.Collections.Generic;

namespace Modules.State.Scripts.Implementation.Adventure.Actions
{
    public class AddCharacterToActivePartyStateAction : StateActionBase<StateData>
    {
        public override StateChangeSource Source => StateChangeSource.AddCharacterToActiveParty;

        private readonly int _characterId;

        public AddCharacterToActivePartyStateAction(int characterId)
        {
            _characterId = characterId;
        }

        public override StateActionValidationResult Validate(StateData state)
        {
            if (state?.Characters?.Characters == null)
                return StateActionValidationResult.Fail("Characters dictionary is null.", 156);

            if (state.Profile?.Parameters == null)
                return StateActionValidationResult.Fail("Profile parameters are null.", 164);

            if (_characterId <= 0)
                return StateActionValidationResult.Fail("Character id must be greater than zero.", 157);

            if (!state.Characters.Characters.TryGetValue(_characterId, out CharacterStateData character) || character == null)
                return StateActionValidationResult.Fail("Character is not found by id.", 158);

            List<int> partyIds = state.Characters.ActivePartyCharacterIds;
            if (partyIds != null && partyIds.Contains(_characterId))
                return StateActionValidationResult.Fail("Character is already in the active party.", 159);

            state.Profile.Parameters.TryGetValue(Glossary.ProfileState.MAX_PARTY_SLOTS, out int maxPartySlots);
            int partyCount = partyIds?.Count ?? 0;
            if (partyCount >= maxPartySlots)
                return StateActionValidationResult.Fail("Active party is already full.", 160);

            return StateActionValidationResult.Ok;
        }

        public override void Execute(StateData state)
        {
            state.Characters.ActivePartyCharacterIds ??= new List<int>();
            state.Characters.ActivePartyCharacterIds.Add(_characterId);
        }
    }
}
