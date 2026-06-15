using Modules.Definitions.Scripts.Implementation.Defs.GameZones;
using Modules.Match3.Scripts.Interfaces;

namespace Modules.Match3.Scripts.Implementation.Data
{
    public class GameZoneData : IGameZoneData
    {
        private GameZoneDef _def;

        public GameZoneData(GameZoneDef def)
        {
            _def = def;
        }

        public int[,] GetMask()
        {
            return _def.Mask;
        }

        public int[,] GetPresets()
        {
            return _def.Presets;
        }
    }
}
