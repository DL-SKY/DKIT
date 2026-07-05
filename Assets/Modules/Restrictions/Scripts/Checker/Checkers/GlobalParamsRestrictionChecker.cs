using Modules.Restrictions.Scripts.Core;
using Modules.State.Scripts.Implementation.Adventure;
using Modules.State.Scripts.Implementation.Adventure.StateDatas;
using Zenject;

namespace Modules.Restrictions.Scripts.Checker.Checkers
{
    public class GlobalParamsRestrictionChecker : IChecker
    {
        [InjectOptional] private readonly AdventureStateManager _stateManager;

        public bool Check(Restriction restriction)
        {
            AdventureStateParamsData parameters = _stateManager?.State?.Adventures?.Global?.Parameters;
            return AdventureStateParamsRestrictionCheckHelper.Check(parameters, restriction);
        }
    }
}
