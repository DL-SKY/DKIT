using System.Collections.Generic;
using Modules.Definitions.Scripts.Defs;
using UnityEngine;

namespace Modules.Space.Scripts.Implementation.Definitions
{
    /// <summary>
    /// One map sector. The file name is the sector id.
    /// Placements create bodies at runtime. The player hull is not one of them.
    /// </summary>
    public sealed class SpaceSectorDef : AbstractDefinition
    {
        public string SceneName;
        public List<SpaceSectorPlacement> Placements;

        public bool IsUsable(IReadOnlyDictionary<string, SpaceObjectDef> objects)
        {
            if (string.IsNullOrWhiteSpace(SceneName) || objects == null || !TryCollect(objects, out var spawns, out var valid))
                return false;

            return valid && spawns.Count > 0;
        }

        public bool TryPickSpawnPoint(IReadOnlyDictionary<string, SpaceObjectDef> objects, out Vector3 position, out float yaw)
        {
            position = Vector3.zero;
            yaw = 0f;
            if (!TryCollect(objects, out var spawns, out var valid) || !valid || spawns.Count == 0)
                return false;

            var placement = spawns.Count == 1
                ? spawns[0]
                : spawns[UnityEngine.Random.Range(0, spawns.Count)];
            position = new Vector3(placement.X, placement.Y, placement.Z);
            yaw = placement.Yaw;
            return true;
        }

        private bool TryCollect(IReadOnlyDictionary<string, SpaceObjectDef> objects, out List<SpaceSectorPlacement> spawns, out bool valid)
        {
            spawns = new List<SpaceSectorPlacement>();
            valid = false;
            if (Placements == null || Placements.Count == 0 || objects == null)
                return false;

            for (var i = 0; i < Placements.Count; i++)
            {
                var placement = Placements[i];
                if (placement == null || string.IsNullOrWhiteSpace(placement.ObjectId))
                    return true;

                if (!objects.TryGetValue(placement.ObjectId, out var body) || body == null || !body.IsUsable())
                    return true;

                if (body.Kind == SpaceObjectKind.Spawn)
                    spawns.Add(placement);
            }

            valid = true;
            return true;
        }
    }

    /// <summary>
    /// One placed body. ObjectId is the file name of a SpaceObjectDef.
    /// Axes match the old spawn point: X right, Y up, Z forward, yaw around Y.
    /// </summary>
    public sealed class SpaceSectorPlacement
    {
        public string ObjectId;
        public float X;
        public float Y;
        public float Z;
        public float Yaw;
    }
}
