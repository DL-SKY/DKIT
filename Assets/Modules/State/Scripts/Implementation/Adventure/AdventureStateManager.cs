using Modules.Definitions.Scripts.Implementation.Adventures;
using Modules.State.Scripts.Core;
using Modules.State.Scripts.Implementation.Adventure.Actions;
using Modules.State.Scripts.Implementation.Adventure.Factories;
using Modules.State.Scripts.Implementation.Adventure.Logic;
using Modules.Utils.Scripts.Extensions;
using System;
using Zenject;

namespace Modules.State.Scripts.Implementation.Adventure
{
    public class AdventureStateManager : StateManager<StateData>
    {
        [Inject] private readonly DiContainer _container;
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
            var factory = _container.Resolve<IAdventureStateDataFactory>();
            return factory.Create(profileId);
        }
    }
}
