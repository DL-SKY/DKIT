using Modules.State.Scripts.Actions.Core;
using Modules.State.Scripts.Actions.Models;
using Modules.State.Scripts.Implementation.Adventure.Actions.Models;
using Modules.State.Scripts.Implementation.Adventure.StateDatas;
using System.Collections.Generic;

namespace Modules.State.Scripts.Implementation.Adventure.Actions
{
    /// <summary>
    /// Обновляет mutable-блок существующего персонажа из <see cref="Models.UpdateCharacterRequestData"/>.
    /// </summary>
    /// <remarks>
    /// Только для прокачки. <see cref="Models.UpdateCharacterRequestData.CharacterData"/>.Parameters
    /// должны быть собраны через <see cref="CharacterParametersOperator"/>
    /// (слепок + последовательные ApplyFeat / ApplyPatch). Экшен только персистит готовый слепок
    /// wholesale-заменой mutable-блока.
    /// </remarks>
    public class UpdateCharacterStateAction : StateActionBase<StateData>
    {
        public override StateChangeSource Source => StateChangeSource.Characters;

        private readonly UpdateCharacterRequestData _request;

        public UpdateCharacterStateAction(UpdateCharacterRequestData request)
        {
            _request = request;
        }

        public override StateActionValidationResult Validate(StateData state)
        {
            if (state?.Characters?.Characters == null)
                return StateActionValidationResult.Fail("Characters dictionary is null.", 71);

            if (_request == null)
                return StateActionValidationResult.Fail("Update character request is null.", 72);

            if (_request.CharacterId <= 0)
                return StateActionValidationResult.Fail("Character id must be greater than zero.", 73);

            if (!state.Characters.Characters.TryGetValue(_request.CharacterId, out CharacterStateData character) || character == null)
                return StateActionValidationResult.Fail("Character is not found by id.", 74);

            if (_request.CharacterData == null)
                return StateActionValidationResult.Fail("Character data is null.", 75);

            List<EquippedItemStateData> newEquippedItems = _request.CharacterData.EquippedItems;
            if (newEquippedItems != null)
            {
                for (int i = 0; i < newEquippedItems.Count; i++)
                {
                    EquippedItemStateData equipped = newEquippedItems[i];
                    if (equipped == null)
                        return StateActionValidationResult.Fail($"Equipped item at index {i} is null.", 76);

                    if (string.IsNullOrWhiteSpace(equipped.Slot))
                        return StateActionValidationResult.Fail($"Equipped item slot at index {i} is null or empty.", 77);
                }
            }

            return StateActionValidationResult.Ok;
        }

        public override void Execute(StateData state)
        {
            CharacterStateData character = state.Characters.Characters[_request.CharacterId];
            CharacterRequestData data = _request.CharacterData;

            character.Parameters = CloneDictionary(data.Parameters);
            character.EquippedItems = CloneEquippedItems(data.EquippedItems);
            character.Spells = CloneDictionary(data.Spells);
            character.StatusEffects = CloneDictionary(data.StatusEffects);
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
