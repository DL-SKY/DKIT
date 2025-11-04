using Modules.Definitions.Scripts.Defs;

namespace Modules.Definitions.Scripts.Implementation.Defs.GameZones
{
    public class GameZoneDef : AbstractDefinition
    {
        public int Width;
        public int Height;

        public int[,] Mask;
        public int[,] Presets;
    }
}
