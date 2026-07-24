using Modules.Definitions.Scripts.Defs;
using Modules.State.Scripts.Implementation.Adventure.StateDatas;
using System.Collections.Generic;

namespace Modules.Definitions.Scripts.Implementation.Adventures.Defs.PregeneratedCharacters
{
    public class PregeneratedCharacterDef : AbstractDefinition
    {
        public string Avatar;
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
