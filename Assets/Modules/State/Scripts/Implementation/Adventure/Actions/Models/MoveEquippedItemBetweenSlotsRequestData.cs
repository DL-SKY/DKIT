namespace Modules.State.Scripts.Implementation.Adventure.Actions.Models
{
    /// <summary>
    /// Перенос предмета между слотами одного персонажа (без участия Inventory.Items).
    /// </summary>
    public class MoveEquippedItemBetweenSlotsRequestData
    {
        public int CharacterId;

        /// <summary>
        /// Индекс исходного слота в CharacterStateData.EquippedItems.
        /// </summary>
        public int FromSlotIndex;

        /// <summary>
        /// Индекс целевого слота в CharacterStateData.EquippedItems.
        /// </summary>
        public int ToSlotIndex;
    }
}
