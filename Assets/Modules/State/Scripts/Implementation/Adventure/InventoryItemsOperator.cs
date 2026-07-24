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

        public static void Add(Dictionary<string, int> items, string itemId, int amount = 1)
        {
            if (items == null || string.IsNullOrWhiteSpace(itemId) || amount <= 0)
                return;

            items.TryGetValue(itemId, out int availableCount);
            items[itemId] = availableCount + amount;
        }
    }
}
