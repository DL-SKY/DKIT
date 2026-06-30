using Modules.RPG.Scripts.Adventure.Data;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Modules.Definitions.Scripts.Editor.Adventures
{
    public sealed class AdventureValidationWindow : EditorWindow
    {
        private readonly AdventureValidationService _validationService = new AdventureValidationService();
        private AdventureData _adventureData;
        private Vector2 _scroll;

        public static void Open(AdventureData adventureData)
        {
            AdventureValidationWindow window = GetWindow<AdventureValidationWindow>();
            window.titleContent = new GUIContent("Adventure Validation");
            window.minSize = new Vector2(520f, 340f);
            window._adventureData = adventureData;
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

            List<string> errors = _validationService.Validate(_adventureData);
            using (EditorGUILayout.ScrollViewScope scope = new EditorGUILayout.ScrollViewScope(_scroll))
            {
                _scroll = scope.scrollPosition;

                if (errors.Count == 0)
                {
                    EditorGUILayout.HelpBox("No validation errors.", MessageType.Info);
                    return;
                }

                for (int i = 0; i < errors.Count; i++)
                    EditorGUILayout.HelpBox(errors[i], MessageType.Warning);
            }
        }
    }
}
