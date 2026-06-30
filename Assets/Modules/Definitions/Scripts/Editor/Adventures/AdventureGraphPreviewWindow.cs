using Modules.RPG.Scripts.Adventure.Choice;
using Modules.RPG.Scripts.Adventure.Choice.Actions;
using Modules.RPG.Scripts.Adventure.Data;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Modules.Definitions.Scripts.Editor.Adventures.CreateOptions;

namespace Modules.Definitions.Scripts.Editor.Adventures
{
    public sealed class AdventureGraphPreviewWindow : EditorWindow
    {
        private const float NODE_WIDTH = 220f;
        private const float NODE_HEIGHT = 94f;
        private const float LAYER_SPACING_X = 320f;
        private const float LAYER_SPACING_Y = 130f;
        private const float CANVAS_PADDING = 24f;
        private const float CONTENT_ICON_SIZE = 14f;
        private const float CONTENT_ICON_SPACING = 4f;
        private const float ICON_ROW_GAP = 3f;
        private const float ICON_BOTTOM_MARGIN = 4f;

        private readonly Dictionary<string, PreviewNode> _nodes = new Dictionary<string, PreviewNode>(StringComparer.Ordinal);
        private readonly List<PreviewEdge> _edges = new List<PreviewEdge>();
        private readonly Dictionary<string, Vector2> _nodePositions = new Dictionary<string, Vector2>(StringComparer.Ordinal);
        private readonly Dictionary<string, Rect> _nodeScreenRects = new Dictionary<string, Rect>(StringComparer.Ordinal);
        private readonly Dictionary<SceneContentType, Texture> _contentIconsByType = new Dictionary<SceneContentType, Texture>();
        private readonly Dictionary<ChoiceType, Texture> _choiceIconsByType = new Dictionary<ChoiceType, Texture>();

        private Vector2 _pan = new Vector2(40f, 40f);
        private float _zoom = 1f;
        private string _selectedNodeId;
        private bool _isPanning;
        private string _draggedNodeId;
        private Vector2 _dragNodeOffsetWorld;
        private Vector2 _lastMouseScreen;

        private int _existingNodesCount;
        private int _brokenNodesCount;
        private int _unreachableNodesCount;

        private GUIStyle _nodeLabelStyle;
        private GUIStyle _nodeTagsStyle;
        private GUIStyle _legendStyle;
        private GUIStyle _statsStyle;
        private Rect _canvasRect;

        public static void Open(AdventureData adventureData, string selectedSceneId)
        {
            AdventureGraphPreviewWindow window = GetWindow<AdventureGraphPreviewWindow>();
            window.titleContent = new GUIContent("Scene Graph Preview");
            window.minSize = new Vector2(900f, 540f);
            window.BuildGraph(adventureData, selectedSceneId);
            window.Show();
            window.Focus();
        }

        private void OnGUI()
        {
            DrawTopBar();

            _canvasRect = GUILayoutUtility.GetRect(10f, 100000f, 10f, 100000f, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            EditorGUI.DrawRect(_canvasRect, new Color(0.12f, 0.12f, 0.12f));
            DrawGrid(_canvasRect, 24f, new Color(1f, 1f, 1f, 0.03f));
            DrawGrid(_canvasRect, 120f, new Color(1f, 1f, 1f, 0.06f));

            DrawEdges();
            DrawNodes();
            HandleInput(Event.current);

            if (GUI.changed)
                Repaint();
        }

        private void DrawTopBar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                EditorGUILayout.LabelField(
                    $"Nodes: {_existingNodesCount}  Broken: {_brokenNodesCount}  Unreachable: {_unreachableNodesCount}",
                    GetStatsStyle());

                GUILayout.FlexibleSpace();
                GUILayout.Label("Zoom", EditorStyles.miniLabel, GUILayout.Width(32f));
                float nextZoom = GUILayout.HorizontalSlider(_zoom, 0.45f, 2.5f, GUILayout.Width(130f));
                if (!Mathf.Approximately(nextZoom, _zoom))
                    _zoom = nextZoom;

                GUILayout.Space(8f);
                if (GUILayout.Button("Reset View", EditorStyles.toolbarButton, GUILayout.Width(90f)))
                {
                    _pan = new Vector2(40f, 40f);
                    _zoom = 1f;
                }

                if (GUILayout.Button("Auto Layout", EditorStyles.toolbarButton, GUILayout.Width(90f)))
                {
                    AutoLayout();
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Legend:", EditorStyles.boldLabel, GUILayout.Width(50f));
                GUILayout.Label("Start", GetLegendStyle(new Color(0.27f, 0.66f, 1f, 1f)));
                GUILayout.Space(10f);
                GUILayout.Label("Regular", GetLegendStyle(new Color(0.48f, 0.48f, 0.48f, 1f)));
                GUILayout.Space(10f);
                GUILayout.Label("Unreachable", GetLegendStyle(new Color(0.85f, 0.55f, 0.2f, 1f)));
                GUILayout.Space(10f);
                GUILayout.Label("Broken Target", GetLegendStyle(new Color(0.9f, 0.3f, 0.3f, 1f)));
                GUILayout.FlexibleSpace();
            }
        }

        private void DrawGrid(Rect rect, float spacing, Color color)
        {
            Handles.BeginGUI();
            Handles.color = color;

            float offsetX = Mathf.Repeat(_pan.x, spacing);
            float offsetY = Mathf.Repeat(_pan.y, spacing);

            for (float x = rect.xMin + offsetX; x <= rect.xMax; x += spacing)
                Handles.DrawLine(new Vector3(x, rect.yMin), new Vector3(x, rect.yMax));

            for (float y = rect.yMin + offsetY; y <= rect.yMax; y += spacing)
                Handles.DrawLine(new Vector3(rect.xMin, y), new Vector3(rect.xMax, y));

            Handles.EndGUI();
        }

        private void DrawEdges()
        {
            Handles.BeginGUI();
            for (int i = 0; i < _edges.Count; i++)
            {
                PreviewEdge edge = _edges[i];
                if (!_nodes.TryGetValue(edge.FromNodeId, out PreviewNode fromNode))
                    continue;
                if (!_nodes.TryGetValue(edge.ToNodeId, out PreviewNode toNode))
                    continue;

                Vector2 from = ToScreenPoint(_nodePositions[fromNode.Id] + new Vector2(NODE_WIDTH, NODE_HEIGHT * 0.5f));
                Vector2 to = ToScreenPoint(_nodePositions[toNode.Id] + new Vector2(0f, NODE_HEIGHT * 0.5f));
                Vector2 tangentA = from + Vector2.right * (70f * _zoom);
                Vector2 tangentB = to + Vector2.left * (70f * _zoom);

                Color edgeColor = toNode.IsBrokenTarget
                    ? new Color(0.9f, 0.3f, 0.3f, 0.95f)
                    : new Color(0.82f, 0.82f, 0.82f, 0.85f);

                Handles.DrawBezier(from, to, tangentA, tangentB, edgeColor, null, 2f);
                DrawArrowHead(to, (to - tangentB).normalized, edgeColor);
            }

            Handles.EndGUI();
        }

        private void DrawArrowHead(Vector2 tip, Vector2 direction, Color color)
        {
            Vector2 dir = direction.sqrMagnitude < 0.0001f ? Vector2.right : direction.normalized;
            Vector2 normal = new Vector2(-dir.y, dir.x);
            float size = 6f;

            Vector3 p1 = tip;
            Vector3 p2 = tip - dir * size + normal * (size * 0.55f);
            Vector3 p3 = tip - dir * size - normal * (size * 0.55f);

            Handles.color = color;
            Handles.DrawAAConvexPolygon(p1, p2, p3);
        }

        private void DrawNodes()
        {
            _nodeScreenRects.Clear();

            foreach (KeyValuePair<string, PreviewNode> pair in _nodes)
            {
                PreviewNode node = pair.Value;
                Vector2 worldPos = _nodePositions[node.Id];
                Rect screenRect = ToScreenRect(new Rect(worldPos.x, worldPos.y, NODE_WIDTH, NODE_HEIGHT));
                _nodeScreenRects[node.Id] = screenRect;

                Color fill = GetNodeColor(node);
                EditorGUI.DrawRect(screenRect, fill);

                Rect borderRect = new Rect(screenRect.x, screenRect.y, screenRect.width, 1f);
                EditorGUI.DrawRect(borderRect, new Color(1f, 1f, 1f, 0.22f));
                EditorGUI.DrawRect(new Rect(screenRect.x, screenRect.yMax - 1f, screenRect.width, 1f), new Color(0f, 0f, 0f, 0.35f));

                if (string.Equals(node.Id, _selectedNodeId, StringComparison.Ordinal))
                {
                    float outline = 2f;
                    EditorGUI.DrawRect(new Rect(screenRect.x - outline, screenRect.y - outline, screenRect.width + outline * 2f, outline), new Color(1f, 1f, 1f, 0.9f));
                    EditorGUI.DrawRect(new Rect(screenRect.x - outline, screenRect.yMax, screenRect.width + outline * 2f, outline), new Color(1f, 1f, 1f, 0.9f));
                    EditorGUI.DrawRect(new Rect(screenRect.x - outline, screenRect.y, outline, screenRect.height), new Color(1f, 1f, 1f, 0.9f));
                    EditorGUI.DrawRect(new Rect(screenRect.xMax, screenRect.y, outline, screenRect.height), new Color(1f, 1f, 1f, 0.9f));
                }

                Rect idRect = new Rect(screenRect.x + 6f, screenRect.y + 4f, screenRect.width - 12f, 20f);
                GUI.Label(idRect, node.Id, GetNodeLabelStyle());

                float iconRowHeight = (CONTENT_ICON_SIZE * _zoom);
                float iconAreaHeight = iconRowHeight * 2f + ICON_ROW_GAP * _zoom + ICON_BOTTOM_MARGIN * _zoom;
                Rect tagsRect = new Rect(screenRect.x + 6f, screenRect.y + 24f, screenRect.width - 12f, screenRect.height - 28f - iconAreaHeight);
                GUI.Label(tagsRect, node.TagsLabel, GetNodeTagsStyle());

                DrawNodeContentIcons(screenRect, node);
                DrawNodeChoiceIcons(screenRect, node);
            }
        }

        private void DrawNodeContentIcons(Rect screenRect, PreviewNode node)
        {
            if (node.ContentIcons == null || node.ContentIcons.Count == 0)
                return;

            float iconSize = CONTENT_ICON_SIZE * _zoom;
            float iconSpacing = CONTENT_ICON_SPACING * _zoom;
            float y = screenRect.yMax - iconSize - ICON_BOTTOM_MARGIN * _zoom - iconSize - ICON_ROW_GAP * _zoom;
            float x = screenRect.x + 6f;
            float maxX = screenRect.xMax - 6f;

            for (int i = 0; i < node.ContentIcons.Count; i++)
            {
                Texture icon = node.ContentIcons[i];
                if (icon == null)
                    continue;

                if (x + iconSize > maxX)
                    break;

                Rect iconRect = new Rect(x, y, iconSize, iconSize);
                GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit, true);
                x += iconSize + iconSpacing;
            }
        }

        private void DrawNodeChoiceIcons(Rect screenRect, PreviewNode node)
        {
            if (node.ChoiceIcons == null || node.ChoiceIcons.Count == 0)
                return;

            float iconSize = CONTENT_ICON_SIZE * _zoom;
            float iconSpacing = CONTENT_ICON_SPACING * _zoom;
            float y = screenRect.yMax - iconSize - ICON_BOTTOM_MARGIN * _zoom;
            float x = screenRect.x + 6f;
            float maxX = screenRect.xMax - 6f;

            for (int i = 0; i < node.ChoiceIcons.Count; i++)
            {
                Texture icon = node.ChoiceIcons[i];
                if (icon == null)
                    continue;

                if (x + iconSize > maxX)
                    break;

                Rect iconRect = new Rect(x, y, iconSize, iconSize);
                GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit, true);
                x += iconSize + iconSpacing;
            }
        }

        private Color GetNodeColor(PreviewNode node)
        {
            if (node.IsBrokenTarget)
                return new Color(0.64f, 0.22f, 0.22f, 0.96f);
            if (node.IsStart)
                return new Color(0.19f, 0.42f, 0.66f, 0.96f);
            if (!node.IsReachable)
                return new Color(0.53f, 0.36f, 0.18f, 0.96f);

            return new Color(0.26f, 0.26f, 0.26f, 0.96f);
        }

        private void HandleInput(Event evt)
        {
            if (!_canvasRect.Contains(evt.mousePosition))
                return;

            if (evt.type == EventType.ScrollWheel)
            {
                float zoomBefore = _zoom;
                float delta = -evt.delta.y * 0.06f;
                _zoom = Mathf.Clamp(_zoom + delta, 0.45f, 2.5f);

                Vector2 mouseCanvas = evt.mousePosition - _canvasRect.position;
                Vector2 worldBefore = (mouseCanvas - _pan) / Mathf.Max(zoomBefore, 0.001f);
                _pan = mouseCanvas - worldBefore * _zoom;

                evt.Use();
                GUI.changed = true;
                return;
            }

            if (evt.type == EventType.MouseDown && evt.button == 0)
            {
                string hitNode = FindTopNodeAt(evt.mousePosition);
                if (!string.IsNullOrWhiteSpace(hitNode))
                {
                    _selectedNodeId = hitNode;
                    _draggedNodeId = hitNode;
                    _dragNodeOffsetWorld = ScreenToWorld(evt.mousePosition) - _nodePositions[hitNode];
                    evt.Use();
                    GUI.changed = true;
                }
                return;
            }

            if (evt.type == EventType.MouseDrag && evt.button == 0 && !string.IsNullOrWhiteSpace(_draggedNodeId))
            {
                _nodePositions[_draggedNodeId] = ScreenToWorld(evt.mousePosition) - _dragNodeOffsetWorld;
                evt.Use();
                GUI.changed = true;
                return;
            }

            if (evt.type == EventType.MouseUp && evt.button == 0 && !string.IsNullOrWhiteSpace(_draggedNodeId))
            {
                _draggedNodeId = null;
                evt.Use();
                GUI.changed = true;
                return;
            }

            if (evt.type == EventType.MouseDown && evt.button == 2)
            {
                _isPanning = true;
                _lastMouseScreen = evt.mousePosition;
                evt.Use();
                return;
            }

            if (evt.type == EventType.MouseDrag && evt.button == 2 && _isPanning)
            {
                Vector2 delta = evt.mousePosition - _lastMouseScreen;
                _pan += delta;
                _lastMouseScreen = evt.mousePosition;
                evt.Use();
                GUI.changed = true;
                return;
            }

            if (evt.type == EventType.MouseUp && evt.button == 2)
            {
                _isPanning = false;
                evt.Use();
            }
        }

        private string FindTopNodeAt(Vector2 mouseScreen)
        {
            foreach (KeyValuePair<string, Rect> pair in _nodeScreenRects)
            {
                if (pair.Value.Contains(mouseScreen))
                    return pair.Key;
            }

            return null;
        }

        private Vector2 ScreenToWorld(Vector2 screen)
        {
            Vector2 canvasLocal = screen - _canvasRect.position;
            return (canvasLocal - _pan) / Mathf.Max(_zoom, 0.001f);
        }

        private Vector2 ToScreenPoint(Vector2 world)
        {
            return _canvasRect.position + _pan + world * _zoom;
        }

        private Rect ToScreenRect(Rect world)
        {
            Vector2 pos = ToScreenPoint(world.position);
            Vector2 size = world.size * _zoom;
            return new Rect(pos, size);
        }

        private void BuildGraph(AdventureData adventureData, string selectedSceneId)
        {
            _nodes.Clear();
            _edges.Clear();
            _nodePositions.Clear();
            EnsureContentIconsMap();
            EnsureChoiceIconsMap();

            if (adventureData?.Scenes == null)
            {
                _existingNodesCount = 0;
                _brokenNodesCount = 0;
                _unreachableNodesCount = 0;
                return;
            }

            HashSet<string> startSet = new HashSet<string>(adventureData.StartScenes ?? new List<string>(), StringComparer.Ordinal);

            foreach (KeyValuePair<string, SceneData> pair in adventureData.Scenes)
            {
                string id = pair.Key;
                SceneData sceneData = pair.Value;
                if (string.IsNullOrWhiteSpace(id))
                    continue;

                _nodes[id] = new PreviewNode
                {
                    Id = id,
                    TagsLabel = BuildTagsLabel(sceneData?.Tags),
                    ContentIcons = BuildContentIcons(sceneData?.Content),
                    ChoiceIcons = BuildChoiceIcons(sceneData?.Choices),
                    IsExistingScene = true,
                    IsStart = startSet.Contains(id),
                    IsReachable = false,
                };
            }

            foreach (KeyValuePair<string, SceneData> pair in adventureData.Scenes)
            {
                string fromScene = pair.Key;
                SceneData sceneData = pair.Value;
                if (sceneData?.Choices == null)
                    continue;

                for (int choiceIndex = 0; choiceIndex < sceneData.Choices.Count; choiceIndex++)
                {
                    ChoiceData choice = sceneData.Choices[choiceIndex];
                    if (choice?.Actions == null)
                        continue;

                    for (int actionIndex = 0; actionIndex < choice.Actions.Count; actionIndex++)
                    {
                        var action = choice.Actions[actionIndex];
                        if (action == null || action.Type != ChoiceActionType.GoToScene)
                            continue;

                        string target = AdventureGraphBuilder.GetSceneId(action);
                        if (string.IsNullOrWhiteSpace(target))
                            continue;

                        if (!_nodes.ContainsKey(target))
                        {
                            _nodes[target] = new PreviewNode
                            {
                                Id = target,
                                TagsLabel = "tags: -",
                                ContentIcons = new List<Texture>(),
                                ChoiceIcons = new List<Texture>(),
                                IsExistingScene = false,
                                IsBrokenTarget = true,
                                IsStart = false,
                                IsReachable = false,
                            };
                        }

                        _edges.Add(new PreviewEdge
                        {
                            FromNodeId = fromScene,
                            ToNodeId = target,
                        });
                    }
                }
            }

            MarkReachableNodes(startSet);
            AutoLayout();

            _selectedNodeId = _nodes.ContainsKey(selectedSceneId ?? string.Empty) ? selectedSceneId : null;
            if (string.IsNullOrWhiteSpace(_selectedNodeId))
            {
                foreach (KeyValuePair<string, PreviewNode> pair in _nodes)
                {
                    _selectedNodeId = pair.Key;
                    break;
                }
            }

            _existingNodesCount = 0;
            _brokenNodesCount = 0;
            _unreachableNodesCount = 0;
            foreach (KeyValuePair<string, PreviewNode> pair in _nodes)
            {
                PreviewNode node = pair.Value;
                if (node.IsExistingScene)
                    _existingNodesCount++;
                if (node.IsBrokenTarget)
                    _brokenNodesCount++;
                if (!node.IsBrokenTarget && !node.IsReachable)
                    _unreachableNodesCount++;
            }
        }

        private void MarkReachableNodes(HashSet<string> starts)
        {
            foreach (KeyValuePair<string, PreviewNode> pair in _nodes)
                pair.Value.IsReachable = false;

            Queue<string> queue = new Queue<string>();
            HashSet<string> visited = new HashSet<string>(StringComparer.Ordinal);

            foreach (string start in starts)
            {
                if (!_nodes.ContainsKey(start))
                    continue;

                queue.Enqueue(start);
                visited.Add(start);
                _nodes[start].IsReachable = true;
            }

            while (queue.Count > 0)
            {
                string current = queue.Dequeue();
                for (int i = 0; i < _edges.Count; i++)
                {
                    PreviewEdge edge = _edges[i];
                    if (!string.Equals(edge.FromNodeId, current, StringComparison.Ordinal))
                        continue;

                    if (!visited.Add(edge.ToNodeId))
                        continue;

                    if (_nodes.TryGetValue(edge.ToNodeId, out PreviewNode node))
                        node.IsReachable = true;

                    queue.Enqueue(edge.ToNodeId);
                }
            }
        }

        private void AutoLayout()
        {
            Dictionary<string, int> normalDepths = new Dictionary<string, int>(StringComparer.Ordinal);
            Queue<string> queue = new Queue<string>();
            List<string> leftColumnNodes = new List<string>();

            foreach (KeyValuePair<string, PreviewNode> pair in _nodes)
            {
                PreviewNode node = pair.Value;
                if (node.IsBrokenTarget || !node.IsReachable)
                {
                    leftColumnNodes.Add(pair.Key);
                    continue;
                }

                if (node.IsStart)
                {
                    normalDepths[pair.Key] = 0;
                    queue.Enqueue(pair.Key);
                }
            }

            if (queue.Count == 0)
            {
                foreach (KeyValuePair<string, PreviewNode> pair in _nodes)
                {
                    PreviewNode node = pair.Value;
                    if (node.IsBrokenTarget || !node.IsReachable)
                        continue;

                    if (node.IsExistingScene)
                    {
                        normalDepths[pair.Key] = 0;
                        queue.Enqueue(pair.Key);
                        break;
                    }
                }
            }

            while (queue.Count > 0)
            {
                string current = queue.Dequeue();
                int currentDepth = normalDepths[current];

                for (int i = 0; i < _edges.Count; i++)
                {
                    PreviewEdge edge = _edges[i];
                    if (!string.Equals(edge.FromNodeId, current, StringComparison.Ordinal))
                        continue;
                    if (!_nodes.TryGetValue(edge.ToNodeId, out PreviewNode targetNode))
                        continue;
                    if (targetNode.IsBrokenTarget || !targetNode.IsReachable)
                        continue;
                    if (normalDepths.ContainsKey(edge.ToNodeId))
                        continue;

                    normalDepths[edge.ToNodeId] = currentDepth + 1;
                    queue.Enqueue(edge.ToNodeId);
                }
            }

            int maxDepth = 0;
            foreach (KeyValuePair<string, int> pair in normalDepths)
                maxDepth = Mathf.Max(maxDepth, pair.Value);

            foreach (KeyValuePair<string, PreviewNode> pair in _nodes)
            {
                PreviewNode node = pair.Value;
                if (node.IsBrokenTarget || !node.IsReachable)
                    continue;

                if (!normalDepths.ContainsKey(pair.Key))
                {
                    maxDepth++;
                    normalDepths[pair.Key] = maxDepth;
                }
            }

            Dictionary<int, List<string>> normalLayers = new Dictionary<int, List<string>>();
            foreach (KeyValuePair<string, int> pair in normalDepths)
            {
                if (!normalLayers.TryGetValue(pair.Value, out List<string> list))
                {
                    list = new List<string>();
                    normalLayers[pair.Value] = list;
                }

                list.Add(pair.Key);
            }

            leftColumnNodes.Sort(StringComparer.Ordinal);

            List<int> depthKeys = new List<int>(normalLayers.Keys);
            depthKeys.Sort();

            bool hasLeftColumn = leftColumnNodes.Count > 0;
            float leftColumnX = CANVAS_PADDING;

            for (int i = 0; i < leftColumnNodes.Count; i++)
            {
                float y = CANVAS_PADDING + i * LAYER_SPACING_Y;
                _nodePositions[leftColumnNodes[i]] = new Vector2(leftColumnX, y);
            }

            for (int k = 0; k < depthKeys.Count; k++)
            {
                List<string> layerNodes = normalLayers[depthKeys[k]];
                layerNodes.Sort(StringComparer.Ordinal);

                int columnIndex = hasLeftColumn ? depthKeys[k] + 1 : depthKeys[k];
                float x = CANVAS_PADDING + columnIndex * LAYER_SPACING_X;
                for (int i = 0; i < layerNodes.Count; i++)
                {
                    float y = CANVAS_PADDING + i * LAYER_SPACING_Y;
                    _nodePositions[layerNodes[i]] = new Vector2(x, y);
                }
            }
        }

        private GUIStyle GetNodeLabelStyle()
        {
            if (_nodeLabelStyle == null)
            {
                _nodeLabelStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    alignment = TextAnchor.UpperLeft,
                    clipping = TextClipping.Clip,
                    fontSize = 11,
                };
                _nodeLabelStyle.normal.textColor = new Color(0.94f, 0.94f, 0.94f);
            }

            return _nodeLabelStyle;
        }

        private GUIStyle GetNodeTagsStyle()
        {
            if (_nodeTagsStyle == null)
            {
                _nodeTagsStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    alignment = TextAnchor.UpperLeft,
                    wordWrap = true,
                    clipping = TextClipping.Clip,
                    fontSize = 10,
                };
                _nodeTagsStyle.normal.textColor = new Color(0.82f, 0.86f, 0.9f);
            }

            return _nodeTagsStyle;
        }

        private static string BuildTagsLabel(List<string> tags)
        {
            if (tags == null || tags.Count == 0)
                return "tags: -";

            List<string> preparedTags = new List<string>();
            for (int i = 0; i < tags.Count; i++)
            {
                string tag = (tags[i] ?? string.Empty).Trim();
                if (!string.IsNullOrWhiteSpace(tag))
                    preparedTags.Add(tag);
            }

            return preparedTags.Count == 0
                ? "tags: -"
                : $"tags: {string.Join(", ", preparedTags)}";
        }

        private void EnsureContentIconsMap()
        {
            if (_contentIconsByType.Count > 0)
                return;

            SceneContentCreateOptionsRegistry registry = new SceneContentCreateOptionsRegistry();
            IReadOnlyList<CreateOptionDescriptor<SceneContentData>> options = registry.GetOptions();
            for (int i = 0; i < options.Count; i++)
            {
                CreateOptionDescriptor<SceneContentData> option = options[i];
                if (option == null)
                    continue;

                SceneContentData prototype = option.Create?.Invoke();
                if (prototype == null)
                    continue;

                Texture icon = string.IsNullOrWhiteSpace(option.IconName)
                    ? null
                    : EditorGUIUtility.IconContent(option.IconName)?.image;

                if (icon != null)
                    _contentIconsByType[prototype.Type] = icon;
            }
        }

        private List<Texture> BuildContentIcons(List<SceneContentData> content)
        {
            List<Texture> result = new List<Texture>();
            if (content == null)
                return result;

            for (int i = 0; i < content.Count; i++)
            {
                SceneContentData contentData = content[i];
                if (contentData == null)
                    continue;

                if (_contentIconsByType.TryGetValue(contentData.Type, out Texture icon))
                    result.Add(icon);
            }

            return result;
        }

        private void EnsureChoiceIconsMap()
        {
            if (_choiceIconsByType.Count > 0)
                return;

            ChoiceCreateOptionsRegistry registry = new ChoiceCreateOptionsRegistry();
            IReadOnlyList<CreateOptionDescriptor<ChoiceData>> options = registry.GetOptions();
            for (int i = 0; i < options.Count; i++)
            {
                CreateOptionDescriptor<ChoiceData> option = options[i];
                if (option == null)
                    continue;

                ChoiceData prototype = option.Create?.Invoke();
                if (prototype == null)
                    continue;

                Texture icon = string.IsNullOrWhiteSpace(option.IconName)
                    ? null
                    : EditorGUIUtility.IconContent(option.IconName)?.image;

                if (icon != null)
                    _choiceIconsByType[prototype.Type] = icon;
            }
        }

        private List<Texture> BuildChoiceIcons(List<ChoiceData> choices)
        {
            List<Texture> result = new List<Texture>();
            if (choices == null)
                return result;

            for (int i = 0; i < choices.Count; i++)
            {
                ChoiceData choiceData = choices[i];
                if (choiceData == null)
                    continue;

                if (_choiceIconsByType.TryGetValue(choiceData.Type, out Texture icon))
                    result.Add(icon);
            }

            return result;
        }

        private GUIStyle GetLegendStyle(Color color)
        {
            if (_legendStyle == null)
            {
                _legendStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    fontStyle = FontStyle.Bold,
                };
            }

            _legendStyle.normal.textColor = color;
            return _legendStyle;
        }

        private GUIStyle GetStatsStyle()
        {
            if (_statsStyle == null)
            {
                _statsStyle = new GUIStyle(EditorStyles.toolbarButton)
                {
                    alignment = TextAnchor.MiddleLeft,
                };
            }

            return _statsStyle;
        }

        private sealed class PreviewNode
        {
            public string Id;
            public string TagsLabel;
            public List<Texture> ContentIcons;
            public List<Texture> ChoiceIcons;
            public bool IsExistingScene;
            public bool IsBrokenTarget;
            public bool IsStart;
            public bool IsReachable;
        }

        private sealed class PreviewEdge
        {
            public string FromNodeId;
            public string ToNodeId;
        }
    }
}
