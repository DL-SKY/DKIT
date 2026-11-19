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
                int slotCount = hull.ModuleSlotIds != null ? hull.ModuleSlotIds.Count : 0;
                int commandCount = hull.CommandIds != null ? hull.CommandIds.Count : 0;
                EditorGUILayout.LabelField((i + 1) + ". " + hull.Id + "  slots " + slotCount + ", commands " + commandCount);
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
                EditorGUILayout.LabelField((i + 1) + ". " + module.Id + "  slot " + module.SlotId + ", group " + weaponGroup);
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
                EditorGUILayout.LabelField((i + 1) + ". " + ids[i]);

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
