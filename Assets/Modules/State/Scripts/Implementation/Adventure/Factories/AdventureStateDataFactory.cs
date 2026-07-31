using Modules.Definitions.Scripts.Implementation.Adventures;
using Modules.State.Scripts.Implementation.Adventure;
using Modules.State.Scripts.Implementation.Adventure.StateDatas;
using Modules.State.Scripts.Implementation.Wallet.StateDatas;
using Modules.Utils.Scripts.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Modules.State.Scripts.Implementation.Adventure.Factories
{
    public class AdventureStateDataFactory : IAdventureStateDataFactory
    {
        [Inject] private readonly DefinitionsManager _definitionsManager;

        public StateData Create(string profileId)
        {
            return new StateData
            {
                Profile = CreateNewProfileState(),
                Wallet = CreateNewWalletState(),
                Localization = CreateNewLocalizationState(),
                Characters = CreateNewCharactersState(),
                Inventory = CreateNewInventoryState(),
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
                Parameters = CreateDefaultProfileParameters(),
            };
        }

        private Dictionary<string, int> CreateDefaultProfileParameters()
        {
            var defaults = _definitionsManager?.ProfileStateSettings?.DefaultParameters;
            return defaults != null
                ? new Dictionary<string, int>(defaults)
                : new Dictionary<string, int>();
        }

        private WalletStateData CreateNewWalletState()
        {
            return new WalletStateData
            {
                Resources = new Dictionary<WalletResourceType, int>(),
            };
        }

        private LocalizationStateData CreateNewLocalizationState()
        {
            return new LocalizationStateData
            {
                Language = SystemLanguage.Unknown,
            };
        }

        private CharactersStateData CreateNewCharactersState()
        {
            return new CharactersStateData
            {
                NextCharacterId = 1,
                HeroPoints = 0,
                Characters = new Dictionary<int, CharacterStateData>(),
                CurrentActiveCharacterId = 0,
                ActivePartyCharacterIds = new List<int>(),
            };
        }

        private InventoryStateData CreateNewInventoryState()
        {
            return new InventoryStateData
            {
                Items = new Dictionary<string, int>(),
            };
        }

        private AdventuresStateData CreateNewAdventuresState()
        {
            return new AdventuresStateData
            {
                World = new WorldStateData
                {
                    Parameters = AdventureStateParamsOperator.CreateEmpty(),
                },
                Global = new GlobalStateData
                {
                    Parameters = AdventureStateParamsOperator.CreateEmpty(),
                },
                Adventures = new Dictionary<string, AdventureStateData>(),
            };
        }

    }
}
