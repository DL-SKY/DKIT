using Modules.RPG.Scripts.Adventure.Data;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Modules.Definitions.Scripts.Editor.Adventures
{
    public sealed class AdventureValidationWindow : EditorWindow
    {
        private readonly AdventureValidationService _validationService = new AdventureValidationService();
        private AdventureData _adventureData;
        private Action _onDataChanged;
        private Vector2 _scroll;

        public static void Open(AdventureData adventureData, Action onDataChanged = null)
        {
            AdventureValidationWindow window = GetWindow<AdventureValidationWindow>();
            window.titleContent = new GUIContent("Adventure Validation");
            window.minSize = new Vector2(520f, 340f);
            window._adventureData = adventureData;
            window._onDataChanged = onDataChanged;
            window.Show();
            window.Focus();
        }

        private void OnGUI()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label("Validation", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(70f)))
                    Repaint();
            }

            if (_adventureData == null)
            {
                EditorGUILayout.HelpBox("No adventure selected.", MessageType.Info);
                return;
            }

            List<AdventureValidationIssue> issues = _validationService.ValidateDetailed(_adventureData);
            using (EditorGUILayout.ScrollViewScope scope = new EditorGUILayout.ScrollViewScope(_scroll))
            {
                _scroll = scope.scrollPosition;

                if (issues.Count == 0)
                {
                    EditorGUILayout.HelpBox("No validation errors.", MessageType.Info);
                    return;
                }

                for (int i = 0; i < issues.Count; i++)
                {
                    AdventureValidationIssue issue = issues[i];
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.HelpBox(issue.Message, MessageType.Warning);
                        if (issue.CanFix && GUILayout.Button("Fix", GUILayout.Width(64f), GUILayout.Height(38f)))
                        {
                            if (issue.ApplyFix())
                            {
                                _onDataChanged?.Invoke();
                                Repaint();
                            }
                        }
                    }
                }
            }
        }
    }
}
