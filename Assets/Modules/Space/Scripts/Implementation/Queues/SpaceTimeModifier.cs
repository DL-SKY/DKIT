namespace Modules.Space.Scripts.Implementation.Queues
{
    public enum SpaceTimeFactorKind
    {
        Percent = 0,
        Multiplier = 1
    }

    /// <summary>
    /// A factor applied on top of BaseTimeMs. It is not written back into the definition.
    /// </summary>
    public sealed class SpaceTimeModifier
    {
        public SpaceTimeFactorKind Kind;
        public float Value;
    }
}
