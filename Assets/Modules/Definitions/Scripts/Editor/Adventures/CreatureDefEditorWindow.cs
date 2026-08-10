using Modules.Definitions.Scripts.Implementation.Adventures;
using Modules.Definitions.Scripts.Implementation.Adventures.Defs.Ancestries;
using Modules.Definitions.Scripts.Implementation.Adventures.Defs.Creatures;
using Modules.Definitions.Scripts.Implementation.Adventures.Defs.Feats;
using Modules.Definitions.Scripts.Implementation.Adventures.Defs.Items;
using Modules.Definitions.Scripts.Implementation.Adventures.Defs.Spells;
using Modules.State.Scripts.Implementation.Adventure;
using Modules.State.Scripts.Implementation.Adventure.Actions.Models;
using Modules.State.Scripts.Implementation.Adventure.StateDatas;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Modules.Definitions.Scripts.Editor.Adventures
{
    public sealed class CreatureDefEditorWindow : AdventureDefEditorWindowBase<CreatureDef>
    {
        private const string MENU_PATH = "Tools/Definitions/Adventures/Creature Editor";
        private const string DEFINITIONS_DIRECTORY = "Assets/Modules/Definitions/Resources/Definitions/_ADVENTURES_/Creatures";
        private const string CLASSES_DIRECTORY = "Assets/Modules/Definitions/Resources/Definitions/_ADVENTURES_/Classes";
        private const string ANCESTRIES_DIRECTORY = "Assets/Modules/Definitions/Resources/Definitions/_ADVENTURES_/Ancestries";
        private const string BACKGROUNDS_DIRECTORY = "Assets/Modules/Definitions/Resources/Definitions/_ADVENTURES_/Backgrounds";
        private const string BATTLE_ACTIONS_DIRECTORY = "Assets/Modules/Definitions/Resources/Definitions/_ADVENTURES_/BattleActions";
        private const string FEATS_DIRECTORY = "Assets/Modules/Definitions/Resources/Definitions/_ADVENTURES_/Feats";
        private const string SPELLS_DIRECTORY = "Assets/Modules/Definitions/Resources/Definitions/_ADVENTURES_/Spells";
        private const string ITEMS_DIRECTORY = "Assets/Modules/Definitions/Resources/Definitions/_ADVENTURES_/Items";
        private const string DEFAULT_NEW_ID = "_NewCreature";

        private readonly AdventureCharacterDerivedDataService _derivedDataService =
            new AdventureCharacterDerivedDataService(new AdventureDefinitionEditorRepository());

        private Vector2 _mainScroll;
        private List<string> _availableClasses = new List<string>();
        private List<string> _availableAncestries = new List<string>();
        private List<string> _availableBackgrounds = new List<string>();
        private List<string> _availableBattleActions = new List<string>();
        private Dictionary<string, FeatDef> _featDefsById = new Dictionary<string, FeatDef>(StringComparer.Ordinal);
        private Dictionary<string, SpellDef> _spellDefsById = new Dictionary<string, SpellDef>(StringComparer.Ordinal);
        private Dictionary<string, ItemDef> _itemDefsById = new Dictionary<string, ItemDef>(StringComparer.Ordinal);
        private readonly Dictionary<string, List<string>> _sessionAppliedFeatsByDefinitionId =
            new Dictionary<string, List<string>>(StringComparer.Ordinal);

        protected override string WindowTitle => "Creature Editor";
        protected override string DefinitionsDirectory => DEFINITIONS_DIRECTORY;
        protected override string DefaultNewDefinitionId => DEFAULT_NEW_ID;
        protected override string DeleteDialogName => "CreatureDef";

        [MenuItem(MENU_PATH)]
        public static CreatureDefEditorWindow Open()
        {
            CreatureDefEditorWindow window = GetWindow<CreatureDefEditorWindow>();
            window.titleContent = new GUIContent("Creature Editor");
            return window;
        }

        protected override void OnRefreshMetadata()
        {
            _availableClasses = BuildDefinitionIdList(CLASSES_DIRECTORY);
            _availableAncestries = BuildDefinitionIdList(ANCESTRIES_DIRECTORY);
            _availableBackgrounds = BuildDefinitionIdList(BACKGROUNDS_DIRECTORY);
            _availableBattleActions = BuildDefinitionIdList(BATTLE_ACTIONS_DIRECTORY);
            _featDefsById = REPOSITORY.LoadAllDefinitions<FeatDef>(FEATS_DIRECTORY);
            _spellDefsById = REPOSITORY.LoadAllDefinitions<SpellDef>(SPELLS_DIRECTORY);
            _itemDefsById = REPOSITORY.LoadAllDefinitions<ItemDef>(ITEMS_DIRECTORY);
        }

        protected override CreatureDef CreateNewDefinition(string definitionId)
        {
            return new CreatureDef
            {
                Id = definitionId,
                Tags = new List<string>(),
                BattleActionIds = new List<string>(),
                EquippedItems = new List<EquippedItemStateData>(),
                Spells = new Dictionary<string, int>(),
                Parameters = new Dictionary<string, int>(),
                StatusEffects = new Dictionary<string, int>(),
                Avatar = string.Empty,
                Name = string.Empty,
                Icon = string.Empty,
                Title = string.Empty,
                Description = string.Empty,
                Ancestry = string.Empty,
                Class = string.Empty,
                Background = string.Empty,
                Size = AncestrySize.Medium,
            };
        }

        protected override void NormalizeDefinition(CreatureDef definition)
        {
            definition.Tags ??= new List<string>();
            definition.BattleActionIds ??= new List<string>();
            definition.EquippedItems ??= new List<EquippedItemStateData>();
            definition.Spells ??= new Dictionary<string, int>();
            definition.Parameters ??= new Dictionary<string, int>();
            definition.StatusEffects ??= new Dictionary<string, int>();
            definition.Avatar ??= string.Empty;
            definition.Name ??= string.Empty;
            definition.Icon ??= string.Empty;
            definition.Title ??= string.Empty;
            definition.Description ??= string.Empty;
            definition.Ancestry ??= string.Empty;
            definition.Class ??= string.Empty;
            definition.Background ??= string.Empty;
        }

        protected override void DrawToolbarActions()
        {
            if (GUILayout.Button("Apply Linked Defs", EditorStyles.toolbarButton, GUILayout.Width(130f)))
                ApplyLinkedDefinitions();
        }

        protected override void DrawDefinitionEditor(CreatureDef definition)
        {
            using (EditorGUILayout.ScrollViewScope scope = new EditorGUILayout.ScrollViewScope(_mainScroll))
            {
                _mainScroll = scope.scrollPosition;

                EditorGUILayout.LabelField($"Id: {definition.Id}", EditorStyles.boldLabel);
                EditorGUILayout.Space(6f);

                DrawField(() => definition.Disabled = EditorGUILayout.Toggle("Disabled", definition.Disabled));
                DrawField(() => definition.ChallengeRating = EditorGUILayout.FloatField("Challenge Rating", definition.ChallengeRating));
                DrawField(() => definition.Size = (AncestrySize)EditorGUILayout.EnumPopup("Size", definition.Size));
                DrawField(() => definition.Speed = EditorGUILayout.IntField("Speed", definition.Speed));
                DrawField(() => definition.ArmorClass = EditorGUILayout.IntField("Armor Class", definition.ArmorClass));
                DrawField(() => definition.HitPoints = EditorGUILayout.IntField("Hit Points", definition.HitPoints));

                DrawField(() => definition.Avatar = EditorGUILayout.TextField("Avatar", definition.Avatar));
                DrawField(() => definition.Name = EditorGUILayout.TextField("Name", definition.Name));
                DrawField(() => definition.Icon = EditorGUILayout.TextField("Legacy Icon", definition.Icon));
                DrawField(() => definition.Title = EditorGUILayout.TextField("Legacy Title", definition.Title));
                DrawField(() => definition.Description = EditorGUILayout.TextField("Description", definition.Description));
                DrawField(() => definition.Gender = (CharacterGender)EditorGUILayout.EnumPopup("Gender", definition.Gender));

                DrawField(() => definition.Tags = AdventureDefEditorGui.ParseCsv(
                    EditorGUILayout.TextField("Tags (csv)", AdventureDefEditorGui.JoinCsv(definition.Tags))));

                DrawField(() =>
                {
                    definition.Ancestry = AdventureDefEditorGui.DrawStringPopupWithFallback(
                        "Ancestry",
                        definition.Ancestry,
                        _availableAncestries);
                });

                DrawField(() =>
                {
                    definition.Class = AdventureDefEditorGui.DrawStringPopupWithFallback(
                        "Class",
                        definition.Class,
                        _availableClasses);
                });

                DrawField(() =>
                {
                    definition.Background = AdventureDefEditorGui.DrawStringPopupWithFallback(
                        "Background",
                        definition.Background,
                        _availableBackgrounds);
                });

                EditorGUILayout.Space(8f);
                AdventureDefEditorGui.DrawStringListEditor(
                    definition.BattleActionIds,
                    "Battle Action Ids",
                    MarkDirty,
                    "Add Battle Action");

                if (_availableBattleActions.Count > 0)
                {
                    if (GUILayout.Button("Append Battle Action From List", GUILayout.Width(220f)))
                        OpenBattleActionPicker(definition.BattleActionIds);
                }

                EditorGUILayout.Space(8f);
                DrawParametersSection(definition);
                EditorGUILayout.Space(8f);
                DrawSpellsSection(definition);
                EditorGUILayout.Space(8f);
                DrawStatusEffectsSection(definition);
                EditorGUILayout.Space(8f);
                DrawEquippedItemsSection(definition);
            }
        }

        private void ApplyLinkedDefinitions()
        {
            if (CurrentDefinition == null)
                return;

            CharacterRequestData rebuiltData = _derivedDataService.BuildFromLinkedDefinitions(
                CurrentDefinition.Ancestry,
                CurrentDefinition.Class,
                CurrentDefinition.Background,
                CurrentDefinition.Gender);

            CurrentDefinition.Parameters = new Dictionary<string, int>(rebuiltData.Parameters);
            CurrentDefinition.EquippedItems = CloneEquippedItems(rebuiltData.EquippedItems);
            CurrentDefinition.StatusEffects = new Dictionary<string, int>(rebuiltData.StatusEffects);
            if (!string.IsNullOrWhiteSpace(CurrentDefinition.Id))
                _sessionAppliedFeatsByDefinitionId.Remove(CurrentDefinition.Id);
            MarkDirty();
            Repaint();
        }

        private void DrawParametersSection(CreatureDef definition)
        {
            EditorGUILayout.LabelField("Parameters", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Add Parameter", GUILayout.Width(130f)))
                    AddDictionaryEntry(definition.Parameters, "key", 0);

                if (GUILayout.Button("Apply Feat", GUILayout.Width(120f)))
                    OpenFeatPickerAndApply(definition);
            }

            DrawIntDictionaryRows(definition.Parameters);
            DrawAppliedFeatsSection(definition);
        }

        private void DrawSpellsSection(CreatureDef definition)
        {
            EditorGUILayout.LabelField("Spells", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Add Spell", GUILayout.Width(130f)))
                    OpenSpellPicker(definition.Spells);
            }

            DrawIntDictionaryRows(definition.Spells);
        }

        private void DrawStatusEffectsSection(CreatureDef definition)
        {
            EditorGUILayout.LabelField("Status Effects", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Add Status", GUILayout.Width(130f)))
                    OpenConditionFeatPicker(definition.StatusEffects);
            }

            DrawIntDictionaryRows(definition.StatusEffects);
        }

        private void DrawEquippedItemsSection(CreatureDef definition)
        {
            EditorGUILayout.LabelField("Equipped Items", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Add Item", GUILayout.Width(130f)))
                    OpenItemPicker(definition.EquippedItems);
            }

            AdventureDefEditorGui.DrawEquippedItemsEditor(definition.EquippedItems, MarkDirty, showManualAddButton: false);
        }

        private void DrawAppliedFeatsSection(CreatureDef definition)
        {
            List<string> applied = GetAppliedFeatsSessionList(definition);
            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Applied Feats (Editor Session)", EditorStyles.miniBoldLabel);
            if (applied.Count == 0)
            {
                EditorGUILayout.LabelField(
                    "No feats applied in this session. List resets after closing or reload from disk.",
                    EditorStyles.miniLabel);
                return;
            }

            for (int i = 0; i < applied.Count; i++)
            {
                string featId = applied[i];
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(featId, EditorStyles.miniLabel);
                    if (GUILayout.Button("Unapply", GUILayout.Width(80f)))
                    {
                        UnapplyFeat(definition, featId);
                        break;
                    }
                }
            }
        }

        private void OpenBattleActionPicker(List<string> battleActionIds)
        {
            List<DefinitionSearchPickerWindow.Entry> entries = BuildIdOnlyEntries(_availableBattleActions, "BattleAction");
            DefinitionSearchPickerWindow.Open("Select Battle Action", entries, selectedId =>
            {
                if (string.IsNullOrWhiteSpace(selectedId))
                    return;

                battleActionIds ??= new List<string>();
                battleActionIds.Add(selectedId);
                MarkDirty();
                Repaint();
            });
        }

        private void OpenSpellPicker(Dictionary<string, int> spells)
        {
            List<DefinitionSearchPickerWindow.Entry> entries = BuildEntriesForSpells();
            DefinitionSearchPickerWindow.Open("Select Spell", entries, selectedId =>
            {
                if (string.IsNullOrWhiteSpace(selectedId))
                    return;

                AddDictionaryEntry(spells, selectedId, 1);
                Repaint();
            });
        }

        private void OpenConditionFeatPicker(Dictionary<string, int> statusEffects)
        {
            List<DefinitionSearchPickerWindow.Entry> entries = BuildEntriesForConditionFeats();
            DefinitionSearchPickerWindow.Open("Select Condition Feat", entries, selectedId =>
            {
                if (string.IsNullOrWhiteSpace(selectedId))
                    return;

                AddDictionaryEntry(statusEffects, selectedId, 1);
                Repaint();
            });
        }

        private void OpenItemPicker(List<EquippedItemStateData> equippedItems)
        {
            List<DefinitionSearchPickerWindow.Entry> entries = BuildEntriesForItems();
            DefinitionSearchPickerWindow.Open("Select Item", entries, selectedId =>
            {
                if (string.IsNullOrWhiteSpace(selectedId))
                    return;

                equippedItems ??= new List<EquippedItemStateData>();
                string slot = ResolvePreferredSlotForItem(selectedId);
                equippedItems.Add(new EquippedItemStateData
                {
                    Slot = slot,
                    ItemId = selectedId,
                });

                MarkDirty();
                Repaint();
            });
        }

        private void OpenFeatPickerAndApply(CreatureDef definition)
        {
            List<DefinitionSearchPickerWindow.Entry> entries = BuildEntriesForFeats();
            DefinitionSearchPickerWindow.Open("Select Feat To Apply", entries, selectedId =>
            {
                if (string.IsNullOrWhiteSpace(selectedId))
                    return;

                ApplyFeat(definition, selectedId);
            });
        }

        private void ApplyFeat(CreatureDef definition, string featId)
        {
            if (definition == null || string.IsNullOrWhiteSpace(featId))
                return;

            if (!_featDefsById.ContainsKey(featId))
            {
                EditorUtility.DisplayDialog(WindowTitle, $"Feat was not found: {featId}", "OK");
                return;
            }

            var runtimeManager = new DefinitionsManager
            {
                Feats = _featDefsById,
            };

            var requestData = new CharacterRequestData
            {
                Parameters = new Dictionary<string, int>(definition.Parameters),
                EquippedItems = CloneEquippedItems(definition.EquippedItems),
                StatusEffects = new Dictionary<string, int>(definition.StatusEffects),
                Spells = new Dictionary<string, int>(definition.Spells),
            };

            requestData.ApplyFeat(featId, runtimeManager);

            definition.Parameters = new Dictionary<string, int>(requestData.Parameters);
            definition.EquippedItems = CloneEquippedItems(requestData.EquippedItems);
            definition.StatusEffects = new Dictionary<string, int>(requestData.StatusEffects);
            AddAppliedFeatForSession(definition, featId);
            MarkDirty();
            Repaint();
        }

        private void UnapplyFeat(CreatureDef definition, string featId)
        {
            if (definition == null || string.IsNullOrWhiteSpace(featId))
                return;

            var runtimeManager = new DefinitionsManager
            {
                Feats = _featDefsById,
            };

            var requestData = new CharacterRequestData
            {
                Parameters = new Dictionary<string, int>(definition.Parameters),
                EquippedItems = CloneEquippedItems(definition.EquippedItems),
                StatusEffects = new Dictionary<string, int>(definition.StatusEffects),
                Spells = new Dictionary<string, int>(definition.Spells),
            };

            requestData.UnapplyFeat(featId, runtimeManager);

            definition.Parameters = new Dictionary<string, int>(requestData.Parameters);
            definition.EquippedItems = CloneEquippedItems(requestData.EquippedItems);
            definition.StatusEffects = new Dictionary<string, int>(requestData.StatusEffects);
            RemoveAppliedFeatForSession(definition, featId);
            MarkDirty();
            Repaint();
        }

        private void DrawIntDictionaryRows(Dictionary<string, int> dictionary)
        {
            dictionary ??= new Dictionary<string, int>();
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
                        MarkDirty();
                        continue;
                    }

                    if (!string.Equals(nextKey, key, StringComparison.Ordinal))
                    {
                        string uniqueKey = BuildUniqueDictionaryKey(dictionary, nextKey, key);
                        dictionary.Remove(key);
                        dictionary[uniqueKey] = nextValue;
                        MarkDirty();
                        continue;
                    }

                    if (nextValue != value)
                    {
                        dictionary[key] = nextValue;
                        MarkDirty();
                    }
                }
            }
        }

        private void AddDictionaryEntry(Dictionary<string, int> dictionary, string key, int value)
        {
            dictionary ??= new Dictionary<string, int>();
            string uniqueKey = BuildUniqueDictionaryKey(dictionary, key);
            dictionary[uniqueKey] = value;
            MarkDirty();
        }

        private List<string> GetAppliedFeatsSessionList(CreatureDef definition)
        {
            if (definition == null || string.IsNullOrWhiteSpace(definition.Id))
                return new List<string>();

            if (!_sessionAppliedFeatsByDefinitionId.TryGetValue(definition.Id, out List<string> list))
            {
                list = new List<string>();
                _sessionAppliedFeatsByDefinitionId[definition.Id] = list;
            }

            return list;
        }

        private void AddAppliedFeatForSession(CreatureDef definition, string featId)
        {
            List<string> list = GetAppliedFeatsSessionList(definition);
            list.Add(featId);
        }

        private void RemoveAppliedFeatForSession(CreatureDef definition, string featId)
        {
            List<string> list = GetAppliedFeatsSessionList(definition);
            int index = list.LastIndexOf(featId);
            if (index >= 0)
                list.RemoveAt(index);
        }

        private List<DefinitionSearchPickerWindow.Entry> BuildEntriesForFeats()
        {
            List<DefinitionSearchPickerWindow.Entry> result = new List<DefinitionSearchPickerWindow.Entry>(_featDefsById.Count);
            foreach (KeyValuePair<string, FeatDef> pair in _featDefsById)
            {
                FeatDef feat = pair.Value;
                string title = string.IsNullOrWhiteSpace(feat?.Title) ? pair.Key : feat.Title;
                string subtitle = feat == null ? string.Empty : $"{feat.Type}, Lvl {feat.Level}";
                result.Add(DefinitionSearchPickerWindow.BuildEntry(pair.Key, title, subtitle));
            }

            result.Sort((a, b) => string.Compare(a.Id, b.Id, StringComparison.Ordinal));
            return result;
        }

        private List<DefinitionSearchPickerWindow.Entry> BuildEntriesForConditionFeats()
        {
            List<DefinitionSearchPickerWindow.Entry> result = new List<DefinitionSearchPickerWindow.Entry>();
            foreach (KeyValuePair<string, FeatDef> pair in _featDefsById)
            {
                FeatDef feat = pair.Value;
                if (feat == null || feat.Type != FeatType.Condition)
                    continue;

                string title = string.IsNullOrWhiteSpace(feat.Title) ? pair.Key : feat.Title;
                string subtitle = $"Condition, Lvl {feat.Level}";
                result.Add(DefinitionSearchPickerWindow.BuildEntry(pair.Key, title, subtitle));
            }

            result.Sort((a, b) => string.Compare(a.Id, b.Id, StringComparison.Ordinal));
            return result;
        }

        private List<DefinitionSearchPickerWindow.Entry> BuildEntriesForSpells()
        {
            List<DefinitionSearchPickerWindow.Entry> result = new List<DefinitionSearchPickerWindow.Entry>(_spellDefsById.Count);
            foreach (KeyValuePair<string, SpellDef> pair in _spellDefsById)
            {
                SpellDef spell = pair.Value;
                string title = string.IsNullOrWhiteSpace(spell?.Title) ? pair.Key : spell.Title;
                string subtitle = spell == null ? string.Empty : $"{spell.Type}, Lvl {spell.Level}";
                result.Add(DefinitionSearchPickerWindow.BuildEntry(pair.Key, title, subtitle));
            }

            result.Sort((a, b) => string.Compare(a.Id, b.Id, StringComparison.Ordinal));
            return result;
        }

        private List<DefinitionSearchPickerWindow.Entry> BuildEntriesForItems()
        {
            List<DefinitionSearchPickerWindow.Entry> result = new List<DefinitionSearchPickerWindow.Entry>(_itemDefsById.Count);
            foreach (KeyValuePair<string, ItemDef> pair in _itemDefsById)
            {
                ItemDef item = pair.Value;
                string title = string.IsNullOrWhiteSpace(item?.Title) ? pair.Key : item.Title;
                string subtitle = item == null ? string.Empty : $"{item.Category}, Lvl {item.Level}";
                result.Add(DefinitionSearchPickerWindow.BuildEntry(pair.Key, title, subtitle));
            }

            result.Sort((a, b) => string.Compare(a.Id, b.Id, StringComparison.Ordinal));
            return result;
        }

        private static List<DefinitionSearchPickerWindow.Entry> BuildIdOnlyEntries(
            List<string> ids,
            string secondaryLabel)
        {
            List<DefinitionSearchPickerWindow.Entry> result = new List<DefinitionSearchPickerWindow.Entry>(ids?.Count ?? 0);
            if (ids == null)
                return result;

            for (int i = 0; i < ids.Count; i++)
            {
                string id = ids[i];
                result.Add(DefinitionSearchPickerWindow.BuildEntry(id, id, secondaryLabel));
            }

            return result;
        }

        private string ResolvePreferredSlotForItem(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
                return string.Empty;

            if (!_itemDefsById.TryGetValue(itemId, out ItemDef itemDef) || itemDef?.AvailableSlots == null)
                return string.Empty;

            for (int i = 0; i < itemDef.AvailableSlots.Count; i++)
            {
                string slot = itemDef.AvailableSlots[i];
                if (!string.IsNullOrWhiteSpace(slot))
                    return slot;
            }

            return string.Empty;
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

        private List<string> BuildDefinitionIdList(string directoryPath)
        {
            List<string> files = REPOSITORY.GetDefinitionFiles(directoryPath);
            List<string> result = new List<string>(files.Count);
            for (int i = 0; i < files.Count; i++)
                result.Add(AdventureDefinitionEditorRepository.GetFileNameWithoutExtension(files[i]));

            result.Sort(System.StringComparer.Ordinal);
            return result;
        }

        private static List<EquippedItemStateData> CloneEquippedItems(List<EquippedItemStateData> source)
        {
            if (source == null)
                return new List<EquippedItemStateData>();

            List<EquippedItemStateData> result = new List<EquippedItemStateData>(source.Count);
            for (int i = 0; i < source.Count; i++)
            {
                EquippedItemStateData item = source[i];
                if (item == null)
                    continue;

                result.Add(new EquippedItemStateData
                {
                    Slot = item.Slot,
                    ItemId = item.ItemId,
                });
            }

            return result;
        }
    }
}
