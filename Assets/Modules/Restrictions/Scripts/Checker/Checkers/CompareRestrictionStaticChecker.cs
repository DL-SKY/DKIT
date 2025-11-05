using Modules.Restrictions.Scripts.Core;

namespace Modules.Restrictions.Scripts.Checker.Checkers
{
    public static class CompareRestrictionStaticChecker
    {
        public static bool Check<T>(T value, T requiredValue, CompareType compareType)
        {
            ICompareChecker<T> comparer = null;
            
            var type = typeof(T);
            if (type == typeof(string))
                comparer = new CompareStringChecker() as ICompareChecker<T>;
            if (type == typeof(int))
                comparer = new CompareIntChecker() as ICompareChecker<T>;
            if (type == typeof(long))
                comparer = new CompareLongChecker() as ICompareChecker<T>;

            return comparer != null
                ? comparer.Check(value, requiredValue, compareType)
                : false;
        }
    }


    internal interface ICompareChecker<T>
    {
        public bool Check(T value, T requiredValue, CompareType compareType);
    }

    internal class CompareStringChecker : ICompareChecker<string>
    {
        public bool Check(string value, string requiredValue, CompareType compareType)
        {
            switch (compareType)
            {
                case CompareType.Equal:
                    return value.Equals(requiredValue);
                case CompareType.NotEqual:
                    return !value.Equals(requiredValue);

                default:
                    return false;
            }
        }
    }

    internal class CompareIntChecker : ICompareChecker<int>
    {
        public bool Check(int value, int requiredValue, CompareType compareType)
        {
            switch (compareType)
            {
                case CompareType.Equal:
                    return value == requiredValue;
                case CompareType.NotEqual:
                    return value != requiredValue;

                case CompareType.More:
                    return value > requiredValue;
                case CompareType.MoreEqual:
                    return value >= requiredValue;

                case CompareType.Less:
                    return value < requiredValue;
                case CompareType.LessEqual:
                    return value <= requiredValue;

                default:
                    return false;
            }
        }
    }

    internal class CompareLongChecker : ICompareChecker<long>
    {
        public bool Check(long value, long requiredValue, CompareType compareType)
        {
            switch (compareType)
            {
                case CompareType.Equal:
                    return value == requiredValue;
                case CompareType.NotEqual:
                    return value != requiredValue;

                case CompareType.More:
                    return value > requiredValue;
                case CompareType.MoreEqual:
                    return value >= requiredValue;

                case CompareType.Less:
                    return value < requiredValue;
                case CompareType.LessEqual:
                    return value <= requiredValue;

                default:
                    return false;
            }
        }
    }
}
