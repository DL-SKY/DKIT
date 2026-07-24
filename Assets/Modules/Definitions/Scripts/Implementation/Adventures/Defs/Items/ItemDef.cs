using Modules.Definitions.Scripts.Defs;
using System.Collections.Generic;

namespace Modules.Definitions.Scripts.Implementation.Adventures.Defs.Items
{
    public enum ItemCategory
    {
        Weapon = 0,
        Armor = 1,
        Shield = 2,
        Consumable = 3,
        Equipment = 4,
    }


    public class ItemDef : AbstractDefinition
    {
        public bool Disabled;
        public bool IsQuestItem;

        public ItemCategory Category;
        public int Level;

        public List<string> Tags;

        public string Title;
        public string Description;

        public int Price;

        public string Type;         // например для оружия - Weapon.Martial
        public string Group;        // например для оружия - Hammer
        public List<string> AbilityDependencies;    // Характеристики, отвечающие за предмет. Для оружия - модификатор атаки (если список не с одним элементом - берется наилучшее)
    }
}
