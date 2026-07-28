using Modules.Definitions.Scripts.Implementation.Adventures;
using Modules.State.Scripts.Implementation.Adventure.StateDatas;

namespace Modules.State.Scripts.Implementation.Adventure.Conditions
{
    public class ConditionTickContext
    {
        public string ConditionFeatId;
        public CharacterStateData Character;
        public DefinitionsManager DefinitionsManager;
    }
}
