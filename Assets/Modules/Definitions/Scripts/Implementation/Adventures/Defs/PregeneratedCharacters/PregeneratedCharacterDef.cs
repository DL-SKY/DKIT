using Modules.Definitions.Scripts.Defs;
using Modules.State.Scripts.Implementation.Adventure.StateDatas;
using System.Collections.Generic;

namespace Modules.Definitions.Scripts.Implementation.Adventures.Defs.PregeneratedCharacters
{
    public class PregeneratedCharacterDef : AbstractDefinition
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

        public string Avatar;
        /// <summary>
        /// Localized KEY
        /// </summary>
        public string Name;
        public CharacterGender Gender;

        public string Ancestry;
        public string Class;
        public string Background;

        public Dictionary<string, int> Parameters;
        public List<EquippedItemStateData> EquippedItems;
        public Dictionary<string, int> Spells;
    }
}
