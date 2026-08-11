using Modules.Definitions.Scripts.Implementation.Adventures.Constants;
using Modules.State.Scripts.Implementation.Adventure.StateDatas;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Modules.Definitions.Scripts.Editor.Adventures
{
    public static class AdventureDefEditorGui
    {
        /// <summary>
        /// Pregen defaults: Level / Experience / BoostPoints / abilities.
        /// Totals for Perception / skills / saves come from formulas (ability + ProfRank + ItemsBonus),
        /// so authored raw totals are not seeded here.
        /// </summary>
        private static readonly KeyValuePair<string, int>[] DEFAULT_PREGENERATED_CHARACTER_PARAMETERS =
        {
            new KeyValuePair<string, int>(Glossary.Characters.LEVEL, 1),
            new KeyValuePair<string, int>(Glossary.Characters.EXPERIENCE, 0),
            new KeyValuePair<string, int>(Glossary.Characters.BOOST_POINTS, 0),
            new KeyValuePair<string, int>(Glossary.Characters.STR, 0),
            new KeyValuePair<string, int>(Glossary.Characters.DEX, 0),
            new KeyValuePair<string, int>(Glossary.Characters.CON, 0),
            new KeyValuePair<string, int>(Glossary.Characters.INT, 0),
            new KeyValuePair<string, int>(Glossary.Characters.WIS, 0),
            new KeyValuePair<string, int>(Glossary.Characters.CHA, 0),
        };

        /// <summary>
        /// Creature defaults: Level / abilities / Perception / saves as authored totals
        /// (baked into *.ItemsBonus by CreatureCombatantFactory at spawn).
        /// Skills are not seeded — add only those needed for the stat block.
        /// </summary>
        private static readonly KeyValuePair<string, int>[] DEFAULT_CREATURE_PARAMETERS =
        {
            new KeyValuePair<string, int>(Glossary.Characters.LEVEL, 1),
            new KeyValuePair<string, int>(Glossary.Characters.STR, 0),
            new KeyValuePair<string, int>(Glossary.Characters.DEX, 0),
            new KeyValuePair<string, int>(Glossary.Characters.CON, 0),
            new KeyValuePair<string, int>(Glossary.Characters.INT, 0),
            new KeyValuePair<string, int>(Glossary.Characters.WIS, 0),
            new KeyValuePair<string, int>(Glossary.Characters.CHA, 0),
            new KeyValuePair<string, int>(Glossary.Characters.PERCEPTION, 0),
            new KeyValuePair<string, int>(Glossary.Characters.SAVING_FORTITUDE, 0),
            new KeyValuePair<string, int>(Glossary.Characters.SAVING_REFLEX, 0),
            new KeyValuePair<string, int>(Glossary.Characters.SAVING_WILL, 0),
        };

        /// <summary>
        /// Adds missing pregen Parameters without clearing or overwriting existing keys.
        /// </summary>
        public static bool EnsureDefaultPregeneratedCharacterParameters(Dictionary<string, int> parameters)
        {
            return EnsureDefaultParameters(parameters, DEFAULT_PREGENERATED_CHARACTER_PARAMETERS);
        }

        /// <summary>
        /// Adds missing creature Parameters without clearing or overwriting existing keys.
        /// </summary>
        public static bool EnsureDefaultCreatureParameters(Dictionary<string, int> parameters)
        {
            return EnsureDefaultParameters(parameters, DEFAULT_CREATURE_PARAMETERS);
        }

        /// <summary>
        /// Creates a Parameters dictionary with all pregen default keys filled in.
        /// </summary>
        public static Dictionary<string, int> CreateDefaultPregeneratedCharacterParameters()
        {
            var parameters = new Dictionary<string, int>(DEFAULT_PREGENERATED_CHARACTER_PARAMETERS.Length);
            EnsureDefaultPregeneratedCharacterParameters(parameters);
            return parameters;
        }

        /// <summary>
        /// Creates a Parameters dictionary with all creature default keys filled in.
        /// </summary>
        public static Dictionary<string, int> CreateDefaultCreatureParameters()
        {
            var parameters = new Dictionary<string, int>(DEFAULT_CREATURE_PARAMETERS.Length);
            EnsureDefaultCreatureParameters(parameters);
            return parameters;
        }

        private static bool EnsureDefaultParameters(
            Dictionary<string, int> parameters,
            KeyValuePair<string, int>[] defaults)
        {
            if (parameters == null || defaults == null)
                return false;

            bool changed = false;
            for (int i = 0; i < defaults.Length; i++)
            {
                KeyValuePair<string, int> entry = defaults[i];
                if (parameters.ContainsKey(entry.Key))
                    continue;

                parameters[entry.Key] = entry.Value;
                changed = true;
            }

            return changed;
        }

        public static List<string> ParseCsv(string csv)
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

        public static string JoinCsv(List<string> values)
        {
            if (values == null || values.Count == 0)
                return string.Empty;

            return string.Join(", ", values);
        }

        public static void DrawStringListEditor(
            List<string> list,
            string sectionTitle,
            Action markDirty,
            string addButtonText = "Add")
        {
            list ??= new List<string>();
            EditorGUILayout.LabelField(sectionTitle, EditorStyles.boldLabel);

            for (int i = 0; i < list.Count; i++)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    string current = list[i] ?? string.Empty;
                    string next = EditorGUILayout.TextField($"[{i}]", current);
                    if (!string.Equals(next, current, StringComparison.Ordinal))
                    {
                        list[i] = next;
                        markDirty?.Invoke();
                    }

                    if (GUILayout.Button("X", GUILayout.Width(22f)))
                    {
                        list.RemoveAt(i);
                        markDirty?.Invoke();
                        break;
                    }
                }
            }

            if (GUILayout.Button(addButtonText, GUILayout.Width(120f)))
            {
                list.Add(string.Empty);
                markDirty?.Invoke();
            }
        }

        public static void DrawIntDictionaryEditor(
            Dictionary<string, int> dictionary,
            string sectionTitle,
            Action markDirty)
        {
            dictionary ??= new Dictionary<string, int>();
            EditorGUILayout.LabelField(sectionTitle, EditorStyles.boldLabel);
            if (GUILayout.Button($"Add {sectionTitle}", GUILayout.Width(150f)))
            {
                string newKey = BuildUniqueDictionaryKey(dictionary, "key");
                dictionary[newKey] = 0;
                markDirty?.Invoke();
            }

            List<string> keys = new List<string>(dictionary.Keys);
            for (int i = 0; i < keys.Count; i++)
            {
                string key = keys[i];
                if (!dictionary.TryGetValue(key, out int value))
                    continue;

                using (new EditorGUILayout.HorizontalScope())
                {
                    string nextKey = EditorGUILayout.TextField(key, GUILayout.Width(240f));
                    int nextValue = EditorGUILayout.IntField(value);

                    if (GUILayout.Button("X", GUILayout.Width(22f)))
                    {
                        dictionary.Remove(key);
                        markDirty?.Invoke();
                        continue;
                    }

                    if (!string.Equals(nextKey, key, StringComparison.Ordinal))
                    {
                        string uniqueKey = BuildUniqueDictionaryKey(dictionary, nextKey, key);
                        dictionary.Remove(key);
                        dictionary[uniqueKey] = nextValue;
                        markDirty?.Invoke();
                        continue;
                    }

                    if (nextValue != value)
                    {
                        dictionary[key] = nextValue;
                        markDirty?.Invoke();
                    }
                }
            }
        }

        public static void DrawEquippedItemsEditor(
            List<EquippedItemStateData> equippedItems,
            Action markDirty,
            bool showManualAddButton = true)
        {
            equippedItems ??= new List<EquippedItemStateData>();
            EditorGUILayout.LabelField("Equipped Items", EditorStyles.boldLabel);

            for (int i = 0; i < equippedItems.Count; i++)
            {
                equippedItems[i] ??= new EquippedItemStateData();
                EquippedItemStateData item = equippedItems[i];

                using (new EditorGUILayout.HorizontalScope())
                {
                    string nextSlot = EditorGUILayout.TextField(item.Slot ?? string.Empty, GUILayout.Width(180f));
                    string nextItemId = EditorGUILayout.TextField(item.ItemId ?? string.Empty);

                    if (!string.Equals(nextSlot, item.Slot, StringComparison.Ordinal)
                        || !string.Equals(nextItemId, item.ItemId, StringComparison.Ordinal))
                    {
                        item.Slot = nextSlot;
                        item.ItemId = nextItemId;
                        markDirty?.Invoke();
                    }

                    if (GUILayout.Button("X", GUILayout.Width(22f)))
                    {
                        equippedItems.RemoveAt(i);
                        markDirty?.Invoke();
                        break;
                    }
                }
            }

            if (showManualAddButton && GUILayout.Button("Add Equipped Item", GUILayout.Width(160f)))
            {
                equippedItems.Add(new EquippedItemStateData());
                markDirty?.Invoke();
            }
        }

        public static string DrawStringPopupWithFallback(
            string label,
            string currentValue,
            List<string> options,
            string emptyLabel = "<none>")
        {
            options ??= new List<string>();
            List<string> popupOptions = new List<string>();
            popupOptions.Add(emptyLabel);
            popupOptions.AddRange(options);

            string normalized = currentValue ?? string.Empty;
            int selectedIndex = 0;
            for (int i = 0; i < popupOptions.Count; i++)
            {
                if (string.Equals(popupOptions[i], normalized, StringComparison.Ordinal))
                {
                    selectedIndex = i;
                    break;
                }
            }

            if (selectedIndex == 0 && !string.IsNullOrWhiteSpace(normalized))
            {
                popupOptions.Insert(1, normalized);
                selectedIndex = 1;
            }

            int nextIndex = EditorGUILayout.Popup(label, selectedIndex, popupOptions.ToArray());
            string selected = popupOptions[nextIndex];
            return string.Equals(selected, emptyLabel, StringComparison.Ordinal) ? string.Empty : selected;
        }

        private static string BuildUniqueDictionaryKey(
            Dictionary<string, int> dictionary,
            string requestedKey,
            string ignoredExistingKey = null)
        {
            string keyBase = string.IsNullOrWhiteSpace(requestedKey) ? "key" : requestedKey.Trim();
            if (!dictionary.ContainsKey(keyBase)
                || string.Equals(keyBase, ignoredExistingKey, StringComparison.Ordinal))
            {
                return keyBase;
            }

            int suffix = 1;
            while (true)
            {
                string candidate = $"{keyBase}_{suffix}";
                if (!dictionary.ContainsKey(candidate)
                    || string.Equals(candidate, ignoredExistingKey, StringComparison.Ordinal))
                {
                    return candidate;
                }

                suffix++;
            }
        }
    }
}
