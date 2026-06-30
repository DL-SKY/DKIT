using Modules.Definitions.Scripts.Editor.Adventures.CreateOptions;
using Modules.RPG.Scripts.Adventure.Choice;
using Modules.RPG.Scripts.Adventure.Choice.Actions;
using Modules.RPG.Scripts.Adventure.Data;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Modules.Definitions.Scripts.Editor.Adventures
{
    public sealed class AdventureEditorWindow : EditorWindow
    {
        private const string MENU_PATH = "Tools/Definitions/Adventures/Adventure Editor";
        private const float LEFT_PANEL_WIDTH = 240f;
        private const string DEFAULT_NEW_ADVENTURE_FILE_NAME = "NewAdventure";

        private readonly AdventureEditorFileRepository _repository = new AdventureEditorFileRepository();
        private readonly AdventureGraphBuilder _graphBuilder = new AdventureGraphBuilder();
        private readonly AdventureValidationService _validationService = new AdventureValidationService();
        private readonly IAdventureLocalizationKeyCollector _localizationCollector = new DefaultAdventureLocalizationKeyCollector();
        private readonly AdventureCreateOptionsRegistry _adventureCreateOptionsRegistry = new AdventureCreateOptionsRegistry();
        private readonly SceneCreateOptionsRegistry _sceneCreateOptionsRegistry = new SceneCreateOptionsRegistry();
        private readonly SceneContentCreateOptionsRegistry _sceneContentCreateOptionsRegistry = new SceneContentCreateOptionsRegistry();
        private readonly ChoiceCreateOptionsRegistry _choiceCreateOptionsRegistry = new ChoiceCreateOptionsRegistry();
        private readonly ChoiceActionCreateOptionsRegistry _choiceActionCreateOptionsRegistry = new ChoiceActionCreateOptionsRegistry();

        private List<string> _filePaths = new List<string>();
        private AdventureData _adventureData;
        private string _selectedFilePath;
        private bool _isDirty;

        private string _newAdventureFileName = DEFAULT_NEW_ADVENTURE_FILE_NAME;

        private string _selectedSceneId;
        private int _selectedContentIndex = -1;
        private int _selectedChoiceIndex = -1;
        private int _selectedActionIndex = -1;

        private Vector2 _filesScroll;
        private Vector2 _scenesScroll;
        private Vector2 _contentScroll;
        private Vector2 _choicesScroll;
        private Vector2 _rightScroll;
        private GUIStyle _fileButtonStyle;
        private GUIStyle _fileSelectedButtonStyle;

        [MenuItem(MENU_PATH)]
        public static AdventureEditorWindow Open()
        {
            AdventureEditorWindow window = GetWindow<AdventureEditorWindow>();
            window.titleContent = new GUIContent("Adventure Editor");
            window.minSize = new Vector2(1320f, 680f);
            window.RefreshFiles();
            return window;
        }

        private void OnEnable()
        {
            RefreshFiles();
        }

        private void OnGUI()
        {
            DrawToolbar();
            EditorGUILayout.Space(6f);

            using (new EditorGUILayout.HorizontalScope())
            {
                DrawFilesPanel();
                DrawAdventurePanel();
                DrawRightPanel();
            }
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(70f)))
                {
                    RefreshFiles();
                }

                GUI.enabled = _adventureData != null && _isDirty;
                Color previousSaveButtonColor = GUI.contentColor;
                if (GUI.enabled)
                    GUI.contentColor = new Color(1f, 0.86f, 0.2f);

                if (GUILayout.Button("Save", EditorStyles.toolbarButton, GUILayout.Width(70f)))
                {
                    SaveCurrentAdventure();
                }
                GUI.contentColor = previousSaveButtonColor;

                GUI.enabled = _adventureData != null;
                if (GUILayout.Button("Revert", EditorStyles.toolbarButton, GUILayout.Width(70f)))
                {
                    ReloadCurrentAdventure();
                }

                GUI.enabled = true;
                GUILayout.Space(8f);
                EditorGUILayout.LabelField("Folder:", GUILayout.Width(44f));
                EditorGUILayout.SelectableLabel(
                    AdventureEditorFileRepository.ADVENTURES_DIRECTORY,
                    EditorStyles.toolbarTextField,
                    GUILayout.Width(780f),
                    GUILayout.Height(18f));

                GUILayout.FlexibleSpace();
                string statusText;
                Color statusColor;
                if (_adventureData == null)
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
        }

        private void DrawFilesPanel()
        {
            using (new EditorGUILayout.VerticalScope(
                GUILayout.MinWidth(LEFT_PANEL_WIDTH),
                GUILayout.MaxWidth(LEFT_PANEL_WIDTH),
                GUILayout.ExpandWidth(false)))
            {
                EditorGUILayout.LabelField("Adventure Files", EditorStyles.boldLabel);
                EditorGUILayout.Space(4f);

                DrawCreateOptionButtons(
                    _adventureCreateOptionsRegistry.GetOptions(),
                    option => CreateAdventureFile(option),
                    "Create Adventure",
                    LEFT_PANEL_WIDTH);

                EditorGUILayout.Space(8f);
                EditorGUILayout.LabelField("Files", EditorStyles.boldLabel);

                using (EditorGUILayout.ScrollViewScope scope = new EditorGUILayout.ScrollViewScope(_filesScroll))
                {
                    _filesScroll = scope.scrollPosition;

                    for (int i = 0; i < _filePaths.Count; i++)
                    {
                        string path = _filePaths[i];
                        string fileName = AdventureEditorFileRepository.GetFileNameWithoutExtension(path);
                        bool isSelected = string.Equals(path, _selectedFilePath, StringComparison.Ordinal);
                        GUIStyle style = isSelected ? GetFileSelectedButtonStyle() : GetFileButtonStyle();
                        GUIContent content = new GUIContent(fileName, path);

                        using (new EditorGUILayout.HorizontalScope())
                        {
                            Color previousFileColor = GUI.contentColor;
                            if (isSelected)
                            {
                                GUI.contentColor = _isDirty
                                    ? new Color(1f, 0.86f, 0.2f)
                                    : new Color(0.36f, 0.9f, 0.36f);
                            }

                            if (GUILayout.Button(content, style, GUILayout.Width((LEFT_PANEL_WIDTH - 52f))))
                                SelectAdventure(path);

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

        private void DrawAdventurePanel()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                if (_adventureData == null)
                {
                    EditorGUILayout.HelpBox("Select an adventure file or create a new one.", MessageType.Info);
                    return;
                }

                EnsureAdventureDataInitialized();

                DrawAdventureMeta();
                EditorGUILayout.Space(8f);
                using (new EditorGUILayout.HorizontalScope())
                {
                    DrawScenesPanel();
                    EditorGUILayout.Space(8f);
                    DrawSceneDetailsPanel();
                }
            }
        }

        private void DrawAdventureMeta()
        {
            EditorGUILayout.LabelField("Adventure", EditorStyles.boldLabel);

            DrawField(() => _adventureData.Disabled = EditorGUILayout.Toggle("Disabled", _adventureData.Disabled));
            DrawField(() => _adventureData.IsRepeatable = EditorGUILayout.Toggle("Is Repeatable", _adventureData.IsRepeatable));
            DrawField(() => _adventureData.Type = (AdventureType)EditorGUILayout.EnumPopup("Type", _adventureData.Type));
            DrawField(() => _adventureData.Title = EditorGUILayout.TextField("Title", _adventureData.Title ?? string.Empty));
            DrawField(() => _adventureData.Description = EditorGUILayout.TextField("Description", _adventureData.Description ?? string.Empty));
            DrawField(() => _adventureData.Tags = ParseCsv(EditorGUILayout.TextField("Tags (csv)", JoinCsv(_adventureData.Tags))));
            DrawField(() => _adventureData.IgnoredTags = ParseCsv(EditorGUILayout.TextField("Ignored Tags (csv)", JoinCsv(_adventureData.IgnoredTags))));
            DrawField(() => _adventureData.AdventureLinks = ParseCsv(EditorGUILayout.TextField("Adventure Links (csv)", JoinCsv(_adventureData.AdventureLinks))));

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Start Scenes", EditorStyles.boldLabel);

            List<string> sceneIds = GetSortedSceneIds();
            for (int i = 0; i < _adventureData.StartScenes.Count; i++)
            {
                string value = _adventureData.StartScenes[i];
                using (new EditorGUILayout.HorizontalScope())
                {
                    int selectedIndex = Mathf.Max(0, sceneIds.IndexOf(value));
                    if (sceneIds.Count > 0)
                    {
                        int newIndex = EditorGUILayout.Popup(selectedIndex, sceneIds.ToArray());
                        if (newIndex >= 0 && newIndex < sceneIds.Count && !string.Equals(value, sceneIds[newIndex], StringComparison.Ordinal))
                        {
                            _adventureData.StartScenes[i] = sceneIds[newIndex];
                            MarkDirty();
                        }
                    }
                    else
                    {
                        DrawField(() => _adventureData.StartScenes[i] = EditorGUILayout.TextField(value ?? string.Empty));
                    }

                    if (GUILayout.Button("X", GUILayout.Width(26f)))
                    {
                        _adventureData.StartScenes.RemoveAt(i);
                        MarkDirty();
                        break;
                    }
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Add Selected Scene To Start", GUILayout.Width(220f)))
                {
                    if (!string.IsNullOrWhiteSpace(_selectedSceneId))
                    {
                        _adventureData.StartScenes.Add(_selectedSceneId);
                        MarkDirty();
                    }
                }

                if (GUILayout.Button("Add Empty", GUILayout.Width(90f)))
                {
                    _adventureData.StartScenes.Add(string.Empty);
                    MarkDirty();
                }
            }
        }

        private void DrawScenesPanel()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(280f)))
            {
                EditorGUILayout.LabelField("Scenes", EditorStyles.boldLabel);
                DrawCreateOptionButtons(
                    _sceneCreateOptionsRegistry.GetOptions(),
                    AddSceneFromOption,
                    "Add Scene");
                EditorGUILayout.Space(5f);
                EditorGUILayout.LabelField(string.Empty, GUI.skin.horizontalSlider);
                EditorGUILayout.Space(5f);

                using (EditorGUILayout.ScrollViewScope scope = new EditorGUILayout.ScrollViewScope(_scenesScroll, GUILayout.Height(340f)))
                {
                    _scenesScroll = scope.scrollPosition;
                    List<string> sceneIds = GetSortedSceneIds();
                    for (int i = 0; i < sceneIds.Count; i++)
                    {
                        string sceneId = sceneIds[i];
                        bool isSelected = string.Equals(sceneId, _selectedSceneId, StringComparison.Ordinal);
                        GUIStyle style = isSelected ? EditorStyles.helpBox : EditorStyles.miniButton;
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            if (GUILayout.Button(sceneId, style))
                                SelectScene(sceneId);

                            if (GUILayout.Button("R", GUILayout.Width(22f)))
                            {
                                PromptRenameScene(sceneId);
                                break;
                            }

                            if (GUILayout.Button("X", GUILayout.Width(22f)))
                            {
                                _selectedSceneId = sceneId;
                                DeleteSelectedScene();
                                break;
                            }
                        }
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    GUI.enabled = !string.IsNullOrWhiteSpace(_selectedSceneId);
                    if (GUILayout.Button("Delete Scene"))
                        DeleteSelectedScene();

                    if (GUILayout.Button("Duplicate Scene"))
                        DuplicateSelectedScene();

                    GUI.enabled = true;
                }
            }
        }

        private void DrawSceneDetailsPanel()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                if (string.IsNullOrWhiteSpace(_selectedSceneId) || !_adventureData.Scenes.TryGetValue(_selectedSceneId, out SceneData sceneData))
                {
                    EditorGUILayout.HelpBox("Select a scene to edit details.", MessageType.Info);
                    return;
                }

                EditorGUILayout.LabelField($"Scene: {_selectedSceneId}", EditorStyles.boldLabel);

                string nextSceneId = EditorGUILayout.TextField("Scene Id", sceneData.Id ?? string.Empty);
                if (!string.Equals(nextSceneId, sceneData.Id, StringComparison.Ordinal))
                {
                    if (GUILayout.Button("Apply Scene Id Rename", GUILayout.Width(170f)))
                        RenameScene(_selectedSceneId, nextSceneId);
                }

                DrawField(() => sceneData.NotClearScene = EditorGUILayout.Toggle("Not Clear Scene", sceneData.NotClearScene));
                DrawField(() => sceneData.Tags = ParseCsv(EditorGUILayout.TextField("Tags (csv)", JoinCsv(sceneData.Tags))));

                EditorGUILayout.Space(4f);
                using (new EditorGUILayout.HorizontalScope())
                {
                    using (new EditorGUILayout.VerticalScope(GUILayout.ExpandWidth(true)))
                    {
                        DrawContentSection(sceneData);
                    }

                    GUILayout.Space(8f);

                    using (new EditorGUILayout.VerticalScope(GUILayout.ExpandWidth(true)))
                    {
                        DrawChoicesSection(sceneData);
                    }
                }
            }
        }

        private void DrawContentSection(SceneData sceneData)
        {
            EditorGUILayout.LabelField("Content", EditorStyles.boldLabel);
            DrawCreateOptionButtons(
                _sceneContentCreateOptionsRegistry.GetOptions(),
                option =>
                {
                    sceneData.Content.Add(option.Create());
                    _selectedContentIndex = sceneData.Content.Count - 1;
                    MarkDirty();
                },
                "Add Content");
            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField(string.Empty, GUI.skin.horizontalSlider);
            EditorGUILayout.Space(4f);

            using (EditorGUILayout.ScrollViewScope scope = new EditorGUILayout.ScrollViewScope(_contentScroll, GUILayout.Height(140f)))
            {
                _contentScroll = scope.scrollPosition;
                for (int i = 0; i < sceneData.Content.Count; i++)
                {
                    var contentData = sceneData.Content[i];
                    string title = $"{i}: {contentData?.Type}";
                    bool selected = i == _selectedContentIndex;
                    GUIStyle style = selected ? EditorStyles.helpBox : EditorStyles.miniButton;
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button(title, style))
                            _selectedContentIndex = i;

                        if (GUILayout.Button("X", GUILayout.Width(22f)))
                        {
                            sceneData.Content.RemoveAt(i);
                            if (_selectedContentIndex == i)
                                _selectedContentIndex = -1;
                            else if (_selectedContentIndex > i)
                                _selectedContentIndex--;

                            MarkDirty();
                            break;
                        }
                    }
                }
            }

            DrawSelectedContentEditor(sceneData);
        }

        private void DrawChoicesSection(SceneData sceneData)
        {
            EditorGUILayout.LabelField("Choices", EditorStyles.boldLabel);
            DrawCreateOptionButtons(
                _choiceCreateOptionsRegistry.GetOptions(),
                option =>
                {
                    sceneData.Choices.Add(option.Create());
                    _selectedChoiceIndex = sceneData.Choices.Count - 1;
                    _selectedActionIndex = -1;
                    MarkDirty();
                },
                "Add Choice");
            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField(string.Empty, GUI.skin.horizontalSlider);
            EditorGUILayout.Space(4f);

            using (EditorGUILayout.ScrollViewScope scope = new EditorGUILayout.ScrollViewScope(_choicesScroll, GUILayout.Height(140f)))
            {
                _choicesScroll = scope.scrollPosition;
                for (int i = 0; i < sceneData.Choices.Count; i++)
                {
                    ChoiceData choiceData = sceneData.Choices[i];
                    string choiceName = string.IsNullOrWhiteSpace(choiceData?.Id) ? $"choice_{i}" : choiceData.Id;
                    bool selected = i == _selectedChoiceIndex;
                    GUIStyle style = selected ? EditorStyles.helpBox : EditorStyles.miniButton;
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button($"{i}: {choiceName}", style))
                        {
                            _selectedChoiceIndex = i;
                            _selectedActionIndex = -1;
                        }

                        if (GUILayout.Button("X", GUILayout.Width(22f)))
                        {
                            sceneData.Choices.RemoveAt(i);

                            if (_selectedChoiceIndex == i)
                            {
                                _selectedChoiceIndex = -1;
                                _selectedActionIndex = -1;
                            }
                            else if (_selectedChoiceIndex > i)
                            {
                                _selectedChoiceIndex--;
                            }

                            MarkDirty();
                            break;
                        }
                    }
                }
            }

            DrawSelectedChoiceEditor(sceneData);
        }

        private void DrawSelectedContentEditor(SceneData sceneData)
        {
            if (_selectedContentIndex < 0 || _selectedContentIndex >= sceneData.Content.Count)
                return;

            SceneContentData contentData = sceneData.Content[_selectedContentIndex];
            if (contentData == null)
            {
                sceneData.Content[_selectedContentIndex] = new SceneContentData();
                contentData = sceneData.Content[_selectedContentIndex];
                MarkDirty();
            }

            EditorGUILayout.Space(2f);
            EditorGUILayout.LabelField("Selected Content", EditorStyles.boldLabel);
            DrawField(() => contentData.Type = (SceneContentType)EditorGUILayout.EnumPopup("Type", contentData.Type));
            DrawField(() => contentData.Value = EditorGUILayout.TextField("Value", contentData.Value ?? string.Empty));

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Remove Selected Content", GUILayout.Width(190f)))
                {
                    sceneData.Content.RemoveAt(_selectedContentIndex);
                    _selectedContentIndex = -1;
                    MarkDirty();
                }
            }
        }

        private void DrawSelectedChoiceEditor(SceneData sceneData)
        {
            if (_selectedChoiceIndex < 0 || _selectedChoiceIndex >= sceneData.Choices.Count)
                return;

            ChoiceData choiceData = sceneData.Choices[_selectedChoiceIndex];
            if (choiceData == null)
            {
                sceneData.Choices[_selectedChoiceIndex] = new ChoiceData();
                choiceData = sceneData.Choices[_selectedChoiceIndex];
                MarkDirty();
            }

            EditorGUILayout.Space(2f);
            EditorGUILayout.LabelField("Selected Choice", EditorStyles.boldLabel);
            DrawField(() => choiceData.Id = EditorGUILayout.TextField("Choice Id", choiceData.Id ?? string.Empty));
            DrawField(() => choiceData.Type = (ChoiceType)EditorGUILayout.EnumPopup("Type", choiceData.Type));
            DrawField(() => choiceData.Text = EditorGUILayout.TextField("Text", choiceData.Text ?? string.Empty));
            DrawField(() => choiceData.Description = EditorGUILayout.TextField("Description", choiceData.Description ?? string.Empty));
            DrawField(() => choiceData.AlwaysShow = EditorGUILayout.Toggle("Always Show", choiceData.AlwaysShow));
            DrawField(() => choiceData.Tags = ParseCsv(EditorGUILayout.TextField("Tags (csv)", JoinCsv(choiceData.Tags))));

            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);
            DrawCreateOptionButtons(
                _choiceActionCreateOptionsRegistry.GetOptions(),
                option =>
                {
                    choiceData.Actions ??= new List<ChoiceActionData>();
                    choiceData.Actions.Add(option.Create());
                    _selectedActionIndex = choiceData.Actions.Count - 1;
                    MarkDirty();
                },
                "Add Action");
            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField(string.Empty, GUI.skin.horizontalSlider);
            EditorGUILayout.Space(4f);

            if (choiceData.Actions != null)
            {
                for (int i = 0; i < choiceData.Actions.Count; i++)
                {
                    ChoiceActionData actionData = choiceData.Actions[i];
                    string label = $"{i}: {actionData?.Type}";
                    bool selected = i == _selectedActionIndex;
                    GUIStyle style = selected ? EditorStyles.helpBox : EditorStyles.miniButton;
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button(label, style))
                            _selectedActionIndex = i;

                        if (GUILayout.Button("X", GUILayout.Width(22f)))
                        {
                            choiceData.Actions.RemoveAt(i);

                            if (_selectedActionIndex == i)
                                _selectedActionIndex = -1;
                            else if (_selectedActionIndex > i)
                                _selectedActionIndex--;

                            MarkDirty();
                            break;
                        }
                    }
                }
            }

            DrawSelectedActionEditor(sceneData, choiceData);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Remove Selected Choice", GUILayout.Width(185f)))
                {
                    sceneData.Choices.RemoveAt(_selectedChoiceIndex);
                    _selectedChoiceIndex = -1;
                    _selectedActionIndex = -1;
                    MarkDirty();
                }
            }
        }

        private void DrawSelectedActionEditor(SceneData sceneData, ChoiceData choiceData)
        {
            if (choiceData.Actions == null || _selectedActionIndex < 0 || _selectedActionIndex >= choiceData.Actions.Count)
                return;

            ChoiceActionData actionData = choiceData.Actions[_selectedActionIndex];
            if (actionData == null)
            {
                actionData = new ChoiceActionData { Type = ChoiceActionType.GoToScene };
                choiceData.Actions[_selectedActionIndex] = actionData;
                MarkDirty();
            }

            actionData.Params ??= new ChoiceActionParamsData();
            actionData.Params.Strings ??= new Dictionary<string, string>();
            actionData.Params.Ints ??= new Dictionary<string, int>();
            actionData.Params.Bools ??= new Dictionary<string, bool>();

            EditorGUILayout.Space(2f);
            EditorGUILayout.LabelField("Selected Action", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Type: {actionData.Type}");

            if (actionData.Type == ChoiceActionType.GoToScene)
            {
                string currentTarget = AdventureGraphBuilder.GetSceneId(actionData);
                List<string> sceneIds = GetSortedSceneIds();

                if (sceneIds.Count > 0)
                {
                    int selectedIndex = Mathf.Max(0, sceneIds.IndexOf(currentTarget));
                    int nextIndex = EditorGUILayout.Popup("Target Scene", selectedIndex, sceneIds.ToArray());
                    string selectedSceneId = sceneIds[nextIndex];
                    if (!string.Equals(currentTarget, selectedSceneId, StringComparison.Ordinal))
                    {
                        actionData.Params.Strings["sceneId"] = selectedSceneId;
                        MarkDirty();
                    }
                }
                else
                {
                    DrawField(() =>
                    {
                        string nextValue = EditorGUILayout.TextField("Target Scene", currentTarget ?? string.Empty);
                        actionData.Params.Strings["sceneId"] = nextValue;
                    });
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Remove Selected Action", GUILayout.Width(180f)))
                {
                    choiceData.Actions.RemoveAt(_selectedActionIndex);
                    _selectedActionIndex = -1;
                    MarkDirty();
                }
            }
        }

        private void DrawRightPanel()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(380f)))
            {
                using (EditorGUILayout.ScrollViewScope scope = new EditorGUILayout.ScrollViewScope(_rightScroll))
                {
                    _rightScroll = scope.scrollPosition;

                    DrawGraphPanel();
                    EditorGUILayout.Space(8f);
                    DrawLocalizationPanel();
                    EditorGUILayout.Space(8f);
                    DrawValidationPanel();
                }
            }
        }

        private void DrawGraphPanel()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Scene Graph", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Preview", GUILayout.Width(80f)))
                {
                    AdventureGraphPreviewWindow.Open(_adventureData, _selectedSceneId);
                }
            }

            if (_adventureData == null)
            {
                EditorGUILayout.HelpBox("No adventure selected.", MessageType.Info);
                return;
            }

            AdventureGraphData graphData = _graphBuilder.Build(_adventureData);
            EditorGUILayout.LabelField($"Nodes: {graphData.Nodes.Count}");
            EditorGUILayout.LabelField($"Edges: {graphData.Edges.Count}");

            if (graphData.Edges.Count == 0)
            {
                EditorGUILayout.HelpBox("No scene links found yet.", MessageType.None);
                return;
            }

            for (int i = 0; i < graphData.Edges.Count; i++)
            {
                AdventureGraphEdge edge = graphData.Edges[i];
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button(edge.FromSceneId, GUILayout.Width(110f)))
                        SelectScene(edge.FromSceneId);

                    GUILayout.Label("->", GUILayout.Width(18f));

                    if (GUILayout.Button(edge.ToSceneId, GUILayout.Width(110f)))
                        SelectScene(edge.ToSceneId);

                    string edgeLabel = string.IsNullOrWhiteSpace(edge.ChoiceId) ? edge.ChoiceText : edge.ChoiceId;
                    GUILayout.Label(string.IsNullOrWhiteSpace(edgeLabel) ? "(choice)" : edgeLabel);
                }
            }
        }

        private void DrawLocalizationPanel()
        {
            EditorGUILayout.LabelField("Localization Keys", EditorStyles.boldLabel);
            if (_adventureData == null)
            {
                EditorGUILayout.HelpBox("No adventure selected.", MessageType.Info);
                return;
            }

            List<string> keys = _localizationCollector.Collect(_adventureData);
            EditorGUILayout.LabelField($"Found keys: {keys.Count} (prefix 'loc:')");
            if (keys.Count == 0)
            {
                EditorGUILayout.HelpBox("No localization keys found by the default collector.", MessageType.None);
                return;
            }

            for (int i = 0; i < keys.Count; i++)
                EditorGUILayout.SelectableLabel(keys[i], GUILayout.Height(16f));
        }

        private void DrawValidationPanel()
        {
            EditorGUILayout.LabelField("Validation", EditorStyles.boldLabel);
            if (_adventureData == null)
            {
                EditorGUILayout.HelpBox("No adventure selected.", MessageType.Info);
                return;
            }

            List<string> errors = _validationService.Validate(_adventureData);
            if (errors.Count == 0)
            {
                EditorGUILayout.HelpBox("No validation errors.", MessageType.Info);
                return;
            }

            for (int i = 0; i < errors.Count; i++)
                EditorGUILayout.HelpBox(errors[i], MessageType.Warning);
        }

        private void DrawCreateOptionButtons<T>(
            IReadOnlyList<CreateOptionDescriptor<T>> options,
            Action<CreateOptionDescriptor<T>> onClick,
            string sectionTitle,
            float buttonWidth = 0f)
        {
            if (options == null || options.Count == 0)
                return;

            EditorGUILayout.LabelField(sectionTitle, EditorStyles.miniBoldLabel);
            for (int i = 0; i < options.Count; i++)
            {
                CreateOptionDescriptor<T> option = options[i];
                if (option == null)
                    continue;

                bool clicked = buttonWidth > 0f
                    ? GUILayout.Button(option.ToGuiContent(), GUILayout.Width(buttonWidth), GUILayout.Height(24f))
                    : GUILayout.Button(option.ToGuiContent(), GUILayout.Height(24f));

                if (clicked)
                    onClick?.Invoke(option);
            }
        }

        private void CreateAdventureFile(CreateOptionDescriptor<AdventureData> option)
        {
            if (option?.Create == null)
                return;

            AdventureData template = option.Create();
            string initialName = string.IsNullOrWhiteSpace(template?.Id) ? _newAdventureFileName : template.Id;
            IdentifierPromptWindow.Open("Create Adventure", initialName, "Create", value =>
            {
                string normalizedName = AdventureEditorFileRepository.NormalizeFileName(value);
                string filePath = AdventureEditorFileRepository.BuildProjectRelativePath(normalizedName);
                if (_repository.Exists(filePath))
                {
                    EditorUtility.DisplayDialog("Adventure Editor", $"File already exists: {filePath}", "OK");
                    return;
                }

                _newAdventureFileName = normalizedName;
                AdventureData newAdventure = option.Create();
                _repository.Save(filePath, newAdventure);
                RefreshFiles();
                SelectAdventure(filePath);
            });
        }

        private void SaveCurrentAdventure()
        {
            if (_adventureData == null || string.IsNullOrWhiteSpace(_selectedFilePath))
                return;

            _repository.Save(_selectedFilePath, _adventureData);
            _isDirty = false;
        }

        private void ReloadCurrentAdventure()
        {
            if (string.IsNullOrWhiteSpace(_selectedFilePath))
                return;

            if (_repository.TryLoad(_selectedFilePath, out AdventureData loadedAdventure, out string error))
            {
                _adventureData = loadedAdventure;
                _isDirty = false;
                EnsureSceneSelection();
                return;
            }

            EditorUtility.DisplayDialog("Adventure Editor", $"Failed to reload file.\n{error}", "OK");
        }

        private void RefreshFiles()
        {
            _filePaths = _repository.GetAdventureFiles();
            if (!string.IsNullOrWhiteSpace(_selectedFilePath) && !_filePaths.Contains(_selectedFilePath))
            {
                _selectedFilePath = null;
                _adventureData = null;
                _isDirty = false;
            }
        }

        private void SelectAdventure(string projectRelativePath)
        {
            if (!CanLoseUnsavedChanges())
                return;

            if (!_repository.TryLoad(projectRelativePath, out AdventureData loadedAdventure, out string error))
            {
                EditorUtility.DisplayDialog("Adventure Editor", $"Failed to open adventure file.\n{error}", "OK");
                return;
            }

            _selectedFilePath = projectRelativePath;
            _adventureData = loadedAdventure;
            _isDirty = false;

            _selectedContentIndex = -1;
            _selectedChoiceIndex = -1;
            _selectedActionIndex = -1;

            EnsureSceneSelection();
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
                "Delete Adventure",
                $"Delete file '{_selectedFilePath}'?",
                "Delete",
                "Cancel");
            if (!confirmed)
                return;

            _repository.Delete(_selectedFilePath);
            _selectedFilePath = null;
            _adventureData = null;
            _isDirty = false;
            RefreshFiles();
        }

        private void DeleteFileByPath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return;

            bool confirmed = EditorUtility.DisplayDialog(
                "Delete Adventure",
                $"Delete file '{filePath}'?",
                "Delete",
                "Cancel");
            if (!confirmed)
                return;

            _repository.Delete(filePath);

            if (string.Equals(_selectedFilePath, filePath, StringComparison.Ordinal))
            {
                _selectedFilePath = null;
                _adventureData = null;
                _isDirty = false;
            }

            RefreshFiles();
        }

        private void AddSceneFromOption(CreateOptionDescriptor<SceneData> option)
        {
            if (_adventureData == null || option?.Create == null)
                return;

            SceneData template = option.Create() ?? new SceneData();
            string initialSceneId = string.IsNullOrWhiteSpace(template.Id) ? "new_scene" : template.Id;
            IdentifierPromptWindow.Open("Create Scene", initialSceneId, "Create", value =>
            {
                string requestedId = (value ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(requestedId))
                    requestedId = "new_scene";

                SceneData sceneData = option.Create() ?? new SceneData();
                string uniqueId = BuildUniqueSceneId(requestedId);
                sceneData.Id = uniqueId;

                sceneData.Tags ??= new List<string>();
                sceneData.Content ??= new List<SceneContentData>();
                sceneData.Choices ??= new List<ChoiceData>();

                _adventureData.Scenes[uniqueId] = sceneData;
                _selectedSceneId = uniqueId;
                _selectedContentIndex = -1;
                _selectedChoiceIndex = -1;
                _selectedActionIndex = -1;

                if (_adventureData.StartScenes.Count == 0)
                    _adventureData.StartScenes.Add(uniqueId);

                MarkDirty();
                Repaint();
            });
        }

        private void DeleteSelectedScene()
        {
            if (_adventureData == null || string.IsNullOrWhiteSpace(_selectedSceneId))
                return;

            bool confirmed = EditorUtility.DisplayDialog(
                "Delete Scene",
                $"Delete scene '{_selectedSceneId}'?\nAll links to this scene will be removed from start scenes.",
                "Delete",
                "Cancel");
            if (!confirmed)
                return;

            _adventureData.Scenes.Remove(_selectedSceneId);
            _adventureData.StartScenes.RemoveAll(x => string.Equals(x, _selectedSceneId, StringComparison.Ordinal));

            string deletedSceneId = _selectedSceneId;
            foreach (KeyValuePair<string, SceneData> pair in _adventureData.Scenes)
            {
                SceneData scene = pair.Value;
                if (scene?.Choices == null)
                    continue;

                for (int choiceIndex = 0; choiceIndex < scene.Choices.Count; choiceIndex++)
                {
                    ChoiceData choice = scene.Choices[choiceIndex];
                    if (choice?.Actions == null)
                        continue;

                    for (int actionIndex = choice.Actions.Count - 1; actionIndex >= 0; actionIndex--)
                    {
                        ChoiceActionData action = choice.Actions[actionIndex];
                        if (action?.Type != ChoiceActionType.GoToScene)
                            continue;

                        string targetScene = AdventureGraphBuilder.GetSceneId(action);
                        if (string.Equals(targetScene, deletedSceneId, StringComparison.Ordinal))
                            choice.Actions.RemoveAt(actionIndex);
                    }
                }
            }

            EnsureSceneSelection();
            MarkDirty();
        }

        private void DuplicateSelectedScene()
        {
            if (_adventureData == null || string.IsNullOrWhiteSpace(_selectedSceneId))
                return;

            if (!_adventureData.Scenes.TryGetValue(_selectedSceneId, out SceneData sourceScene))
                return;

            SceneData copiedScene = CloneScene(sourceScene);
            string newSceneId = BuildUniqueSceneId($"{_selectedSceneId}_copy");
            copiedScene.Id = newSceneId;

            _adventureData.Scenes[newSceneId] = copiedScene;
            _selectedSceneId = newSceneId;
            _selectedContentIndex = -1;
            _selectedChoiceIndex = -1;
            _selectedActionIndex = -1;
            MarkDirty();
        }

        private void RenameScene(string oldSceneId, string requestedSceneId)
        {
            if (_adventureData == null)
                return;

            string newSceneId = (requestedSceneId ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(newSceneId))
            {
                EditorUtility.DisplayDialog("Adventure Editor", "Scene id cannot be empty.", "OK");
                return;
            }

            if (string.Equals(oldSceneId, newSceneId, StringComparison.Ordinal))
                return;

            if (_adventureData.Scenes.ContainsKey(newSceneId))
            {
                EditorUtility.DisplayDialog("Adventure Editor", $"Scene '{newSceneId}' already exists.", "OK");
                return;
            }

            if (!_adventureData.Scenes.TryGetValue(oldSceneId, out SceneData sceneData))
                return;

            _adventureData.Scenes.Remove(oldSceneId);
            sceneData.Id = newSceneId;
            _adventureData.Scenes[newSceneId] = sceneData;

            for (int i = 0; i < _adventureData.StartScenes.Count; i++)
            {
                if (string.Equals(_adventureData.StartScenes[i], oldSceneId, StringComparison.Ordinal))
                    _adventureData.StartScenes[i] = newSceneId;
            }

            foreach (KeyValuePair<string, SceneData> pair in _adventureData.Scenes)
            {
                SceneData scene = pair.Value;
                if (scene?.Choices == null)
                    continue;

                for (int choiceIndex = 0; choiceIndex < scene.Choices.Count; choiceIndex++)
                {
                    ChoiceData choice = scene.Choices[choiceIndex];
                    if (choice?.Actions == null)
                        continue;

                    for (int actionIndex = 0; actionIndex < choice.Actions.Count; actionIndex++)
                    {
                        ChoiceActionData action = choice.Actions[actionIndex];
                        if (action?.Type != ChoiceActionType.GoToScene)
                            continue;

                        string targetScene = AdventureGraphBuilder.GetSceneId(action);
                        if (string.Equals(targetScene, oldSceneId, StringComparison.Ordinal))
                        {
                            action.Params.Strings["sceneId"] = newSceneId;
                        }
                    }
                }
            }

            _selectedSceneId = newSceneId;
            MarkDirty();
        }

        private void PromptRenameFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return;

            string currentId = AdventureEditorFileRepository.GetFileNameWithoutExtension(filePath);
            IdentifierPromptWindow.Open("Rename Adventure", currentId, "Rename", value =>
            {
                string normalized = AdventureEditorFileRepository.NormalizeFileName(value);
                string newPath = BuildRenamedFilePath(filePath, normalized);
                if (string.Equals(newPath, filePath, StringComparison.Ordinal))
                    return;

                if (_repository.Exists(newPath))
                {
                    EditorUtility.DisplayDialog("Adventure Editor", $"File already exists: {newPath}", "OK");
                    return;
                }

                try
                {
                    _repository.Rename(filePath, newPath);
                    if (string.Equals(_selectedFilePath, filePath, StringComparison.Ordinal))
                        _selectedFilePath = newPath;

                    RefreshFiles();
                    Repaint();
                }
                catch (Exception exception)
                {
                    EditorUtility.DisplayDialog("Adventure Editor", $"Rename failed:\n{exception.Message}", "OK");
                }
            });
        }

        private void PromptRenameScene(string sceneId)
        {
            if (string.IsNullOrWhiteSpace(sceneId))
                return;

            IdentifierPromptWindow.Open("Rename Scene", sceneId, "Rename", value =>
            {
                RenameScene(sceneId, value);
                Repaint();
            });
        }

        private static string BuildRenamedFilePath(string oldPath, string newId)
        {
            string normalized = AdventureEditorFileRepository.NormalizeFileName(newId);
            string directory = Path.GetDirectoryName(oldPath) ?? AdventureEditorFileRepository.ADVENTURES_DIRECTORY;
            directory = directory.Replace('\\', '/');
            return $"{directory}/{normalized}.json";
        }

        private bool CanLoseUnsavedChanges()
        {
            if (!_isDirty)
                return true;

            return EditorUtility.DisplayDialog(
                "Unsaved changes",
                "Current adventure has unsaved changes. Continue without saving?",
                "Continue",
                "Cancel");
        }

        private void EnsureAdventureDataInitialized()
        {
            _adventureData.Tags ??= new List<string>();
            _adventureData.IgnoredTags ??= new List<string>();
            _adventureData.AdventureLinks ??= new List<string>();
            _adventureData.Restrictions ??= new List<Modules.Restrictions.Scripts.Core.Restriction>();
            _adventureData.StartScenes ??= new List<string>();
            _adventureData.Scenes ??= new Dictionary<string, SceneData>();

            List<string> keys = new List<string>(_adventureData.Scenes.Keys);
            for (int i = 0; i < keys.Count; i++)
            {
                string sceneId = keys[i];
                SceneData scene = _adventureData.Scenes[sceneId];
                if (scene == null)
                {
                    scene = new SceneData();
                    _adventureData.Scenes[sceneId] = scene;
                    MarkDirty();
                }

                scene.Id = sceneId;
                scene.Tags ??= new List<string>();
                scene.Content ??= new List<SceneContentData>();
                scene.Choices ??= new List<ChoiceData>();

                for (int contentIndex = 0; contentIndex < scene.Content.Count; contentIndex++)
                {
                    scene.Content[contentIndex] ??= new SceneContentData
                    {
                        Restrictions = new List<Modules.Restrictions.Scripts.Core.Restriction>(),
                    };
                    scene.Content[contentIndex].Restrictions ??= new List<Modules.Restrictions.Scripts.Core.Restriction>();
                }

                for (int choiceIndex = 0; choiceIndex < scene.Choices.Count; choiceIndex++)
                {
                    scene.Choices[choiceIndex] ??= new ChoiceData();
                    scene.Choices[choiceIndex].Tags ??= new List<string>();
                    scene.Choices[choiceIndex].Restrictions ??= new List<Modules.Restrictions.Scripts.Core.Restriction>();
                    scene.Choices[choiceIndex].Actions ??= new List<ChoiceActionData>();

                    for (int actionIndex = 0; actionIndex < scene.Choices[choiceIndex].Actions.Count; actionIndex++)
                    {
                        ChoiceActionData action = scene.Choices[choiceIndex].Actions[actionIndex] ?? new ChoiceActionData();
                        action.Params ??= new ChoiceActionParamsData();
                        action.Params.Strings ??= new Dictionary<string, string>();
                        action.Params.Ints ??= new Dictionary<string, int>();
                        action.Params.Bools ??= new Dictionary<string, bool>();
                        scene.Choices[choiceIndex].Actions[actionIndex] = action;
                    }
                }
            }
        }

        private void EnsureSceneSelection()
        {
            List<string> sceneIds = GetSortedSceneIds();
            if (sceneIds.Count == 0)
            {
                _selectedSceneId = null;
                _selectedContentIndex = -1;
                _selectedChoiceIndex = -1;
                _selectedActionIndex = -1;
                return;
            }

            if (!sceneIds.Contains(_selectedSceneId))
                _selectedSceneId = sceneIds[0];
        }

        private List<string> GetSortedSceneIds()
        {
            List<string> sceneIds = new List<string>();
            if (_adventureData?.Scenes == null)
                return sceneIds;

            foreach (var pair in _adventureData.Scenes)
            {
                if (!string.IsNullOrWhiteSpace(pair.Key))
                    sceneIds.Add(pair.Key);
            }

            sceneIds.Sort(StringComparer.Ordinal);
            return sceneIds;
        }

        private string BuildUniqueSceneId(string baseId)
        {
            string safeBaseId = string.IsNullOrWhiteSpace(baseId) ? "scene" : baseId.Trim();
            if (!_adventureData.Scenes.ContainsKey(safeBaseId))
                return safeBaseId;

            int postfix = 1;
            while (true)
            {
                string candidate = $"{safeBaseId}_{postfix}";
                if (!_adventureData.Scenes.ContainsKey(candidate))
                    return candidate;
                postfix++;
            }
        }

        private void SelectScene(string sceneId)
        {
            _selectedSceneId = sceneId;
            _selectedContentIndex = -1;
            _selectedChoiceIndex = -1;
            _selectedActionIndex = -1;
        }

        private static SceneData CloneScene(SceneData sourceScene)
        {
            if (sourceScene == null)
                return new SceneData();

            SceneData clone = new SceneData
            {
                Id = sourceScene.Id,
                Tags = new List<string>(sourceScene.Tags ?? new List<string>()),
                NotClearScene = sourceScene.NotClearScene,
                Content = new List<SceneContentData>(),
                Choices = new List<ChoiceData>(),
            };

            if (sourceScene.Content != null)
            {
                for (int i = 0; i < sourceScene.Content.Count; i++)
                {
                    SceneContentData content = sourceScene.Content[i];
                    clone.Content.Add(new SceneContentData
                    {
                        Type = content?.Type ?? SceneContentType.Text,
                        Value = content?.Value ?? string.Empty,
                        Restrictions = new List<Modules.Restrictions.Scripts.Core.Restriction>(content?.Restrictions ?? new List<Modules.Restrictions.Scripts.Core.Restriction>()),
                    });
                }
            }

            if (sourceScene.Choices != null)
            {
                for (int i = 0; i < sourceScene.Choices.Count; i++)
                {
                    ChoiceData sourceChoice = sourceScene.Choices[i];
                    ChoiceData choiceClone = new ChoiceData
                    {
                        Id = sourceChoice?.Id ?? string.Empty,
                        Tags = new List<string>(sourceChoice?.Tags ?? new List<string>()),
                        Type = sourceChoice?.Type ?? ChoiceType.Dafault,
                        Text = sourceChoice?.Text ?? string.Empty,
                        Description = sourceChoice?.Description ?? string.Empty,
                        AlwaysShow = sourceChoice?.AlwaysShow ?? false,
                        Restrictions = new List<Modules.Restrictions.Scripts.Core.Restriction>(sourceChoice?.Restrictions ?? new List<Modules.Restrictions.Scripts.Core.Restriction>()),
                        Actions = new List<ChoiceActionData>(),
                    };

                    if (sourceChoice?.Actions != null)
                    {
                        for (int actionIndex = 0; actionIndex < sourceChoice.Actions.Count; actionIndex++)
                        {
                            ChoiceActionData sourceAction = sourceChoice.Actions[actionIndex];
                            choiceClone.Actions.Add(new ChoiceActionData
                            {
                                Type = sourceAction?.Type ?? ChoiceActionType.None,
                                Params = new ChoiceActionParamsData
                                {
                                    Strings = new Dictionary<string, string>(sourceAction?.Params?.Strings ?? new Dictionary<string, string>()),
                                    Ints = new Dictionary<string, int>(sourceAction?.Params?.Ints ?? new Dictionary<string, int>()),
                                    Bools = new Dictionary<string, bool>(sourceAction?.Params?.Bools ?? new Dictionary<string, bool>()),
                                },
                            });
                        }
                    }

                    clone.Choices.Add(choiceClone);
                }
            }

            return clone;
        }

        private static List<string> ParseCsv(string csv)
        {
            List<string> result = new List<string>();
            if (string.IsNullOrWhiteSpace(csv))
                return result;

            string[] parts = csv.Split(',');
            for (int i = 0; i < parts.Length; i++)
            {
                string value = parts[i].Trim();
                if (!string.IsNullOrWhiteSpace(value))
                    result.Add(value);
            }

            return result;
        }

        private static string JoinCsv(List<string> values)
        {
            if (values == null || values.Count == 0)
                return string.Empty;

            return string.Join(", ", values);
        }

        private void DrawField(Action drawAction)
        {
            EditorGUI.BeginChangeCheck();
            drawAction?.Invoke();
            if (EditorGUI.EndChangeCheck())
                MarkDirty();
        }

        private void MarkDirty()
        {
            _isDirty = true;
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
