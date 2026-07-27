using Modules.Definitions.Scripts.Defs;
using System.Collections.Generic;

namespace Modules.Definitions.Scripts.Implementation.Adventures.Defs.BattleRules
{
    public class BattleRuleDef : AbstractDefinition
    {
        public List<string> Tags;


        public bool UseMultipleAttackPenalty;

        /// <summary>
        /// Теги боевой способности/действия, по которым оно считается атакой для MAP.
        /// Если теги действия пересекаются с этим списком, атака учитывается в счётчике
        /// множественных атак и получает штраф. Пример: <c>Glossary.Rules.ATTACK</c>.
        /// </summary>
        public List<string> AttackTags;

        /// <summary>
        /// Таблица штрафа множественной атаки по умолчанию
        /// (если нет подходящего override по тегу оружия).
        /// Последовательность атак в раунде, начиная с первой:
        /// индекс <c>0</c> — 1-я атака, индекс <c>1</c> — 2-я и т.д.
        /// Если номер атаки выходит за длину списка, берётся последнее значение.
        /// Пример (PF2e): <c>[0, -5, -10, -10, -10, -10]</c>.
        /// </summary>
        public List<int> MultipleAttackPenaltyTable;

        /// <summary>
        /// Таблицы штрафа множественной атаки по тегу оружия.
        /// Если у оружия атаки есть совпадающий тег, эта таблица заменяет
        /// <see cref="MultipleAttackPenaltyTable"/>.
        /// Индексация та же: с первой атаки (индекс 0).
        /// Пример (PF2e Agile / Быстрое): <c>{ "AGILE": [0, -4, -8, -8, -8, -8] }</c>.
        /// </summary>
        public Dictionary<string, List<int>> MultipleAttackPenaltyTableByWeaponTag;
    }
}
