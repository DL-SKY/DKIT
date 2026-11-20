using Leopotam.Ecs;
using Modules.ECS.Scripts.Space.Components;
using UnityEngine;

namespace Modules.ECS.Scripts.Space.Systems
{
    /// <summary>
    /// Reads the live pose of the entity stored on a move order. A dead or position-less target is not a pose.
    /// </summary>
    public static class SpaceMoveTargetRead
    {
        public static bool TryGetPose(EcsEntity target, out Vector3 position, out float radius)
        {
            position = default;
            radius = 0f;
            if (!target.IsAlive() || !target.Has<SpacePosition>())
                return false;

            position = target.Get<SpacePosition>().Value;
            if (target.Has<SpaceRadius>() && target.Get<SpaceRadius>().Meters > 0f)
                radius = target.Get<SpaceRadius>().Meters;

            return true;
        }
    }
}
