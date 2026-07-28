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
        /// <summary>
        /// IAP / store product key.
        /// </summary>
        public string ProductId;

        /// <summary>
        /// Soft-currency price (0 when unused or IAP-only).
        /// </summary>
        public int Price;


        public bool Disabled;
        public bool IsQuestItem;

        public ItemCategory Category;
        public int Level;

        public List<string> Tags;

        public string Title;
        public string Description;

        public int IngamePrice;

        /// <summary>
        /// Allowed equipment slot types for this item
        /// (for example Hand, Legs, Head, Body, Bag, Finger, Neck, Tail).
        /// </summary>
        public List<string> AvailableSlots;

        /// <summary>
        /// Id связанных feat/feature (<c>FeatDef</c>), которые применяются к персонажу
        /// при экипировке предмета (через Write API Apply / Unapply). Как у <c>BackgroundDef.Features</c>.
        /// </summary>
        public List<string> Features;

        /// <summary>
        /// Weapon type key, for example <c>Weapon.Martial</c>.
        /// </summary>
        public string Type;

        /// <summary>
        /// Weapon group key, for example <c>Hammer</c>.
        /// </summary>
        public string Group;

        /// <summary>
        /// Кандидатные характеристики для оружейных формул (например <c>STR</c>, <c>DEX</c>).
        /// Используются ключевыми словами <c>ABILITY_BEST</c> / <c>ATTACK_ABILITY_BEST</c> /
        /// <c>DAMAGE_ABILITY_BEST</c>: берётся максимальное сырое значение среди списка.
        /// Для finesse-оружия обычно указывают и STR, и DEX.
        /// </summary>
        public List<string> AbilityDependencies;

        /// <summary>
        /// Формула модификатора атаки оружием (без броска d20).
        /// Вычисляется в <c>WeaponProxy.GetAttackModifier</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Синтаксис: литералы (числа), операторы <c>+</c>, <c>-</c>, <c>*</c>, <c>/</c>
        /// и скобки. Деление целочисленное; при делителе 0 результат слагаемого — 0.
        /// Имена токенов регистронезависимы; в токене допустимы буквы, цифры, <c>_</c> и <c>.</c>.
        /// Пустая формула даёт 0.
        /// </para>
        /// <para>
        /// Специальные ключевые слова (контекст Attack):
        /// <list type="bullet">
        /// <item><c>PROFICIENCY</c> — владение типом оружия: Level + ранг из <c>Type.ProfRank</c> (например <c>Weapon.Martial.ProfRank</c>).</item>
        /// <item><c>ITEMS</c> — предметный бонус атаки: <c>{Id}.Attack.ItemsBonus</c>.</item>
        /// <item><c>GROUP_ATTACK_BONUS</c> — бонус группы: <c>{Group}.Attack.Group.Bonus</c>.</item>
        /// <item><c>ABILITY_BEST</c>, <c>ATTACK_ABILITY_BEST</c>, <c>DAMAGE_ABILITY_BEST</c> — max среди <see cref="AbilityDependencies"/>.</item>
        /// </list>
        /// Любой другой токен — сырой параметр персонажа (<c>STR</c>, <c>DEX</c>, <c>Level</c> и т.д.).
        /// </para>
        /// <para>Примеры:</para>
        /// <code>
        /// "ATTACK_ABILITY_BEST+PROFICIENCY+ITEMS+GROUP_ATTACK_BONUS"
        /// "DEX+PROFICIENCY+ITEMS"
        /// "STR+PROFICIENCY"
        /// "ATTACK_ABILITY_BEST+PROFICIENCY+1"
        /// </code>
        /// </remarks>
        public string AttackModifierFormula;

        /// <summary>
        /// Формула модификатора урона оружием (без результата броска костей урона).
        /// Вычисляется в <c>WeaponProxy.GetDamageModifier</c>.
        /// Кости урона задаются отдельно в <see cref="DamageDice"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Синтаксис тот же, что у <see cref="AttackModifierFormula"/>:
        /// литералы, <c>+</c>/<c>-</c>/<c>*</c>/<c>/</c>, скобки.
        /// Пустая формула даёт 0.
        /// </para>
        /// <para>
        /// Специальные ключевые слова (контекст Damage):
        /// <list type="bullet">
        /// <item><c>PROFICIENCY</c> — владение типом оружия (как в атаке).</item>
        /// <item><c>ITEMS</c> — предметный бонус урона: <c>{Id}.Damage.ItemsBonus</c>.</item>
        /// <item><c>GROUP_DAMAGE_BONUS</c> — бонус группы: <c>{Group}.Damage.Group.Bonus</c>.</item>
        /// <item><c>ABILITY_BEST</c>, <c>ATTACK_ABILITY_BEST</c>, <c>DAMAGE_ABILITY_BEST</c> — max среди <see cref="AbilityDependencies"/>.</item>
        /// </list>
        /// Любой другой токен — сырой параметр персонажа (часто <c>STR</c> для рукопашного урона).
        /// </para>
        /// <para>Примеры:</para>
        /// <code>
        /// "STR+ITEMS+GROUP_DAMAGE_BONUS"
        /// "DAMAGE_ABILITY_BEST+ITEMS"
        /// "STR+ITEMS"
        /// "(STR+DEX)/2"
        /// </code>
        /// </remarks>
        public string DamageModifierFormula;

        /// <summary>
        /// Base damage dice setup (for example 1d8, or 2d4, or d6+d4 via multiple entries).
        /// This list is not rolled by proxy; it is returned as weapon metadata for dice systems.
        /// </summary>
        public List<WeaponDamageDicePartData> DamageDice;

        /// <summary>
        /// Optional semantic type key for consumers (for example Fire, Poison).
        /// </summary>
        public string DamageType;
    }

    public class WeaponDamageDicePartData
    {
        public int Count;
        public DiceType DiceType;
    }
}
