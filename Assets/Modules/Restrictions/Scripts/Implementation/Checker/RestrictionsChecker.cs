using Modules.Restrictions.Scripts.Checker;
using Zenject;

namespace Modules.Restrictions.Scripts.Implementation.Checker
{
    public class RestrictionsChecker : RestrictionsCheckerBase
    {
        protected override ICheckerContext CreateContext()
        {
            return ProjectContext.Instance.Container.Instantiate<CheckerContext>();
        }

        protected override void FillCheckers()
        {
            //TODO: ...

            //_checkers.Add(RestrictionType.);
        }
    }
}
