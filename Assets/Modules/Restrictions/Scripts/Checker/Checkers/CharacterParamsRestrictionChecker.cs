using Modules.Restrictions.Scripts.Core;
using Modules.State.Scripts.Implementation.Adventure;
using Modules.State.Scripts.Implementation.Adventure.StateDatas;
using Zenject;

namespace Modules.Restrictions.Scripts.Checker.Checkers
{
    /// <summary>
    /// Compares a value from <c>CharacterStateData.Parameters</c> to <c>Restriction.IntValues[0]</c>
    /// using <c>Restriction.CompareOptions</c>.
    /// <para>
    /// Format: <c>StringValues[0]</c> = parameter key; <c>IntValues[0]</c> = required int.
    /// Missing key is treated as <c>0</c>.
    /// </para>
    /// <para>
    /// Target character comes from <see cref="CharacterRestrictionContext"/>
    /// (override id or first living active-party member).
    /// </para>
    /// </summary>
    public class CharacterParamsRestrictionChecker : IChecker
    {
        [InjectOptional] private readonly AdventureStateManager _stateManager;
        [InjectOptional] private readonly CharacterRestrictionContext _characterContext;

        public bool Check(Restriction restriction)
        {
            if (restriction?.StringValues == null || restriction.StringValues.Count == 0)
                return false;

            if (restriction.IntValues == null || restriction.IntValues.Count == 0)
                return false;

            string parameterKey = restriction.StringValues[0];
            if (string.IsNullOrWhiteSpace(parameterKey))
                return false;

            if (_characterContext == null || !_characterContext.TryGetCharacterId(out int characterId))
                return false;

            CharactersStateData charactersState = _stateManager?.State?.Characters;
            if (charactersState?.Characters == null)
                return false;

            if (!charactersState.Characters.TryGetValue(characterId, out CharacterStateData character)
                || character == null)
            {
                return false;
            }

            int actualValue = 0;
            if (character.Parameters != null)
                character.Parameters.TryGetValue(parameterKey, out actualValue);

            int requiredValue = restriction.IntValues[0];
            return CompareRestrictionStaticChecker.Check(actualValue, requiredValue, restriction.CompareOptions);
        }
    }
}
