using Modules.Definitions.Scripts.Editor.Adventures.CreateOptions;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Modules.Definitions.Scripts.Editor.Adventures
{
    public sealed class CreateOptionPickerWindow : EditorWindow
    {
        private readonly List<OptionEntry> _entries = new List<OptionEntry>();
        private Vector2 _scroll;

        public static void Open<T>(
            string title,
            IReadOnlyList<CreateOptionDescriptor<T>> options,
            Action<CreateOptionDescriptor<T>> onSelected)
        {
            CreateOptionPickerWindow window = CreateInstance<CreateOptionPickerWindow>();
            window.titleContent = new GUIContent(string.IsNullOrWhiteSpace(title) ? "Create Option" : title);
            window.minSize = new Vector2(320f, 180f);
            window.maxSize = new Vector2(420f, 700f);

            window._entries.Clear();
            if (options != null)
            {
                for (int i = 0; i < options.Count; i++)
                {
                    CreateOptionDescriptor<T> option = options[i];
                    if (option == null)
                        continue;

                    window._entries.Add(new OptionEntry
                    {
                        Content = option.ToGuiContent(),
                        OnClick = () =>
                        {
                            onSelected?.Invoke(option);
                            window.Close();
                        },
                    });
                }
            }

            window.ShowUtility();
            window.Focus();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Choose template", EditorStyles.boldLabel);
            EditorGUILayout.Space(4f);

            if (_entries.Count == 0)
            {
                EditorGUILayout.HelpBox("No options available.", MessageType.Info);
                return;
            }

            using (EditorGUILayout.ScrollViewScope scope = new EditorGUILayout.ScrollViewScope(_scroll))
            {
                _scroll = scope.scrollPosition;

                for (int i = 0; i < _entries.Count; i++)
                {
                    OptionEntry entry = _entries[i];
                    if (GUILayout.Button(entry.Content, GUILayout.Height(26f)))
                        entry.OnClick?.Invoke();
                }
            }
        }

        private sealed class OptionEntry
        {
            public GUIContent Content;
            public Action OnClick;
        }
    }
}
