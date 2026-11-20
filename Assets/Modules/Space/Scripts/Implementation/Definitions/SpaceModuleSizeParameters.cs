namespace Modules.Space.Scripts.Implementation.Definitions
{
    /// <summary>
    /// Engine numbers for one <see cref="SpaceSizeType"/>. A weapon row is not read.
    /// </summary>
    public sealed class SpaceModuleSizeParameters
    {
        public SpaceSizeType SizeType;
        public float MaxSpeedMetersPerSecond;
        public float InertiaSeconds;

        public bool IsUsable()
        {
            return IsFinite(MaxSpeedMetersPerSecond)
                && IsFinite(InertiaSeconds)
                && MaxSpeedMetersPerSecond > 0f
                && InertiaSeconds > 0f;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
