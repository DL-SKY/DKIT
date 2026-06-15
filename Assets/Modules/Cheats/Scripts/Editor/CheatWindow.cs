using Modules.Cheats.Scripts.Editor.Core;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Modules.Cheats.Scripts.Editor
{
    public sealed class CheatWindow : EditorWindow
    {
        private const string MENU_PATH = "Tools/Cheats";
        private const string FOLDOUT_KEY_PREFIX = "Modules.Cheats.Editor.CheatWindow.Foldout.";

        private GUIStyle _captionStyle;
        private GUIStyle _foldoutStyle;

        private List<CheatSectionBase> _sections = new List<CheatSectionBase>();
        private Vector2 _scrollPosition;
        private string _filter = string.Empty;

        [MenuItem(MENU_PATH)]
        public static CheatWindow Open()
        {
            CheatWindow window = GetWindow<CheatWindow>();
            window.titleContent = new GUIContent("Cheats");
            window.minSize = new Vector2(420f, 320f);
            window.LoadSections();
            return window;
        }

        private void OnEnable()
        {
            LoadSections();
        }

        private void OnGUI()
        {
            EnsureSectionsInitialized();
            EnsureStylesInitialized();
            DrawToolbar();
            EditorGUILayout.Space(4f);

            using (EditorGUILayout.ScrollViewScope scope = new EditorGUILayout.ScrollViewScope(_scrollPosition))
            {
                _scrollPosition = scope.scrollPosition;

                bool hasVisibleSections = false;
                for (int i = 0; i < _sections.Count; i++)
                {
                    CheatSectionBase section = _sections[i];
                    if (section == null)
                    {
                        continue;
                    }

                    if (!section.IsVisible(_filter))
                    {
                        continue;
                    }

                    hasVisibleSections = true;
                    DrawSection(section);
                    EditorGUILayout.Space(6f);
                }

                if (!hasVisibleSections)
                {
                    EditorGUILayout.HelpBox("No sections match current filter.", MessageType.Info);
                }
            }
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Filter", GUILayout.Width(36f));
                _filter = EditorGUILayout.TextField(_filter);

                if (GUILayout.Button("Reload", GUILayout.Width(72f)))
                {
                    LoadSections();
                }
            }

            EditorGUILayout.LabelField("Sections: " + _sections.Count, EditorStyles.miniLabel);
        }

        private void DrawSection(CheatSectionBase section)
        {
            EditorGUILayout.LabelField(string.Empty, GUI.skin.horizontalSlider);

            using (new EditorGUILayout.HorizontalScope())
            {
                string key = FOLDOUT_KEY_PREFIX + section.Id;
                bool isOpen = EditorPrefs.GetBool(key, false);
                bool nextIsOpen = EditorGUILayout.Foldout(isOpen, GUIContent.none, true, _foldoutStyle);
                if (nextIsOpen != isOpen)
                {
                    EditorPrefs.SetBool(key, nextIsOpen);
                    isOpen = nextIsOpen;
                }

                EditorGUILayout.LabelField(section.Id, _captionStyle);
            }

            if (EditorPrefs.GetBool(FOLDOUT_KEY_PREFIX + section.Id, false))
            {
                EditorGUI.indentLevel++;
                section.DrawContent();
                EditorGUI.indentLevel--;
            }
        }

        private void LoadSections()
        {
            _sections = CheatSectionsProvider.CreateSections() ?? new List<CheatSectionBase>();
            Repaint();
        }

        private void EnsureSectionsInitialized()
        {
            if (_sections == null)
            {
                _sections = new List<CheatSectionBase>();
                LoadSections();
            }
        }

        private void EnsureStylesInitialized()
        {
            if (_captionStyle == null)
            {
                GUIStyle source = EditorStyles.boldLabel ?? GUI.skin.label;
                _captionStyle = new GUIStyle(source);
                _captionStyle.alignment = TextAnchor.MiddleLeft;
                _captionStyle.fontSize = 12;
            }

            if (_foldoutStyle == null)
            {
                GUIStyle source = EditorStyles.foldout ?? GUI.skin.toggle;
                _foldoutStyle = new GUIStyle(source);
                _foldoutStyle.fixedWidth = 12f;
            }
        }
    }
}
