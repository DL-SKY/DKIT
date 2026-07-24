using Modules.Definitions.Scripts.Defs;
using System.Collections.Generic;

namespace Modules.Definitions.Scripts.Implementation.Adventures.Defs.Rules
{
    public class RuleDef : AbstractDefinition
    {
        public List<string> Tags;

        public Dictionary<int, int> AbilityBoostPointCost;
        public Dictionary<string, string> SkillDependencies;

        /// <summary>
        /// Formulas for computed character parameters (skills, MaxHitPoints, etc.).
        /// Key matches a parameter id (e.g. <c>Athletics</c>, <c>MaxHitPoints</c>);
        /// value is an expression with <c>+</c>, <c>*</c>, and parentheses
        /// (e.g. <c>STR+PROFICIENCY+ITEMS</c>, <c>ANCESTRY_HP+(CLASS_HP+CON+PER_LEVEL)*Level+BONUS</c>).
        /// Resolved by <c>CharacterParametersProxy.GetTotalValue</c>.
        /// </summary>
        public Dictionary<string, string> ParameterFormulas;
    }
}
