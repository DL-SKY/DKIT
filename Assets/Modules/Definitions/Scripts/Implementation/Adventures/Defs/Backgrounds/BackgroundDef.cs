using Modules.Definitions.Scripts.Defs;
using Modules.Restrictions.Scripts.Core;
using System.Collections.Generic;

namespace Modules.Definitions.Scripts.Implementation.Adventures.Defs.Backgrounds
{
    public class BackgroundDef : AbstractDefinition
    {
        /// <summary>
        /// IAP / store product key.
        /// </summary>
        public string ProductId;

        /// <summary>
        /// Soft-currency price (0 when unused or IAP-only).
        /// </summary>
        public int Price;


        public bool Disabled;

        public List<Restriction> Restrictions;

        public List<string> Tags;

        public string Icon;
        public string Title;
        public string Description;

        public List<string> Features;
    }
}
