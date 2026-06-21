using Modules.Definitions.Scripts.Defs;
using System.Collections.Generic;

namespace Modules.Definitions.Scripts.Implementation.Adventures.Defs.Single
{
    public class RuleSettingsDef : AbstractDefinition
    {
        public List<string> Tags;

        /// <summary>
        /// RuleDef
        /// </summary>
        public string Rule;

        /// <summary>
        /// BattleRuleDef
        /// </summary>
        public string BattleRule;

        public string StartAdventure;
    }
}
