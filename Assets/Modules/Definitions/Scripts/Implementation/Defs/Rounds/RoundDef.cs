using Modules.Definitions.Scripts.Defs;
using System.Collections.Generic;

namespace Modules.Definitions.Scripts.Implementation.Defs.Rounds
{
    public class RoundDef : AbstractDefinition
    {
        public List<string> Tags;

        /// <summary>
        /// GameZoneDef
        /// </summary>
        public string GameZone;

        /// <summary>
        /// GameZoneGemsDef
        /// </summary>
        public string Gems;
    }
}
