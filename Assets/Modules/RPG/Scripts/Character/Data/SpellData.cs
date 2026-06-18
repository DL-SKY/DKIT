using Modules.Definitions.Scripts.Defs;
using System.Collections.Generic;

namespace Modules.RPG.Scripts.Character.Data
{
    public enum SpellType
    {
        Cantrip = 0,
        Spell = 1,
        Focus = 2,
        Ritual = 3,
    }

    public class SpellData : AbstractDefinition
    {
        public string Title;
        public string Description;

        public SpellType Type;
        public int Level;

        /// <summary>
        /// Традиции заклинания (Arcane, Divine, Occult, Primal).
        /// </summary>
        public List<string> Traditions;
    }
}
