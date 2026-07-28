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
        /// Сырые числовые параметры: abilities, level/experience, proficiency ranks,
        /// item/per-level/flat bonuses, текущие HP, speed, флаги черт и т.д.
        /// </summary>
        /// <remarks>
        /// Итоги вроде модификаторов навыков и <c>MaxHitPoints</c> здесь не хранятся —
        /// их даёт <c>CharacterParametersProxy.GetTotalValue</c>.
        /// Чтение предпочтительно через <c>CharacterParametersProxy</c> (<c>GetRawValue</c> / <c>GetTotalValue</c>).
        /// Запись предпочтительно через <see cref="CharacterParametersOperator"/> (Apply / Unapply
        /// <c>CharacterParamsPatchData</c>), а не произвольной мутацией словаря.
        /// </remarks>
        public Dictionary<string, int> Parameters;
        //public Dictionary<string, int> SavingThrows;    // TODO: obsolete? Use Parameters?

        public List<EquippedItemStateData> EquippedItems;

        public Dictionary<string, int> Spells;

        /// <summary>
        /// Таймеры timed Condition: id feat → оставшиеся ходы.
        /// Пишется CharacterParametersOperator при Apply Condition с ConditionDuration &gt; 0.
        /// Повторный Apply того же feat только обновляет значение до ConditionDuration (без удвоения механики/урона).
        /// Декремент — EndCharacterTurnStateAction.
        /// </summary>
        public Dictionary<string, int> StatusEffects;
    }

    public class EquippedItemStateData
    {
        /// <summary>
        /// Character slot type (for example Hand, Legs, Head, Body, Bag, Finger, Neck, Tail).
        /// </summary>
        public string Slot;
        public string ItemId;
    }
}
