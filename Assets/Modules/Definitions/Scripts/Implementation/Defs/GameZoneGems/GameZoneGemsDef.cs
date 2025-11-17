using Modules.Definitions.Scripts.Defs;
using System.Collections.Generic;

namespace Modules.Definitions.Scripts.Implementation.Defs.GameZoneGems
{
    public class GameZoneGemsDef : AbstractDefinition
    {
        /// <summary>
        /// Key - defId; Value - weight for randomize
        /// </summary>
        public Dictionary<string, int> Gems;
    }
}
