using Modules.State.Scripts.Actions.Core;
using Modules.State.Scripts.Actions.Models;
using Modules.State.Scripts.Implementation.Adventure.Actions.Models;
using Modules.State.Scripts.Implementation.Adventure.StateDatas;
using Modules.Utils.Scripts.Extensions;
using System;
using System.Collections.Generic;

namespace Modules.State.Scripts.Implementation.Adventure.Actions
{
    public class CreateCharacterStateAction : StateActionBase<StateData>
    {
        public override StateChangeSource Source => StateChangeSource.CreateCharacter;

        private readonly CreateCharacterRequestData _request;

        public CreateCharacterStateAction(CreateCharacterRequestData request)
        {
            _request = request;
        }

        public override StateActionValidationResult Validate(StateData state)
        {
            if (state?.Characters == null)
                return StateActionValidationResult.Fail("Characters state is null.", 60);

            if (_request == null)
                return StateActionValidationResult.Fail("Create character request is null.", 61);

            if (string.IsNullOrWhiteSpace(_request.Name))
                return StateActionValidationResult.Fail("Character name is null or empty.", 62);

            if (string.IsNullOrWhiteSpace(_request.Ancestry))
                return StateActionValidationResult.Fail("Character ancestry id is null or empty.", 63);

            if (string.IsNullOrWhiteSpace(_request.Class))
                return StateActionValidationResult.Fail("Character class id is null or empty.", 64);

            List<EquippedItemStateData> equippedItems = _request.CharacterData?.EquippedItems;

            if (equippedItems != null)
            {
                for (int i = 0; i < equippedItems.Count; i++)
                {
                    EquippedItemStateData equipped = equippedItems[i];
                    if (equipped == null)
                        return StateActionValidationResult.Fail($"Equipped item at index {i} is null.", 65);

                    if (string.IsNullOrWhiteSpace(equipped.Slot))
                        return StateActionValidationResult.Fail($"Equipped item slot at index {i} is null or empty.", 66);
                }
            }

            if (state.Characters.NextCharacterId <= 0)
                return StateActionValidationResult.Fail("NextCharacterId must be greater than zero.", 67);

            if (state.Characters.Characters != null
                && state.Characters.Characters.ContainsKey(state.Characters.NextCharacterId))
            {
                return StateActionValidationResult.Fail("NextCharacterId already exists in characters dictionary.", 68);
            }

            return StateActionValidationResult.Ok;
        }

        public override void Execute(StateData state)
        {
            state.Characters ??= new CharactersStateData();
            state.Characters.Characters ??= new Dictionary<int, CharacterStateData>();
            state.Characters.ActivePartyCharacterIds ??= new List<int>();

            int characterId = state.Characters.NextCharacterId;
            CharacterRequestData characterData = _request.CharacterData;
            List<EquippedItemStateData> equippedItems = characterData?.EquippedItems;

            var character = new CharacterStateData
            {
                Id = characterId,
                CreateTime = DateTime.UtcNow.ToUnixMs(),
                IsDead = false,
                DeathTime = 0,
                Avatar = _request.Avatar,
                Name = _request.Name,
                Gender = _request.Gender,
                Ancestry = _request.Ancestry,
                Class = _request.Class,
                Background = _request.Background,
                Parameters = CloneDictionary(characterData?.Parameters),
                EquippedItems = CloneEquippedItems(equippedItems),
                Spells = CloneDictionary(characterData?.Spells),
                StatusEffects = CloneDictionary(characterData?.StatusEffects),
            };

            state.Characters.Characters[characterId] = character;

            if (_request.AddToActiveParty
                && !state.Characters.ActivePartyCharacterIds.Contains(characterId))
            {
                state.Characters.ActivePartyCharacterIds.Add(characterId);
            }

            state.Characters.NextCharacterId++;
        }

        private static Dictionary<string, int> CloneDictionary(Dictionary<string, int> source)
        {
            return source == null
                ? new Dictionary<string, int>()
                : new Dictionary<string, int>(source);
        }

        private static List<EquippedItemStateData> CloneEquippedItems(List<EquippedItemStateData> source)
        {
            if (source == null)
                return new List<EquippedItemStateData>();

            var result = new List<EquippedItemStateData>(source.Count);
            for (int i = 0; i < source.Count; i++)
            {
                EquippedItemStateData item = source[i];
                if (item == null)
                    continue;

                result.Add(new EquippedItemStateData
                {
                    Slot = item.Slot,
                    ItemId = item.ItemId,
                });
            }

            return result;
        }
    }
}
