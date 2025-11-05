using Modules.Restrictions.Scripts.Core;
using System;

namespace Modules.Restrictions.Scripts.Checker.Checkers
{
    public class TimeNowRestrictionChecker : IRestrictionChecker
    {
        public RestrictionType RestrictionType => RestrictionType.TimeNowRestriction;

        public bool Check(ICheckerContext context, Restriction restriction)
        {
            return CompareRestrictionStaticChecker.Check<long>(
                context.GetContext<long>(RestrictionType),
                restriction.LongValues[0],
                restriction.CompareOptions
            );
        }

        public static long GetContextData()
        {
            return new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
        }
    }
}
