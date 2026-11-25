using System.Collections.Generic;
using Modules.Space.Scripts.Implementation.Definitions;
using Modules.Space.Scripts.Implementation.Queues;
using UnityEditor;
using UnityEngine;

namespace Modules.Space.Scripts.Editor.Cheats
{
    /// <summary>
    /// Reports problems in the definitions <see cref="SpaceDefinitionsManager"/> loads. Does not change the catalog.
    /// </summary>
    public static class SpaceDefinitionsValidation
    {
        public static List<string> Test()
        {
            SpaceDefinitionsManager definitions = new SpaceDefinitionsManager();
            definitions.Init();

            List<string> issues = new List<string>();
            CheckCommands(definitions, issues);
            CheckModules(definitions, issues);
            CheckHulls(definitions, issues);
            CheckObjects(definitions, issues);
            CheckSectors(definitions, issues);
            CheckTimedTargets(definitions, issues);
            CheckCamera(definitions, issues);
            CheckMarkers(definitions, issues);

            if (!definitions.IsReady && issues.Count == 0)
                issues.Add("Catalog is not ready.");

            return issues;
        }

        private static void CheckCommands(SpaceDefinitionsManager definitions, List<string> issues)
        {
            if (definitions.Commands == null || definitions.Commands.Count == 0)
            {
                issues.Add("Commands: catalog is empty.");
                return;
            }

            foreach (KeyValuePair<string, SpaceCommandDef> pair in definitions.Commands)
            {
                SpaceCommandDef command = pair.Value;
                string label = "Command " + pair.Key;
                if (command == null)
                {
                    issues.Add(label + " did not load.");
                    continue;
                }

                RequireText(label, "Name", command.Name, issues);
                RequireText(label, "Description", command.Description, issues);
                CheckSprite(label, "IconResourcesPath", command.IconResourcesPath, issues);
                RequireUnique(label, "ObjectTypes", command.ObjectTypes, issues);
                RequireUnique(label, "Relations", command.Relations, issues);
            }
        }

        private static void CheckModules(SpaceDefinitionsManager definitions, List<string> issues)
        {
            if (definitions.Modules == null || definitions.Modules.Count == 0)
            {
                issues.Add("Modules: catalog is empty.");
                return;
            }

            bool hasEngine = false;
            foreach (KeyValuePair<string, SpaceModuleDef> pair in definitions.Modules)
            {
                SpaceModuleDef module = pair.Value;
                string label = "Module " + pair.Key;
                if (module == null)
                {
                    issues.Add(label + " did not load.");
                    continue;
                }

                if (module.SlotType == SpaceSlotType.Engine)
                    hasEngine = true;

                RequireText(label, "Name", module.Name, issues);
                RequireText(label, "Description", module.Description, issues);
                CheckSprite(label, "IconResourcesPath", module.IconResourcesPath, issues);
                CheckCommandIds(label, module.CommandIds, definitions, issues);
                CheckSizeParameters(label, module, issues);
            }

            if (!hasEngine)
                issues.Add("Modules: no engine is loaded.");
        }

        private static void CheckHulls(SpaceDefinitionsManager definitions, List<string> issues)
        {
            if (definitions.Hulls == null || definitions.Hulls.Count == 0)
            {
                issues.Add("Hulls: catalog is empty.");
                return;
            }

            SpaceCameraSettingsDef camera = definitions.CameraSettings;
            foreach (KeyValuePair<string, SpaceHullDef> pair in definitions.Hulls)
            {
                SpaceHullDef hull = pair.Value;
                string label = "Hull " + pair.Key;
                if (hull == null)
                {
                    issues.Add(label + " did not load.");
                    continue;
                }

                RequireText(label, "Name", hull.Name, issues);
                RequireText(label, "Description", hull.Description, issues);
                CheckSprite(label, "IconResourcesPath", hull.IconResourcesPath, issues);
                CheckPrefab(label, "VisualResourcesPath", hull.VisualResourcesPath, true, issues);
                if (!hull.HasFlightRates())
                    issues.Add(label + " radius or maneuverability is not usable.");

                if (camera != null && camera.IsUsable()
                    && (hull.CameraDistance < camera.MinDistance || hull.CameraDistance > camera.MaxDistance))
                    issues.Add(label + " CameraDistance is outside the camera limits.");

                CheckCommandIds(label, hull.CommandIds, definitions, issues);
                CheckHullSlots(label, hull, issues);
            }
        }

        private static void CheckObjects(SpaceDefinitionsManager definitions, List<string> issues)
        {
            if (definitions.Objects == null || definitions.Objects.Count == 0)
            {
                issues.Add("Objects: catalog is empty.");
                return;
            }

            foreach (KeyValuePair<string, SpaceObjectDef> pair in definitions.Objects)
            {
                SpaceObjectDef body = pair.Value;
                string label = "Object " + pair.Key;
                if (body == null)
                {
                    issues.Add(label + " did not load.");
                    continue;
                }

                if (!body.IsUsable())
                    issues.Add(label + " scale or radius is not usable.");

                CheckPrefab(label, "VisualResourcesPath", body.VisualResourcesPath, false, issues);
                CheckCommandIds(label, body.CommandIds, definitions, issues);
            }
        }

        private static void CheckSectors(SpaceDefinitionsManager definitions, List<string> issues)
        {
            if (definitions.Sectors == null || definitions.Sectors.Count == 0)
            {
                issues.Add("Sectors: catalog is empty.");
                return;
            }

            foreach (KeyValuePair<string, SpaceSectorDef> pair in definitions.Sectors)
            {
                SpaceSectorDef sector = pair.Value;
                string label = "Sector " + pair.Key;
                if (sector == null)
                {
                    issues.Add(label + " did not load.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(sector.SceneName))
                    issues.Add(label + " SceneName is empty.");
                else if (!SceneExists(sector.SceneName))
                    issues.Add(label + " scene was not found: " + sector.SceneName);

                CheckPlacements(label, sector, definitions, issues);
            }
        }

        private static void CheckTimedTargets(SpaceDefinitionsManager definitions, List<string> issues)
        {
            if (definitions.TimedTargets == null || definitions.TimedTargets.Count == 0)
            {
                issues.Add("Timed targets: catalog is empty.");
                return;
            }

            HashSet<long> skillDurations = new HashSet<long>();
            HashSet<SpaceQueueKind> kinds = new HashSet<SpaceQueueKind>();
            foreach (KeyValuePair<string, SpaceTimedTargetDef> pair in definitions.TimedTargets)
            {
                SpaceTimedTargetDef target = pair.Value;
                string label = "Timed target " + pair.Key;
                if (target == null)
                {
                    issues.Add(label + " did not load.");
                    continue;
                }

                if (target.BaseTimeMs < SpaceQueueTime.MINIMUM_MS)
                    issues.Add(label + " BaseTimeMs is below " + SpaceQueueTime.MINIMUM_MS + ".");

                kinds.Add(target.Kind);
                if (target.Kind == SpaceQueueKind.Skill)
                    skillDurations.Add(target.BaseTimeMs);
            }

            if (skillDurations.Count < 3)
                issues.Add("Timed targets: fewer than 3 different skill durations.");

            RequireKind(kinds, SpaceQueueKind.Skill, issues);
            RequireKind(kinds, SpaceQueueKind.BlueprintImprovement, issues);
            RequireKind(kinds, SpaceQueueKind.Disassembly, issues);
            RequireKind(kinds, SpaceQueueKind.BlueprintAttempt, issues);
            RequireKind(kinds, SpaceQueueKind.Production, issues);
        }

        private static void CheckCamera(SpaceDefinitionsManager definitions, List<string> issues)
        {
            SpaceCameraSettingsDef camera = definitions.CameraSettings;
            if (camera == null)
            {
                issues.Add("Camera settings did not load.");
                return;
            }

            if (!camera.IsUsable())
                issues.Add("Camera settings distances, pitch, or steps are not usable.");
        }

        private static void CheckMarkers(SpaceDefinitionsManager definitions, List<string> issues)
        {
            SpaceMarkerSettingsDef markers = definitions.MarkerSettings;
            if (markers == null)
            {
                issues.Add("Marker settings did not load.");
                return;
            }

            if (float.IsNaN(markers.MenuRadius) || float.IsInfinity(markers.MenuRadius) || markers.MenuRadius <= 0f)
                issues.Add("Marker settings MenuRadius must be greater than zero.");

            if (markers.Icons == null)
            {
                issues.Add("Marker settings Icons are missing.");
                return;
            }

            SpaceObjectType[] types = (SpaceObjectType[])System.Enum.GetValues(typeof(SpaceObjectType));
            for (int i = 0; i < types.Length; i++)
            {
                if (types[i] == SpaceObjectType.Ship)
                    continue;

                int matches = 0;
                for (int j = 0; j < markers.Icons.Count; j++)
                {
                    SpaceMarkerKindIcon entry = markers.Icons[j];
                    if (entry == null || entry.ObjectType != types[i])
                        continue;

                    matches++;
                    CheckSprite("Marker " + types[i], "IconResourcesPath", entry.IconResourcesPath, issues);
                }

                if (matches != 1)
                    issues.Add("Marker settings need one icon for " + types[i] + ".");
            }
        }

        private static void CheckSizeParameters(string label, SpaceModuleDef module, List<string> issues)
        {
            List<SpaceModuleSizeParameters> rows = module.SizeParameters;
            if (rows == null || !HasRow(rows))
            {
                issues.Add(label + " SizeParameters are required.");
                return;
            }

            HashSet<SpaceSizeType> seen = new HashSet<SpaceSizeType>();
            for (int i = 0; i < rows.Count; i++)
            {
                SpaceModuleSizeParameters row = rows[i];
                if (row == null)
                {
                    issues.Add(label + " SizeParameters row " + i + " is empty.");
                    continue;
                }

                if (!seen.Add(row.SizeType))
                    issues.Add(label + " SizeParameters repeats " + row.SizeType + ".");

                if (module.SlotType == SpaceSlotType.Engine && !row.IsUsable())
                    issues.Add(label + " SizeParameters " + row.SizeType + " needs speed and inertia greater than zero.");
            }
        }

        private static void CheckHullSlots(string label, SpaceHullDef hull, List<string> issues)
        {
            if (hull.Slots == null)
                return;

            for (int i = 0; i < hull.Slots.Count; i++)
            {
                if (hull.Slots[i] == null)
                    issues.Add(label + " slot " + i + " is empty.");
            }
        }

        private static void CheckPlacements(string label, SpaceSectorDef sector, SpaceDefinitionsManager definitions, List<string> issues)
        {
            if (sector.Placements == null || sector.Placements.Count == 0)
            {
                issues.Add(label + " has no placements.");
                return;
            }

            int spawns = 0;
            for (int i = 0; i < sector.Placements.Count; i++)
            {
                SpaceSectorPlacement placement = sector.Placements[i];
                string placementLabel = label + " placement " + i;
                if (placement == null || string.IsNullOrWhiteSpace(placement.ObjectId))
                {
                    issues.Add(placementLabel + " has no object id.");
                    continue;
                }

                if (!IsFinite(placement.X) || !IsFinite(placement.Y) || !IsFinite(placement.Z) || !IsFinite(placement.Yaw))
                    issues.Add(placementLabel + " pose is not finite.");

                if (definitions.Objects == null || !definitions.Objects.TryGetValue(placement.ObjectId, out SpaceObjectDef body) || body == null)
                {
                    issues.Add(placementLabel + " points at missing object '" + placement.ObjectId + "'.");
                    continue;
                }

                if (!body.IsUsable())
                    issues.Add(placementLabel + " points at unusable object '" + placement.ObjectId + "'.");

                if (body.ObjectType == SpaceObjectType.Spawn)
                    spawns++;
            }

            if (spawns == 0)
                issues.Add(label + " has no spawn point.");
        }

        private static void CheckCommandIds(string label, List<string> commandIds, SpaceDefinitionsManager definitions, List<string> issues)
        {
            if (commandIds == null)
                return;

            HashSet<string> seen = new HashSet<string>();
            for (int i = 0; i < commandIds.Count; i++)
            {
                string commandId = commandIds[i];
                if (string.IsNullOrEmpty(commandId) || !seen.Add(commandId))
                {
                    issues.Add(label + " CommandIds has an empty or repeated id.");
                    continue;
                }

                if (definitions.Commands == null
                    || !definitions.Commands.TryGetValue(commandId, out SpaceCommandDef command)
                    || command == null
                    || !command.IsUsable())
                    issues.Add(label + " CommandIds points at missing command '" + commandId + "'.");
            }
        }

        private static void CheckSprite(string label, string field, string path, List<string> issues)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                issues.Add(label + " " + field + " is empty.");
                return;
            }

            if (Resources.Load<Sprite>(path) == null)
                issues.Add(label + " " + field + " sprite was not found: " + path);
        }

        private static void CheckPrefab(string label, string field, string path, bool required, List<string> issues)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                if (required)
                    issues.Add(label + " " + field + " is empty.");
                return;
            }

            if (Resources.Load<GameObject>(path) == null)
                issues.Add(label + " " + field + " prefab was not found: " + path);
        }

        private static void RequireText(string label, string field, string value, List<string> issues)
        {
            if (string.IsNullOrWhiteSpace(value))
                issues.Add(label + " " + field + " is empty.");
        }

        private static void RequireUnique<T>(string label, string field, List<T> values, List<string> issues)
        {
            if (values == null || values.Count == 0)
            {
                issues.Add(label + " " + field + " is empty.");
                return;
            }

            HashSet<T> seen = new HashSet<T>();
            for (int i = 0; i < values.Count; i++)
            {
                if (!seen.Add(values[i]))
                {
                    issues.Add(label + " " + field + " repeats a value.");
                    return;
                }
            }
        }

        private static void RequireKind(HashSet<SpaceQueueKind> kinds, SpaceQueueKind kind, List<string> issues)
        {
            if (!kinds.Contains(kind))
                issues.Add("Timed targets: missing kind " + kind + ".");
        }

        private static bool HasRow(List<SpaceModuleSizeParameters> rows)
        {
            for (int i = 0; i < rows.Count; i++)
            {
                if (rows[i] != null)
                    return true;
            }

            return false;
        }

        private static bool SceneExists(string sceneName)
        {
            string[] guids = AssetDatabase.FindAssets(sceneName + " t:Scene");
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (System.IO.Path.GetFileNameWithoutExtension(path) == sceneName)
                    return true;
            }

            return false;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
