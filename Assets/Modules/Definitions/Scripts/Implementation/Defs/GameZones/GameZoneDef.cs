using Modules.Definitions.Scripts.Defs;
using Modules.Match3.Scripts.Interfaces;

namespace Modules.Definitions.Scripts.Implementation.Defs.GameZones
{
    public class GameZoneDef : AbstractDefinition, IGameRoundData
    {
        public int[,] Mask;
        public int[,] Presets;

        public int[,] GetMask()
        {
            return Mask;
        }

        public int[,] GetPresets()
        {
            return Presets;
        }
    }
}
