using Modules.Definitions.Scripts.Defs;
using Modules.Restrictions.Scripts.Core;
using Modules.State.Scripts.Implementation.Adventure.StateDatas;
using System.Collections.Generic;

namespace Modules.Definitions.Scripts.Implementation.Adventures.Defs.Ancestries
{
    public enum AncestrySize
    {
        Small = 0,
        Medium = 1,
        Large = 2,
    }


    public class AncestryDef : AbstractDefinition
    {
        public bool Disabled;

        public List<Restriction> Restrictions;

        public List<string> Tags;

        public string Icon;
        public string Title;
        public string Description;

        public AncestrySize Size;
        public int Speed;

        /// <summary>
        /// Hit Points granted by ancestry at character creation (once, not per level).
        /// </summary>
        public int HitPoints;

        public Dictionary<int, List<string>> Features;

        public Dictionary<CharacterGender, List<string>> Names;
        public Dictionary<CharacterGender, List<string>> Avatars;
    }
}
