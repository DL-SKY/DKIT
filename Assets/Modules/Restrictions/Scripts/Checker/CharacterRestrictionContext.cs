using Modules.State.Scripts.Implementation.Adventure;
using Modules.State.Scripts.Implementation.Adventure.StateDatas;
using Zenject;

namespace Modules.Restrictions.Scripts.Checker
{
    /// <summary>
    /// Resolves which character <see cref="Checkers.CharacterParamsRestrictionChecker"/> evaluates.
    /// When an override id is set (e.g. battle actor), that id is used; otherwise the first living
    /// character in <c>ActivePartyCharacterIds</c> is selected.
    /// </summary>
    public class CharacterRestrictionContext
    {
        [InjectOptional] private readonly AdventureStateManager _stateManager;

        private int? _overrideCharacterId;

        public void SetCharacterId(int characterId)
        {
            _overrideCharacterId = characterId;
        }

        public void ClearCharacterId()
        {
            _overrideCharacterId = null;
        }

        public bool TryGetCharacterId(out int characterId)
        {
            if (_overrideCharacterId.HasValue)
            {
                characterId = _overrideCharacterId.Value;
                return true;
            }

            CharactersStateData charactersState = _stateManager?.State?.Characters;
            if (charactersState?.ActivePartyCharacterIds == null || charactersState.Characters == null)
            {
                characterId = 0;
                return false;
            }

            for (int i = 0; i < charactersState.ActivePartyCharacterIds.Count; i++)
            {
                int candidateId = charactersState.ActivePartyCharacterIds[i];
                if (!charactersState.Characters.TryGetValue(candidateId, out CharacterStateData character)
                    || character == null
                    || character.IsDead)
                {
                    continue;
                }

                characterId = candidateId;
                return true;
            }

            characterId = 0;
            return false;
        }
    }
}
