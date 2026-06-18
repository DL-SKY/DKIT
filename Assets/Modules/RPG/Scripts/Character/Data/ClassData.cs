using Modules.Definitions.Scripts.Defs;

namespace Modules.RPG.Scripts.Character.Data
{
    public class ClassData : AbstractDefinition
    {
        public string Title;
        public string Description;

        /// <summary>
        /// Ключевая характеристика класса (PF2e: Strength, Dexterity, и т.д.).
        /// </summary>
        public string KeyAbility;

        /// <summary>
        /// Базовые ОЗ за уровень (без модификатора Телосложения).
        /// </summary>
        public int HitPointsPerLevel;
    }
}
