using Modules.Definitions.Scripts.Implementation.Adventures.Defs.Encounters;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Modules.Definitions.Scripts.Editor.Adventures
{
    public sealed class EncounterDefEditorWindow : AdventureDefEditorWindowBase<EncounterDef>
    {
        private const string MENU_PATH = "Tools/Definitions/Adventures/Encounter Editor";
        private const string DEFINITIONS_DIRECTORY = "Assets/Modules/Definitions/Resources/Definitions/_ADVENTURES_/Encounters";
        private const string CREATURES_DIRECTORY = "Assets/Modules/Definitions/Resources/Definitions/_ADVENTURES_/Creatures";
        private const string DEFAULT_NEW_ID = "_NewEncounter";

        private Vector2 _mainScroll;
        private List<string> _availableCreatures = new List<string>();

        protected override string WindowTitle => "Encounter Editor";
        protected override string DefinitionsDirectory => DEFINITIONS_DIRECTORY;
        protected override string DefaultNewDefinitionId => DEFAULT_NEW_ID;
        protected override string DeleteDialogName => "EncounterDef";

        [MenuItem(MENU_PATH)]
        public static EncounterDefEditorWindow Open()
        {
            EncounterDefEditorWindow window = GetWindow<EncounterDefEditorWindow>();
            window.titleContent = new GUIContent("Encounter Editor");
            return window;
        }

        protected override void OnRefreshMetadata()
        {
            _availableCreatures = BuildDefinitionIdList(CREATURES_DIRECTORY);
        }

        protected override EncounterDef CreateNewDefinition(string definitionId)
        {
            return new EncounterDef
            {
                Id = definitionId,
                Tags = new List<string>(),
                Title = string.Empty,
                Description = string.Empty,
                Creatures = new List<string>(),
            };
        }

        protected override void NormalizeDefinition(EncounterDef definition)
        {
            definition.Tags ??= new List<string>();
            definition.Title ??= string.Empty;
            definition.Description ??= string.Empty;
            definition.Creatures ??= new List<string>();
        }

        protected override void DrawDefinitionEditor(EncounterDef definition)
        {
            using (EditorGUILayout.ScrollViewScope scope = new EditorGUILayout.ScrollViewScope(_mainScroll))
            {
                _mainScroll = scope.scrollPosition;

                EditorGUILayout.LabelField($"Id: {definition.Id}", EditorStyles.boldLabel);
                EditorGUILayout.Space(6f);

                DrawField(() => definition.Disabled = EditorGUILayout.Toggle("Disabled", definition.Disabled));
                DrawField(() => definition.Title = EditorGUILayout.TextField("Title", definition.Title));
                DrawField(() => definition.Description = EditorGUILayout.TextField("Description", definition.Description));
                DrawField(() => definition.Tags = AdventureDefEditorGui.ParseCsv(
                    EditorGUILayout.TextField("Tags (csv)", AdventureDefEditorGui.JoinCsv(definition.Tags))));

                EditorGUILayout.Space(8f);
                AdventureDefEditorGui.DrawStringListEditor(
                    definition.Creatures,
                    "Creatures",
                    MarkDirty,
                    "Add Creature");

                if (_availableCreatures.Count > 0)
                {
                    if (GUILayout.Button("Append Creature From List", GUILayout.Width(200f)))
                        OpenCreaturePicker(definition.Creatures);
                }
            }
        }

        private void OpenCreaturePicker(List<string> creatures)
        {
            List<DefinitionSearchPickerWindow.Entry> entries = new List<DefinitionSearchPickerWindow.Entry>(_availableCreatures.Count);
            for (int i = 0; i < _availableCreatures.Count; i++)
            {
                string creatureId = _availableCreatures[i];
                entries.Add(DefinitionSearchPickerWindow.BuildEntry(creatureId, creatureId, "Creature"));
            }

            DefinitionSearchPickerWindow.Open("Select Creature", entries, selectedId =>
            {
                if (string.IsNullOrWhiteSpace(selectedId))
                    return;

                creatures ??= new List<string>();
                creatures.Add(selectedId);
                MarkDirty();
                Repaint();
            });
        }

        private List<string> BuildDefinitionIdList(string directoryPath)
        {
            List<string> files = REPOSITORY.GetDefinitionFiles(directoryPath);
            List<string> result = new List<string>(files.Count);
            for (int i = 0; i < files.Count; i++)
                result.Add(AdventureDefinitionEditorRepository.GetFileNameWithoutExtension(files[i]));

            result.Sort(System.StringComparer.Ordinal);
            return result;
        }
    }
}
