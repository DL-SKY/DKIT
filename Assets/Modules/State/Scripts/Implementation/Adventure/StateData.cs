using Modules.State.Scripts.Implementation.Adventure.StateDatas;
using Modules.State.Scripts.Implementation.Adventure.Interfaces;
using Modules.State.Scripts.Implementation.Wallet.Interfaces;
using Modules.State.Scripts.Implementation.Wallet.StateDatas;
using Modules.State.Scripts.Interfaces;

namespace Modules.State.Scripts.Implementation.Adventure
{
    public class StateData : IStateData, IWalletStateDataOwner, ILocalizationStateDataOwner
    {
        public ProfileStateData Profile;

        public WalletStateData Wallet;
        public LocalizationStateData Localization;

        public CharactersStateData Characters;
        public InventoryStateData Inventory;

        public AdventuresStateData Adventures;


        public WalletStateData GetWalletStateData()
        {
            return Wallet;
        }

        public LocalizationStateData GetLocalizationStateData()
        {
            return Localization;
        }
    }
}
