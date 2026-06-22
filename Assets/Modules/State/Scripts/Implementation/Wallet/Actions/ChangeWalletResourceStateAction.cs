using Modules.State.Scripts.Actions.Core;
using Modules.State.Scripts.Actions.Models;
using Modules.State.Scripts.Implementation.Wallet.Interfaces;
using Modules.State.Scripts.Implementation.Wallet.StateDatas;
using Modules.State.Scripts.Interfaces;
using System.Collections.Generic;

namespace Modules.State.Scripts.Implementation.Wallet.Actions
{
    public class ChangeWalletResourceStateAction<TStateData> : StateActionBase<TStateData>
        where TStateData : class, IStateData, IWalletStateDataOwner, new()
    {
        public override StateChangeSource Source => StateChangeSource.ChangeWalletResource;

        private readonly WalletResourceType _resourceType;
        private readonly int _delta;

        public ChangeWalletResourceStateAction(WalletResourceType resourceType, int delta)
        {
            _resourceType = resourceType;
            _delta = delta;
        }

        public override StateActionValidationResult Validate(TStateData state)
        {
            var wallet = state.GetWalletStateData();
            if (wallet == null)
                return StateActionValidationResult.Fail("Wallet state is null.", 10);

            var currentAmount = 0;
            if (wallet.Resources != null)
                wallet.Resources.TryGetValue(_resourceType, out currentAmount);

            if (currentAmount + _delta < 0)
                return StateActionValidationResult.Fail("Wallet resource amount can not be negative.", 11);

            return StateActionValidationResult.Ok;
        }

        public override void Execute(TStateData state)
        {
            var wallet = state.GetWalletStateData();
            if (wallet.Resources == null)
                wallet.Resources = new Dictionary<WalletResourceType, int>();

            var currentAmount = 0;
            wallet.Resources.TryGetValue(_resourceType, out currentAmount);

            var newAmount = currentAmount + _delta;
            wallet.Resources[_resourceType] = newAmount;
        }
    }
}
