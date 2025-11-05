using Modules.Restrictions.Scripts.Core;

namespace Modules.Restrictions.Scripts.Checker
{
    public interface IRestrictionChecker
    {
        RestrictionType RestrictionType { get; }

        bool Check(ICheckerContext context, Restriction restriction);
    }
}
