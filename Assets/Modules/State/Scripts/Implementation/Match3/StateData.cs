using Modules.State.Scripts.Implementation.Match3.StateDatas;
using Modules.State.Scripts.Implementation.Wallet.Interfaces;
using Modules.State.Scripts.Implementation.Wallet.StateDatas;
using Modules.State.Scripts.Interfaces;

namespace Modules.State.Scripts.Implementation.Match3
{
    public class StateData : IStateData, IWalletStateDataOwner
    {
        public ProfileStateData Profile;

        public WalletStateData Wallet;

        public HangarStateData Hangar;
        public StorageStateData Storage;

        public WalletStateData GetWalletStateData()
        {
            return Wallet;
        }
    }
}
