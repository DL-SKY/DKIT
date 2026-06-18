using System;
using System.Collections.Generic;
using Modules.Cheats.Scripts.Editor.Core;
using UnityEditor;
using UnityEngine;
using Zenject;
using AdventuresDefinitionsManager = Modules.Definitions.Scripts.Implementation.Adventures.DefinitionsManager;

namespace Modules.Cheats.Scripts.Editor.Implementation.Definitions
{
    public sealed class DefinitionsCheatSection : CheatSectionBase
    {
        public override string Id => "Definitions";

        public override int Order => -998;

        public override void DrawContent()
        {
            AdventuresDefinitionsManager definitionsManager = TryGetAdventuresDefinitionsManager();
            if (definitionsManager == null)
            {
                EditorGUILayout.HelpBox("Нет DefinitionsManager.", MessageType.Info);
                return;
            }

            DrawAdventuresDefinitionsManager(definitionsManager);
        }

        private static AdventuresDefinitionsManager TryGetAdventuresDefinitionsManager()
        {
            if (!Application.isPlaying)
            {
                return null;
            }

            if (!ProjectContext.HasInstance)
            {
                return null;
            }

            return ProjectContext.Instance.Container.TryResolve<AdventuresDefinitionsManager>();
        }

        private void DrawAdventuresDefinitionsManager(AdventuresDefinitionsManager definitionsManager)
        {
            DrawSectionHeader("Adventures.DefinitionsManager:");
            EditorGUILayout.Space(2f);

            string globalSettingsId = definitionsManager.GlobalSettings != null
                ? definitionsManager.GlobalSettings.Id
                : "—";
            EditorGUILayout.LabelField("GlobalSettings: " + globalSettingsId);

            EditorGUILayout.LabelField("Adventures:");
            EditorGUI.indentLevel++;
            DrawDefinitionIds(definitionsManager.Adventures);
            EditorGUI.indentLevel--;

            EditorGUILayout.LabelField("Classes:");
            EditorGUI.indentLevel++;
            DrawDefinitionIds(definitionsManager.Classes);
            EditorGUI.indentLevel--;

            EditorGUILayout.LabelField("Ancestries:");
            EditorGUI.indentLevel++;
            DrawDefinitionIds(definitionsManager.Ancestries);
            EditorGUI.indentLevel--;

            EditorGUILayout.LabelField("Feats:");
            EditorGUI.indentLevel++;
            DrawDefinitionIds(definitionsManager.Feats);
            EditorGUI.indentLevel--;

            EditorGUILayout.LabelField("Items:");
            EditorGUI.indentLevel++;
            DrawDefinitionIds(definitionsManager.Items);
            EditorGUI.indentLevel--;

            EditorGUILayout.LabelField("Spells:");
            EditorGUI.indentLevel++;
            DrawDefinitionIds(definitionsManager.Spells);
            EditorGUI.indentLevel--;
        }

        private static void DrawDefinitionIds<T>(Dictionary<string, T> definitions)
        {
            if (definitions == null || definitions.Count == 0)
            {
                EditorGUILayout.LabelField("—", EditorStyles.miniLabel);
                return;
            }

            List<string> ids = new List<string>(definitions.Keys);
            ids.Sort(StringComparer.Ordinal);

            for (int i = 0; i < ids.Count; i++)
            {
                EditorGUILayout.LabelField((i + 1) + ". " + ids[i]);
            }
        }
    }
}
