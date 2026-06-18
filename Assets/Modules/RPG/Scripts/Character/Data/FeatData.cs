using Modules.Definitions.Scripts.Defs;
using System.Collections.Generic;

namespace Modules.RPG.Scripts.Character.Data
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

    public class FeatData : AbstractDefinition
    {
        public string Title;
        public string Description;

        public FeatType Type;
        public int Level;

        /// <summary>
        /// Идентификаторы черт или текстовые условия предварительных требований.
        /// </summary>
        public List<string> Prerequisites;
    }
}
