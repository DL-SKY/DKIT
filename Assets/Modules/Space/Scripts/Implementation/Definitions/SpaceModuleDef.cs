using Modules.Definitions.Scripts.Defs;

namespace Modules.Space.Scripts.Implementation.Definitions
{
    /// <summary>
    /// One piece of fitted equipment. The file name is the id.
    /// An engine carries maximum speed and the spool time. A weapon does not.
    /// </summary>
    public sealed class SpaceModuleDef : AbstractDefinition
    {
        public SpaceModuleKind Kind;
        public string SlotId;
        public string WeaponGroup;
        public float MaxSpeedMetersPerSecond;
        public float InertiaSeconds;

        public bool IsUsable()
        {
            if (string.IsNullOrWhiteSpace(SlotId))
                return false;

            if (Kind != SpaceModuleKind.Engine)
                return true;

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
