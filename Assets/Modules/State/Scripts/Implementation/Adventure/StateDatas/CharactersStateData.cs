using System.Collections.Generic;

namespace Modules.State.Scripts.Implementation.Adventure.StateDatas
{
    public class CharactersStateData
    {
        public int NextCharacterId;

        public Dictionary<int, CharacterStateData> Characters;
        public List<int> ActivePartyCharacterIds;
    }

    public class CharacterStateData
    {
        public int Id;

        public string Name;
        public string Class;
        public string Ancestry;

        public bool IsDead;
        public long DeathTime;

        public Dictionary<string, int> IntParameters;
        //public Dictionary<string, bool> BoolParameters;
        //public Dictionary<string, string> StringParameters;
        //public Dictionary<string, float> FloatParameters;

        public List<EquippedItemStateData> EquippedItems;

        public Dictionary<string, int> StatusEffects;
    }

    public class EquippedItemStateData
    {
        public string Slot;
        public string ItemId;
    }
}
