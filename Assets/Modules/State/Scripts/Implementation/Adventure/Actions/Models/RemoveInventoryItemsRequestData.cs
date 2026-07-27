namespace Modules.State.Scripts.Implementation.Adventure.Actions.Models
{
    /// <summary>
    /// Удаление предметов из общего инвентаря отряда.
    /// </summary>
    public class RemoveInventoryItemsRequestData
    {
        public string ItemId;
        public int Count;
    }
}
