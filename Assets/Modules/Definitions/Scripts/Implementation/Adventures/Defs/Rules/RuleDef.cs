using Modules.Definitions.Scripts.Defs;
using System.Collections.Generic;

namespace Modules.Definitions.Scripts.Implementation.Adventures.Defs.Rules
{
    public class RuleDef : AbstractDefinition
    {
        public List<string> Tags;

        public Dictionary<int, int> AbilityBoostPointCost;
        public Dictionary<string, string> SkillDependencies;
    }
}
