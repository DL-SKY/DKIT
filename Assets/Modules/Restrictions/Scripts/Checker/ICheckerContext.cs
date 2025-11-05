using Modules.Restrictions.Scripts.Core;

namespace Modules.Restrictions.Scripts.Checker
{
    public interface ICheckerContext
    {
        T GetContext<T>(RestrictionType type);
    }
}
