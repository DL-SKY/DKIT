using System;
using System.Collections.Generic;

namespace Modules.State.Scripts.Implementation.Adventure
{
    public static class InventoryItemsOperator
    {
        public static int GetCount(Dictionary<string, int> items, string itemId)
        {
            if (items == null || string.IsNullOrWhiteSpace(itemId))
                return 0;

            return items.TryGetValue(itemId, out int count) ? count : 0;
        }

        public static bool TryConsume(Dictionary<string, int> items, string itemId, int amount = 1)
        {
            if (items == null || string.IsNullOrWhiteSpace(itemId) || amount <= 0)
                return false;

            if (!items.TryGetValue(itemId, out int availableCount) || availableCount < amount)
                return false;

            int newCount = availableCount - amount;
            if (newCount <= 0)
                items.Remove(itemId);
            else
                items[itemId] = newCount;

            return true;
        }

        /// <summary>
        /// Adds items to shared inventory.
        /// Returns the actual amount added (may be less than requested when capacity is limited).
        /// Capacity clamping is stubbed for now: actual amount always equals requested amount.
        /// </summary>
        public static int TryAdd(Dictionary<string, int> items, string itemId, int requestedAmount)
        {
            if (items == null || string.IsNullOrWhiteSpace(itemId) || requestedAmount <= 0)
                return 0;

            int actualAmount = ClampToAvailableCapacity(items, itemId, requestedAmount);
            if (actualAmount <= 0)
                return 0;

            items.TryGetValue(itemId, out int availableCount);
            items[itemId] = availableCount + actualAmount;
            return actualAmount;
        }

        /// <summary>
        /// Legacy helper for transfer actions that always add the full amount.
        /// </summary>
        public static void Add(Dictionary<string, int> items, string itemId, int amount = 1)
        {
            TryAdd(items, itemId, amount);
        }

        /// <summary>
        /// Removes up to <paramref name="amount"/> items. Result count is Max(0, current - amount).
        /// Key is removed when count reaches 0.
        /// </summary>
        public static void RemoveUpTo(Dictionary<string, int> items, string itemId, int amount)
        {
            if (items == null || string.IsNullOrWhiteSpace(itemId) || amount <= 0)
                return;

            if (!items.TryGetValue(itemId, out int availableCount))
                return;

            int newCount = Math.Max(0, availableCount - amount);
            if (newCount <= 0)
                items.Remove(itemId);
            else
                items[itemId] = newCount;
        }

        /// <summary>
        /// Stub for future shared inventory capacity rules.
        /// Returns how many items can actually be added for the requested amount.
        /// </summary>
        private static int ClampToAvailableCapacity(
            Dictionary<string, int> items,
            string itemId,
            int requestedAmount)
        {
            // TODO: compute free capacity of shared inventory and clamp requestedAmount.
            return requestedAmount;
        }
    }
}
