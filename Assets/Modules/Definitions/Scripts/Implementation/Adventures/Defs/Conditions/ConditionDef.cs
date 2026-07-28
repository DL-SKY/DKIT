using Modules.Definitions.Scripts.Defs;
using Modules.Dices.Scripts;
using System.Collections.Generic;

namespace Modules.Definitions.Scripts.Implementation.Adventures.Defs.Conditions
{
    /// <summary>
    /// Optional advanced runtime settings for a condition feat with the same id.
    /// Id of this definition must match the id of a condition FeatDef.
    /// </summary>
    public class ConditionDef : AbstractDefinition
    {
        public bool Disabled;

        public List<string> Tags;

        public string Title;
        public string Description;

        /// <summary>
        /// Optional tick settings executed on character turn end while the condition timer is active.
        /// </summary>
        public ConditionTickData Tick;
    }

    /// <summary>
    /// Tick behavior settings for a condition.
    /// All fields are optional and can be combined.
    /// </summary>
    public class ConditionTickData
    {
        public bool Disabled;

        /// <summary>
        /// Flat HP damage applied each tick.
        /// </summary>
        public int FlatDamage;

        /// <summary>
        /// Extra dice damage applied each tick (for example 1d6 or 2d4).
        /// </summary>
        public List<ConditionDamageDicePartData> DamageDice;

        /// <summary>
        /// Optional semantic type key for consumers (for example Fire, Poison).
        /// </summary>
        public string DamageType;

        /// <summary>
        /// Optional per-tick patch applied via CharacterParametersOperator.ApplyPatch.
        /// </summary>
        public CharacterParamsPatchData TickPatch;
    }

    public class ConditionDamageDicePartData
    {
        public int Count;
        public DiceType DiceType;
    }
}
