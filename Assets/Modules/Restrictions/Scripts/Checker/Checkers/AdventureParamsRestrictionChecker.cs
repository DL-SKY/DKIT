using Modules.Restrictions.Scripts.Core;
using Modules.State.Scripts.Implementation.Adventure;
using Modules.State.Scripts.Implementation.Adventure.StateDatas;
using Zenject;

namespace Modules.Restrictions.Scripts.Checker.Checkers
{
    public class AdventureParamsRestrictionChecker : IChecker
    {
        [InjectOptional] private readonly AdventureStateManager _stateManager;

        public bool Check(Restriction restriction)
        {
            var adventuresState = _stateManager?.State?.Adventures;
            if (adventuresState?.Adventures == null)
                return false;

            if (string.IsNullOrWhiteSpace(adventuresState.CurrentAdventureId))
                return false;

            if (!adventuresState.Adventures.TryGetValue(adventuresState.CurrentAdventureId, out AdventureStateData adventureState))
                return false;

            return AdventureStateParamsRestrictionCheckHelper.Check(adventureState?.Parameters, restriction);
        }
    }
}
