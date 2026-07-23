using Modules.Definitions.Scripts.Defs;
using Modules.Restrictions.Scripts.Core;
using System.Collections.Generic;

namespace Modules.Definitions.Scripts.Implementation.Adventures.Defs.Backgrounds
{
    public class BackgroundDef : AbstractDefinition
    {
        public bool Disabled;

        public List<Restriction> Restrictions;

        public List<string> Tags;

        public string Icon;
        public string Title;
        public string Description;

        public List<string> Features;
    }
}
