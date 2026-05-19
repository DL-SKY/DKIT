using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Text;

namespace Modules.Definitions.Scripts.Editor.Match3
{
    public sealed class GameZonesEditorWindow : EditorWindow
    {
        private const string MENU_PATH = "Tools/Definitions/Match3/GameZonesEditor";
        private const string GAME_ZONES_DIRECTORY = "Assets/Modules/Definitions/Resources/Definitions/GameZones";
        private const string CELLS_MAP_PATH = "Assets/Modules/Definitions/Resources/Definitions/CellsMap/CellsMap.json";
        private const string JSON_EXTENSION = ".json";
        private const string DEFAULT_NEW_FILE_NAME = "NewGameZone";
        private const int DEFAULT_NEW_MASK_WIDTH = 6;
        private const int DEFAULT_NEW_MASK_HEIGHT = 6;
        private const string DEFAULT_MASK_JSON =
@"[
  [0, 0, 0],
  [0, 0, 0],
  [0, 0, 0]
]";

        private readonly List<string> _fileNames = new List<string>();

        private Vector2 _filesScroll;
        private Vector2 _jsonScroll;

        private string _selectedFileName;
        private string _jsonContent = string.Empty;
        private string _newFileName = DEFAULT_NEW_FILE_NAME;
        private int _newMaskWidth = DEFAULT_NEW_MASK_WIDTH;
        private int _newMaskHeight = DEFAULT_NEW_MASK_HEIGHT;

        private bool _isDirty;
        private readonly List<CellsMapEntry> _cellsMapEntries = new List<CellsMapEntry>();
        private readonly HashSet<int> _availableTileIds = new HashSet<int> { 0 };

        [MenuItem(MENU_PATH)]
        public static GameZonesEditorWindow Open()
        {
            GameZonesEditorWindow window = GetWindow<GameZonesEditorWindow>();
            window.titleContent = new GUIContent("GameZones Editor");
            window.minSize = new Vector2(860f, 520f);
            window.RefreshFiles();
            return window;
        }

        private void OnEnable()
        {
            EnsureDirectoryExists();
            RefreshCellsMapEntries();
            RefreshFiles();
        }

        private void OnGUI()
        {
            DrawToolbar();
            EditorGUILayout.Space(6f);

            using (new EditorGUILayout.HorizontalScope())
            {
                DrawFilesPanel();
                DrawEditorPanel();
            }
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(70f)))
                {
                    RefreshCellsMapEntries();
                    RefreshFiles();
                }

                GUILayout.Space(6f);
                EditorGUILayout.LabelField("Folder:", GUILayout.Width(44f));
                EditorGUILayout.SelectableLabel(GAME_ZONES_DIRECTORY, EditorStyles.toolbarTextField, GUILayout.Height(18f));

                GUILayout.Space(6f);
                if (GUILayout.Button("Open Folder", EditorStyles.toolbarButton, GUILayout.Width(90f)))
                {
                    EnsureDirectoryExists();
                    EditorUtility.RevealInFinder(GAME_ZONES_DIRECTORY);
                }
            }
        }

        private void DrawFilesPanel()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(300f)))
            {
                EditorGUILayout.LabelField("Game zone files", EditorStyles.boldLabel);
                EditorGUILayout.Space(4f);

                using (new EditorGUILayout.HorizontalScope())
                {
                    _newFileName = EditorGUILayout.TextField(_newFileName);
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("W", GUILayout.Width(16f));
                    _newMaskWidth = Mathf.Max(1, EditorGUILayout.IntField(_newMaskWidth, GUILayout.Width(56f)));
                    GUILayout.Space(8f);
                    EditorGUILayout.LabelField("H", GUILayout.Width(16f));
                    _newMaskHeight = Mathf.Max(1, EditorGUILayout.IntField(_newMaskHeight, GUILayout.Width(56f)));
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Создать", GUILayout.Width(80f)))
                    {
                        CreateFile();
                    }
                }

                EditorGUILayout.Space(6f);

                using (EditorGUILayout.ScrollViewScope scope = new EditorGUILayout.ScrollViewScope(_filesScroll))
                {
                    _filesScroll = scope.scrollPosition;

                    for (int i = 0; i < _fileNames.Count; i++)
                    {
                        string fileName = _fileNames[i];
                        bool isSelected = string.Equals(fileName, _selectedFileName, StringComparison.OrdinalIgnoreCase);
                        GUIStyle style = isSelected ? EditorStyles.helpBox : EditorStyles.miniButton;
                        if (GUILayout.Button(fileName, style, GUILayout.ExpandWidth(true)))
                        {
                            SelectFile(fileName);
                        }
                    }
                }

                GUILayout.FlexibleSpace();
                EditorGUILayout.Space(6f);
                EditorGUILayout.LabelField("CellsMapDef:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("0: void space", EditorStyles.boldLabel);

                if (_cellsMapEntries.Count == 0)
                {
                    EditorGUILayout.HelpBox("(No entries found in CellsMap.json)", MessageType.None);
                }
                else
                {
                    for (int i = 0; i < _cellsMapEntries.Count; i++)
                    {
                        CellsMapEntry entry = _cellsMapEntries[i];
                        bool isSelected = _availableTileIds.Contains(entry.Id);

                        using (new EditorGUILayout.HorizontalScope())
                        {
                            string stateButtonText = isSelected ? "[*]" : "[ ]";
                            if (GUILayout.Button(stateButtonText, GUILayout.Width(32f)))
                            {
                                ToggleAvailableTile(entry.Id);
                            }

                            EditorGUILayout.LabelField(entry.Id + ": \"" + entry.DefId + "\"", EditorStyles.miniLabel);
                        }
                    }
                }

                EditorGUILayout.Space(4f);
                EditorGUILayout.LabelField(
                    "Available tile ids: " + string.Join(", ", GetSortedAvailableTileIds()),
                    EditorStyles.miniLabel);
            }
        }

        private void DrawEditorPanel()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                EditorGUILayout.LabelField("Mask Editor", EditorStyles.boldLabel);
                EditorGUILayout.Space(4f);

                if (string.IsNullOrEmpty(_selectedFileName))
                {
                    EditorGUILayout.HelpBox("Select or create a GameZone JSON file to start editing.", MessageType.Info);
                    return;
                }

                string selectedPath = GetFilePath(_selectedFileName);
                EditorGUILayout.LabelField("Selected file:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField(selectedPath, EditorStyles.miniLabel);
                EditorGUILayout.Space(6f);

                using (new EditorGUILayout.HorizontalScope())
                {
                    GUI.enabled = _isDirty;
                    if (GUILayout.Button("Save", GUILayout.Width(90f)))
                    {
                        SaveSelectedFile();
                    }

                    GUI.enabled = true;
                    if (GUILayout.Button("Revert", GUILayout.Width(90f)))
                    {
                        ReloadSelectedFile();
                    }

                    GUILayout.Space(8f);
                    if (GUILayout.Button("Delete", GUILayout.Width(90f)))
                    {
                        DeleteSelectedFile();
                    }
                }

                EditorGUILayout.Space(6f);
                EditorGUILayout.LabelField(_isDirty ? "Status: modified" : "Status: saved", EditorStyles.miniLabel);
                EditorGUILayout.LabelField("Only 'Mask' is editable. 'Presets' is always saved as an empty array.", EditorStyles.miniLabel);
                EditorGUILayout.Space(4f);

                using (EditorGUILayout.ScrollViewScope scope = new EditorGUILayout.ScrollViewScope(_jsonScroll))
                {
                    _jsonScroll = scope.scrollPosition;

                    EditorGUI.BeginChangeCheck();
                    string updatedJson = EditorGUILayout.TextArea(_jsonContent, GUILayout.ExpandHeight(true));
                    if (EditorGUI.EndChangeCheck())
                    {
                        _jsonContent = updatedJson;
                        _isDirty = true;
                    }
                }
            }
        }

        private void RefreshFiles()
        {
            EnsureDirectoryExists();
            RefreshCellsMapEntries();

            _fileNames.Clear();
            string[] paths = Directory.GetFiles(GAME_ZONES_DIRECTORY, "*" + JSON_EXTENSION);
            Array.Sort(paths, StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < paths.Length; i++)
            {
                _fileNames.Add(Path.GetFileNameWithoutExtension(paths[i]));
            }

            if (string.IsNullOrEmpty(_selectedFileName))
            {
                if (_fileNames.Count > 0)
                {
                    SelectFile(_fileNames[0]);
                }

                return;
            }

            bool stillExists = _fileNames.Contains(_selectedFileName);
            if (stillExists)
            {
                ReloadSelectedFile();
            }
            else
            {
                _selectedFileName = null;
                _jsonContent = string.Empty;
                _isDirty = false;
            }
        }

        private void SelectFile(string fileName)
        {
            if (!CanLoseUnsavedChanges())
            {
                return;
            }

            _selectedFileName = fileName;
            ReloadSelectedFile();
        }

        private void ReloadSelectedFile()
        {
            if (string.IsNullOrEmpty(_selectedFileName))
            {
                return;
            }

            string path = GetFilePath(_selectedFileName);
            if (!File.Exists(path))
            {
                _jsonContent = string.Empty;
                _isDirty = false;
                return;
            }

            string fileJson = File.ReadAllText(path);
            if (!TryExtractMaskJson(fileJson, out string maskJson, out string error))
            {
                EditorUtility.DisplayDialog("Invalid GameZone JSON", error, "OK");
                _jsonContent = DEFAULT_MASK_JSON;
            }
            else
            {
                _jsonContent = maskJson;
            }

            _isDirty = false;
        }

        private void CreateFile()
        {
            if (!CanLoseUnsavedChanges())
            {
                return;
            }

            string normalizedName = NormalizeFileName(_newFileName);
            if (string.IsNullOrWhiteSpace(normalizedName))
            {
                EditorUtility.DisplayDialog("GameZones Editor", "Enter valid file name.", "OK");
                return;
            }

            string path = GetFilePath(normalizedName);
            if (File.Exists(path))
            {
                EditorUtility.DisplayDialog("GameZones Editor", "File already exists: " + normalizedName, "OK");
                return;
            }

            File.WriteAllText(path, BuildDefaultGameZoneJson(_newMaskWidth, _newMaskHeight));
            AssetDatabase.Refresh();
            RefreshFiles();
            SelectFile(normalizedName);
        }

        private void SaveSelectedFile()
        {
            if (string.IsNullOrEmpty(_selectedFileName))
            {
                return;
            }

            if (!TryParseMask(_jsonContent, out int[][] parsedMask, out string error))
            {
                EditorUtility.DisplayDialog("Invalid JSON", error, "OK");
                return;
            }

            string formattedJson = BuildGameZoneJson(parsedMask);
            string path = GetFilePath(_selectedFileName);
            File.WriteAllText(path, formattedJson);
            _jsonContent = JsonConvert.SerializeObject(parsedMask, Formatting.Indented);
            _isDirty = false;
            AssetDatabase.Refresh();
        }

        private void DeleteSelectedFile()
        {
            if (string.IsNullOrEmpty(_selectedFileName))
            {
                return;
            }

            bool confirm = EditorUtility.DisplayDialog(
                "Delete GameZone file",
                "Delete file '" + _selectedFileName + JSON_EXTENSION + "'?",
                "Delete",
                "Cancel");
            if (!confirm)
            {
                return;
            }

            string path = GetFilePath(_selectedFileName);
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            _selectedFileName = null;
            _jsonContent = string.Empty;
            _isDirty = false;
            AssetDatabase.Refresh();
            RefreshFiles();
        }

        private bool CanLoseUnsavedChanges()
        {
            if (!_isDirty)
            {
                return true;
            }

            return EditorUtility.DisplayDialog(
                "Unsaved changes",
                "You have unsaved changes. Continue without saving?",
                "Continue",
                "Cancel");
        }

        private static bool TryParseMask(string rawMaskJson, out int[][] mask, out string error)
        {
            mask = null;
            error = string.Empty;

            try
            {
                mask = JsonConvert.DeserializeObject<int[][]>(rawMaskJson);
                if (mask == null || mask.Length == 0)
                {
                    error = "Mask must be a non-empty 2D int array.";
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                error = e.Message;
                return false;
            }
        }

        private static string NormalizeFileName(string name)
        {
            string result = (name ?? string.Empty).Trim();
            foreach (char invalid in Path.GetInvalidFileNameChars())
            {
                result = result.Replace(invalid, '_');
            }

            return result.Replace(" ", string.Empty);
        }

        private static bool TryExtractMaskJson(string fileJson, out string maskJson, out string error)
        {
            maskJson = string.Empty;
            error = string.Empty;

            try
            {
                GameZoneFileDto model = JsonConvert.DeserializeObject<GameZoneFileDto>(fileJson);
                if (model?.Mask == null || model.Mask.Length == 0)
                {
                    error = "Mask is missing or empty.";
                    return false;
                }

                maskJson = JsonConvert.SerializeObject(model.Mask, Formatting.Indented);
                return true;
            }
            catch (Exception e)
            {
                error = e.Message;
                return false;
            }
        }

        private static string BuildGameZoneJson(int[][] mask)
        {
            StringBuilder sb = new StringBuilder(1024);
            sb.AppendLine("{");
            sb.AppendLine("\t\"Mask\": [");
            sb.Append(BuildMatrixRows(mask));
            sb.AppendLine("\t],");
            sb.AppendLine("\t\"Presets\": []");
            sb.Append('}');
            return sb.ToString();
        }

        private static string BuildDefaultGameZoneJson(int width, int height)
        {
            int safeWidth = Mathf.Max(1, width);
            int safeHeight = Mathf.Max(1, height);
            int[][] defaultMask = new int[safeHeight][];
            for (int row = 0; row < safeHeight; row++)
            {
                defaultMask[row] = new int[safeWidth];
            }

            return BuildGameZoneJson(defaultMask);
        }

        private void RefreshCellsMapEntries()
        {
            _cellsMapEntries.Clear();

            if (!File.Exists(CELLS_MAP_PATH))
            {
                return;
            }

            string content = File.ReadAllText(CELLS_MAP_PATH);
            MatchCollection matches = Regex.Matches(content, @"(?m)^\s*(\d+)\s*:\s*""([^""]+)""");
            if (matches.Count == 0)
            {
                return;
            }

            for (int i = 0; i < matches.Count; i++)
            {
                Match m = matches[i];
                if (!int.TryParse(m.Groups[1].Value, out int id))
                {
                    continue;
                }

                string defId = m.Groups[2].Value;
                _cellsMapEntries.Add(new CellsMapEntry
                {
                    Id = id,
                    DefId = defId,
                });

                if (!_availableTileIds.Contains(id))
                {
                    _availableTileIds.Add(id);
                }
            }
        }

        private void ToggleAvailableTile(int id)
        {
            if (id == 0)
            {
                return;
            }

            if (_availableTileIds.Contains(id))
            {
                _availableTileIds.Remove(id);
            }
            else
            {
                _availableTileIds.Add(id);
            }

            _availableTileIds.Add(0);
        }

        private IEnumerable<int> GetSortedAvailableTileIds()
        {
            List<int> ids = new List<int>(_availableTileIds);
            ids.Sort();
            return ids;
        }

        private static void EnsureDirectoryExists()
        {
            if (!Directory.Exists(GAME_ZONES_DIRECTORY))
            {
                Directory.CreateDirectory(GAME_ZONES_DIRECTORY);
            }
        }

        private static string GetFilePath(string fileNameWithoutExtension)
        {
            return Path.Combine(GAME_ZONES_DIRECTORY, fileNameWithoutExtension + JSON_EXTENSION);
        }

        private static string BuildMatrixRows(int[][] matrix)
        {
            if (matrix == null || matrix.Length == 0)
            {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder(matrix.Length * 32);
            for (int row = 0; row < matrix.Length; row++)
            {
                int[] rowValues = matrix[row] ?? Array.Empty<int>();
                sb.Append("\t\t[ ");

                for (int col = 0; col < rowValues.Length; col++)
                {
                    if (col > 0)
                    {
                        sb.Append(", ");
                    }

                    sb.Append(rowValues[col]);
                }

                sb.Append(" ]");
                if (row < matrix.Length - 1)
                {
                    sb.Append(',');
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }

        [Serializable]
        private sealed class GameZoneFileDto
        {
            public int[][] Mask;
            public int[][] Presets;
        }

        private sealed class CellsMapEntry
        {
            public int Id;
            public string DefId;
        }
    }
}
