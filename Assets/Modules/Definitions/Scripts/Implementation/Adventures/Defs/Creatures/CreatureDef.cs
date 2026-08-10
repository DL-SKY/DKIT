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

        /// <summary>
        /// Same semantic as <see cref="CharacterStateData.Avatar"/> / <see cref="PregeneratedCharacters.PregeneratedCharacterDef.Avatar"/>.
        /// Preferred field for new creature defs.
        /// </summary>
        public string Avatar;

        /// <summary>
        /// Same semantic as <see cref="CharacterStateData.Name"/> / <see cref="PregeneratedCharacters.PregeneratedCharacterDef.Name"/>.
        /// Preferred field for new creature defs.
        /// </summary>
        public string Name;

        /// <summary>
        /// Legacy avatar field kept for backward compatibility with existing content.
        /// New defs should use <see cref="Avatar"/>.
        /// </summary>
        public string Icon;

        /// <summary>
        /// Legacy display name field kept for backward compatibility with existing content.
        /// New defs should use <see cref="Name"/>.
        /// </summary>
        public string Title;

        /// <summary>
        /// Optional localized key or raw text. Not used directly by battle runtime.
        /// </summary>
        public string Description;

        /// <summary>
        /// Encounter budget / challenge rating (класс опасности), supports fractional values.
        /// </summary>
        public float ChallengeRating;

        public AncestrySize Size;
        public int Speed;

        public int ArmorClass;
        public int HitPoints;

        /// <summary>
        /// Battle actions available to this creature (<see cref="BattleActions.BattleActionDef"/> ids).
        /// Kept on the def; battle session resolves them via source creature id.
        /// </summary>
        public List<string> BattleActionIds;

        public List<EquippedItemStateData> EquippedItems;
        public Dictionary<string, int> Spells;

        /// <summary>
        /// Main numeric storage for creature stats, aligned with <see cref="CharacterStateData.Parameters"/>.
        /// Includes abilities, level, saves, authored totals for perception/skills, and optional overrides.
        /// </summary>
        public Dictionary<string, int> Parameters;

        /// <summary>
        /// Optional flavor links; not required for NPC combatants.
        /// </summary>
        public string Ancestry;
        public string Class;
        public string Background;

        public CharacterGender Gender;

        /// <summary>
        /// Optional initial timers for timed status effects (status id -&gt; remaining turns).
        /// Mirrors <see cref="CharacterStateData.StatusEffects"/> shape.
        /// </summary>
        public Dictionary<string, int> StatusEffects;
    }
}
