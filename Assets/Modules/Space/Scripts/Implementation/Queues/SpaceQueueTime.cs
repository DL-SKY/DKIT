using System;
using System.Collections.Generic;

namespace Modules.Space.Scripts.Implementation.Queues
{
    /// <summary>
    /// The only queue duration formula. ResultMs = BaseTimeMs * product(factors), minimum 1.
    /// </summary>
    public static class SpaceQueueTime
    {
        public const long MINIMUM_MS = 1;

        public static double CombineFactors(IReadOnlyList<SpaceTimeModifier> modifiers)
        {
            var product = 1d;
            if (modifiers == null)
                return product;

            for (var i = 0; i < modifiers.Count; i++)
            {
                var modifier = modifiers[i];
                var factor = modifier.Kind == SpaceTimeFactorKind.Percent
                    ? 1d + modifier.Value / 100d
                    : modifier.Value;
                product *= factor;
            }

            return product;
        }

        public static long ApplyFactor(long baseTimeMs, double combinedFactor)
        {
            if (baseTimeMs < 0)
                baseTimeMs = 0;

            var result = baseTimeMs * combinedFactor;
            if (double.IsNaN(result) || double.IsInfinity(result) || result < MINIMUM_MS)
                return MINIMUM_MS;

            return (long)Math.Round(result, MidpointRounding.AwayFromZero);
        }

        public static long ComputeResultMs(long baseTimeMs, IReadOnlyList<SpaceTimeModifier> modifiers)
        {
            return ApplyFactor(baseTimeMs, CombineFactors(modifiers));
        }

        public static bool CheckSamples()
        {
            return ComputeResultMs(1000, null) == 1000
                && ComputeResultMs(1000, Percent(-25f)) == 750
                && ComputeResultMs(1000, Percent(25f)) == 1250
                && ComputeResultMs(1000, Multiplier(0.5f)) == 500
                && ComputeResultMs(1000, Multiplier(2f)) == 2000
                && ComputeResultMs(1000, new List<SpaceTimeModifier>
                {
                    new SpaceTimeModifier { Kind = SpaceTimeFactorKind.Percent, Value = -25f },
                    new SpaceTimeModifier { Kind = SpaceTimeFactorKind.Multiplier, Value = 2f }
                }) == 1500
                && ComputeResultMs(10, Multiplier(0.01f)) == MINIMUM_MS;
        }

        private static List<SpaceTimeModifier> Percent(float value)
        {
            return new List<SpaceTimeModifier>
            {
                new SpaceTimeModifier { Kind = SpaceTimeFactorKind.Percent, Value = value }
            };
        }

        private static List<SpaceTimeModifier> Multiplier(float value)
        {
            return new List<SpaceTimeModifier>
            {
                new SpaceTimeModifier { Kind = SpaceTimeFactorKind.Multiplier, Value = value }
            };
        }
    }
}
