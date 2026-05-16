using Modules.State.Scripts.Interfaces;

namespace Modules.State.Scripts.Implementation.Match3
{
    public class StateData : IStateData
    {
        public string ProfileId;
        public int PlayerLevel;
        public int Coins;
        public int Lives;
        public string CurrentRoundDefId;
        public long LastSaveUtcTicks;
    }
}
