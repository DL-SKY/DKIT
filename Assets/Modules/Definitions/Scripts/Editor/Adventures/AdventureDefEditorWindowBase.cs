using Modules.Definitions.Scripts.Defs;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Modules.Definitions.Scripts.Editor.Adventures
{
    public abstract class AdventureDefEditorWindowBase<TDefinition> : EditorWindow
        where TDefinition : AbstractDefinition, new()
    {
        private const float LEFT_PANEL_WIDTH = 290f;

        protected readonly AdventureDefinitionEditorRepository REPOSITORY = new AdventureDefinitionEditorRepository();

        private List<string> _filePaths = new List<string>();
        private TDefinition _currentDefinition;
        private string _selectedFilePath;
        private bool _isDirty;

        private Vector2 _filesScroll;
        private GUIStyle _fileButtonStyle;
        private GUIStyle _fileSelectedButtonStyle;

        protected abstract string WindowTitle { get; }
        protected abstract string DefinitionsDirectory { get; }
        protected abstract string DefaultNewDefinitionId { get; }
        protected virtual Vector2 MinWindowSize => new Vector2(1200f, 700f);
        protected virtual string DeleteDialogName => typeof(TDefinition).Name;
        protected virtual string CreateDialogName => $"Create {typeof(TDefinition).Name}";
        protected virtual string RenameDialogName => $"Rename {typeof(TDefinition).Name}";

        protected TDefinition CurrentDefinition => _currentDefinition;

        protected virtual void OnEnable()
        {
            minSize = MinWindowSize;
            RefreshFiles();
            OnRefreshMetadata();
        }

        protected virtual void OnGUI()
        {
            DrawToolbar();
            EditorGUILayout.Space(6f);

            using (new EditorGUILayout.HorizontalScope())
            {
                DrawFilesPanel();
                DrawDefinitionPanel();
            }
        }

        protected void MarkDirty()
        {
            _isDirty = true;
        }

        protected void DrawField(Action drawAction)
        {
            EditorGUI.BeginChangeCheck();
            drawAction?.Invoke();
            if (EditorGUI.EndChangeCheck())
                MarkDirty();
        }

        protected abstract void DrawDefinitionEditor(TDefinition definition);

        protected virtual void NormalizeDefinition(TDefinition definition)
        {
        }

        protected virtual TDefinition CreateNewDefinition(string definitionId)
        {
            return new TDefinition
            {
                Id = definitionId,
            };
        }

        protected virtual void OnRefreshMetadata()
        {
        }

        protected virtual void DrawToolbarActions()
        {
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(70f)))
                {
                    RefreshFiles();
                    OnRefreshMetadata();
                }

                GUI.enabled = _currentDefinition != null && _isDirty;
                Color previousSaveButtonColor = GUI.contentColor;
                if (GUI.enabled)
                    GUI.contentColor = new Color(1f, 0.86f, 0.2f);

                if (GUILayout.Button("Save", EditorStyles.toolbarButton, GUILayout.Width(70f)))
                    SaveCurrentDefinition();
                GUI.contentColor = previousSaveButtonColor;

                GUI.enabled = _currentDefinition != null;
                if (GUILayout.Button("Revert", EditorStyles.toolbarButton, GUILayout.Width(70f)))
                    ReloadCurrentDefinition();

                GUI.enabled = true;
                DrawToolbarActions();

                GUILayout.Space(8f);
                EditorGUILayout.LabelField("Folder:", GUILayout.Width(44f));
                EditorGUILayout.SelectableLabel(
                    DefinitionsDirectory,
                    EditorStyles.toolbarTextField,
                    GUILayout.MinWidth(160f),
                    GUILayout.ExpandWidth(true),
                    GUILayout.Height(18f));

                GUILayout.Space(8f);
                DrawStatusBadge();
            }
        }

        private void DrawStatusBadge()
        {
            string statusText;
            Color statusColor;
            if (_currentDefinition == null)
            {
                statusText = "No file selected";
                statusColor = Color.white;
            }
            else if (_isDirty)
            {
                statusText = "Modified";
                statusColor = new Color(1f, 0.86f, 0.2f);
            }
            else
            {
                statusText = "Saved";
                statusColor = new Color(0.36f, 0.9f, 0.36f);
            }

            Color previousContentColor = GUI.contentColor;
            GUI.contentColor = statusColor;
            EditorGUILayout.LabelField(statusText, GUILayout.Width(120f));
            GUI.contentColor = previousContentColor;
        }

        private void DrawFilesPanel()
        {
            using (new EditorGUILayout.VerticalScope(
                GUILayout.MinWidth(LEFT_PANEL_WIDTH),
                GUILayout.MaxWidth(LEFT_PANEL_WIDTH),
                GUILayout.ExpandWidth(false)))
            {
                EditorGUILayout.LabelField("Definition Files", EditorStyles.boldLabel);
                EditorGUILayout.Space(4f);

                if (GUILayout.Button($"Create {typeof(TDefinition).Name}", GUILayout.Height(24f)))
                    PromptCreateDefinition();

                EditorGUILayout.Space(8f);
                EditorGUILayout.LabelField("Files", EditorStyles.boldLabel);

                using (EditorGUILayout.ScrollViewScope scope = new EditorGUILayout.ScrollViewScope(_filesScroll))
                {
                    _filesScroll = scope.scrollPosition;

                    for (int i = 0; i < _filePaths.Count; i++)
                    {
                        string path = _filePaths[i];
                        string fileName = AdventureDefinitionEditorRepository.GetFileNameWithoutExtension(path);
                        bool isSelected = string.Equals(path, _selectedFilePath, StringComparison.Ordinal);
                        GUIStyle style = isSelected ? GetFileSelectedButtonStyle() : GetFileButtonStyle();

                        using (new EditorGUILayout.HorizontalScope())
                        {
                            Color previousFileColor = GUI.contentColor;
                            if (isSelected)
                            {
                                GUI.contentColor = _isDirty
                                    ? new Color(1f, 0.86f, 0.2f)
                                    : new Color(0.36f, 0.9f, 0.36f);
                            }

                            if (GUILayout.Button(fileName, style, GUILayout.Width(LEFT_PANEL_WIDTH - 52f)))
                                SelectDefinition(path);

                            GUI.contentColor = previousFileColor;

                            if (GUILayout.Button("R", GUILayout.Width(22f)))
                            {
                                PromptRenameFile(path);
                                break;
                            }

                            if (GUILayout.Button("X", GUILayout.Width(22f)))
                            {
                                DeleteFileByPath(path);
                                break;
                            }
                        }
                    }
                }

                EditorGUILayout.Space(6f);
                GUI.enabled = !string.IsNullOrWhiteSpace(_selectedFilePath);
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Rename File"))
                        RenameCurrentFile();

                    if (GUILayout.Button("Delete File"))
                        DeleteCurrentFile();
                }

                GUI.enabled = true;
            }
        }

        private void DrawDefinitionPanel()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.ExpandHeight(true)))
            {
                if (_currentDefinition == null)
                {
                    EditorGUILayout.HelpBox("Select a definition file or create a new one.", MessageType.Info);
                    return;
                }

                NormalizeDefinition(_currentDefinition);
                DrawDefinitionEditor(_currentDefinition);
            }
        }

        private void PromptCreateDefinition()
        {
            IdentifierPromptWindow.Open(
                CreateDialogName,
                DefaultNewDefinitionId,
                "Create",
                input =>
                {
                    string definitionId = REPOSITORY.NormalizeDefinitionId(input);
                    string filePath = REPOSITORY.BuildProjectRelativePath(DefinitionsDirectory, definitionId);
                    if (REPOSITORY.Exists(filePath))
                    {
                        EditorUtility.DisplayDialog(WindowTitle, $"File already exists: {filePath}", "OK");
                        return;
                    }

                    TDefinition definition = CreateNewDefinition(definitionId);
                    NormalizeDefinition(definition);
                    REPOSITORY.Save(filePath, definition);
                    RefreshFiles();
                    SelectDefinition(filePath);
                });
        }

        private void SaveCurrentDefinition()
        {
            if (_currentDefinition == null || string.IsNullOrWhiteSpace(_selectedFilePath))
                return;

            NormalizeDefinition(_currentDefinition);
            REPOSITORY.Save(_selectedFilePath, _currentDefinition);
            _isDirty = false;
        }

        private void ReloadCurrentDefinition()
        {
            if (string.IsNullOrWhiteSpace(_selectedFilePath))
                return;

            if (REPOSITORY.TryLoad(_selectedFilePath, out TDefinition loadedDefinition, out string error))
            {
                _currentDefinition = loadedDefinition;
                NormalizeDefinition(_currentDefinition);
                _isDirty = false;
                return;
            }

            EditorUtility.DisplayDialog(WindowTitle, $"Failed to reload file.\n{error}", "OK");
        }

        private void RefreshFiles()
        {
            _filePaths = REPOSITORY.GetDefinitionFiles(DefinitionsDirectory);
            if (!string.IsNullOrWhiteSpace(_selectedFilePath) && !_filePaths.Contains(_selectedFilePath))
            {
                _selectedFilePath = null;
                _currentDefinition = null;
                _isDirty = false;
            }
        }

        private void SelectDefinition(string projectRelativePath)
        {
            if (!CanLoseUnsavedChanges())
                return;

            if (!REPOSITORY.TryLoad(projectRelativePath, out TDefinition loadedDefinition, out string error))
            {
                EditorUtility.DisplayDialog(WindowTitle, $"Failed to open definition file.\n{error}", "OK");
                return;
            }

            _selectedFilePath = projectRelativePath;
            _currentDefinition = loadedDefinition;
            NormalizeDefinition(_currentDefinition);
            _isDirty = false;
        }

        private void RenameCurrentFile()
        {
            if (string.IsNullOrWhiteSpace(_selectedFilePath))
                return;

            PromptRenameFile(_selectedFilePath);
        }

        private void DeleteCurrentFile()
        {
            if (string.IsNullOrWhiteSpace(_selectedFilePath))
                return;

            bool confirmed = EditorUtility.DisplayDialog(
                $"Delete {DeleteDialogName}",
                $"Delete file '{_selectedFilePath}'?",
                "Delete",
                "Cancel");
            if (!confirmed)
                return;

            REPOSITORY.Delete(_selectedFilePath);
            _selectedFilePath = null;
            _currentDefinition = null;
            _isDirty = false;
            RefreshFiles();
        }

        private void DeleteFileByPath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return;

            bool confirmed = EditorUtility.DisplayDialog(
                $"Delete {DeleteDialogName}",
                $"Delete file '{filePath}'?",
                "Delete",
                "Cancel");
            if (!confirmed)
                return;

            REPOSITORY.Delete(filePath);

            if (string.Equals(_selectedFilePath, filePath, StringComparison.Ordinal))
            {
                _selectedFilePath = null;
                _currentDefinition = null;
                _isDirty = false;
            }

            RefreshFiles();
        }

        private void PromptRenameFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return;

            string currentId = AdventureDefinitionEditorRepository.GetFileNameWithoutExtension(filePath);
            IdentifierPromptWindow.Open(RenameDialogName, currentId, "Rename", input =>
            {
                string normalizedId = REPOSITORY.NormalizeDefinitionId(input);
                string newPath = BuildRenamedFilePath(filePath, normalizedId);
                if (string.Equals(newPath, filePath, StringComparison.Ordinal))
                    return;

                if (REPOSITORY.Exists(newPath))
                {
                    EditorUtility.DisplayDialog(WindowTitle, $"File already exists: {newPath}", "OK");
                    return;
                }

                try
                {
                    REPOSITORY.Rename(filePath, newPath);
                    if (string.Equals(_selectedFilePath, filePath, StringComparison.Ordinal))
                        _selectedFilePath = newPath;

                    RefreshFiles();
                    Repaint();
                }
                catch (Exception exception)
                {
                    EditorUtility.DisplayDialog(WindowTitle, $"Rename failed:\n{exception.Message}", "OK");
                }
            });
        }

        private string BuildRenamedFilePath(string oldPath, string newId)
        {
            string normalized = REPOSITORY.NormalizeDefinitionId(newId);
            string directory = Path.GetDirectoryName(oldPath) ?? DefinitionsDirectory;
            directory = directory.Replace('\\', '/');
            return $"{directory}/{normalized}.json";
        }

        private bool CanLoseUnsavedChanges()
        {
            if (!_isDirty)
                return true;

            return EditorUtility.DisplayDialog(
                "Unsaved changes",
                $"Current {typeof(TDefinition).Name} has unsaved changes. Continue without saving?",
                "Continue",
                "Cancel");
        }

        private GUIStyle GetFileButtonStyle()
        {
            if (_fileButtonStyle == null)
            {
                _fileButtonStyle = new GUIStyle(EditorStyles.miniButton)
                {
                    alignment = TextAnchor.MiddleLeft,
                    clipping = TextClipping.Clip,
                };
            }

            return _fileButtonStyle;
        }

        private GUIStyle GetFileSelectedButtonStyle()
        {
            if (_fileSelectedButtonStyle == null)
            {
                _fileSelectedButtonStyle = new GUIStyle(EditorStyles.helpBox)
                {
                    alignment = TextAnchor.MiddleLeft,
                    clipping = TextClipping.Clip,
                };
            }

            return _fileSelectedButtonStyle;
        }
    }
}
