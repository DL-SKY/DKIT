using System.Collections.Generic;
using Modules.Definitions.Scripts.Defs;
using UnityEngine;

namespace Modules.Space.Scripts.Implementation.Definitions
{
    /// <summary>
    /// One map sector. The file name is the sector id. The scene and the player spawn points live here.
    /// </summary>
    public sealed class SpaceSectorDef : AbstractDefinition
    {
        public string SceneName;
        public List<SpaceSectorSpawnPoint> SpawnPoints;

        public bool IsUsable()
        {
            if (string.IsNullOrWhiteSpace(SceneName) || SpawnPoints == null || SpawnPoints.Count == 0)
                return false;

            for (var i = 0; i < SpawnPoints.Count; i++)
            {
                if (SpawnPoints[i] == null)
                    return false;
            }

            return true;
        }

        public bool TryPickSpawnPoint(out Vector3 position, out float yaw)
        {
            position = Vector3.zero;
            yaw = 0f;
            if (!IsUsable())
                return false;

            var point = SpawnPoints.Count == 1
                ? SpawnPoints[0]
                : SpawnPoints[UnityEngine.Random.Range(0, SpawnPoints.Count)];
            position = new Vector3(point.X, point.Y, point.Z);
            yaw = point.Yaw;
            return true;
        }
    }

    public sealed class SpaceSectorSpawnPoint
    {
        public float X;
        public float Y;
        public float Z;
        public float Yaw;
    }
}
