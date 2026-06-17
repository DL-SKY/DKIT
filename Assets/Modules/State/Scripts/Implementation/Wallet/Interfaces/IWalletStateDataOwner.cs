using Modules.State.Scripts.Implementation.Wallet.StateDatas;

namespace Modules.State.Scripts.Implementation.Wallet.Interfaces
{
    public interface IWalletStateDataOwner
    {
        WalletStateData GetWalletStateData();
    }
}
