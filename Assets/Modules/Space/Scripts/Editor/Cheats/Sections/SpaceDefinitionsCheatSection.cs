using System;
using System.Collections.Generic;
using Modules.Cheats.Scripts.Editor.Core;
using Modules.Space.Scripts.Implementation.Definitions;
using UnityEditor;
using UnityEngine;
using Zenject;

namespace Modules.Space.Scripts.Editor.Cheats.Sections
{
    public sealed class SpaceDefinitionsCheatSection : CheatSectionBase
    {
        public override string Id => "Definitions";

        public override int Order => -998;

        public override void DrawContent()
        {
            SpaceDefinitionsManager definitions = TryGetDefinitions();
            if (definitions == null || !definitions.IsReady)
            {
                EditorGUILayout.HelpBox("Нет SpaceDefinitionsManager.", MessageType.Info);
                return;
            }

            DrawSectionHeader("Space.DefinitionsManager:");
            EditorGUILayout.Space(2f);
            DrawHulls(definitions);
            DrawModules(definitions);
            DrawCommands(definitions);
            DrawTimedTargets(definitions);
            DrawObjects(definitions);
            DrawSectors(definitions);
            DrawCameraSettings(definitions);
            DrawMarkerIcons(definitions);
        }

        private static SpaceDefinitionsManager TryGetDefinitions()
        {
            if (!Application.isPlaying || !ProjectContext.HasInstance)
                return null;

            return ProjectContext.Instance.Container.TryResolve<SpaceDefinitionsManager>();
        }

        private static void DrawHulls(SpaceDefinitionsManager definitions)
        {
            EditorGUILayout.LabelField("Hulls:");
            EditorGUI.indentLevel++;
            List<string> ids = SortedIds(definitions.Hulls);
            if (ids.Count == 0)
                EditorGUILayout.LabelField("—", EditorStyles.miniLabel);

            for (int i = 0; i < ids.Count; i++)
            {
                SpaceHullDef hull = definitions.Hulls[ids[i]];
                int slotCount = hull.Slots != null ? hull.Slots.Count : 0;
                int commandCount = hull.CommandIds != null ? hull.CommandIds.Count : 0;
                string visual = string.IsNullOrEmpty(hull.VisualResourcesPath) ? "—" : hull.VisualResourcesPath;
                string icon = string.IsNullOrEmpty(hull.IconResourcesPath) ? "—" : hull.IconResourcesPath;
                EditorGUILayout.LabelField((i + 1) + ". " + hull.Id + "  " + hull.SizeType + ", slots " + slotCount + ", commands " + commandCount + ", camera " + hull.CameraDistance + ", radius " + hull.Radius + ", maneuver " + hull.ManeuverabilityDegreesPerSecond + ", visual " + visual + ", icon " + icon);
            }

            EditorGUI.indentLevel--;
        }

        private static void DrawModules(SpaceDefinitionsManager definitions)
        {
            EditorGUILayout.LabelField("Modules:");
            EditorGUI.indentLevel++;
            List<string> ids = SortedIds(definitions.Modules);
            if (ids.Count == 0)
                EditorGUILayout.LabelField("—", EditorStyles.miniLabel);

            for (int i = 0; i < ids.Count; i++)
            {
                SpaceModuleDef module = definitions.Modules[ids[i]];
                string weaponGroup = string.IsNullOrEmpty(module.WeaponGroup) ? "—" : module.WeaponGroup;
                int parameterCount = module.SizeParameters != null ? module.SizeParameters.Count : 0;
                int commandCount = module.CommandIds != null ? module.CommandIds.Count : 0;
                string engine = module.SlotType == SpaceSlotType.Engine
                    ? ", size parameters " + parameterCount
                    : "";
                string icon = string.IsNullOrEmpty(module.IconResourcesPath) ? "—" : module.IconResourcesPath;
                EditorGUILayout.LabelField((i + 1) + ". " + module.Id + "  " + module.SlotType + " " + module.SizeType + ", commands " + commandCount + ", group " + weaponGroup + engine + ", icon " + icon);
            }

            EditorGUI.indentLevel--;
        }

        private static void DrawCommands(SpaceDefinitionsManager definitions)
        {
            EditorGUILayout.LabelField("Commands:");
            EditorGUI.indentLevel++;
            List<string> ids = SortedIds(definitions.Commands);
            if (ids.Count == 0)
                EditorGUILayout.LabelField("—", EditorStyles.miniLabel);

            for (int i = 0; i < ids.Count; i++)
            {
                SpaceCommandDef command = definitions.Commands[ids[i]];
                int typeCount = command.ObjectTypes != null ? command.ObjectTypes.Count : 0;
                int relationCount = command.Relations != null ? command.Relations.Count : 0;
                string icon = string.IsNullOrEmpty(command.IconResourcesPath) ? "—" : command.IconResourcesPath;
                EditorGUILayout.LabelField((i + 1) + ". " + command.Id + "  order " + command.PriorityOrder + ", types " + typeCount + ", relations " + relationCount + ", " + command.Name + ", icon " + icon);
            }

            EditorGUI.indentLevel--;
        }

        private static void DrawTimedTargets(SpaceDefinitionsManager definitions)
        {
            EditorGUILayout.LabelField("Timed targets:");
            EditorGUI.indentLevel++;
            List<string> ids = SortedIds(definitions.TimedTargets);
            if (ids.Count == 0)
                EditorGUILayout.LabelField("—", EditorStyles.miniLabel);

            for (int i = 0; i < ids.Count; i++)
            {
                SpaceTimedTargetDef target = definitions.TimedTargets[ids[i]];
                EditorGUILayout.LabelField((i + 1) + ". " + target.Id + "  " + target.Kind + "  " + target.BaseTimeMs + " ms");
            }

            EditorGUI.indentLevel--;
        }

        private static void DrawObjects(SpaceDefinitionsManager definitions)
        {
            EditorGUILayout.LabelField("Objects:");
            EditorGUI.indentLevel++;
            List<string> ids = SortedIds(definitions.Objects);
            if (ids.Count == 0)
                EditorGUILayout.LabelField("—", EditorStyles.miniLabel);

            for (int i = 0; i < ids.Count; i++)
            {
                SpaceObjectDef body = definitions.Objects[ids[i]];
                string visual = string.IsNullOrEmpty(body.VisualResourcesPath) ? "—" : body.VisualResourcesPath;
                EditorGUILayout.LabelField((i + 1) + ". " + body.Id + "  " + body.ObjectType + ", scale " + body.UniformScale + ", radius " + body.Radius + ", visual " + visual);
            }

            EditorGUI.indentLevel--;
        }

        private static void DrawSectors(SpaceDefinitionsManager definitions)
        {
            EditorGUILayout.LabelField("Sectors:");
            EditorGUI.indentLevel++;
            List<string> ids = SortedIds(definitions.Sectors);
            if (ids.Count == 0)
                EditorGUILayout.LabelField("—", EditorStyles.miniLabel);

            for (int i = 0; i < ids.Count; i++)
            {
                SpaceSectorDef sector = definitions.Sectors[ids[i]];
                int placementCount = sector.Placements != null ? sector.Placements.Count : 0;
                int spawnCount = CountSpawns(definitions, sector);
                EditorGUILayout.LabelField((i + 1) + ". " + sector.Id + "  scene " + sector.SceneName + ", placements " + placementCount + ", spawns " + spawnCount);
            }

            EditorGUI.indentLevel--;
        }

        private static void DrawCameraSettings(SpaceDefinitionsManager definitions)
        {
            SpaceCameraSettingsDef settings = definitions.CameraSettings;
            EditorGUILayout.LabelField("Camera:");
            EditorGUI.indentLevel++;
            if (settings == null)
            {
                EditorGUILayout.LabelField("—", EditorStyles.miniLabel);
                EditorGUI.indentLevel--;
                return;
            }

            EditorGUILayout.LabelField("distance " + settings.MinDistance + ".." + settings.MaxDistance);
            EditorGUILayout.LabelField("yaw " + settings.DefaultYaw + "  pitch " + settings.DefaultPitch + "  " + settings.MinPitch + ".." + settings.MaxPitch);
            EditorGUILayout.LabelField("degrees/pixel " + settings.DegreesPerPixel + "  scroll " + settings.ScrollStep + "  drag " + settings.DragThresholdPixels);
            EditorGUI.indentLevel--;
        }

        private static void DrawMarkerIcons(SpaceDefinitionsManager definitions)
        {
            SpaceMarkerIconsDef icons = definitions.MarkerIcons;
            EditorGUILayout.LabelField("Marker icons:");
            EditorGUI.indentLevel++;
            if (icons == null || icons.Icons == null || icons.Icons.Count == 0)
            {
                EditorGUILayout.LabelField("—", EditorStyles.miniLabel);
                EditorGUI.indentLevel--;
                return;
            }

            for (int i = 0; i < icons.Icons.Count; i++)
            {
                SpaceMarkerKindIcon entry = icons.Icons[i];
                if (entry == null)
                    continue;

                string path = string.IsNullOrEmpty(entry.IconResourcesPath) ? "—" : entry.IconResourcesPath;
                EditorGUILayout.LabelField((i + 1) + ". " + entry.ObjectType + "  " + path);
            }

            EditorGUI.indentLevel--;
        }

        private static int CountSpawns(SpaceDefinitionsManager definitions, SpaceSectorDef sector)
        {
            int count = 0;
            if (sector.Placements == null)
                return count;

            for (int i = 0; i < sector.Placements.Count; i++)
            {
                SpaceSectorPlacement placement = sector.Placements[i];
                if (placement == null
                    || string.IsNullOrEmpty(placement.ObjectId)
                    || definitions.Objects == null
                    || !definitions.Objects.TryGetValue(placement.ObjectId, out SpaceObjectDef body))
                    continue;

                if (body.ObjectType == SpaceObjectType.Spawn)
                    count++;
            }

            return count;
        }

        private static List<string> SortedIds<T>(IReadOnlyDictionary<string, T> source)
        {
            List<string> ids = new List<string>();
            if (source == null)
                return ids;

            foreach (KeyValuePair<string, T> pair in source)
                ids.Add(pair.Key);

            ids.Sort(StringComparer.Ordinal);
            return ids;
        }
    }
}
