using Modules.Definitions.Scripts.Implementation.Adventures;
using Modules.Definitions.Scripts.Implementation.Adventures.Constants;
using Modules.Definitions.Scripts.Implementation.Adventures.Defs.Ancestries;
using Modules.Definitions.Scripts.Implementation.Adventures.Defs.Classes;
using Modules.Definitions.Scripts.Implementation.Adventures.Defs.Rules;
using Modules.State.Scripts.Implementation.Adventure.StateDatas;
using System;
using System.Collections.Generic;

namespace Modules.State.Scripts.Implementation.Adventure
{
    /// <summary>
    /// Read API параметров персонажа: сырые значения из <see cref="CharacterStateData.Parameters"/>
    /// и итоги по <see cref="RuleDef.ParameterFormulas"/>.
    /// </summary>
    /// <remarks>
    /// Конвенция (закреплена документацией; компилятором не enforced): читать через этот proxy
    /// (<see cref="GetRawValue"/> / <see cref="GetTotalValue"/>), а не индексировать
    /// <see cref="CharacterStateData.Parameters"/> напрямую.
    /// Мутации сырых параметров — у <see cref="CharacterParametersOperator"/> (Apply / Unapply
    /// <c>CharacterParamsPatchData</c>), не у этого класса.
    /// </remarks>
    public class CharacterParametersProxy
    {
        private const string PROFICIENCY_KEYWORD = "PROFICIENCY";
        private const string ITEMS_KEYWORD = "ITEMS";
        private const string ANCESTRY_HP_KEYWORD = "ANCESTRY_HP";
        private const string CLASS_HP_KEYWORD = "CLASS_HP";
        private const string PER_LEVEL_KEYWORD = "PER_LEVEL";
        private const string BONUS_KEYWORD = "BONUS";

        private readonly CharacterStateData _characterState;
        private readonly RuleDef _ruleDef;
        private readonly DefinitionsManager _definitionsManager;
        private readonly HashSet<string> _formulaKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            PROFICIENCY_KEYWORD,
            ITEMS_KEYWORD,
            ANCESTRY_HP_KEYWORD,
            CLASS_HP_KEYWORD,
            PER_LEVEL_KEYWORD,
            BONUS_KEYWORD,
        };

        public CharacterParametersProxy(
            CharacterStateData characterState,
            RuleDef ruleDef,
            DefinitionsManager definitionsManager)
        {
            _characterState = characterState;
            _ruleDef = ruleDef;
            _definitionsManager = definitionsManager;
        }

        /// <summary>
        /// Returns the resolved parameter value: evaluates <see cref="RuleDef.ParameterFormulas"/> when present,
        /// otherwise returns the raw value from <see cref="CharacterStateData.Parameters"/> (or 0).
        /// Supports +, *, and parentheses in formulas.
        /// </summary>
        public int GetTotalValue(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return 0;

            if (_ruleDef?.ParameterFormulas != null
                && _ruleDef.ParameterFormulas.TryGetValue(key, out string formula)
                && !string.IsNullOrWhiteSpace(formula))
            {
                return EvaluateFormula(key, formula);
            }

            return GetRawValue(key);
        }

        public int GetRawValue(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return 0;

            if (_characterState?.Parameters == null)
                return 0;

            return _characterState.Parameters.TryGetValue(key, out int value) ? value : 0;
        }

        private int EvaluateFormula(string requestedKey, string formula)
        {
            var parser = new FormulaParser(formula, token => EvaluateFormulaToken(requestedKey, token));
            return parser.ParseExpression();
        }

        private int EvaluateFormulaToken(string requestedKey, string token)
        {
            if (_formulaKeywords.Contains(token))
            {
                if (string.Equals(token, PROFICIENCY_KEYWORD, StringComparison.OrdinalIgnoreCase))
                    return EvaluateProficiency(requestedKey);

                if (string.Equals(token, ITEMS_KEYWORD, StringComparison.OrdinalIgnoreCase))
                    return EvaluateItemsBonus(requestedKey);

                if (string.Equals(token, ANCESTRY_HP_KEYWORD, StringComparison.OrdinalIgnoreCase))
                    return EvaluateAncestryHitPoints();

                if (string.Equals(token, CLASS_HP_KEYWORD, StringComparison.OrdinalIgnoreCase))
                    return EvaluateClassHitPointsPerLevel();

                if (string.Equals(token, PER_LEVEL_KEYWORD, StringComparison.OrdinalIgnoreCase))
                    return GetRawValue(requestedKey + Glossary.Characters.PER_LEVEL_SUFFIX);

                if (string.Equals(token, BONUS_KEYWORD, StringComparison.OrdinalIgnoreCase))
                    return GetRawValue(requestedKey + Glossary.Characters.BONUS_SUFFIX);
            }

            return GetRawValue(token);
        }

        private int EvaluateAncestryHitPoints()
        {
            if (_definitionsManager?.Ancestries == null
                || string.IsNullOrWhiteSpace(_characterState?.Ancestry)
                || !_definitionsManager.Ancestries.TryGetValue(_characterState.Ancestry, out AncestryDef ancestryDef)
                || ancestryDef == null)
            {
                return 0;
            }

            return ancestryDef.HitPoints;
        }

        private int EvaluateClassHitPointsPerLevel()
        {
            if (_definitionsManager?.Classes == null
                || string.IsNullOrWhiteSpace(_characterState?.Class)
                || !_definitionsManager.Classes.TryGetValue(_characterState.Class, out ClassDef classDef)
                || classDef == null)
            {
                return 0;
            }

            return classDef.HitPointsPerLevel;
        }

        private int EvaluateProficiency(string requestedKey)
        {
            string proficiencyKey = requestedKey + Glossary.Characters.PROFICIENCY_SUFFIX;
            int rankValue = GetRawValue(proficiencyKey);
            if (rankValue < (int)ProficiencyType.Untrained || rankValue > (int)ProficiencyType.Legendary)
                return 0;

            ProficiencyType proficiencyType = (ProficiencyType)rankValue;
            if (proficiencyType == ProficiencyType.Untrained)
                return 0;

            int level = GetRawValue(Glossary.Characters.LEVEL);
            int rankBonus = proficiencyType switch
            {
                ProficiencyType.Trained => 2,
                ProficiencyType.Expert => 4,
                ProficiencyType.Master => 6,
                ProficiencyType.Legendary => 8,
                _ => 0,
            };

            return level + rankBonus;
        }

        private int EvaluateItemsBonus(string requestedKey)
        {
            string itemsKey = requestedKey + Glossary.Characters.ITEMS_SUFFIX;
            return GetRawValue(itemsKey);
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
                _index = 0;
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
                    {
                        value *= ParseFactor();
                    }
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
