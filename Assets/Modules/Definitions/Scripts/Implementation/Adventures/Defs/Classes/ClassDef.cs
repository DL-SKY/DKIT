using Modules.Definitions.Scripts.Defs;
using Modules.Restrictions.Scripts.Core;
using System.Collections.Generic;

namespace Modules.Definitions.Scripts.Implementation.Adventures.Defs.Classes
{
    public class ClassDef : AbstractDefinition
    {
        public bool Disabled;

        public List<Restriction> Restrictions;

        public List<string> Tags;

        public string Icon;
        public string Title;
        public string Description;

        /// <summary>
        /// Hit Points granted by the class each level (before Constitution modifier).
        /// </summary>
        public int HitPointsPerLevel;

        public Dictionary<int, List<string>> Features;
    }
}
