using Modules.Restrictions.Scripts.Core;

namespace Modules.Restrictions.Scripts.Checker
{
    public interface IChecker
    {
        bool Check(Restriction restriction);
    }
}
