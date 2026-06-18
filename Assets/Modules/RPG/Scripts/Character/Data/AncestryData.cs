using Modules.Definitions.Scripts.Defs;
using System.Collections.Generic;

namespace Modules.RPG.Scripts.Character.Data
{
    public enum AncestrySize
    {
        Small = 0,
        Medium = 1,
        Large = 2,
    }

    public class AncestryData : AbstractDefinition
    {
        public string Title;
        public string Description;

        public AncestrySize Size;
        public int Speed;

        /// <summary>
        /// Бонусы к характеристикам (PF2e: free/boost).
        /// </summary>
        public List<string> AbilityBoosts;

        /// <summary>
        /// Штрафы к характеристикам (PF2e: flaw).
        /// </summary>
        public List<string> AbilityFlaws;
    }
}
