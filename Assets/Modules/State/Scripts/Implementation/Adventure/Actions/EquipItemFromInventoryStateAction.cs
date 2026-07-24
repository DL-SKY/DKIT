using Modules.State.Scripts.Actions.Core;
using Modules.State.Scripts.Actions.Models;
using Modules.State.Scripts.Implementation.Adventure.Actions.Models;
using Modules.State.Scripts.Implementation.Adventure.StateDatas;
using System.Collections.Generic;

namespace Modules.State.Scripts.Implementation.Adventure.Actions
{
    public class EquipItemFromInventoryStateAction : StateActionBase<StateData>
    {
        public override StateChangeSource Source => StateChangeSource.EquipItemFromInventory;

        private readonly EquipItemFromInventoryRequestData _request;

        public EquipItemFromInventoryStateAction(EquipItemFromInventoryRequestData request)
        {
            _request = request;
        }

        public override StateActionValidationResult Validate(StateData state)
        {
            if (state?.Characters?.Characters == null)
                return StateActionValidationResult.Fail("Characters dictionary is null.", 80);

            if (state.Inventory?.Items == null)
                return StateActionValidationResult.Fail("Inventory items are null.", 81);

            if (_request == null)
                return StateActionValidationResult.Fail("Equip item request is null.", 82);

            if (_request.CharacterId <= 0)
                return StateActionValidationResult.Fail("Character id must be greater than zero.", 83);

            if (string.IsNullOrWhiteSpace(_request.ItemId))
                return StateActionValidationResult.Fail("Item id is null or empty.", 84);

            if (!state.Characters.Characters.TryGetValue(_request.CharacterId, out CharacterStateData character) || character == null)
                return StateActionValidationResult.Fail("Character is not found by id.", 85);

            if (character.EquippedItems == null)
                return StateActionValidationResult.Fail("Character equipped items are null.", 86);

            if (_request.SlotIndex < 0 || _request.SlotIndex >= character.EquippedItems.Count)
                return StateActionValidationResult.Fail("Equipped slot index is out of range.", 87);

            EquippedItemStateData slot = character.EquippedItems[_request.SlotIndex];
            if (slot == null)
                return StateActionValidationResult.Fail("Equipped slot entry is null.", 88);

            if (string.IsNullOrWhiteSpace(slot.Slot))
                return StateActionValidationResult.Fail("Equipped slot type is null or empty.", 89);

            if (InventoryItemsOperator.GetCount(state.Inventory.Items, _request.ItemId) < 1)
            {
                return StateActionValidationResult.Fail(
                    $"Not enough inventory items for '{_request.ItemId}'.",
                    90);
            }

            return StateActionValidationResult.Ok;
        }

        public override void Execute(StateData state)
        {
            CharacterStateData character = state.Characters.Characters[_request.CharacterId];
            EquippedItemStateData slot = character.EquippedItems[_request.SlotIndex];

            state.Inventory.Items ??= new Dictionary<string, int>();

            if (!string.IsNullOrWhiteSpace(slot.ItemId))
                InventoryItemsOperator.Add(state.Inventory.Items, slot.ItemId);

            InventoryItemsOperator.TryConsume(state.Inventory.Items, _request.ItemId);
            slot.ItemId = _request.ItemId;
        }
    }
}
