using System.Collections.Generic;
using Modules.Definitions.Scripts.Defs;

namespace Modules.Space.Scripts.Implementation.Definitions
{
    public sealed class SpaceHullDef : AbstractDefinition
    {
        public List<string> ModuleSlotIds;
        public List<string> CommandIds;
        public string VisualResourcesPath;
        public string IconResourcesPath;
        public float CameraDistance;
        public float Radius;
        public float ManeuverabilityDegreesPerSecond;
        public SpaceSizeType SizeType;

        public bool HasFlightRates()
        {
            return IsFinite(Radius)
                && IsFinite(ManeuverabilityDegreesPerSecond)
                && Radius >= 0f
                && ManeuverabilityDegreesPerSecond > 0f;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
