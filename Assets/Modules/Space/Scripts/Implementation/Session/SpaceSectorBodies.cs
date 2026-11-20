using System.Collections.Generic;
using Leopotam.Ecs;
using Modules.ECS.Scripts.Space.Components;
using Modules.Space.Scripts.Implementation.Definitions;
using UnityEngine;

namespace Modules.Space.Scripts.Implementation.Session
{
    /// <summary>
    /// Creates sector bodies and the player hull entity from definitions.
    /// The scene file does not contain these objects.
    /// </summary>
    public static class SpaceSectorBodies
    {
        public const string ROOT_NAME = "SectorBodies";

        public static List<GameObject> Spawn(EcsWorld world, SpaceDefinitionsManager definitions, SpaceSectorDef sector)
        {
            var created = new List<GameObject>();
            if (world == null || definitions == null || sector == null || sector.Placements == null)
            {
                UnityEngine.Debug.LogError("[Space] Sector bodies were not created. World or sector is missing.");
                return created;
            }

            var root = new GameObject(ROOT_NAME);
            created.Add(root);
            var views = 0;

            for (var i = 0; i < sector.Placements.Count; i++)
            {
                var placement = sector.Placements[i];
                if (placement == null || !definitions.TryGetObject(placement.ObjectId, out var body))
                {
                    UnityEngine.Debug.LogError($"[Space] Placement {i} was skipped. Object '{placement?.ObjectId}' is missing.");
                    continue;
                }

                var position = new Vector3(placement.X, placement.Y, placement.Z);
                var entity = world.NewEntity();
                entity.Get<SpacePosition>() = new SpacePosition { Value = position };
                entity.Get<SpaceYaw>() = new SpaceYaw { Degrees = placement.Yaw };
                entity.Get<SpaceRadius>() = new SpaceRadius { Meters = body.Radius };
                entity.Get<SpaceKind>() = new SpaceKind { Value = body.Kind };

                var view = CreateView(body, position, placement.Yaw);
                if (view == null)
                    continue;

                view.SetParent(root.transform, true);
                entity.Get<SpaceView>() = new SpaceView { Transform = view };
                views++;
            }

            UnityEngine.Debug.Log($"[Space] Sector bodies created. Placements {sector.Placements.Count}, views {views}.");
            return created;
        }

        public static void AttachPlayer(EcsWorld world, Transform ship, SpaceHullDef hull, SpaceModuleDef engine, Vector3 position, float yaw)
        {
            if (world == null || ship == null || hull == null)
            {
                UnityEngine.Debug.LogError("[Space] Player hull entity was not created.");
                return;
            }

            var entity = world.NewEntity();
            entity.Get<SpacePlayerHull>();
            entity.Get<SpacePosition>() = new SpacePosition { Value = position };
            entity.Get<SpaceYaw>() = new SpaceYaw { Degrees = yaw };
            entity.Get<SpaceRadius>() = new SpaceRadius { Meters = hull.Radius };
            entity.Get<SpaceTurnRate>() = new SpaceTurnRate { DegreesPerSecond = hull.ManeuverabilityDegreesPerSecond };
            entity.Get<SpaceMotion>() = CreateMotion(engine);
            entity.Get<SpaceView>() = new SpaceView { Transform = ship };
            AddHullGizmo(ship.gameObject, hull.Radius);
            UnityEngine.Debug.Log($"[Space] Player hull entity created. Radius {hull.Radius}.");
        }

        private static Transform CreateView(SpaceObjectDef body, Vector3 position, float yaw)
        {
            if (string.IsNullOrWhiteSpace(body.VisualResourcesPath))
                return null;

            var prefab = Resources.Load<GameObject>(body.VisualResourcesPath);
            if (prefab == null)
            {
                UnityEngine.Debug.LogError($"[Space] Object '{body.Id}' visual '{body.VisualResourcesPath}' was not found.");
                return null;
            }

            var instance = Object.Instantiate(prefab, position, Quaternion.Euler(0f, yaw, 0f));
            instance.name = body.Id;
            instance.transform.localScale = Vector3.one * body.UniformScale;
            AddBodyGizmo(instance, body);
            return instance.transform;
        }

        private static SpaceMotion CreateMotion(SpaceModuleDef engine)
        {
            if (engine == null || engine.Kind != SpaceModuleKind.Engine)
            {
                UnityEngine.Debug.LogError("[Space] Player hull has no fitted engine. Speed stays at zero.");
                return new SpaceMotion();
            }

            return new SpaceMotion
            {
                MaxMetersPerSecond = engine.MaxSpeedMetersPerSecond,
                InertiaSeconds = engine.InertiaSeconds
            };
        }

        private static void AddBodyGizmo(GameObject instance, SpaceObjectDef body)
        {
#if UNITY_EDITOR
            if (body == null)
                return;

            AddRadiusGizmo(instance, body.Radius, SpaceRadiusGizmo.ColorFor(body.Kind));
#endif
        }

        private static void AddHullGizmo(GameObject instance, float radius)
        {
#if UNITY_EDITOR
            AddRadiusGizmo(instance, radius, SpaceRadiusGizmo.HULL_COLOR);
#endif
        }

#if UNITY_EDITOR
        private static void AddRadiusGizmo(GameObject instance, float radius, Color color)
        {
            if (instance == null || radius <= 0f)
                return;

            var gizmo = instance.AddComponent<SpaceRadiusGizmo>();
            gizmo.Radius = radius;
            gizmo.Color = color;
        }
#endif
    }
}
