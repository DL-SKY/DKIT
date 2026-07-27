using Modules.Definitions.Scripts.Defs;
using System;
using System.Collections.Generic;

namespace Modules.Definitions.Scripts.Implementation.Adventures.Defs.Rules
{
    public class RuleDef : AbstractDefinition
    {
        public List<string> Tags;

        /// <summary>
        /// Стоимость улучшения основных характеристик (Ability Scores) в очках прокачки.
        /// По Pathfinder 2e Remaster характеристики хранятся сразу как модификаторы
        /// (а не как значения 10/15/18 и т.п.).
        /// </summary>
        /// <remarks>
        /// <para>
        /// Ключ — пограничное значение модификатора, значение — стоимость одного шага улучшения
        /// при достижении этого порога. При прокачке берётся ближайший подходящий ключ,
        /// и по его значению определяется цена улучшения.
        /// </para>
        /// <para>
        /// По текущим правилам: повышение до 4 включительно стоит 1 пункт;
        /// улучшение до 5 и выше — по 2 пункта.
        /// </para>
        /// <para>Пример (<c>GeneralRule.json</c>):</para>
        /// <code>
        /// "AbilityBoostPointCost": {
        ///   "4": 1,
        ///   "5": 2
        /// }
        /// </code>
        /// </remarks>
        public Dictionary<int, int> AbilityBoostPointCost;

        /// <summary>
        /// Неактуально. Стоит удалить.
        /// </summary>
        [Obsolete] public Dictionary<string, string> SkillDependencies;

        /// <summary>
        /// Формулы вычисляемых параметров персонажа (навыки, MaxHitPoints и т.п.).
        /// Ключ — id параметра (<c>Athletics</c>, <c>Perception</c>, <c>MaxHitPoints</c>);
        /// значение — выражение, которое разбирает <c>CharacterParametersProxy.GetTotalValue</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Синтаксис: литералы (числа), операторы <c>+</c>, <c>-</c>, <c>*</c>, <c>/</c>
        /// и скобки. Деление целочисленное; при делителе 0 результат слагаемого — 0.
        /// Имена токенов регистронезависимы; в токене допустимы буквы, цифры, <c>_</c> и <c>.</c>.
        /// </para>
        /// <para>
        /// Специальные ключевые слова (зависят от ключа формулы / контекста персонажа):
        /// <list type="bullet">
        /// <item><c>PROFICIENCY</c> — бонус владения: Level + ранг (<c>key.ProfRank</c>), Untrained = 0.</item>
        /// <item><c>ITEMS</c> — предметный бонус из <c>key.ItemsBonus</c>.</item>
        /// <item><c>ANCESTRY_HP</c> — HitPoints из дефа происхождения персонажа.</item>
        /// <item><c>CLASS_HP</c> — HitPointsPerLevel из дефа класса.</item>
        /// <item><c>PER_LEVEL</c> — сырое значение <c>key.PerLevel</c>.</item>
        /// <item><c>BONUS</c> — сырое значение <c>key.Bonus</c>.</item>
        /// </list>
        /// Любой другой токен читается как сырой параметр из <c>CharacterStateData.Parameters</c>
        /// (например <c>STR</c>, <c>DEX</c>, <c>CON</c>, <c>Level</c>).
        /// Если формулы для ключа нет — возвращается сырое значение параметра (или 0).
        /// </para>
        /// <para>Примеры:</para>
        /// <code>
        /// "Athletics": "STR+PROFICIENCY+ITEMS"
        /// "Stealth": "DEX+PROFICIENCY+ITEMS"
        /// "Perception": "WIS+PROFICIENCY+ITEMS"
        /// "MaxHitPoints": "ANCESTRY_HP+(CLASS_HP+CON+PER_LEVEL)*Level+BONUS"
        /// "CustomStat": "(STR+DEX)/2+2"
        /// </code>
        /// </remarks>
        public Dictionary<string, string> ParameterFormulas;
    }
}
