using Modules.Restrictions.Scripts.Core;
using Modules.State.Scripts.Implementation.Adventure.StateDatas;

namespace Modules.Restrictions.Scripts.Checker.Checkers
{
    internal static class AdventureStateParamsRestrictionCheckHelper
    {
        public static bool Check(AdventureStateParamsData parameters, Restriction restriction)
        {
            if (parameters == null || restriction == null)
                return false;

            if (restriction.StringValues == null || restriction.StringValues.Count == 0)
                return false;

            string parameterKey = restriction.StringValues[0];
            if (string.IsNullOrWhiteSpace(parameterKey))
                return false;

            if (restriction.IntValues != null && restriction.IntValues.Count > 0)
            {
                if (parameters.Ints == null || !parameters.Ints.TryGetValue(parameterKey, out int actualInt))
                    return false;

                int requiredInt = restriction.IntValues[0];
                return CompareRestrictionStaticChecker.Check(actualInt, requiredInt, restriction.CompareOptions);
            }

            if (restriction.BoolValues != null && restriction.BoolValues.Count > 0)
            {
                if (parameters.Bools == null || !parameters.Bools.TryGetValue(parameterKey, out bool actualBool))
                    return false;

                bool requiredBool = restriction.BoolValues[0];
                return restriction.CompareOptions switch
                {
                    CompareType.Equal => actualBool == requiredBool,
                    CompareType.NotEqual => actualBool != requiredBool,
                    _ => false,
                };
            }

            if (restriction.StringValues.Count < 2)
                return false;

            if (parameters.Strings == null || !parameters.Strings.TryGetValue(parameterKey, out string actualString))
                return false;

            string requiredString = restriction.StringValues[1];
            return CompareRestrictionStaticChecker.Check(actualString, requiredString, restriction.CompareOptions);
        }
    }
}
