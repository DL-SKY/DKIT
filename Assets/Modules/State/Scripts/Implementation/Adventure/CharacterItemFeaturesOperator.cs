using Modules.Definitions.Scripts.Implementation.Adventures;
using Modules.Definitions.Scripts.Implementation.Adventures.Constants;
using Modules.Definitions.Scripts.Implementation.Adventures.Defs.Items;
using Modules.State.Scripts.Implementation.Adventure.StateDatas;
using System.Collections.Generic;

namespace Modules.State.Scripts.Implementation.Adventure
{
    /// <summary>
    /// Apply / Unapply <see cref="ItemDef.Features"/> на персонаже с учётом правила Bag
    /// (<see cref="Glossary.Items.GrantsItemFeatures"/>).
    /// </summary>
    public static class CharacterItemFeaturesOperator
    {
        public static void ApplyIfWorn(
            CharacterStateData character,
            string slotType,
            string itemId,
            DefinitionsManager definitionsManager)
        {
            if (!Glossary.Items.GrantsItemFeatures(slotType))
                return;

            ApplyFeatures(character, itemId, definitionsManager);
        }

        public static void UnapplyIfWorn(
            CharacterStateData character,
            string slotType,
            string itemId,
            DefinitionsManager definitionsManager)
        {
            if (!Glossary.Items.GrantsItemFeatures(slotType))
                return;

            UnapplyFeatures(character, itemId, definitionsManager);
        }

        /// <summary>
        /// Перенос одного предмета между слотами: носимый→Bag Unapply, Bag→носимый Apply,
        /// носимый↔носимый и Bag↔Bag — без изменений Features.
        /// </summary>
        public static void OnMovedBetweenSlots(
            CharacterStateData character,
            string fromSlotType,
            string toSlotType,
            string itemId,
            DefinitionsManager definitionsManager)
        {
            if (character == null || string.IsNullOrWhiteSpace(itemId))
                return;

            bool fromGrants = Glossary.Items.GrantsItemFeatures(fromSlotType);
            bool toGrants = Glossary.Items.GrantsItemFeatures(toSlotType);
            if (fromGrants == toGrants)
                return;

            if (fromGrants)
                UnapplyFeatures(character, itemId, definitionsManager);
            else
                ApplyFeatures(character, itemId, definitionsManager);
        }

        public static void ApplyFeatures(
            CharacterStateData character,
            string itemId,
            DefinitionsManager definitionsManager)
        {
            if (!TryGetFeatures(itemId, definitionsManager, out List<string> features))
                return;

            for (int i = 0; i < features.Count; i++)
                CharacterParametersOperator.ApplyFeat(character, features[i], definitionsManager);
        }

        public static void UnapplyFeatures(
            CharacterStateData character,
            string itemId,
            DefinitionsManager definitionsManager)
        {
            if (!TryGetFeatures(itemId, definitionsManager, out List<string> features))
                return;

            for (int i = features.Count - 1; i >= 0; i--)
                CharacterParametersOperator.UnapplyFeat(character, features[i], definitionsManager);
        }

        private static bool TryGetFeatures(
            string itemId,
            DefinitionsManager definitionsManager,
            out List<string> features)
        {
            features = null;
            if (string.IsNullOrWhiteSpace(itemId))
                return false;

            if (definitionsManager?.Items == null)
            {
                UnityEngine.Debug.LogWarning(
                    $"[CharacterItemFeaturesOperator] DefinitionsManager.Items is null; cannot resolve '{itemId}'.");
                return false;
            }

            if (!definitionsManager.Items.TryGetValue(itemId, out ItemDef itemDef) || itemDef == null)
            {
                UnityEngine.Debug.LogWarning(
                    $"[CharacterItemFeaturesOperator] Item not found: '{itemId}'.");
                return false;
            }

            if (itemDef.Features == null || itemDef.Features.Count == 0)
                return false;

            features = itemDef.Features;
            return true;
        }
    }
}
