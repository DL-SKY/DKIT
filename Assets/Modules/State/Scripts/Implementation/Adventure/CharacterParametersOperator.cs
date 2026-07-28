using Modules.Definitions.Scripts.Implementation.Adventures;
using Modules.Definitions.Scripts.Implementation.Adventures.Constants;
using Modules.Definitions.Scripts.Implementation.Adventures.Defs;
using Modules.Definitions.Scripts.Implementation.Adventures.Defs.Feats;
using Modules.State.Scripts.Implementation.Adventure.Actions.Models;
using Modules.State.Scripts.Implementation.Adventure.StateDatas;
using System.Collections.Generic;

namespace Modules.State.Scripts.Implementation.Adventure
{
    /// <summary>
    /// Write API сырых параметров персонажа: Apply / Unapply
    /// <see cref="CharacterParamsPatchData"/> и <see cref="FeatDef.Apply"/>.
    /// </summary>
    public static class CharacterParametersOperator
    {
        public static void ApplyPatch(
            Dictionary<string, int> parameters,
            CharacterParamsPatchData patch,
            DefinitionsManager definitionsManager)
        {
            ApplyPatch(parameters, null, patch, definitionsManager, null);
        }

        public static void UnapplyPatch(
            Dictionary<string, int> parameters,
            CharacterParamsPatchData patch,
            DefinitionsManager definitionsManager)
        {
            UnapplyPatch(parameters, null, patch, definitionsManager, null);
        }

        public static void ApplyFeat(
            Dictionary<string, int> parameters,
            string featId,
            DefinitionsManager definitionsManager)
        {
            ApplyFeat(parameters, null, null, null, featId, definitionsManager, null);
        }

        public static void UnapplyFeat(
            Dictionary<string, int> parameters,
            string featId,
            DefinitionsManager definitionsManager)
        {
            UnapplyFeat(parameters, null, null, null, featId, definitionsManager, null);
        }

        public static void ApplyFeat(
            Dictionary<string, int> parameters,
            Dictionary<string, int> statusEffects,
            List<EquippedItemStateData> equippedItems,
            Dictionary<string, int> inventoryItems,
            string featId,
            DefinitionsManager definitionsManager)
        {
            ApplyFeat(parameters, statusEffects, equippedItems, inventoryItems, featId, definitionsManager, null);
        }

        public static void UnapplyFeat(
            Dictionary<string, int> parameters,
            Dictionary<string, int> statusEffects,
            List<EquippedItemStateData> equippedItems,
            Dictionary<string, int> inventoryItems,
            string featId,
            DefinitionsManager definitionsManager)
        {
            UnapplyFeat(parameters, statusEffects, equippedItems, inventoryItems, featId, definitionsManager, null);
        }

        public static void ApplyFeat(
            Dictionary<string, int> parameters,
            Dictionary<string, int> statusEffects,
            string featId,
            DefinitionsManager definitionsManager)
        {
            ApplyFeat(parameters, statusEffects, null, null, featId, definitionsManager, null);
        }

        public static void UnapplyFeat(
            Dictionary<string, int> parameters,
            Dictionary<string, int> statusEffects,
            string featId,
            DefinitionsManager definitionsManager)
        {
            UnapplyFeat(parameters, statusEffects, null, null, featId, definitionsManager, null);
        }

        public static void ApplyPatch(
            CharacterStateData character,
            CharacterParamsPatchData patch,
            DefinitionsManager definitionsManager)
        {
            if (character == null)
                return;

            character.Parameters ??= new Dictionary<string, int>();
            ApplyPatch(character.Parameters, null, patch, definitionsManager, null);
        }

        public static void UnapplyPatch(
            CharacterStateData character,
            CharacterParamsPatchData patch,
            DefinitionsManager definitionsManager)
        {
            if (character == null)
                return;

            character.Parameters ??= new Dictionary<string, int>();
            UnapplyPatch(character.Parameters, null, patch, definitionsManager, null);
        }

        public static void ApplyFeat(
            CharacterStateData character,
            string featId,
            DefinitionsManager definitionsManager)
        {
            if (character == null)
                return;

            character.Parameters ??= new Dictionary<string, int>();
            character.StatusEffects ??= new Dictionary<string, int>();
            character.EquippedItems ??= new List<EquippedItemStateData>();
            ApplyFeat(character.Parameters, character.StatusEffects, character.EquippedItems, null, featId, definitionsManager);
        }

        public static void UnapplyFeat(
            CharacterStateData character,
            string featId,
            DefinitionsManager definitionsManager)
        {
            if (character == null)
                return;

            character.Parameters ??= new Dictionary<string, int>();
            character.StatusEffects ??= new Dictionary<string, int>();
            character.EquippedItems ??= new List<EquippedItemStateData>();
            UnapplyFeat(character.Parameters, character.StatusEffects, character.EquippedItems, null, featId, definitionsManager);
        }

        public static void ApplyFeat(
            CharacterStateData character,
            Dictionary<string, int> inventoryItems,
            string featId,
            DefinitionsManager definitionsManager)
        {
            if (character == null)
                return;

            character.Parameters ??= new Dictionary<string, int>();
            character.StatusEffects ??= new Dictionary<string, int>();
            character.EquippedItems ??= new List<EquippedItemStateData>();
            ApplyFeat(character.Parameters, character.StatusEffects, character.EquippedItems, inventoryItems, featId, definitionsManager);
        }

        public static void UnapplyFeat(
            CharacterStateData character,
            Dictionary<string, int> inventoryItems,
            string featId,
            DefinitionsManager definitionsManager)
        {
            if (character == null)
                return;

            character.Parameters ??= new Dictionary<string, int>();
            character.StatusEffects ??= new Dictionary<string, int>();
            character.EquippedItems ??= new List<EquippedItemStateData>();
            UnapplyFeat(character.Parameters, character.StatusEffects, character.EquippedItems, inventoryItems, featId, definitionsManager);
        }

        /// <summary>
        /// Полный слепок mutable-блока персонажа для прокачки (копии словарей и слотов).
        /// </summary>
        public static CharacterRequestData CreateCharacterRequestSnapshot(CharacterStateData source)
        {
            if (source == null)
            {
                return new CharacterRequestData
                {
                    Parameters = new Dictionary<string, int>(),
                    EquippedItems = new List<EquippedItemStateData>(),
                    Spells = new Dictionary<string, int>(),
                    StatusEffects = new Dictionary<string, int>(),
                };
            }

            return new CharacterRequestData
            {
                Parameters = CloneParameters(source.Parameters),
                EquippedItems = CloneEquippedItems(source.EquippedItems),
                Spells = CloneParameters(source.Spells),
                StatusEffects = CloneParameters(source.StatusEffects),
            };
        }

        public static Dictionary<string, int> CloneParameters(Dictionary<string, int> source)
        {
            return source == null
                ? new Dictionary<string, int>()
                : new Dictionary<string, int>(source);
        }

        private static void ApplyPatch(
            Dictionary<string, int> parameters,
            Dictionary<string, int> statusEffects,
            CharacterParamsPatchData patch,
            DefinitionsManager definitionsManager,
            HashSet<string> visitedFeatIds)
        {
            if (parameters == null || patch == null)
                return;

            ApplyAdd(parameters, patch.Add);
            ApplySet(parameters, patch.Set);

            if (patch.AlsoApplyFeatIds == null || patch.AlsoApplyFeatIds.Count == 0)
                return;

            for (int i = 0; i < patch.AlsoApplyFeatIds.Count; i++)
                ApplyFeat(parameters, statusEffects, null, null, patch.AlsoApplyFeatIds[i], definitionsManager, visitedFeatIds);
        }

        private static void UnapplyPatch(
            Dictionary<string, int> parameters,
            Dictionary<string, int> statusEffects,
            CharacterParamsPatchData patch,
            DefinitionsManager definitionsManager,
            HashSet<string> visitedFeatIds)
        {
            if (parameters == null || patch == null)
                return;

            if (patch.AlsoApplyFeatIds != null && patch.AlsoApplyFeatIds.Count > 0)
            {
                for (int i = patch.AlsoApplyFeatIds.Count - 1; i >= 0; i--)
                    UnapplyFeat(parameters, statusEffects, null, null, patch.AlsoApplyFeatIds[i], definitionsManager, visitedFeatIds);
            }

            UnapplyAdd(parameters, patch.Add);
            UnapplySet(parameters, patch.Set);
        }

        private static void ApplyFeat(
            Dictionary<string, int> parameters,
            Dictionary<string, int> statusEffects,
            List<EquippedItemStateData> equippedItems,
            Dictionary<string, int> inventoryItems,
            string featId,
            DefinitionsManager definitionsManager,
            HashSet<string> visitedFeatIds)
        {
            if (parameters == null || string.IsNullOrWhiteSpace(featId))
                return;

            visitedFeatIds ??= new HashSet<string>();
            if (!visitedFeatIds.Add(featId))
            {
                UnityEngine.Debug.LogWarning(
                    $"[CharacterParametersOperator] Cycle or duplicate feat Apply skipped: '{featId}'.");
                return;
            }

            if (!TryGetFeat(definitionsManager, featId, out FeatDef featDef))
                return;

            if (featDef.Apply == null)
            {
                UnityEngine.Debug.LogWarning(
                    $"[CharacterParametersOperator] Feat '{featId}' has null Apply; nothing to apply.");
                return;
            }

            if (TryHandleTimedConditionReapply(statusEffects, featId, featDef))
                return;

            ApplyPatch(parameters, statusEffects, featDef.Apply, definitionsManager, visitedFeatIds);
            ApplyTimedConditionStatus(statusEffects, featId, featDef);
            ApplyAdditionalSlots(equippedItems, featDef);
        }

        private static void UnapplyFeat(
            Dictionary<string, int> parameters,
            Dictionary<string, int> statusEffects,
            List<EquippedItemStateData> equippedItems,
            Dictionary<string, int> inventoryItems,
            string featId,
            DefinitionsManager definitionsManager,
            HashSet<string> visitedFeatIds)
        {
            if (parameters == null || string.IsNullOrWhiteSpace(featId))
                return;

            visitedFeatIds ??= new HashSet<string>();
            if (!visitedFeatIds.Add(featId))
            {
                UnityEngine.Debug.LogWarning(
                    $"[CharacterParametersOperator] Cycle or duplicate feat Unapply skipped: '{featId}'.");
                return;
            }

            if (!TryGetFeat(definitionsManager, featId, out FeatDef featDef))
                return;

            if (featDef.Apply == null)
            {
                UnityEngine.Debug.LogWarning(
                    $"[CharacterParametersOperator] Feat '{featId}' has null Apply; nothing to unapply.");
                return;
            }

            UnapplyPatch(parameters, statusEffects, featDef.Apply, definitionsManager, visitedFeatIds);
            RemoveTimedConditionStatus(statusEffects, featId, featDef);
            UnapplyAdditionalSlots(
                parameters,
                statusEffects,
                equippedItems,
                inventoryItems,
                featDef,
                definitionsManager);
        }

        /// <summary>
        /// Повторное наложение уже активного timed Condition.
        /// Если в StatusEffects уже есть этот featId — только обновляет таймер до ConditionDuration из дефа
        /// и возвращает true (патч Add/Set не применяется повторно: штрафы и урон не удваиваются).
        /// Для Condition с тиком урон/патч задаются ConditionDef и не стакаются от повторного Apply.
        /// </summary>
        private static bool TryHandleTimedConditionReapply(
            Dictionary<string, int> statusEffects,
            string featId,
            FeatDef featDef)
        {
            if (!IsTimedCondition(featDef) || statusEffects == null)
                return false;

            if (!statusEffects.ContainsKey(featId))
                return false;

            // Refresh таймера до максимума из настроек дефа; механика Parameters не трогается.
            statusEffects[featId] = featDef.Apply.ConditionDuration;
            return true;
        }

        /// <summary>
        /// Первая регистрация таймера timed Condition: StatusEffects[featId] = ConditionDuration.
        /// </summary>
        private static void ApplyTimedConditionStatus(
            Dictionary<string, int> statusEffects,
            string featId,
            FeatDef featDef)
        {
            if (!IsTimedCondition(featDef) || statusEffects == null)
                return;

            statusEffects[featId] = featDef.Apply.ConditionDuration;
        }

        /// <summary>
        /// Снятие таймера timed Condition из StatusEffects при Unapply.
        /// </summary>
        private static void RemoveTimedConditionStatus(
            Dictionary<string, int> statusEffects,
            string featId,
            FeatDef featDef)
        {
            if (!IsTimedCondition(featDef) || statusEffects == null)
                return;

            statusEffects.Remove(featId);
        }

        /// <summary>
        /// Timed Condition: Type == Condition и Apply.ConditionDuration &gt; 0.
        /// </summary>
        private static bool IsTimedCondition(FeatDef featDef)
        {
            return featDef != null
                && featDef.Type == FeatType.Condition
                && featDef.Apply != null
                && featDef.Apply.ConditionDuration > 0;
        }

        private static void ApplyAdditionalSlots(
            List<EquippedItemStateData> equippedItems,
            FeatDef featDef)
        {
            if (equippedItems == null || featDef?.AdditionalSlots == null || featDef.AdditionalSlots.Count == 0)
                return;

            for (int i = 0; i < featDef.AdditionalSlots.Count; i++)
            {
                string slotType = featDef.AdditionalSlots[i];
                if (string.IsNullOrWhiteSpace(slotType))
                    continue;

                equippedItems.Add(new EquippedItemStateData
                {
                    Slot = slotType,
                    ItemId = null,
                });
            }
        }

        private static void UnapplyAdditionalSlots(
            Dictionary<string, int> parameters,
            Dictionary<string, int> statusEffects,
            List<EquippedItemStateData> equippedItems,
            Dictionary<string, int> inventoryItems,
            FeatDef featDef,
            DefinitionsManager definitionsManager)
        {
            if (equippedItems == null || featDef?.AdditionalSlots == null || featDef.AdditionalSlots.Count == 0)
                return;

            var character = new CharacterStateData
            {
                Parameters = parameters,
                StatusEffects = statusEffects,
                EquippedItems = equippedItems,
            };

            for (int i = featDef.AdditionalSlots.Count - 1; i >= 0; i--)
            {
                string slotType = featDef.AdditionalSlots[i];
                if (string.IsNullOrWhiteSpace(slotType))
                    continue;

                TryRemoveAdditionalSlot(character, slotType, inventoryItems, definitionsManager);
            }
        }

        private static bool TryRemoveAdditionalSlot(
            CharacterStateData character,
            string slotType,
            Dictionary<string, int> inventoryItems,
            DefinitionsManager definitionsManager)
        {
            if (!TryFindRemovableSlotIndex(character?.EquippedItems, slotType, out int removableSlotIndex))
                return false;

            EquippedItemStateData removedSlot = character.EquippedItems[removableSlotIndex];
            string movedItemId = removedSlot?.ItemId;
            if (!string.IsNullOrWhiteSpace(movedItemId))
            {
                if (!TryMoveItemOutOfRemovedSlot(
                    character,
                    removableSlotIndex,
                    slotType,
                    movedItemId,
                    inventoryItems,
                    definitionsManager))
                {
                    UnityEngine.Debug.LogWarning(
                        $"[CharacterParametersOperator] Cannot remove additional slot '{slotType}' because item '{movedItemId}' cannot be relocated.");
                    return false;
                }
            }

            character.EquippedItems.RemoveAt(removableSlotIndex);
            return true;
        }

        private static bool TryMoveItemOutOfRemovedSlot(
            CharacterStateData character,
            int removedSlotIndex,
            string removedSlotType,
            string itemId,
            Dictionary<string, int> inventoryItems,
            DefinitionsManager definitionsManager)
        {
            bool removedIsBag = string.Equals(
                removedSlotType,
                Glossary.Items.SLOT_TYPE_BAG,
                System.StringComparison.Ordinal);

            if (TryFindFreeBagSlotIndex(character.EquippedItems, removedSlotIndex, out int bagSlotIndex))
            {
                character.EquippedItems[bagSlotIndex].ItemId = itemId;
                CharacterItemFeaturesOperator.OnMovedBetweenSlots(
                    character,
                    removedSlotType,
                    Glossary.Items.SLOT_TYPE_BAG,
                    itemId,
                    definitionsManager);
                return true;
            }

            if (inventoryItems != null)
            {
                InventoryItemsOperator.Add(inventoryItems, itemId);
                CharacterItemFeaturesOperator.UnapplyIfWorn(character, removedSlotType, itemId, definitionsManager);
                return true;
            }

            if (removedIsBag)
            {
                UnityEngine.Debug.LogWarning(
                    $"[CharacterParametersOperator] Cannot relocate bag item '{itemId}' while removing slot '{removedSlotType}': no free Bag slot and no shared inventory.");
            }
            else
            {
                UnityEngine.Debug.LogWarning(
                    $"[CharacterParametersOperator] Cannot relocate worn item '{itemId}' from removed slot '{removedSlotType}': no free Bag slot and no shared inventory.");
            }

            return false;
        }

        private static bool TryFindRemovableSlotIndex(
            List<EquippedItemStateData> equippedItems,
            string slotType,
            out int removableSlotIndex)
        {
            removableSlotIndex = -1;
            if (equippedItems == null || string.IsNullOrWhiteSpace(slotType))
                return false;

            // Prefer empty slot of the requested type.
            for (int i = equippedItems.Count - 1; i >= 0; i--)
            {
                EquippedItemStateData slot = equippedItems[i];
                if (slot == null
                    || !string.Equals(slot.Slot, slotType, System.StringComparison.Ordinal)
                    || !string.IsNullOrWhiteSpace(slot.ItemId))
                {
                    continue;
                }

                removableSlotIndex = i;
                return true;
            }

            // If all slots are occupied, remove the last matching slot.
            for (int i = equippedItems.Count - 1; i >= 0; i--)
            {
                EquippedItemStateData slot = equippedItems[i];
                if (slot == null || !string.Equals(slot.Slot, slotType, System.StringComparison.Ordinal))
                    continue;

                removableSlotIndex = i;
                return true;
            }

            return false;
        }

        private static bool TryFindFreeBagSlotIndex(
            List<EquippedItemStateData> equippedItems,
            int excludedIndex,
            out int bagSlotIndex)
        {
            bagSlotIndex = -1;
            if (equippedItems == null)
                return false;

            for (int i = 0; i < equippedItems.Count; i++)
            {
                if (i == excludedIndex)
                    continue;

                EquippedItemStateData slot = equippedItems[i];
                if (slot == null
                    || !string.Equals(slot.Slot, Glossary.Items.SLOT_TYPE_BAG, System.StringComparison.Ordinal)
                    || !string.IsNullOrWhiteSpace(slot.ItemId))
                {
                    continue;
                }

                bagSlotIndex = i;
                return true;
            }

            return false;
        }

        private static bool TryGetFeat(
            DefinitionsManager definitionsManager,
            string featId,
            out FeatDef featDef)
        {
            featDef = null;
            if (definitionsManager?.Feats == null)
            {
                UnityEngine.Debug.LogWarning(
                    $"[CharacterParametersOperator] DefinitionsManager.Feats is null; cannot resolve '{featId}'.");
                return false;
            }

            if (!definitionsManager.Feats.TryGetValue(featId, out featDef) || featDef == null)
            {
                UnityEngine.Debug.LogWarning(
                    $"[CharacterParametersOperator] Feat not found: '{featId}'.");
                return false;
            }

            return true;
        }

        private static void ApplyAdd(Dictionary<string, int> parameters, Dictionary<string, int> add)
        {
            if (add == null)
                return;

            foreach (KeyValuePair<string, int> pair in add)
            {
                if (string.IsNullOrWhiteSpace(pair.Key))
                    continue;

                parameters.TryGetValue(pair.Key, out int current);
                parameters[pair.Key] = current + pair.Value;
            }
        }

        private static void UnapplyAdd(Dictionary<string, int> parameters, Dictionary<string, int> add)
        {
            if (add == null)
                return;

            foreach (KeyValuePair<string, int> pair in add)
            {
                if (string.IsNullOrWhiteSpace(pair.Key))
                    continue;

                parameters.TryGetValue(pair.Key, out int current);
                parameters[pair.Key] = current - pair.Value;
            }
        }

        private static void ApplySet(Dictionary<string, int> parameters, Dictionary<string, int> set)
        {
            if (set == null)
                return;

            foreach (KeyValuePair<string, int> pair in set)
            {
                if (string.IsNullOrWhiteSpace(pair.Key))
                    continue;

                parameters[pair.Key] = pair.Value;
            }
        }

        private static void UnapplySet(Dictionary<string, int> parameters, Dictionary<string, int> set)
        {
            if (set == null)
                return;

            foreach (KeyValuePair<string, int> pair in set)
            {
                if (string.IsNullOrWhiteSpace(pair.Key))
                    continue;

                // Flag invert: non-zero patch value → 0; zero patch value → 1.
                parameters[pair.Key] = pair.Value != 0 ? 0 : 1;
            }
        }

        private static List<EquippedItemStateData> CloneEquippedItems(List<EquippedItemStateData> source)
        {
            if (source == null)
                return new List<EquippedItemStateData>();

            var result = new List<EquippedItemStateData>(source.Count);
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
