using System.Collections.Generic;

namespace Modules.Restrictions.Scripts.Core
{
    public class Restriction
    {
        public RestrictionType Type;

        public List<string> StringValues;
        public List<int> IntValues;
        public List<long> LongValues;
        public CompareType CompareOptions;
    }
}
