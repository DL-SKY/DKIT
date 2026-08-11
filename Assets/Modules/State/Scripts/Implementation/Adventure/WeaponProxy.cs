using Modules.Definitions.Scripts.Implementation.Adventures.Constants;
using Modules.Definitions.Scripts.Implementation.Adventures.Defs.Items;
using Modules.State.Scripts.Implementation.Adventure.StateDatas;
using System;
using System.Collections.Generic;

namespace Modules.State.Scripts.Implementation.Adventure
{
    public class WeaponProxy
    {
        private readonly CharacterStateData _characterState;
        private readonly HashSet<string> _formulaKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Glossary.Weapons.PROFICIENCY_KEYWORD,
            Glossary.Weapons.ITEMS_KEYWORD,
            Glossary.Weapons.GROUP_ATTACK_BONUS_KEYWORD,
            Glossary.Weapons.GROUP_DAMAGE_BONUS_KEYWORD,
            Glossary.Weapons.ABILITY_BEST_KEYWORD,
            Glossary.Weapons.ATTACK_ABILITY_BEST_KEYWORD,
            Glossary.Weapons.DAMAGE_ABILITY_BEST_KEYWORD,
        };

        public WeaponProxy(CharacterStateData characterState)
        {
            _characterState = characterState;
        }

        public int GetAttackModifier(ItemDef itemDef)
        {
            if (itemDef == null || string.IsNullOrWhiteSpace(itemDef.AttackModifierFormula))
                return 0;

            return EvaluateFormula(itemDef, itemDef.AttackModifierFormula, WeaponFormulaContext.Attack);
        }

        public int GetDamageModifier(ItemDef itemDef)
        {
            if (itemDef == null || string.IsNullOrWhiteSpace(itemDef.DamageModifierFormula))
                return 0;

            return EvaluateFormula(itemDef, itemDef.DamageModifierFormula, WeaponFormulaContext.Damage);
        }

        public IReadOnlyList<WeaponDamageDicePartData> GetDamageDice(ItemDef itemDef)
        {
            if (itemDef?.DamageDice == null)
                return Array.Empty<WeaponDamageDicePartData>();

            return itemDef.DamageDice;
        }

        private int EvaluateFormula(ItemDef itemDef, string formula, WeaponFormulaContext context)
        {
            var parser = new FormulaParser(formula, token => EvaluateFormulaToken(itemDef, token, context));
            return parser.ParseExpression();
        }

        private int EvaluateFormulaToken(ItemDef itemDef, string token, WeaponFormulaContext context)
        {
            if (_formulaKeywords.Contains(token))
            {
                if (string.Equals(token, Glossary.Weapons.PROFICIENCY_KEYWORD, StringComparison.OrdinalIgnoreCase))
                    return EvaluateProficiency(itemDef);

                if (string.Equals(token, Glossary.Weapons.ITEMS_KEYWORD, StringComparison.OrdinalIgnoreCase))
                    return EvaluateItemsBonus(itemDef, context);

                if (string.Equals(token, Glossary.Weapons.GROUP_ATTACK_BONUS_KEYWORD, StringComparison.OrdinalIgnoreCase))
                    return EvaluateGroupAttackBonus(itemDef);

                if (string.Equals(token, Glossary.Weapons.GROUP_DAMAGE_BONUS_KEYWORD, StringComparison.OrdinalIgnoreCase))
                    return EvaluateGroupDamageBonus(itemDef);

                if (string.Equals(token, Glossary.Weapons.ABILITY_BEST_KEYWORD, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(token, Glossary.Weapons.ATTACK_ABILITY_BEST_KEYWORD, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(token, Glossary.Weapons.DAMAGE_ABILITY_BEST_KEYWORD, StringComparison.OrdinalIgnoreCase))
                {
                    return EvaluateBestAbility(itemDef);
                }
            }

            return GetRawValue(token);
        }

        private int EvaluateBestAbility(ItemDef itemDef)
        {
            if (itemDef?.AbilityDependencies == null || itemDef.AbilityDependencies.Count == 0)
                return 0;

            int best = int.MinValue;
            bool found = false;
            foreach (string abilityKey in itemDef.AbilityDependencies)
            {
                if (string.IsNullOrWhiteSpace(abilityKey))
                    continue;

                int value = GetRawValue(abilityKey);
                if (!found || value > best)
                {
                    best = value;
                    found = true;
                }
            }

            return found ? best : 0;
        }

        private int EvaluateProficiency(ItemDef itemDef)
        {
            if (string.IsNullOrWhiteSpace(itemDef?.Type))
                return 0;

            string proficiencyKey = itemDef.Type + Glossary.Characters.PROFICIENCY_SUFFIX;
            int rankValue = GetRawValue(proficiencyKey);
            int level = GetRawValue(Glossary.Characters.LEVEL);
            return ProficiencyBonus.Evaluate(rankValue, level);
        }

        private int EvaluateItemsBonus(ItemDef itemDef, WeaponFormulaContext context)
        {
            if (string.IsNullOrWhiteSpace(itemDef?.Id))
                return 0;

            string area = context == WeaponFormulaContext.Attack ? "Attack" : "Damage";
            string key = $"{itemDef.Id}.{area}{Glossary.Characters.ITEMS_SUFFIX}";
            return GetRawValue(key);
        }

        private int EvaluateGroupAttackBonus(ItemDef itemDef)
        {
            if (string.IsNullOrWhiteSpace(itemDef?.Group))
                return 0;

            string key = $"{itemDef.Group}.Attack.Group.Bonus";
            return GetRawValue(key);
        }

        private int EvaluateGroupDamageBonus(ItemDef itemDef)
        {
            if (string.IsNullOrWhiteSpace(itemDef?.Group))
                return 0;

            string key = $"{itemDef.Group}.Damage.Group.Bonus";
            return GetRawValue(key);
        }

        private int GetRawValue(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return 0;

            if (_characterState?.Parameters == null)
                return 0;

            return _characterState.Parameters.TryGetValue(key, out int value) ? value : 0;
        }

        private enum WeaponFormulaContext
        {
            Attack = 0,
            Damage = 1,
        }

        private sealed class FormulaParser
        {
            private readonly string _formula;
            private readonly Func<string, int> _resolveToken;
            private int _index;

            public FormulaParser(string formula, Func<string, int> resolveToken)
            {
                _formula = formula ?? string.Empty;
                _resolveToken = resolveToken;
            }

            public int ParseExpression()
            {
                int value = ParseTerm();

                while (true)
                {
                    SkipWhitespace();
                    if (Match('+'))
                        value += ParseTerm();
                    else if (Match('-'))
                        value -= ParseTerm();
                    else
                        break;
                }

                return value;
            }

            private int ParseTerm()
            {
                int value = ParseFactor();

                while (true)
                {
                    SkipWhitespace();
                    if (Match('*'))
                        value *= ParseFactor();
                    else if (Match('/'))
                    {
                        int divisor = ParseFactor();
                        value = divisor == 0 ? 0 : value / divisor;
                    }
                    else
                    {
                        break;
                    }
                }

                return value;
            }

            private int ParseFactor()
            {
                SkipWhitespace();

                if (Match('('))
                {
                    int value = ParseExpression();
                    SkipWhitespace();
                    Match(')');
                    return value;
                }

                if (_index < _formula.Length
                    && (char.IsDigit(_formula[_index])
                        || (_formula[_index] == '-'
                            && _index + 1 < _formula.Length
                            && char.IsDigit(_formula[_index + 1]))))
                {
                    int start = _index;
                    if (_formula[_index] == '-')
                        _index++;

                    while (_index < _formula.Length && char.IsDigit(_formula[_index]))
                        _index++;

                    string numberText = _formula.Substring(start, _index - start);
                    return int.TryParse(numberText, out int number) ? number : 0;
                }

                int tokenStart = _index;
                while (_index < _formula.Length && IsTokenChar(_formula[_index]))
                    _index++;

                if (tokenStart == _index)
                    return 0;

                string token = _formula.Substring(tokenStart, _index - tokenStart);
                return _resolveToken(token);
            }

            private static bool IsTokenChar(char c)
            {
                return char.IsLetterOrDigit(c) || c == '_' || c == '.';
            }

            private void SkipWhitespace()
            {
                while (_index < _formula.Length && char.IsWhiteSpace(_formula[_index]))
                    _index++;
            }

            private bool Match(char expected)
            {
                SkipWhitespace();
                if (_index >= _formula.Length || _formula[_index] != expected)
                    return false;

                _index++;
                return true;
            }
        }
    }
}
