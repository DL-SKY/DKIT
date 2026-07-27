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

            public const string ATTACK = "ATTACK";
            public const string SPELL = "SPELL";

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

            public const string BOARD = "BOARD";
            public const string CHARACTER = "CHARACTER";
            public const string CROSSROADS = "CROSSROADS";
            public const string ENTRY = "ENTRY";
            public const string EXIT = "EXIT";
            public const string GUIDEBOOK = "GUIDEBOOK";
            public const string HUB = "HUB";
            public const string INTRO = "INTRO";
            public const string LOCATION = "LOCATION";
            public const string LOOK = "LOOK";
            public const string MAIN = "MAIN";
            public const string PARTY = "PARTY";
            public const string QUEST = "QUEST";
            public const string SAFE = "SAFE";
            public const string SOCIAL = "SOCIAL";
            public const string START = "START";
            public const string TAVERN = "TAVERN";
            public const string TRADE = "TRADE";
            public const string TUTORIAL = "TUTORIAL";

            // --- Parameters ---
        }

        public static class Characters
        {
            // --- Tags ---

            // --- Parameters ---

            public const string LEVEL = "Level";
            public const string EXPERIENCE = "Experience";

            public const string STR = "STR";    // Сила
            public const string DEX = "DEX";    // Ловкость
            public const string CON = "CON";    // Телосложение
            public const string INT = "INT";    // Интеллект
            public const string WIS = "WIS";    // Мудрость
            public const string CHA = "CHA";    // Харизма

            public const string ARMOR_CLASS = "AC"; // Класс брони

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
            public const string LORE = "Lore";  // Знание
            public const string MEDICINE = "Medicine";  // Медицина
            public const string NATURE = "Nature";  // Природа
            public const string OCCULTISM = "Occultism";  // Оккультизм
            public const string PERFORMANCE = "Performance";  // Выступление
            public const string RELIGION = "Religion";  // Религия
            public const string SOCIETY = "Society";  // Общество
            public const string STEALTH = "Stealth";  // Скрытность
            public const string SURVIVAL = "Survival";  // Выживание
            public const string THIEVERY = "Thievery";  // Воровство

            // --- Game Logic Suffix ---

            public const string PROFICIENCY_SUFFIX = ".ProfRank";  // Proficiency rank (enum ProficiencyType)
            public const string ITEMS_SUFFIX = ".ItemsBonus";  // Item bonus for a skill/parameter
            public const string PER_LEVEL_SUFFIX = ".PerLevel";  // Extra value applied each level (e.g. MaxHitPoints.PerLevel)
            public const string BONUS_SUFFIX = ".Bonus";  // Flat bonus for a computed parameter (e.g. MaxHitPoints.Bonus)
        }

        public static class Classes
        {
            // --- Tags ---

            public const string ALCHEMIST = "ALCHEMIST";
            public const string ANIMIST = "ANIMIST";
            public const string BARBARIAN = "BARBARIAN";
            public const string BARD = "BARD";
            public const string CHAMPION = "CHAMPION";
            public const string CLERIC = "CLERIC";
            public const string COMMANDER = "COMMANDER";
            public const string DRUID = "DRUID";
            public const string FIGHTER = "FIGHTER";
            public const string GUARDIAN = "GUARDIAN";
            public const string INVESTIGATOR = "INVESTIGATOR";
            public const string KINETICIST = "KINETICIST";
            public const string MAGUS = "MAGUS";
            public const string MONK = "MONK";
            public const string ORACLE = "ORACLE";
            public const string PSYCHIC = "PSYCHIC";
            public const string RANGER = "RANGER";
            public const string ROGUE = "ROGUE";
            public const string SORCERER = "SORCERER";
            public const string SUMMONER = "SUMMONER";
            public const string SWASHBUCKLER = "SWASHBUCKLER";
            public const string THAUMATURGE = "THAUMATURGE";
            public const string WITCH = "WITCH";
            public const string WIZARD = "WIZARD";

            // --- Parameters ---
        }

        public static class Ancestries
        {
            // --- Tags ---

            public const string DWARF = "DWARF";
            public const string ELF = "ELF";
            public const string GNOME = "GNOME";
            public const string GOBLIN = "GOBLIN";
            public const string HALFLING = "HALFLING";
            public const string HUMAN = "HUMAN";
            public const string LESHY = "LESHY";
            public const string ORC = "ORC";

            // --- Parameters ---
        }

        public static class Backgrounds
        {
            // --- Tags ---

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

            // --- Character Equipment Slots ---

            public const string SLOT_TYPE_HAND = "Hand";
            public const string SLOT_TYPE_LEGS = "Legs";
            public const string SLOT_TYPE_HEAD = "Head";
            public const string SLOT_TYPE_BODY = "Body";
            public const string SLOT_TYPE_BAG = "Bag";
        }

        public static class Weapons
        {
            // --- Types ---

            public const string SIMPLE = "Weapon.Simple";                    // Простое оружие
            public const string MARTIAL = "Weapon.Martial";                  // Воинское оружие
            public const string ADVANCED = "Weapon.Advanced";                // Продвинутое оружие

            // --- Groups ---

            public const string AXE = "Axe";                // Топоры
            public const string BRAWLING = "Brawling";      // Рукопашная (в т.ч. безоружные атаки)
            public const string CLUB = "Club";              // Дубинки
            public const string FLAIL = "Flail";            // Цепы
            public const string HAMMER = "Hammer";          // Молоты
            public const string KNIFE = "Knife";            // Ножи
            public const string PICK = "Pick";              // Кирки
            public const string POLEARM = "Polearm";        // Древковое
            public const string SHIELD = "Shield";          // Щиты
            public const string SPEAR = "Spear";            // Копья
            public const string SWORD = "Sword";            // Мечи
            public const string BOW = "Bow";                // Луки
            public const string CROSSBOW = "Crossbow";      // Арбалеты
            public const string DART = "Dart";              // Дротики
            public const string SLING = "Sling";            // Пращи
            public const string BOMB = "Bomb";              // Бомбы
            public const string FIREARM = "Firearm";        // Огнестрельное

            // --- Tags ---

            public const string AGILE = "AGILE";  // Быстрое (сниженный Multiple Attack Penalty)

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
