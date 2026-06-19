using Modules.Definitions.Scripts.Defs;
using System.Collections.Generic;

namespace Modules.Definitions.Scripts.Implementation.Adventures.Defs.Spells
{
    public enum SpellType
    {
        Cantrip = 0,
        Spell = 1,
        Focus = 2,
        Ritual = 3,
    }


    public class SpellDef : AbstractDefinition
    {
        public bool Disabled;

        public SpellType Type;
        public int Level;

        public List<string> Tags;

        public string Title;
        public string Description;
    }
}
