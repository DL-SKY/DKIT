using System;
using System.IO;
using Modules.Cheats.Scripts.Editor.Core;
using Modules.Save.Scripts.Core;
using UnityEditor;
using UnityEngine;

namespace Modules.Cheats.Scripts.Editor.Implementation.Save
{
    public sealed class SaveCheatSection : CheatSectionBase
    {
        private const string GLOBAL_SETTINGS_RELATIVE_PATH = "Modules/Definitions/Resources/Definitions/GlobalSettings/GlobalSettings.json";
        private const string DEFAULT_SAVE_NAME = "NA";
        private const string DEFAULT_SAVE_FOLDER = SaveManager.DEFAULT_FILE_FOLDER;
        private const string DEFAULT_FILE_EXTENSION = SaveManager.DEFAULT_FILE_EXTENSION;
        private GUIStyle _compactPathStyle;

        public override string Id => "Save";

        public override int Order => -999;

        public override void DrawContent()
        {
            string globalSettingsPath = Path.Combine(Application.dataPath, GLOBAL_SETTINGS_RELATIVE_PATH);
            if (!File.Exists(globalSettingsPath))
            {
                EditorGUILayout.HelpBox("Global settings file was not found: " + globalSettingsPath, MessageType.Warning);
                return;
            }

            SaveSettingsDto settings = ReadSettings(globalSettingsPath);
            string saveName = GetValueOrDefault(settings?.SaveName, DEFAULT_SAVE_NAME);
            string saveFolder = GetValueOrDefault(settings?.SaveFolder, DEFAULT_SAVE_FOLDER);
            string fileExtension = NormalizeExtension(GetValueOrDefault(settings?.FileExtension, DEFAULT_FILE_EXTENSION));

            string savesDirectory = Path.Combine(Application.persistentDataPath, saveFolder);
            string saveFileName = saveName + fileExtension;
            string fullSavePath = Path.Combine(savesDirectory, saveFileName);
            GUIStyle compactPathStyle = GetCompactPathStyle();

            EditorGUILayout.LabelField("Global settings file:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(globalSettingsPath, compactPathStyle);
            EditorGUILayout.Space(4f);

            EditorGUILayout.LabelField("Saves directory:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(savesDirectory, compactPathStyle);
            EditorGUILayout.Space(4f);

            EditorGUILayout.LabelField("Expected save file name:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(saveFileName, EditorStyles.label);
            EditorGUILayout.Space(4f);

            EditorGUILayout.LabelField("Expected save file path:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(fullSavePath, compactPathStyle);
            EditorGUILayout.Space(8f);

            if (GUILayout.Button("Open saves folder", GUILayout.Height(24f)))
            {
                Directory.CreateDirectory(savesDirectory);
                EditorUtility.RevealInFinder(savesDirectory);
            }
        }

        private static SaveSettingsDto ReadSettings(string globalSettingsPath)
        {
            try
            {
                string json = File.ReadAllText(globalSettingsPath);
                return JsonUtility.FromJson<SaveSettingsDto>(json);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("[SaveCheatSection] Failed to read global settings. Error: " + e);
                return new SaveSettingsDto();
            }
        }

        private static string GetValueOrDefault(string value, string defaultValue)
        {
            return string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim();
        }

        private static string NormalizeExtension(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
            {
                return DEFAULT_FILE_EXTENSION;
            }

            string trimmed = extension.Trim();
            return trimmed.StartsWith(".") ? trimmed : "." + trimmed;
        }

        private GUIStyle GetCompactPathStyle()
        {
            if (_compactPathStyle != null)
            {
                return _compactPathStyle;
            }

            _compactPathStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                fontSize = 9,
                wordWrap = true
            };

            return _compactPathStyle;
        }

        [Serializable]
        private sealed class SaveSettingsDto
        {
            public string SaveName;
            public string SaveFolder;
            public string FileExtension;
        }
    }
}
