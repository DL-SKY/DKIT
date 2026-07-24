namespace Modules.State.Scripts.Implementation.Adventure.Actions.Models
{
    /// <summary>
    /// Перенос предмета из общего инвентаря отряда в слот персонажа.
    /// </summary>
    public class EquipItemFromInventoryRequestData
    {
        public int CharacterId;

        /// <summary>
        /// Индекс записи в CharacterStateData.EquippedItems.
        /// </summary>
        public int SlotIndex;

        /// <summary>
        /// Id предмета (ItemDef), который нужно надеть из Inventory.Items.
        /// </summary>
        public string ItemId;
    }
}
