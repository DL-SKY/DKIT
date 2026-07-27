namespace Modules.State.Scripts.Implementation.Adventure.Actions.Models
{
    /// <summary>
    /// Удаление предмета из слота персонажа (без возврата в Inventory.Items).
    /// SlotIndex = -1 — найти первый слот с указанным ItemId.
    /// </summary>
    public class RemoveCharacterEquippedItemRequestData
    {
        public int CharacterId;

        /// <summary>
        /// Индекс слота в EquippedItems. Значение -1 означает поиск по ItemId.
        /// </summary>
        public int SlotIndex;

        public string ItemId;
    }
}
