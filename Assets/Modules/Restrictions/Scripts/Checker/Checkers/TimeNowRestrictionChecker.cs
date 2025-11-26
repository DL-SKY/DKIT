using Modules.Restrictions.Scripts.Core;
using System;

namespace Modules.Restrictions.Scripts.Checker.Checkers
{
    public class TimeNowRestrictionChecker : IChecker
    {
        public bool Check(Restriction restriction)
        {
            var now = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
            return CompareRestrictionStaticChecker.Check(now, restriction.LongValues[0], restriction.CompareOptions);
        }
    }
}
