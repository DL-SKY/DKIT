using Modules.Definitions.Scripts.Defs;
using System.Collections.Generic;

namespace Modules.Definitions.Scripts.Implementation.Defs.Single
{
    public class CellsMapDef : AbstractDefinition
    {
        /// <summary>
        /// Key - value in [,] matrix; Value - defId
        /// </summary>
        public Dictionary<int, string> Map;
    }
}
