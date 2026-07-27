using Modules.Definitions.Scripts.Defs;
using Modules.Restrictions.Scripts.Core;
using Modules.State.Scripts.Implementation.Adventure.StateDatas;
using System.Collections.Generic;

namespace Modules.Definitions.Scripts.Implementation.Adventures.Defs.Ancestries
{
    public enum AncestrySize
    {
        Small = 0,
        Medium = 1,
        Large = 2,
    }


    public class AncestryDef : AbstractDefinition
    {
        /// <summary>
        /// IAP / store product key.
        /// </summary>
        public string ProductId;

        /// <summary>
        /// Soft-currency price (0 when unused or IAP-only).
        /// </summary>
        public int Price;


        public bool Disabled;

        public List<Restriction> Restrictions;

        public List<string> Tags;

        public string Icon;
        public string Title;
        public string Description;

        public AncestrySize Size;
        public int Speed;

        /// <summary>
        /// Hit Points granted by ancestry at character creation (once, not per level).
        /// </summary>
        public int HitPoints;

        /// <summary>
        /// Фичи происхождения по уровню: ключ — уровень персонажа, значение — список id feat/feature.
        /// Пример JSON: { "1": ["_Darkvision", "_ClanDagger", "_DwarfHeritage"], "5": ["_DwarfAncestryFeat5"] }.
        /// </summary>
        public Dictionary<int, List<string>> Features;

        /// <summary>
        /// Ancestry-specific equipment slots and optional starting items
        /// (for example Finger, Neck, Tail for jewelry). Weapon/armor slot layout usually comes from
        /// <c>ClassDef.EquippedItems</c>; ancestry adds race-dependent slots.
        /// </summary>
        public List<EquippedItemStateData> EquippedItems;

        /// <summary>
        /// Localized KEY
        /// </summary>
        public Dictionary<CharacterGender, List<string>> Names;
    }
}
