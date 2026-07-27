using Modules.Definitions.Scripts.Defs;
using Modules.Restrictions.Scripts.Core;
using Modules.State.Scripts.Implementation.Adventure.StateDatas;
using System.Collections.Generic;

namespace Modules.Definitions.Scripts.Implementation.Adventures.Defs.Classes
{
    public class ClassDef : AbstractDefinition
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

        /// <summary>
        /// Hit Points granted by the class each level (before Constitution modifier).
        /// </summary>
        public int HitPointsPerLevel;

        /// <summary>
        /// Фичи класса по уровню: ключ — уровень персонажа, значение — список id feat/feature.
        /// Пример JSON: { "1": ["_FighterKeyAttribute", "_ReactiveStrike"], "2": ["_FighterFeat2"] }.
        /// </summary>
        public Dictionary<int, List<string>> Features;

        public List<EquippedItemStateData> EquippedItems;
    }
}
