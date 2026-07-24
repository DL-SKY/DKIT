namespace Modules.State.Scripts.Implementation.Adventure.Actions.Models
{
    /// <summary>
    /// Перенос предмета из слота персонажа обратно в общий инвентарь отряда.
    /// </summary>
    public class UnequipItemToInventoryRequestData
    {
        public int CharacterId;

        /// <summary>
        /// Индекс записи в CharacterStateData.EquippedItems.
        /// </summary>
        public int SlotIndex;
    }
}
