using Modules.State.Scripts.Implementation.Adventure.StateDatas;
using Modules.State.Scripts.Implementation.Wallet.Interfaces;
using Modules.State.Scripts.Implementation.Wallet.StateDatas;
using Modules.State.Scripts.Interfaces;

namespace Modules.State.Scripts.Implementation.Adventure
{
    public class StateData : IStateData, IWalletStateDataOwner
    {
        public ProfileStateData Profile;

        public WalletStateData Wallet;

        public CharactersStateData Characters;
        public AdventuresStateData Adventures;

        public WalletStateData GetWalletStateData()
        {
            return Wallet;
        }
    }
}
