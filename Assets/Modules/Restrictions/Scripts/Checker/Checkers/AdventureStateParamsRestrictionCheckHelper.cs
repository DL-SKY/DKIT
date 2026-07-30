using Modules.Restrictions.Scripts.Core;
using Modules.State.Scripts.Implementation.Adventure.StateDatas;

namespace Modules.Restrictions.Scripts.Checker.Checkers
{
    internal static class AdventureStateParamsRestrictionCheckHelper
    {
        public static bool Check(AdventureStateParamsData parameters, Restriction restriction)
        {
            if (restriction == null)
                return false;

            if (restriction.StringValues == null || restriction.StringValues.Count == 0)
                return false;

            string parameterKey = restriction.StringValues[0];
            if (string.IsNullOrWhiteSpace(parameterKey))
                return false;

            if (restriction.IntValues != null && restriction.IntValues.Count > 0)
            {
                // Missing key is treated as 0.
                int actualInt = 0;
                if (parameters?.Ints != null)
                    parameters.Ints.TryGetValue(parameterKey, out actualInt);

                int requiredInt = restriction.IntValues[0];
                return CompareRestrictionStaticChecker.Check(actualInt, requiredInt, restriction.CompareOptions);
            }

            if (restriction.BoolValues != null && restriction.BoolValues.Count > 0)
            {
                // Missing key is treated as false.
                bool actualBool = false;
                if (parameters?.Bools != null)
                    parameters.Bools.TryGetValue(parameterKey, out actualBool);

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

            // Missing key is treated as empty string.
            string actualString = string.Empty;
            if (parameters?.Strings != null
                && parameters.Strings.TryGetValue(parameterKey, out string storedString)
                && storedString != null)
            {
                actualString = storedString;
            }

            string requiredString = restriction.StringValues[1];
            return CompareRestrictionStaticChecker.Check(actualString, requiredString, restriction.CompareOptions);
        }
    }
}
