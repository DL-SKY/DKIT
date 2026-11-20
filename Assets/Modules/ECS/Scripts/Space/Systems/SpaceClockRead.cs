using Leopotam.Ecs;
using Modules.ECS.Scripts.Space.Components;

namespace Modules.ECS.Scripts.Space.Systems
{
    internal static class SpaceClockRead
    {
        public static bool TryGetDelta(EcsFilter<SpaceClock> clocks, out float deltaSeconds)
        {
            deltaSeconds = 0f;
            if (clocks == null)
                return false;

            foreach (var index in clocks)
            {
                deltaSeconds = clocks.Get1(index).DeltaSeconds;
                return deltaSeconds > 0f;
            }

            return false;
        }
    }
}
