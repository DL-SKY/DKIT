using Modules.Definitions.Scripts.Defs;
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
        public string Title;
        public string Description;
        public FeatType Type;
        public int Level;
        public List<string> Prerequisites;
    }
}
