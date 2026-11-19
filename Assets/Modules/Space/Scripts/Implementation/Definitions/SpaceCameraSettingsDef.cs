using Modules.Definitions.Scripts.Defs;

namespace Modules.Space.Scripts.Implementation.Definitions
{
    /// <summary>
    /// Single record for the orbit camera. Default pose is yaw and pitch.
    /// Distance limits live here. The starting distance of a hull lives on that hull.
    /// </summary>
    public sealed class SpaceCameraSettingsDef : AbstractDefinition
    {
        public float MinDistance;
        public float MaxDistance;
        public float DefaultYaw;
        public float DefaultPitch;
        public float MinPitch;
        public float MaxPitch;
        public float DegreesPerPixel;
        public float ScrollStep;
        public float DragThresholdPixels;

        public bool IsUsable()
        {
            return MinDistance > 0f
                && MinDistance < MaxDistance
                && MinPitch < MaxPitch
                && DefaultPitch >= MinPitch
                && DefaultPitch <= MaxPitch
                && DegreesPerPixel > 0f
                && ScrollStep > 0f
                && DragThresholdPixels > 0f;
        }
    }
}
