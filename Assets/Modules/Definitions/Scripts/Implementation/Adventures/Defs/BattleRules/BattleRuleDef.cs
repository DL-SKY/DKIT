using Modules.Definitions.Scripts.Defs;
using System.Collections.Generic;

namespace Modules.Definitions.Scripts.Implementation.Adventures.Defs.BattleRules
{
    public class BattleRuleDef : AbstractDefinition
    {
        public List<string> Tags;


        public bool UseMultipleAttackPenalty;

        /// <summary>
        /// Например как в PF2e - Glossary.Rules.ATTACK
        /// </summary>
        public List<string> AttackTags;

        //public Dictionary<string, int> 
    }
}
