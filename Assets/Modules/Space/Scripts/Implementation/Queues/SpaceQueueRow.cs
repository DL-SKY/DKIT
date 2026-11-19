using System.Collections.Generic;

namespace Modules.Space.Scripts.Implementation.Queues
{
    public sealed class SpaceQueueRow
    {
        public SpaceQueueKind Kind;
        public string TargetId;
        public long BaseTimeMs;
        public List<SpaceTimeModifier> Modifiers = new List<SpaceTimeModifier>();
        public long StartedAtUnixMs;
        public double CombinedFactor;

        public bool IsStarted => StartedAtUnixMs > 0;

        public long GetRemainingMs(long nowUnixMs)
        {
            var durationMs = IsStarted
                ? SpaceQueueTime.ApplyFactor(BaseTimeMs, CombinedFactor)
                : SpaceQueueTime.ComputeResultMs(BaseTimeMs, Modifiers);

            if (!IsStarted)
                return durationMs;

            var elapsedMs = nowUnixMs - StartedAtUnixMs;
            if (elapsedMs >= durationMs)
                return 0;

            return durationMs - elapsedMs;
        }

        public void Start(long nowUnixMs)
        {
            CombinedFactor = SpaceQueueTime.CombineFactors(Modifiers);
            StartedAtUnixMs = nowUnixMs;
        }
    }
}
