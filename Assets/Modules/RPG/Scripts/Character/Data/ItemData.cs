using Modules.Definitions.Scripts.Defs;

namespace Modules.RPG.Scripts.Character.Data
{
    public enum ItemCategory
    {
        Weapon = 0,
        Armor = 1,
        Shield = 2,
        Consumable = 3,
        Equipment = 4,
    }

    public class ItemData : AbstractDefinition
    {
        public string Title;
        public string Description;

        public ItemCategory Category;
        public int Level;

        /// <summary>
        /// Цена в медных монетах.
        /// </summary>
        public int Price;

        /// <summary>
        /// Объём/масса предмета (PF2e: L, 1, 2, …).
        /// </summary>
        public string Bulk;
    }
}
