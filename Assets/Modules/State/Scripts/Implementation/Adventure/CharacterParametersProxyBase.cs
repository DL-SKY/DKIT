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
    /// Shared Read API for character parameters: raw values from a parameters dictionary
    /// and totals via <see cref="RuleDef.ParameterFormulas"/>.
    /// Source of parameters / ancestry / class is provided by subclasses.
    /// </summary>
    public abstract class CharacterParametersProxyBase
    {
        private readonly RuleDef _ruleDef;
        private readonly DefinitionsManager _definitionsManager;
        private readonly HashSet<string> _formulaKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Glossary.Characters.PROFICIENCY_KEYWORD,
            Glossary.Characters.ITEMS_KEYWORD,
            Glossary.Characters.ANCESTRY_HP_KEYWORD,
            Glossary.Characters.CLASS_HP_KEYWORD,
            Glossary.Characters.PER_LEVEL_KEYWORD,
            Glossary.Characters.BONUS_KEYWORD,
        };

        protected CharacterParametersProxyBase(RuleDef ruleDef, DefinitionsManager definitionsManager)
        {
            _ruleDef = ruleDef;
            _definitionsManager = definitionsManager;
        }

        /// <summary>
        /// Raw parameter dictionary for the character source (state or create request).
        /// </summary>
        protected abstract Dictionary<string, int> Parameters { get; }

        /// <summary>
        /// Ancestry definition id used for <c>ANCESTRY_HP</c> formula tokens.
        /// </summary>
        protected abstract string AncestryId { get; }

        /// <summary>
        /// Class definition id used for <c>CLASS_HP</c> formula tokens.
        /// </summary>
        protected abstract string ClassId { get; }

        /// <summary>
        /// Returns the resolved parameter value: evaluates <see cref="RuleDef.ParameterFormulas"/> when present,
        /// otherwise returns the raw value from <see cref="Parameters"/> (or 0).
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

            Dictionary<string, int> parameters = Parameters;
            if (parameters == null)
                return 0;

            return parameters.TryGetValue(key, out int value) ? value : 0;
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
                if (string.Equals(token, Glossary.Characters.PROFICIENCY_KEYWORD, StringComparison.OrdinalIgnoreCase))
                    return EvaluateProficiency(requestedKey);

                if (string.Equals(token, Glossary.Characters.ITEMS_KEYWORD, StringComparison.OrdinalIgnoreCase))
                    return EvaluateItemsBonus(requestedKey);

                if (string.Equals(token, Glossary.Characters.ANCESTRY_HP_KEYWORD, StringComparison.OrdinalIgnoreCase))
                    return EvaluateAncestryHitPoints();

                if (string.Equals(token, Glossary.Characters.CLASS_HP_KEYWORD, StringComparison.OrdinalIgnoreCase))
                    return EvaluateClassHitPointsPerLevel();

                if (string.Equals(token, Glossary.Characters.PER_LEVEL_KEYWORD, StringComparison.OrdinalIgnoreCase))
                    return GetRawValue(requestedKey + Glossary.Characters.PER_LEVEL_SUFFIX);

                if (string.Equals(token, Glossary.Characters.BONUS_KEYWORD, StringComparison.OrdinalIgnoreCase))
                    return GetRawValue(requestedKey + Glossary.Characters.BONUS_SUFFIX);
            }

            return GetRawValue(token);
        }

        private int EvaluateAncestryHitPoints()
        {
            if (_definitionsManager?.Ancestries == null
                || string.IsNullOrWhiteSpace(AncestryId)
                || !_definitionsManager.Ancestries.TryGetValue(AncestryId, out AncestryDef ancestryDef)
                || ancestryDef == null)
            {
                return 0;
            }

            return ancestryDef.HitPoints;
        }

        private int EvaluateClassHitPointsPerLevel()
        {
            if (_definitionsManager?.Classes == null
                || string.IsNullOrWhiteSpace(ClassId)
                || !_definitionsManager.Classes.TryGetValue(ClassId, out ClassDef classDef)
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
