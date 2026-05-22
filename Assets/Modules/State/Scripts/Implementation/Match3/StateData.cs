using Modules.State.Scripts.Implementation.Match3.StateDatas;
using Modules.State.Scripts.Interfaces;

namespace Modules.State.Scripts.Implementation.Match3
{
    public class StateData : IStateData
    {
        public ProfileStateData Profile;

        public WalletStateData Wallet;

        public HangarStateData Hangar;
        public StorageStateData Storage;
    }
}
