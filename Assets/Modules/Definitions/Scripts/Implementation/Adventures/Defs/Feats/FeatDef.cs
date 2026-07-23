using Modules.Definitions.Scripts.Defs;
using Modules.Definitions.Scripts.Implementation.Adventures.Defs;
using Modules.Restrictions.Scripts.Core;
using System.Collections.Generic;

namespace Modules.Definitions.Scripts.Implementation.Adventures.Defs.Feats
{
    public enum FeatType
    {
        AncestryFeat = 0,
        BackgroundSkillFeat = 1,
        SkillFeat = 2,
        GeneralFeat = 3,
        ClassFeat = 4,
        ClassFeature = 5,
        Boost = 6,
    }


    public class FeatDef : AbstractDefinition
    {
        public bool Disabled;

        public List<Restriction> Restrictions;

        public List<string> Tags;

        public string Icon;
        public string Title;
        public string Description;

        public FeatType Type;
        public int Level;

        /// <summary>
        /// Статический эффект черты при взятии или при полном пересчёте билда персонажа.
        /// </summary>
        public CharacterParamsPatchData Apply;

        /// <summary>
        /// Для черт с выбором (например ClassFeature): id дочерних feat, один из которых выбирает игрок.
        /// </summary>
        public List<string> Options;
    }
}
