using Modules.Definitions.Scripts.Implementation.Adventures;
using Modules.State.Scripts.Core;
using Modules.State.Scripts.Implementation.Adventure.StateDatas;
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

        public void SaveState()
        {
            var settings = _definitionsManager.GlobalSettings;
            SaveProfileState(settings.SaveName, settings.EnabledEncryption, settings.EncryptionKey);
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
