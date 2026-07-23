using Modules.RPG.Scripts.Adventure.Choice;
using Modules.RPG.Scripts.Adventure.Choice.Actions;
using Modules.RPG.Scripts.Adventure.Data;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Modules.Definitions.Scripts.Editor.Adventures.CreateOptions;
using Modules.Definitions.Scripts.Implementation.Adventures.Constants;

namespace Modules.Definitions.Scripts.Editor.Adventures
{
    public sealed class AdventureGraphPreviewWindow : EditorWindow
    {
        private const float NODE_WIDTH = 220f;
        private const float NODE_TOP_BLOCK_HEIGHT = 58f;
        private const float CHOICE_ROW_HEIGHT = 18f;
        private const float LAYER_SPACING_X = 320f;
        private const float LAYER_SPACING_Y = 34f;
        private const float CANVAS_PADDING = 24f;
        private const float EDGE_WIDTH = 2f;
        private const int EDGE_SEGMENTS = 16;//32;
        private const float CONTENT_ICON_SIZE = 14f;
        private const float CONTENT_ICON_SPACING = 4f;
        private const float ICON_BOTTOM_MARGIN = 4f;
        private const float CHOICE_EXIT_OUTLINE = 2f;

        private static readonly Color SCENARIO_EXIT_COLOR = new Color(0.72f, 0.55f, 0.08f, 1f);
        private static readonly Color UNREACHABLE_COLOR = new Color(0.48f, 0.30f, 0.14f, 1f);

        private readonly Dictionary<string, PreviewNode> _nodes = new Dictionary<string, PreviewNode>(StringComparer.Ordinal);
        private readonly List<PreviewEdge> _edges = new List<PreviewEdge>();
        private readonly Dictionary<string, Vector2> _nodePositions = new Dictionary<string, Vector2>(StringComparer.Ordinal);
        private readonly Dictionary<string, Rect> _nodeScreenRects = new Dictionary<string, Rect>(StringComparer.Ordinal);
        private readonly Dictionary<SceneContentType, Texture> _contentIconsByType = new Dictionary<SceneContentType, Texture>();
        private readonly Dictionary<ChoiceType, Texture> _choiceIconsByType = new Dictionary<ChoiceType, Texture>();
        private readonly Dictionary<ChoiceActionType, Texture> _choiceActionIconsByType = new Dictionary<ChoiceActionType, Texture>();

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
        private GUIStyle _choiceIdStyle;
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

            RebuildNodeScreenRects();
            DrawEdges();
            DrawNodeBackgrounds();
            DrawNodeContent();
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
                GUILayout.Label("Unreachable", GetLegendStyle(UNREACHABLE_COLOR));
                GUILayout.Space(10f);
                GUILayout.Label("Broken Target", GetLegendStyle(new Color(0.9f, 0.3f, 0.3f, 1f)));
                GUILayout.Space(10f);
                GUILayout.Label("Scenario Exit", GetLegendStyle(SCENARIO_EXIT_COLOR));
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

                Vector2 from = GetEdgeStartPoint(fromNode, edge.FromChoiceIndex);
                Vector2 to = GetEdgeEndPoint(toNode);
                float tangentDistance = GetEdgeTangentDistance(from, to);
                Vector2 tangentA = from + Vector2.right * tangentDistance;
                Vector2 tangentB = to + Vector2.left * tangentDistance;

                Color edgeColor = toNode.IsBrokenTarget
                    ? new Color(0.9f, 0.3f, 0.3f, 0.95f)
                    : new Color(0.9f, 0.9f, 0.9f, 1f);

                DrawEdgeCurve(from, to, tangentA, tangentB, edgeColor);
                DrawArrowHead(to, (to - tangentB).normalized, edgeColor);
            }

            Handles.EndGUI();
        }

        private void DrawEdgeCurve(Vector2 from, Vector2 to, Vector2 tangentA, Vector2 tangentB, Color color)
        {
            Vector3[] points = new Vector3[EDGE_SEGMENTS + 1];
            for (int i = 0; i <= EDGE_SEGMENTS; i++)
            {
                float t = i / (float)EDGE_SEGMENTS;
                points[i] = GetBezierPoint(from, tangentA, tangentB, to, t);
            }

            Handles.color = color;
            Handles.DrawAAPolyLine(EDGE_WIDTH, points);
        }

        private static Vector2 GetBezierPoint(Vector2 start, Vector2 controlA, Vector2 controlB, Vector2 end, float t)
        {
            float inverseT = 1f - t;
            return inverseT * inverseT * inverseT * start
                + 3f * inverseT * inverseT * t * controlA
                + 3f * inverseT * t * t * controlB
                + t * t * t * end;
        }

        private float GetEdgeTangentDistance(Vector2 from, Vector2 to)
        {
            float horizontalGap = Mathf.Abs(to.x - from.x);
            if (horizontalGap <= 0.001f)
                return 24f * _zoom;

            return Mathf.Clamp(horizontalGap * 0.45f, 24f * _zoom, 70f * _zoom);
        }

        private Vector2 GetEdgeStartPoint(PreviewNode fromNode, int fromChoiceIndex)
        {
            if (fromNode.ChoiceRowScreenRects != null &&
                fromChoiceIndex >= 0 &&
                fromChoiceIndex < fromNode.ChoiceRowScreenRects.Count)
            {
                Rect rowRect = fromNode.ChoiceRowScreenRects[fromChoiceIndex];
                return new Vector2(rowRect.xMax, rowRect.center.y);
            }

            if (_nodeScreenRects.TryGetValue(fromNode.Id, out Rect nodeRect))
                return new Vector2(nodeRect.xMax, nodeRect.center.y);

            return _canvasRect.center;
        }

        private Vector2 GetEdgeEndPoint(PreviewNode toNode)
        {
            if (_nodeScreenRects.TryGetValue(toNode.Id, out Rect nodeRect))
                return new Vector2(nodeRect.xMin, nodeRect.center.y);

            return _canvasRect.center;
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

        private void DrawNodeBackgrounds()
        {
            foreach (KeyValuePair<string, PreviewNode> pair in _nodes)
            {
                PreviewNode node = pair.Value;
                if (!_nodeScreenRects.TryGetValue(node.Id, out Rect screenRect))
                    continue;

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
            }
        }

        private void DrawNodeContent()
        {
            foreach (KeyValuePair<string, PreviewNode> pair in _nodes)
            {
                PreviewNode node = pair.Value;
                if (!_nodeScreenRects.TryGetValue(node.Id, out Rect screenRect))
                    continue;

                Rect idRect = new Rect(screenRect.x + 6f, screenRect.y + 4f, screenRect.width - 12f, 20f);
                GUI.Label(idRect, node.Id, GetNodeLabelStyle());

                float topBlockBottomY = screenRect.y + NODE_TOP_BLOCK_HEIGHT * _zoom;
                Rect tagsRect = new Rect(screenRect.x + 6f, screenRect.y + 24f, screenRect.width - 12f, (topBlockBottomY - screenRect.y) - 30f);
                GUI.Label(tagsRect, node.TagsLabel, GetNodeTagsStyle());

                DrawNodeContentIcons(screenRect, node, topBlockBottomY);
                DrawChoiceRows(screenRect, node, topBlockBottomY);
            }
        }

        private void DrawNodeContentIcons(Rect screenRect, PreviewNode node, float topBlockBottomY)
        {
            if (node.ContentIcons == null || node.ContentIcons.Count == 0)
                return;

            float iconSize = CONTENT_ICON_SIZE * _zoom;
            float iconSpacing = CONTENT_ICON_SPACING * _zoom;
            float y = topBlockBottomY - iconSize - ICON_BOTTOM_MARGIN * _zoom;
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

        private void DrawChoiceRows(Rect screenRect, PreviewNode node, float topBlockBottomY)
        {
            if (node.ChoiceCount <= 0)
                return;

            for (int i = 0; i < node.ChoiceCount; i++)
            {
                if (i >= node.ChoiceRowScreenRects.Count)
                    break;

                Rect rowRect = node.ChoiceRowScreenRects[i];
                EditorGUI.DrawRect(rowRect, new Color(0f, 0f, 0f, 0.12f));
                EditorGUI.DrawRect(new Rect(rowRect.x, rowRect.y, rowRect.width, 1f), new Color(1f, 1f, 1f, 0.15f));
                EditorGUI.DrawRect(new Rect(rowRect.x, rowRect.yMax - 1f, rowRect.width, 1f), new Color(0f, 0f, 0f, 0.28f));

                bool exitsScenario = node.ChoiceExitsScenario != null
                    && i < node.ChoiceExitsScenario.Count
                    && node.ChoiceExitsScenario[i];
                if (exitsScenario)
                    DrawRectOutline(rowRect, SCENARIO_EXIT_COLOR, CHOICE_EXIT_OUTLINE);

                Texture icon = (node.ChoiceIcons != null && i < node.ChoiceIcons.Count) ? node.ChoiceIcons[i] : null;
                float iconSize = CONTENT_ICON_SIZE * _zoom;
                float iconSpacing = CONTENT_ICON_SPACING * _zoom;
                float margin = 6f;
                float iconX = rowRect.x + margin;
                float iconY = rowRect.center.y - iconSize * 0.5f;

                float textStartX = rowRect.x + margin;
                if (icon != null)
                {
                    GUI.DrawTexture(new Rect(iconX, iconY, iconSize, iconSize), icon, ScaleMode.ScaleToFit, true);
                    textStartX = iconX + iconSize + margin;
                }

                List<Texture> actionIcons = (node.ChoiceActionIconsPerRow != null && i < node.ChoiceActionIconsPerRow.Count)
                    ? node.ChoiceActionIconsPerRow[i]
                    : null;
                float actionIconsWidth = GetActionIconsRowWidth(actionIcons, iconSize, iconSpacing, margin);

                string choiceId = (node.ChoiceIds != null && i < node.ChoiceIds.Count)
                    ? node.ChoiceIds[i]
                    : string.Empty;
                if (!string.IsNullOrWhiteSpace(choiceId))
                {
                    float textWidth = rowRect.xMax - textStartX - actionIconsWidth - margin;
                    Rect textRect = new Rect(textStartX, rowRect.y + 1f, Mathf.Max(0f, textWidth), rowRect.height - 2f);
                    GUI.Label(textRect, choiceId, GetChoiceIdStyle());
                }

                DrawChoiceActionIcons(rowRect, actionIcons, iconSize, iconSpacing, margin);
            }
        }

        private static float GetActionIconsRowWidth(List<Texture> actionIcons, float iconSize, float iconSpacing, float margin)
        {
            if (actionIcons == null || actionIcons.Count == 0)
                return 0f;

            int visibleCount = 0;
            for (int i = 0; i < actionIcons.Count; i++)
            {
                if (actionIcons[i] != null)
                    visibleCount++;
            }

            if (visibleCount == 0)
                return 0f;

            return visibleCount * iconSize + (visibleCount - 1) * iconSpacing + margin;
        }

        private static void DrawChoiceActionIcons(Rect rowRect, List<Texture> actionIcons, float iconSize, float iconSpacing, float margin)
        {
            if (actionIcons == null || actionIcons.Count == 0)
                return;

            float x = rowRect.xMax - margin;
            float iconY = rowRect.center.y - iconSize * 0.5f;

            for (int i = actionIcons.Count - 1; i >= 0; i--)
            {
                Texture actionIcon = actionIcons[i];
                if (actionIcon == null)
                    continue;

                x -= iconSize;
                GUI.DrawTexture(new Rect(x, iconY, iconSize, iconSize), actionIcon, ScaleMode.ScaleToFit, true);
                x -= iconSpacing;
            }
        }

        private Color GetNodeColor(PreviewNode node)
        {
            if (node.IsBrokenTarget)
                return new Color(0.64f, 0.22f, 0.22f, 0.96f);
            if (node.IsStart)
                return new Color(0.19f, 0.42f, 0.66f, 0.96f);
            if (!node.IsReachable)
                return new Color(UNREACHABLE_COLOR.r, UNREACHABLE_COLOR.g, UNREACHABLE_COLOR.b, 0.96f);

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
            EnsureChoiceActionIconsMap();

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
                    ChoiceActionIconsPerRow = BuildChoiceActionIcons(sceneData?.Choices),
                    ChoiceIds = BuildChoiceIds(sceneData?.Choices),
                    ChoiceExitsScenario = BuildChoiceExitsScenario(sceneData?.Choices),
                    ChoiceCount = sceneData?.Choices?.Count ?? 0,
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
                        if (action == null || !AdventureGraphBuilder.IsSceneTransitionAction(action))
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
                                ChoiceActionIconsPerRow = new List<List<Texture>>(),
                                ChoiceIds = new List<string>(),
                                ChoiceExitsScenario = new List<bool>(),
                                ChoiceCount = 0,
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
                            FromChoiceIndex = choiceIndex,
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

            float leftY = CANVAS_PADDING;
            for (int i = 0; i < leftColumnNodes.Count; i++)
            {
                string nodeId = leftColumnNodes[i];
                _nodePositions[nodeId] = new Vector2(leftColumnX, leftY);
                if (_nodes.TryGetValue(leftColumnNodes[i], out PreviewNode node))
                    leftY += node.NodeHeight + LAYER_SPACING_Y;
                else
                    leftY += CHOICE_ROW_HEIGHT + LAYER_SPACING_Y;
            }

            for (int k = 0; k < depthKeys.Count; k++)
            {
                List<string> layerNodes = normalLayers[depthKeys[k]];
                layerNodes.Sort(StringComparer.Ordinal);

                int columnIndex = hasLeftColumn ? depthKeys[k] + 1 : depthKeys[k];
                float x = CANVAS_PADDING + columnIndex * LAYER_SPACING_X;
                float y = CANVAS_PADDING;
                for (int i = 0; i < layerNodes.Count; i++)
                {
                    string nodeId = layerNodes[i];
                    _nodePositions[nodeId] = new Vector2(x, y);
                    if (_nodes.TryGetValue(nodeId, out PreviewNode node))
                        y += node.NodeHeight + LAYER_SPACING_Y;
                    else
                        y += CHOICE_ROW_HEIGHT + LAYER_SPACING_Y;
                }
            }
        }

        private void RebuildNodeScreenRects()
        {
            _nodeScreenRects.Clear();

            foreach (KeyValuePair<string, PreviewNode> pair in _nodes)
            {
                PreviewNode node = pair.Value;
                if (!_nodePositions.TryGetValue(node.Id, out Vector2 worldPos))
                    continue;

                Rect worldRect = new Rect(worldPos.x, worldPos.y, NODE_WIDTH, node.NodeHeight);
                Rect screenRect = ToScreenRect(worldRect);
                _nodeScreenRects[node.Id] = screenRect;

                node.ChoiceRowScreenRects.Clear();
                if (node.ChoiceCount <= 0)
                    continue;

                float rowHeight = CHOICE_ROW_HEIGHT * _zoom;
                float firstRowY = screenRect.y + NODE_TOP_BLOCK_HEIGHT * _zoom;
                for (int i = 0; i < node.ChoiceCount; i++)
                {
                    Rect rowRect = new Rect(screenRect.x, firstRowY + i * rowHeight, screenRect.width, rowHeight);
                    node.ChoiceRowScreenRects.Add(rowRect);
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
                    wordWrap = false,
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

                Texture icon = option.ResolveIcon();

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

                Texture icon = option.ResolveIcon();

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
                {
                    result.Add(null);
                    continue;
                }

                _choiceIconsByType.TryGetValue(choiceData.Type, out Texture icon);
                result.Add(icon);
            }

            return result;
        }

        private void EnsureChoiceActionIconsMap()
        {
            if (_choiceActionIconsByType.Count > 0)
                return;

            ChoiceActionCreateOptionsRegistry registry = new ChoiceActionCreateOptionsRegistry();
            IReadOnlyList<CreateOptionDescriptor<ChoiceActionData>> options = registry.GetOptions();
            for (int i = 0; i < options.Count; i++)
            {
                CreateOptionDescriptor<ChoiceActionData> option = options[i];
                if (option == null)
                    continue;

                ChoiceActionData prototype = option.Create?.Invoke();
                if (prototype == null)
                    continue;

                Texture icon = option.ResolveIcon();
                if (icon != null)
                    _choiceActionIconsByType[prototype.Type] = icon;
            }
        }

        private List<List<Texture>> BuildChoiceActionIcons(List<ChoiceData> choices)
        {
            List<List<Texture>> result = new List<List<Texture>>();
            if (choices == null)
                return result;

            for (int i = 0; i < choices.Count; i++)
            {
                ChoiceData choiceData = choices[i];
                List<Texture> rowIcons = new List<Texture>();
                if (choiceData?.Actions != null)
                {
                    for (int j = 0; j < choiceData.Actions.Count; j++)
                    {
                        ChoiceActionData action = choiceData.Actions[j];
                        if (action == null)
                        {
                            rowIcons.Add(null);
                            continue;
                        }

                        rowIcons.Add(ResolveChoiceActionIcon(action));
                    }
                }

                result.Add(rowIcons);
            }

            return result;
        }

        private Texture ResolveChoiceActionIcon(ChoiceActionData action)
        {
            if (action == null)
                return null;

            if (action.Type == ChoiceActionType.OpenWindow)
                return ResolveOpenWindowIcon(action);

            _choiceActionIconsByType.TryGetValue(action.Type, out Texture icon);
            return icon;
        }

        private static Texture ResolveOpenWindowIcon(ChoiceActionData action)
        {
            string windowId = null;
            action.Params?.Strings?.TryGetValue(Glossary.ChoiceActions.WINDOW_ID, out windowId);

            string iconAssetName = GetOpenWindowIconAssetName(windowId);
            Texture icon = AdventureEditorButtonIcons.Resolve(null, iconAssetName);
            if (icon != null)
                return icon;

            return AdventureEditorButtonIcons.Resolve(null, "WindowDefault");
        }

        private static string GetOpenWindowIconAssetName(string windowId)
        {
            if (string.Equals(windowId, Glossary.Windows.TRADE, StringComparison.Ordinal))
                return "WindowTrade";

            if (string.Equals(windowId, Glossary.Windows.CREATE_CHARACTER, StringComparison.Ordinal)
                || string.Equals(windowId, Glossary.Windows.SELECT_CHARACTER, StringComparison.Ordinal))
                return "WindowCharacter";

            if (string.Equals(windowId, Glossary.Windows.PARTY, StringComparison.Ordinal))
                return "WindowParty";

            if (string.Equals(windowId, Glossary.Windows.ADVENTURE_LIST, StringComparison.Ordinal))
                return "WindowBoard";

            return "WindowDefault";
        }

        private static List<string> BuildChoiceIds(List<ChoiceData> choices)
        {
            List<string> result = new List<string>();
            if (choices == null)
                return result;

            for (int i = 0; i < choices.Count; i++)
            {
                ChoiceData choice = choices[i];
                string id = choice?.Id;
                if (string.IsNullOrWhiteSpace(id))
                    id = $"choice_{i}";
                result.Add(id);
            }

            return result;
        }

        private static List<bool> BuildChoiceExitsScenario(List<ChoiceData> choices)
        {
            List<bool> result = new List<bool>();
            if (choices == null)
                return result;

            for (int i = 0; i < choices.Count; i++)
                result.Add(ChoiceLeadsToScenarioExit(choices[i]));

            return result;
        }

        private static bool ChoiceLeadsToScenarioExit(ChoiceData choice)
        {
            if (choice == null)
                return false;

            if (HasScenarioExitAction(choice.Actions))
                return true;

            ChoiceDiceCheckData diceCheck = choice.DiceCheck;
            if (diceCheck == null)
                return false;

            return HasScenarioExitAction(diceCheck.OnCriticalSuccess)
                || HasScenarioExitAction(diceCheck.OnSuccess)
                || HasScenarioExitAction(diceCheck.OnFailure)
                || HasScenarioExitAction(diceCheck.OnCriticalFailure);
        }

        private static bool HasScenarioExitAction(List<ChoiceActionData> actions)
        {
            if (actions == null)
                return false;

            for (int i = 0; i < actions.Count; i++)
            {
                if (IsScenarioExitAction(actions[i]))
                    return true;
            }

            return false;
        }

        private static bool IsScenarioExitAction(ChoiceActionData action)
        {
            if (action == null)
                return false;

            if (action.Type == ChoiceActionType.GoToAdventure
                || action.Type == ChoiceActionType.GoToRandomAdventure)
                return true;

            if (action.Type != ChoiceActionType.OpenWindow)
                return false;

            if (action.Params?.Strings == null)
                return false;

            return action.Params.Strings.TryGetValue(Glossary.ChoiceActions.WINDOW_ID, out string windowId)
                && string.Equals(windowId, Glossary.Windows.ADVENTURE_LIST, StringComparison.Ordinal);
        }

        private static void DrawRectOutline(Rect rect, Color color, float thickness)
        {
            EditorGUI.DrawRect(new Rect(rect.x - thickness, rect.y - thickness, rect.width + thickness * 2f, thickness), color);
            EditorGUI.DrawRect(new Rect(rect.x - thickness, rect.yMax, rect.width + thickness * 2f, thickness), color);
            EditorGUI.DrawRect(new Rect(rect.x - thickness, rect.y, thickness, rect.height), color);
            EditorGUI.DrawRect(new Rect(rect.xMax, rect.y, thickness, rect.height), color);
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

        private GUIStyle GetChoiceIdStyle()
        {
            if (_choiceIdStyle == null)
            {
                _choiceIdStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    alignment = TextAnchor.MiddleLeft,
                    clipping = TextClipping.Clip,
                    fontSize = 10,
                };
                _choiceIdStyle.normal.textColor = new Color(0.93f, 0.93f, 0.93f, 0.95f);
            }

            return _choiceIdStyle;
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
            public List<List<Texture>> ChoiceActionIconsPerRow;
            public List<string> ChoiceIds;
            public List<bool> ChoiceExitsScenario;
            public int ChoiceCount;
            public List<Rect> ChoiceRowScreenRects = new List<Rect>();
            public bool IsExistingScene;
            public bool IsBrokenTarget;
            public bool IsStart;
            public bool IsReachable;

            public float NodeHeight => NODE_TOP_BLOCK_HEIGHT + CHOICE_ROW_HEIGHT * Mathf.Max(0, ChoiceCount);
        }

        private sealed class PreviewEdge
        {
            public string FromNodeId;
            public string ToNodeId;
            public int FromChoiceIndex;
        }
    }
}
