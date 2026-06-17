using Modules.State.Scripts.Actions.Core;
using Modules.State.Scripts.Actions.Models;
using Modules.State.Scripts.Implementation.Wallet.Interfaces;
using Modules.State.Scripts.Implementation.Wallet.StateDatas;
using Modules.State.Scripts.Interfaces;
using System.Collections.Generic;

namespace Modules.State.Scripts.Implementation.Wallet.Actions
{
    public class SetWalletResourceStateAction<TStateData> : StateActionBase<TStateData>
        where TStateData : class, IStateData, IWalletStateDataOwner, new()
    {
        private readonly WalletResourceType _resourceType;
        private readonly int _amount;

        public SetWalletResourceStateAction(WalletResourceType resourceType, int amount)
        {
            _resourceType = resourceType;
            _amount = amount;
        }

        public override StateActionValidationResult Validate(TStateData state)
        {
            var wallet = state.GetWalletStateData();
            if (wallet == null)
                return StateActionValidationResult.Fail("Wallet state is null.", 10);

            if (_amount < 0)
                return StateActionValidationResult.Fail("Wallet resource amount can not be negative.", 11);

            return StateActionValidationResult.Ok;
        }

        public override void Execute(TStateData state)
        {
            var wallet = state.GetWalletStateData();
            if (wallet.Resources == null)
                wallet.Resources = new Dictionary<WalletResourceType, int>();

            wallet.Resources[_resourceType] = _amount;
        }
    }
}
