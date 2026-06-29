using Modules.State.Scripts.Implementation.Adventure.StateDatas;
using Modules.State.Scripts.Implementation.Wallet.StateDatas;
using Modules.Utils.Scripts.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Modules.State.Scripts.Implementation.Adventure.Factories
{
    public class AdventureStateDataFactory : IAdventureStateDataFactory
    {
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
            };
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
                Characters = new Dictionary<int, CharacterStateData>(),
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
                    Parameters = CreateEmptyAdventureStateParamsData(),
                },
                Adventures = new Dictionary<string, AdventureStateData>(),
            };
        }

        private static AdventureStateParamsData CreateEmptyAdventureStateParamsData()
        {
            return new AdventureStateParamsData
            {
                Strings = new Dictionary<string, string>(),
                Ints = new Dictionary<string, int>(),
                Bools = new Dictionary<string, bool>(),
            };
        }
    }
}
