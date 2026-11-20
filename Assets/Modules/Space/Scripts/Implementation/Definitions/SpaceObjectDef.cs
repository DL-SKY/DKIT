using System.Collections.Generic;
using Modules.Definitions.Scripts.Defs;

namespace Modules.Space.Scripts.Implementation.Definitions
{
    /// <summary>
    /// One kind of body that a sector can place. The file name is the id.
    /// Radius and scale live here, not on the sector placement.
    /// </summary>
    public sealed class SpaceObjectDef : AbstractDefinition
    {
        public SpaceObjectType ObjectType;
        public string VisualResourcesPath;
        public float UniformScale;
        public float Radius;
        public List<string> CommandIds;

        public bool IsUsable()
        {
            return IsFinite(UniformScale)
                && IsFinite(Radius)
                && UniformScale > 0f
                && Radius >= 0f;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
