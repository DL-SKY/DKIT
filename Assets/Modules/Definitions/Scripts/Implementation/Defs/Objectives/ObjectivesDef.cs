using Modules.Definitions.Scripts.Defs;
using Modules.ECS.Scripts.Match3.Components;
using Modules.Restrictions.Scripts.Core;
using System.Collections.Generic;

namespace Modules.Definitions.Scripts.Implementation.Defs.Objectives
{
    public class ObjectivesDef : AbstractDefinition
    {
        public List<ScoreData> StartScores;
        public List<Restriction> VictoryConditions;
        public List<Restriction> DefeatConditions;
    }
}
