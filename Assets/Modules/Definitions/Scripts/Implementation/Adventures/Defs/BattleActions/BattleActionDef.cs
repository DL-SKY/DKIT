using Modules.Definitions.Scripts.Defs;
using Modules.Restrictions.Scripts.Core;
using System.Collections.Generic;

namespace Modules.Definitions.Scripts.Implementation.Adventures.Defs.BattleActions
{
    public enum BattleActionTargetType
    {
        Self = 0,
        AllySingle = 1,
        AllyAll = 2,
        EnemySingle = 10,
        EnemyAll = 11,
        RandomEnemy = 12,
    }

    public enum BattleActionEffectType
    {
        Damage = 0,
        Heal = 1,
        AddStatus = 2,
        RemoveStatus = 3,
        ModifyParam = 4,
    }

    /// <summary>
    /// Combat action catalog entry (Strike, CastSpell, class/feat options, etc.).
    /// Early battle-system prototype — runtime resolver is not implemented yet.
    /// </summary>
    public class BattleActionDef : AbstractDefinition
    {
        public bool Disabled;

        public List<string> Tags;

        public string Icon;
        public string Title;
        public string Description;

        /// <summary>
        /// Action-point cost from the side's turn pool.
        /// </summary>
        public int ActionCost;

        public BattleActionTargetType TargetType;

        /// <summary>
        /// Availability gate (e.g. <c>RestrictionType.CharacterParams</c> for feat-unlocked actions).
        /// Empty/null = always available (subject to other runtime checks).
        /// </summary>
        public List<Restriction> AvailabilityRestrictions;

        /// <summary>
        /// Optional: equipped weapon must have at least one of these tags.
        /// </summary>
        public List<string> RequiredWeaponTagsAny;

        /// <summary>
        /// Optional: id of <see cref="Spells.SpellDef"/>. Empty means spell is chosen at runtime (CastSpell wrapper).
        /// </summary>
        public string RequiredSpellId;

        public List<BattleActionEffectData> Effects;
    }

    public class BattleActionEffectData
    {
        public BattleActionEffectType Type;

        /// <summary>
        /// Flat value when no formula is used. For weapon damage, leave 0 and keep <see cref="ValueFormula"/> empty
        /// so runtime can resolve dice/modifier from the equipped weapon.
        /// </summary>
        public int Value;

        /// <summary>
        /// Optional expression for the effect magnitude. Empty = use <see cref="Value"/> or weapon damage for <see cref="BattleActionEffectType.Damage"/>.
        /// </summary>
        public string ValueFormula;

        public string StatusId;
        public int StatusValue;
        public int DurationRounds;

        /// <summary>
        /// When true, apply only after a successful attack hit-check.
        /// </summary>
        public bool OnHitOnly;
    }
}
