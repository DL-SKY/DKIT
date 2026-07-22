namespace Modules.Definitions.Scripts.Implementation.Adventures.Constants
{
    //
    //Storyforge
    //Adventure Tavern by Martha
    //

    public static class Glossary
    {
        public static class Rules
        {
            // --- Tags ---

            // --- Parameters ---
        }

        public static class World
        {
            // --- Tags ---

            // --- Parameters ---
        }

        public static class Adventures
        {
            // --- Tags ---
            public const string HUB = "HUB";

            // --- Parameters ---
        }

        public static class Characters
        {
            // --- Tags ---

            // --- Parameters ---

            public const string STR = "STR";    // Сила
            public const string DEX = "DEX";    // Лвкость
            public const string CON = "CON";    // Телосложение
            public const string INT = "INT";    // Интеллект
            public const string WIS = "WIS";    // Мудрость
            public const string CHA = "CHA";    // Харизма

            public const string ARMOR_CLASS = "AC"; // Класс Брони

            public const string MAX_HIT_POINTS = "MaxHitPoints";
            public const string HIT_POINTS = "HitPoints";

            public const string PERCEPTION = "Perception";  // Восприятие

            public const string ACROBATICS = "Acrobatics";  // Акробатика
            public const string ARCANA = "Arcana";  // Мистицизм
            public const string ATHLETICS = "Athletics";  // Атлетика
            public const string CRAFTING = "Crafting";  // Ремесло
            public const string DECEPTION = "Deception";  // Обман
            public const string DIPLOMACY = "Diplomacy";  // Дипломатия
            public const string INTIMIDATION = "Intimidation";  // Запугивание
            public const string LORE = "Lore";  // Знания
            public const string MEDICINE = "Medicine";  // Медицина
            public const string NATURE = "Nature";  // Природа
            public const string OCCULTISM = "Occultism";  // Оккультизм
            public const string PERFORMANCE = "Performance";  // Выступление
            public const string RELIGION = "Religion";  // Религия
            public const string SOCIETY = "Society";  // Общество
            public const string STEALTH = "Stealth";  // Скрытность
            public const string SURVIVAL = "Survival";  // Выживание
            public const string THIEVERY = "Thievery";  // Воровство
        }

        public static class Classes
        {
            // --- Tags ---

            public const string ALCHEMIST = "Alchemist";
            public const string ANIMIST = "Animist";
            public const string BARBARIAN = "Barbarian";
            public const string BARD = "Bard";
            public const string CHAMPION = "Champion";
            public const string CLERIC = "Cleric";
            public const string COMMANDER = "Commander";
            public const string DRUID = "Druid";
            public const string FIGHTER = "Fighter";
            public const string GUARDIAN = "Guardian";
            public const string INVESTIGATOR = "Investigator";
            public const string KINETICIST = "Kineticist";
            public const string MAGUS = "Magus";
            public const string MONK = "Monk";
            public const string ORACLE = "Oracle";
            public const string PSYCHIC = "Psychic";
            public const string RANGER = "Ranger";
            public const string ROGUE = "Rogue";
            public const string SORCERER = "Sorcerer";
            public const string SUMMONER = "Summoner";
            public const string SWASHBUCKLER = "Swashbuckler";
            public const string THAUMATURGE = "Thaumaturge";
            public const string WITCH = "Witch";
            public const string WIZARD = "Wizard";

            // --- Parameters ---
        }

        public static class Ancestries
        {
            // --- Tags ---

            public const string DWARF = "Dwarf";
            public const string ELF = "Elf";
            public const string GNOME = "Gnome";
            public const string GOBLIN = "Goblin";
            public const string HALFLING = "Halfling";
            public const string HUMAN = "Human";
            public const string LESHY = "Leshy";
            public const string ORC = "Orc";

            // --- Parameters ---
        }

        public static class Feats
        {
            // --- Tags ---

            // --- Parameters ---
        }

        public static class Items
        {
            // --- Tags ---

            // --- Parameters ---
        }

        public static class Spells
        {
            // --- Tags ---

            // --- Parameters ---
        }

        public static class ChoiceActions
        {
            public const string SCENE_ID = "SceneId";
            public const string ADVENTURE_ID = "AdventureId";
            public const string WINDOW_ID = "WindowId";

            /// <summary>
            /// Separator for multiple scene ids in
            /// <see cref="Modules.RPG.Scripts.Adventure.Choice.Actions.ChoiceActionType.GoToRandomScene"/>
            /// (<c>Params.Strings[SceneId]</c>).
            /// </summary>
            public const string SCENE_IDS_SEPARATOR = ";";
        }

        /// <summary>
        /// Window ids for <see cref="Modules.RPG.Scripts.Adventure.Choice.Actions.ChoiceActionType.OpenWindow"/>.
        /// </summary>
        public static class Windows
        {
            public const string CREATE_CHARACTER = "CreateCharacter";
            public const string SELECT_CHARACTER = "SelectCharacter";
            public const string TRADE = "Trade";
            public const string PARTY = "Party";
            public const string ADVENTURE_LIST = "AdventureList";
        }
    }
}
