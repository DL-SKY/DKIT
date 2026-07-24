using Modules.Definitions.Scripts.Defs;
using Modules.Dices.Scripts;
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

        /// <summary>
        /// Weapon type key, for example <c>Weapon.Martial</c>.
        /// </summary>
        public string Type;

        /// <summary>
        /// Weapon group key, for example <c>Hammer</c>.
        /// </summary>
        public string Group;

        /// <summary>
        /// Candidate ability keys used by weapon formulas.
        /// If more than one key is provided, the highest value can be selected by formula keyword.
        /// </summary>
        public List<string> AbilityDependencies;

        /// <summary>
        /// Formula for attack modifier calculation.
        /// Supports literals, operators (+, -, *, /, parentheses) and weapon formula keywords.
        /// </summary>
        public string AttackModifierFormula;

        /// <summary>
        /// Formula for damage modifier calculation (without damage dice roll result).
        /// Supports literals, operators (+, -, *, /, parentheses) and weapon formula keywords.
        /// </summary>
        public string DamageModifierFormula;

        /// <summary>
        /// Base damage dice setup (for example 1d8, or 2d4, or d6+d4 via multiple entries).
        /// This list is not rolled by proxy; it is returned as weapon metadata for dice systems.
        /// </summary>
        public List<WeaponDamageDicePartData> DamageDice;
    }

    public class WeaponDamageDicePartData
    {
        public int Count;
        public DiceType DiceType;
    }
}
