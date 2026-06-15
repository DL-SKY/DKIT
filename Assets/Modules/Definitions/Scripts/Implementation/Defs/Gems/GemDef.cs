using Modules.Definitions.Scripts.Defs;
using System.Collections.Generic;

namespace Modules.Definitions.Scripts.Implementation.Defs.Gems
{
    public class GemDef : AbstractDefinition
    {
        public List<string> Tags;

        public string PrefabPath;

        public Dictionary<int, MatchActionsData> MatchCountActions;
    }

    public enum MatchActionType
    { 
        NA = 0,

        TurnsChange = 1,
        ScoreChange = 2,
    }

    public class MatchAction
    {
        public MatchActionType Type;

        public int IntParameter1;
        public int IntParameter2;
        public int IntParameter3;

        public string StringParameter1;
        public string StringParameter2;
        public string StringParameter3;
    }

    public class MatchActionsData
    {
        public List<MatchAction> Actions;
    }
}
