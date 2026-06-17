using Modules.Definitions.Scripts.Implementation.Adventures;
using Modules.State.Scripts.Core;
using Modules.State.Scripts.Implementation.Adventure.Actions;
using Modules.State.Scripts.Implementation.Adventure.Logic;
using Modules.State.Scripts.Implementation.Adventure.StateDatas;
using Modules.State.Scripts.Implementation.Wallet.Actions;
using Modules.State.Scripts.Implementation.Wallet.StateDatas;
using Modules.Utils.Scripts.Extensions;
using System;
using System.Collections.Generic;
using Zenject;

namespace Modules.State.Scripts.Implementation.Adventure
{
    public class AdventureStateManager : StateManager<StateData>
    {
        [Inject] private readonly DefinitionsManager _definitionsManager;
        [Inject] private readonly LazyInject<AdventureStateLogic> _stateLogic;

        public void SaveState()
        {
            var settings = _definitionsManager.GlobalSettings;
            SaveProfileState(settings.SaveName, settings.EnabledEncryption, settings.EncryptionKey);
        }

        protected override void OnApplicationQuitting()
        {
            UnityEngine.Debug.Log("[AdventureStateManager] OnApplicationQuitting()");

            //------------------------------------------------------

            UnityEngine.Debug.LogError($"TODO: delete after test!");

            //var walletResult = _stateLogic.Value.ProcessAction(
            //    new ChangeWalletResourceStateAction<StateData>(WalletResourceType.Coin, 100));
            //if (!walletResult.IsValid)
            //    UnityEngine.Debug.LogWarning($"[AdventureStateManager] Wallet action failed: {walletResult.ErrorMessage}");

            var profileResult = _stateLogic.Value.ProcessAction(
                new SetProfileUpdateTimeStateAction(DateTime.UtcNow.ToUnixMs()));
            if (!profileResult.IsValid)
                UnityEngine.Debug.LogWarning($"[AdventureStateManager] Profile action failed: {profileResult.ErrorMessage}");

            //------------------------------------------------------

            SaveState();
        }

        protected override StateData CreateNewState(string profileId)
        {
            return new StateData
            {
                Profile = CreateNewProfileState(),
                Wallet = CreateNewWalletState(),
                Characters = CreateNewCharactersState(),
                Adventures = CreateNewAdventuresState(),
            };
        }

        private ProfileStateData CreateNewProfileState()
        {
            var now = DateTime.UtcNow.ToUnixMs();
            return new ProfileStateData
            {
                CreateTime = now,
                UpdateTime = now,
            };
        }

        private WalletStateData CreateNewWalletState()
        {
            return new WalletStateData
            {
                Resources = new Dictionary<WalletResourceType, int>(),
            };
        }

        private CharactersStateData CreateNewCharactersState()
        {
            return new CharactersStateData();
        }

        private AdventuresStateData CreateNewAdventuresState()
        {
            return new AdventuresStateData();
        }
    }
}
