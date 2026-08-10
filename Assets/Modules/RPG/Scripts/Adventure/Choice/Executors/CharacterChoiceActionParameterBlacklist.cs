using System.Collections.Generic;
using static Modules.Definitions.Scripts.Implementation.Adventures.Constants.Glossary;

namespace Modules.RPG.Scripts.Adventure.Choice.Executors
{
    internal static class CharacterChoiceActionParameterBlacklist
    {
        private static readonly HashSet<string> FORBIDDEN_KEYS = new HashSet<string>
        {
            Characters.BOOST_POINTS,

            Characters.STR,
            Characters.DEX,
            Characters.CON,
            Characters.INT,
            Characters.WIS,
            Characters.CHA,

            Characters.SAVING_FORTITUDE,
            Characters.SAVING_REFLEX,
            Characters.SAVING_WILL,
            Characters.PERCEPTION,

            Characters.ACROBATICS,
            Characters.ARCANA,
            Characters.ATHLETICS,
            Characters.CRAFTING,
            Characters.DECEPTION,
            Characters.DIPLOMACY,
            Characters.INTIMIDATION,
            Characters.LORE,
            Characters.MEDICINE,
            Characters.NATURE,
            Characters.OCCULTISM,
            Characters.PERFORMANCE,
            Characters.RELIGION,
            Characters.SOCIETY,
            Characters.STEALTH,
            Characters.SURVIVAL,
            Characters.THIEVERY,
        };

        public static bool Contains(string parameterKey)
        {
            if (string.IsNullOrWhiteSpace(parameterKey))
                return false;

            return FORBIDDEN_KEYS.Contains(parameterKey);
        }
    }
}
