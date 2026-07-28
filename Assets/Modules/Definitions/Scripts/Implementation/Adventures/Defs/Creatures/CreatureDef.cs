using Modules.Definitions.Scripts.Defs;
using Modules.Definitions.Scripts.Implementation.Adventures.Defs.Ancestries;
using Modules.State.Scripts.Implementation.Adventure.StateDatas;
using System.Collections.Generic;

namespace Modules.Definitions.Scripts.Implementation.Adventures.Defs.Creatures
{
    /// <summary>
    /// Monster / NPC combatant template (stat block), authored like a Monster Manual entry.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Runtime battle does <b>not</b> use this type directly for Apply/Unapply.
    /// Convert with <c>CreatureCombatantFactory.CreateFromCreature</c> into
    /// <see cref="CharacterStateData"/> so player and NPC share the same combat shape.
    /// </para>
    /// <para>
    /// JSON path: <c>Definitions/_ADVENTURES_/Creatures</c>.
    /// Loaded into <c>DefinitionsManager.Creatures</c>.
    /// Practical guide: <c>.cursor/docs/modules/Creatures.md</c>.
    /// </para>
    /// <example>
    /// Lookup and spawn (session NPC id must be negative):
    /// <code>
    /// CreatureDef def = definitionsManager.Creatures["_GoblinWarrior"];
    /// CharacterStateData npc = CreatureCombatantFactory.CreateFromCreature(
    ///     def,
    ///     instanceId: -1,
    ///     definitionsManager);
    /// // Same status API as for the player:
    /// CharacterParametersOperator.ApplyFeat(npc, "_ConditionFrightened", definitionsManager);
    /// </code>
    /// </example>
    /// </remarks>
    public class CreatureDef : AbstractDefinition
    {
        public bool Disabled;

        public List<string> Tags;

        public string Icon;
        public string Title;
        public string Description;

        /// <summary>
        /// Creature level (PF2e-style power band / proficiency scaling).
        /// </summary>
        public int Level;

        /// <summary>
        /// Encounter budget / challenge rating (класс опасности).
        /// </summary>
        public int ChallengeRating;

        public AncestrySize Size;
        public int Speed;

        public int ArmorClass;
        public int HitPoints;

        /// <summary>
        /// Ability modifiers keyed by <c>Glossary.Characters</c> (<c>STR</c>…<c>CHA</c>).
        /// </summary>
        public Dictionary<string, int> Abilities;

        /// <summary>
        /// Final Perception modifier (stat-block total).
        /// </summary>
        public int Perception;

        /// <summary>
        /// Final save modifiers: <c>Fortitude</c> / <c>Reflex</c> / <c>Will</c>
        /// (<c>Glossary.Characters.FORTITUDE</c> / <c>REFLEX</c> / <c>WILL</c>).
        /// </summary>
        public Dictionary<string, int> Saves;

        /// <summary>
        /// Final skill modifiers for skills that matter (id → total).
        /// </summary>
        public Dictionary<string, int> Skills;

        /// <summary>
        /// Passive / special ability feat ids applied on spawn via <c>ApplyFeat</c>.
        /// </summary>
        public List<string> Features;

        /// <summary>
        /// Battle actions available to this creature (<see cref="BattleActions.BattleActionDef"/> ids).
        /// Kept on the def; battle session resolves them via source creature id.
        /// </summary>
        public List<string> BattleActionIds;

        public List<EquippedItemStateData> EquippedItems;
        public Dictionary<string, int> Spells;

        /// <summary>
        /// Optional raw parameter overrides merged last (escape hatch).
        /// </summary>
        public Dictionary<string, int> Parameters;

        /// <summary>
        /// Optional flavor links; not required for NPC combatants.
        /// </summary>
        public string Ancestry;
        public string Class;
        public string Background;

        public CharacterGender Gender;
    }
}
