using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Modules.Definitions.Scripts.Editor.Adventures
{
    public sealed class DefinitionSearchPickerWindow : EditorWindow
    {
        private readonly List<Entry> _entries = new List<Entry>();
        private readonly List<int> _filteredIndices = new List<int>();

        private Action<string> _onSelected;
        private Vector2 _scroll;
        private string _search = string.Empty;

        public static void Open(
            string title,
            IReadOnlyList<Entry> entries,
            Action<string> onSelected)
        {
            DefinitionSearchPickerWindow window = CreateInstance<DefinitionSearchPickerWindow>();
            window.titleContent = new GUIContent(string.IsNullOrWhiteSpace(title) ? "Select Definition" : title);
            window.minSize = new Vector2(420f, 360f);
            window.maxSize = new Vector2(620f, 840f);
            window._onSelected = onSelected;

            window._entries.Clear();
            if (entries != null)
            {
                for (int i = 0; i < entries.Count; i++)
                {
                    Entry entry = entries[i];
                    if (entry == null || string.IsNullOrWhiteSpace(entry.Id))
                        continue;

                    window._entries.Add(entry);
                }
            }

            window.RebuildFilteredIndices();
            window.ShowUtility();
            window.Focus();
        }

        public static Entry BuildEntry(string id, string displayName = null, string secondaryText = null)
        {
            return new Entry
            {
                Id = id ?? string.Empty,
                DisplayName = string.IsNullOrWhiteSpace(displayName) ? id ?? string.Empty : displayName,
                SecondaryText = secondaryText ?? string.Empty,
            };
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Search", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            _search = EditorGUILayout.TextField(_search ?? string.Empty);
            if (EditorGUI.EndChangeCheck())
                RebuildFilteredIndices();

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField($"Found: {_filteredIndices.Count}", EditorStyles.miniLabel);
            EditorGUILayout.Space(4f);

            if (_filteredIndices.Count == 0)
            {
                EditorGUILayout.HelpBox("No definitions found for the current filter.", MessageType.Info);
                return;
            }

            using (EditorGUILayout.ScrollViewScope scope = new EditorGUILayout.ScrollViewScope(_scroll))
            {
                _scroll = scope.scrollPosition;
                for (int i = 0; i < _filteredIndices.Count; i++)
                {
                    Entry entry = _entries[_filteredIndices[i]];
                    DrawEntry(entry);
                }
            }
        }

        private void DrawEntry(Entry entry)
        {
            if (entry == null)
                return;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                if (GUILayout.Button(entry.DisplayName ?? entry.Id, GUILayout.Height(24f)))
                {
                    _onSelected?.Invoke(entry.Id);
                    Close();
                    return;
                }

                EditorGUILayout.LabelField(entry.Id ?? string.Empty, EditorStyles.miniBoldLabel);
                if (!string.IsNullOrWhiteSpace(entry.SecondaryText))
                    EditorGUILayout.LabelField(entry.SecondaryText, EditorStyles.miniLabel);
            }
        }

        private void RebuildFilteredIndices()
        {
            _filteredIndices.Clear();
            string query = (_search ?? string.Empty).Trim();
            bool hasQuery = !string.IsNullOrWhiteSpace(query);

            if (!hasQuery)
            {
                for (int i = 0; i < _entries.Count; i++)
                {
                    if (_entries[i] != null)
                        _filteredIndices.Add(i);
                }

                return;
            }

            for (int i = 0; i < _entries.Count; i++)
            {
                Entry entry = _entries[i];
                if (entry == null)
                    continue;

                if (MatchesQuery(entry, query))
                    _filteredIndices.Add(i);
            }
        }

        private static bool MatchesQuery(Entry entry, string query)
        {
            StringComparison comparison = StringComparison.OrdinalIgnoreCase;
            return (entry.Id ?? string.Empty).IndexOf(query, comparison) >= 0
                || (entry.DisplayName ?? string.Empty).IndexOf(query, comparison) >= 0
                || (entry.SecondaryText ?? string.Empty).IndexOf(query, comparison) >= 0;
        }

        public sealed class Entry
        {
            public string Id;
            public string DisplayName;
            public string SecondaryText;
        }
    }
}
