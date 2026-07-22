using Modules.Restrictions.Scripts.Core;
using Modules.State.Scripts.Implementation.Adventure;
using System.Collections.Generic;
using Zenject;

namespace Modules.Restrictions.Scripts.Checker.Checkers
{
    /// <summary>
    /// Compares <c>StateData.Characters.ActivePartyCharacterIds.Count</c> to <c>Restriction.IntValues[0]</c>
    /// using <c>Restriction.CompareOptions</c>.
    /// </summary>
    public class ActivePartyCountRestrictionChecker : IChecker
    {
        [InjectOptional] private readonly AdventureStateManager _stateManager;

        public bool Check(Restriction restriction)
        {
            if (restriction?.IntValues == null || restriction.IntValues.Count == 0)
                return false;

            List<int> partyIds = _stateManager?.State?.Characters?.ActivePartyCharacterIds;
            int actualCount = partyIds?.Count ?? 0;
            int requiredCount = restriction.IntValues[0];

            return CompareRestrictionStaticChecker.Check(actualCount, requiredCount, restriction.CompareOptions);
        }
    }
}
