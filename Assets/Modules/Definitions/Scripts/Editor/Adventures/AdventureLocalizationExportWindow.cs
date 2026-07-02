using Modules.RPG.Scripts.Adventure.Data;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Modules.Definitions.Scripts.Editor.Adventures
{
    public sealed class AdventureLocalizationExportWindow : EditorWindow
    {
        private readonly AdventureLocalizationGenerationService _generationService = new AdventureLocalizationGenerationService();

        private AdventureData _adventureData;
        private string _selectedAdventurePath;
        private Action _onAdventureChanged;
        private string _exportDirectory;
        private string _lastExportFilePath;
        private string _statusMessage;
        private MessageType _statusType = MessageType.Info;

        public static void Open(AdventureData adventureData, string selectedAdventurePath, Action onAdventureChanged)
        {
            AdventureLocalizationExportWindow window = CreateInstance<AdventureLocalizationExportWindow>();
            window.titleContent = new GUIContent("TEA Localization");
            window.minSize = new Vector2(640f, 230f);
            window.maxSize = new Vector2(980f, 380f);
            window._adventureData = adventureData;
            window._selectedAdventurePath = selectedAdventurePath;
            window._onAdventureChanged = onAdventureChanged;
            window._statusMessage = "Choose an export folder and generate keys.";
            window._statusType = MessageType.Info;
            window.ShowUtility();
            window.Focus();
        }

        private void OnEnable()
        {
            if (string.IsNullOrWhiteSpace(_exportDirectory))
                _exportDirectory = GetDefaultExportDirectory();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Localization Export", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "The window generates localization keys in selected adventure fields and exports a tab-separated .txt file for quick paste into Google Sheets.",
                MessageType.None);
            EditorGUILayout.Space(6f);

            EditorGUILayout.LabelField("Export Folder", EditorStyles.miniBoldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.SelectableLabel(
                    string.IsNullOrWhiteSpace(_exportDirectory) ? "<not selected>" : _exportDirectory,
                    EditorStyles.textField,
                    GUILayout.Height(18f));

                if (GUILayout.Button("Choose...", GUILayout.Width(90f)))
                {
                    string selected = EditorUtility.OpenFolderPanel(
                        "Select export folder",
                        string.IsNullOrWhiteSpace(_exportDirectory) ? GetDefaultExportDirectory() : _exportDirectory,
                        string.Empty);

                    if (!string.IsNullOrWhiteSpace(selected))
                        _exportDirectory = selected;
                }
            }

            EditorGUILayout.Space(8f);
            GUI.enabled = _adventureData != null && !string.IsNullOrWhiteSpace(_exportDirectory);
            if (GUILayout.Button("Generate Localization Keys", GUILayout.Height(28f)))
                GenerateLocalizationKeys();
            GUI.enabled = !string.IsNullOrWhiteSpace(_lastExportFilePath) && File.Exists(_lastExportFilePath);
            if (GUILayout.Button("Reveal Export File", GUILayout.Height(22f)))
                EditorUtility.RevealInFinder(_lastExportFilePath);
            GUI.enabled = true;

            EditorGUILayout.Space(8f);
            if (!string.IsNullOrWhiteSpace(_statusMessage))
                EditorGUILayout.HelpBox(_statusMessage, _statusType);

            if (!string.IsNullOrWhiteSpace(_lastExportFilePath))
            {
                EditorGUILayout.LabelField("Last Export File", EditorStyles.miniBoldLabel);
                EditorGUILayout.SelectableLabel(_lastExportFilePath, EditorStyles.textField, GUILayout.Height(18f));
            }
        }

        private void GenerateLocalizationKeys()
        {
            if (_adventureData == null)
            {
                _statusMessage = "Adventure data is not loaded.";
                _statusType = MessageType.Warning;
                return;
            }

            if (string.IsNullOrWhiteSpace(_exportDirectory) || !Directory.Exists(_exportDirectory))
            {
                _statusMessage = "Select existing folder before generation.";
                _statusType = MessageType.Warning;
                return;
            }

            try
            {
                AdventureLocalizationGenerationResult result = _generationService.GenerateAndExport(
                    _adventureData,
                    _selectedAdventurePath,
                    _exportDirectory);

                _lastExportFilePath = result.ExportFilePath;
                if (result.UpdatedFieldsCount > 0)
                    _onAdventureChanged?.Invoke();

                _statusMessage =
                    $"Done. Updated fields: {result.UpdatedFieldsCount}. Generated keys: {result.GeneratedKeysCount}. Reused keys: {result.ReusedKeysCount}. Export lines: {result.ExportEntries.Count}.";
                _statusType = MessageType.Info;
            }
            catch (Exception exception)
            {
                _statusMessage = $"Generation failed: {exception.Message}";
                _statusType = MessageType.Error;
            }
        }

        private static string GetDefaultExportDirectory()
        {
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            if (!string.IsNullOrWhiteSpace(desktop) && Directory.Exists(desktop))
                return desktop;

            return Environment.CurrentDirectory;
        }
    }
}
