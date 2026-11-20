using System.Collections.Generic;
using System.IO;
using Modules.Space.Scripts.Implementation.Definitions;
using Modules.Space.Scripts.Implementation.Session;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Modules.Space.Scripts.Editor
{
    /// <summary>
    /// Draws definition radii in the scene view before Play.
    /// During Play the runtime gizmo follows the spawned transform.
    /// </summary>
    [InitializeOnLoad]
    public static class SpaceSectorRadiusGizmos
    {
        private const string SECTORS_FOLDER = "Assets/Modules/Space/Resources/Definitions/Space/Sectors";
        private const string OBJECTS_FOLDER = "Assets/Modules/Space/Resources/Definitions/Space/Objects";
        private const string HULLS_FOLDER = "Assets/Modules/Space/Resources/Definitions/Space/Hulls";

        private static readonly JsonSerializerSettings SETTINGS = new JsonSerializerSettings
        {
            Converters = { new StringEnumConverter() }
        };

        private static Dictionary<string, SpaceObjectDef> _objects = new Dictionary<string, SpaceObjectDef>();
        private static List<SpaceHullDef> _hulls = new List<SpaceHullDef>();
        private static List<SpaceSectorDef> _sectors = new List<SpaceSectorDef>();
        private static bool _loadFailed;

        static SpaceSectorRadiusGizmos()
        {
            EditorApplication.projectChanged += Load;
            SceneView.duringSceneGui += OnSceneGui;
            Load();
        }

        private static void OnSceneGui(SceneView view)
        {
            if (view == null || !view.drawGizmos || Application.isPlaying || _loadFailed)
                return;

            var previousTest = Handles.zTest;
            Handles.zTest = CompareFunction.Always;

            for (var i = 0; i < _sectors.Count; i++)
            {
                var sector = _sectors[i];
                if (sector == null || !SceneIsOpen(sector.SceneName) || sector.Placements == null)
                    continue;

                for (var p = 0; p < sector.Placements.Count; p++)
                    DrawPlacement(sector.Placements[p]);
            }

            Handles.zTest = previousTest;
        }

        private static void DrawPlacement(SpaceSectorPlacement placement)
        {
            if (placement == null || string.IsNullOrEmpty(placement.ObjectId))
                return;

            if (!_objects.TryGetValue(placement.ObjectId, out var body) || body == null)
                return;

            var center = new Vector3(placement.X, placement.Y, placement.Z);
            if (body.Radius > 0f)
                DrawSphere(center, body.Radius, SpaceRadiusGizmo.ColorFor(body.ObjectType), body.Id);

            if (body.ObjectType != SpaceObjectType.Spawn)
                return;

            for (var i = 0; i < _hulls.Count; i++)
            {
                var hull = _hulls[i];
                if (hull == null || hull.Radius <= 0f)
                    continue;

                DrawSphere(center, hull.Radius, SpaceRadiusGizmo.HULL_COLOR, hull.Id);
            }
        }

        private static void DrawSphere(Vector3 center, float radius, Color color, string label)
        {
            Handles.color = color;
            Handles.DrawWireDisc(center, Vector3.up, radius);
            Handles.DrawWireDisc(center, Vector3.right, radius);
            Handles.DrawWireDisc(center, Vector3.forward, radius);
            Handles.Label(center + Vector3.up * radius, label + "  r " + radius.ToString("0.###"));
        }

        private static bool SceneIsOpen(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
                return false;

            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.IsValid() && scene.isLoaded && scene.name == sceneName)
                    return true;
            }

            return false;
        }

        private static void Load()
        {
            _loadFailed = false;
            _objects = LoadFolder<SpaceObjectDef>(OBJECTS_FOLDER);
            _hulls = new List<SpaceHullDef>(LoadFolder<SpaceHullDef>(HULLS_FOLDER).Values);
            _sectors = new List<SpaceSectorDef>(LoadFolder<SpaceSectorDef>(SECTORS_FOLDER).Values);
        }

        private static Dictionary<string, T> LoadFolder<T>(string folder) where T : Modules.Definitions.Scripts.Defs.AbstractDefinition
        {
            var result = new Dictionary<string, T>();
            if (!AssetDatabase.IsValidFolder(folder))
                return result;

            var guids = AssetDatabase.FindAssets("t:TextAsset", new[] { folder });
            for (var i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                if (asset == null || string.IsNullOrWhiteSpace(asset.text))
                    continue;

                try
                {
                    var definition = JsonConvert.DeserializeObject<T>(asset.text, SETTINGS);
                    if (definition == null)
                        continue;

                    definition.Id = Path.GetFileNameWithoutExtension(path);
                    result[definition.Id] = definition;
                }
                catch (JsonException exception)
                {
                    _loadFailed = true;
                    UnityEngine.Debug.LogError($"[Space] Radius gizmo skipped '{path}'. {exception.Message}");
                }
            }

            return result;
        }
    }
}
