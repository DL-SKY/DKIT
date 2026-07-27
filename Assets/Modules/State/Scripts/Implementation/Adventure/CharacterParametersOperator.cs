using Modules.Definitions.Scripts.Implementation.Adventures;
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
            ApplyPatch(parameters, patch, definitionsManager, null);
        }

        public static void UnapplyPatch(
            Dictionary<string, int> parameters,
            CharacterParamsPatchData patch,
            DefinitionsManager definitionsManager)
        {
            UnapplyPatch(parameters, patch, definitionsManager, null);
        }

        public static void ApplyFeat(
            Dictionary<string, int> parameters,
            string featId,
            DefinitionsManager definitionsManager)
        {
            ApplyFeat(parameters, featId, definitionsManager, null);
        }

        public static void UnapplyFeat(
            Dictionary<string, int> parameters,
            string featId,
            DefinitionsManager definitionsManager)
        {
            UnapplyFeat(parameters, featId, definitionsManager, null);
        }

        public static void ApplyPatch(
            CharacterStateData character,
            CharacterParamsPatchData patch,
            DefinitionsManager definitionsManager)
        {
            if (character == null)
                return;

            character.Parameters ??= new Dictionary<string, int>();
            ApplyPatch(character.Parameters, patch, definitionsManager);
        }

        public static void UnapplyPatch(
            CharacterStateData character,
            CharacterParamsPatchData patch,
            DefinitionsManager definitionsManager)
        {
            if (character == null)
                return;

            character.Parameters ??= new Dictionary<string, int>();
            UnapplyPatch(character.Parameters, patch, definitionsManager);
        }

        public static void ApplyFeat(
            CharacterStateData character,
            string featId,
            DefinitionsManager definitionsManager)
        {
            if (character == null)
                return;

            character.Parameters ??= new Dictionary<string, int>();
            ApplyFeat(character.Parameters, featId, definitionsManager);
        }

        public static void UnapplyFeat(
            CharacterStateData character,
            string featId,
            DefinitionsManager definitionsManager)
        {
            if (character == null)
                return;

            character.Parameters ??= new Dictionary<string, int>();
            UnapplyFeat(character.Parameters, featId, definitionsManager);
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
                ApplyFeat(parameters, patch.AlsoApplyFeatIds[i], definitionsManager, visitedFeatIds);
        }

        private static void UnapplyPatch(
            Dictionary<string, int> parameters,
            CharacterParamsPatchData patch,
            DefinitionsManager definitionsManager,
            HashSet<string> visitedFeatIds)
        {
            if (parameters == null || patch == null)
                return;

            if (patch.AlsoApplyFeatIds != null && patch.AlsoApplyFeatIds.Count > 0)
            {
                for (int i = patch.AlsoApplyFeatIds.Count - 1; i >= 0; i--)
                    UnapplyFeat(parameters, patch.AlsoApplyFeatIds[i], definitionsManager, visitedFeatIds);
            }

            UnapplyAdd(parameters, patch.Add);
            UnapplySet(parameters, patch.Set);
        }

        private static void ApplyFeat(
            Dictionary<string, int> parameters,
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

            ApplyPatch(parameters, featDef.Apply, definitionsManager, visitedFeatIds);
        }

        private static void UnapplyFeat(
            Dictionary<string, int> parameters,
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

            UnapplyPatch(parameters, featDef.Apply, definitionsManager, visitedFeatIds);
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
