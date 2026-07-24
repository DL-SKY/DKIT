using System.Collections.Generic;

namespace Modules.State.Scripts.Implementation.Adventure.StateDatas
{
    public class CharactersStateData
    {
        public int NextCharacterId;

        public int HeroPoints;

        public Dictionary<int, CharacterStateData> Characters;
        public List<int> ActivePartyCharacterIds;
    }

    public enum CharacterGender
    {
        Male = 0,
        Female = 1,
    }

    public enum ProficiencyType
    {
        Untrained = 0,
        
        Trained,
        Expert,
        Master,
        Legendary
    }

    public class CharacterStateData
    {
        public int Id;

        public long CreateTime;
        public bool IsDead;
        public long DeathTime;

        public string Avatar;
        public string Name;
        public CharacterGender Gender;

        public string Ancestry;
        public string Class;
        public string Background;

        /// <summary>
        /// Abilities, Skills, HP, Speed, Feats, etc...
        /// </summary>
        public Dictionary<string, int> Parameters;
        //public Dictionary<string, int> SavingThrows;    // TODO: obsolete? Use Parameters?

        public List<EquippedItemStateData> EquippedItems;

        public Dictionary<string, int> Spells;

        /// <summary>
        /// <Effect, Value>
        /// </summary>
        public Dictionary<string, int> StatusEffects;
    }

    public class EquippedItemStateData
    {
        /// <summary>
        /// Character slot type (for example Hand, Legs, Head, Body, Bag).
        /// </summary>
        public string Slot;
        public string ItemId;
    }
}
