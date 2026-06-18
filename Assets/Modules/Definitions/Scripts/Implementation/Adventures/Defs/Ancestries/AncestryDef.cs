using Modules.Definitions.Scripts.Defs;
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
        public string Title;
        public string Description;
        public AncestrySize Size;
        public int Speed;
        public List<string> AbilityBoosts;
        public List<string> AbilityFlaws;
    }
}
