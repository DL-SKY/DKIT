using Modules.Definitions.Scripts.Defs;

namespace Modules.Space.Scripts.Implementation.Definitions
{
    /// <summary>
    /// Single record for the orbit camera. Rotation, distance, and zoom live here, not in code.
    /// </summary>
    public sealed class SpaceCameraSettingsDef : AbstractDefinition
    {
        public float DefaultDistance;
        public float MinDistance;
        public float MaxDistance;
        public float DefaultPitch;
        public float MinPitch;
        public float MaxPitch;
        public float DegreesPerPixel;
        public float ScrollStep;
        public float DragThresholdPixels;

        public bool IsUsable()
        {
            return MinDistance > 0f
                && MinDistance <= DefaultDistance
                && DefaultDistance <= MaxDistance
                && MinPitch < MaxPitch
                && DefaultPitch >= MinPitch
                && DefaultPitch <= MaxPitch
                && DegreesPerPixel > 0f
                && ScrollStep > 0f
                && DragThresholdPixels > 0f;
        }
    }
}
