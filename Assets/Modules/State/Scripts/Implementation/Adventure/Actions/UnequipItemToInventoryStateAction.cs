using Modules.State.Scripts.Actions.Core;
using Modules.State.Scripts.Actions.Models;
using Modules.State.Scripts.Implementation.Adventure.Actions.Models;
using Modules.State.Scripts.Implementation.Adventure.StateDatas;
using System.Collections.Generic;

namespace Modules.State.Scripts.Implementation.Adventure.Actions
{
    public class UnequipItemToInventoryStateAction : StateActionBase<StateData>
    {
        public override StateChangeSource Source => StateChangeSource.UnequipItemToInventory;

        private readonly UnequipItemToInventoryRequestData _request;

        public UnequipItemToInventoryStateAction(UnequipItemToInventoryRequestData request)
        {
            _request = request;
        }

        public override StateActionValidationResult Validate(StateData state)
        {
            if (state?.Characters?.Characters == null)
                return StateActionValidationResult.Fail("Characters dictionary is null.", 91);

            if (state.Inventory == null)
                return StateActionValidationResult.Fail("Inventory state is null.", 92);

            if (_request == null)
                return StateActionValidationResult.Fail("Unequip item request is null.", 93);

            if (_request.CharacterId <= 0)
                return StateActionValidationResult.Fail("Character id must be greater than zero.", 94);

            if (!state.Characters.Characters.TryGetValue(_request.CharacterId, out CharacterStateData character) || character == null)
                return StateActionValidationResult.Fail("Character is not found by id.", 95);

            if (character.EquippedItems == null)
                return StateActionValidationResult.Fail("Character equipped items are null.", 96);

            if (_request.SlotIndex < 0 || _request.SlotIndex >= character.EquippedItems.Count)
                return StateActionValidationResult.Fail("Equipped slot index is out of range.", 97);

            EquippedItemStateData slot = character.EquippedItems[_request.SlotIndex];
            if (slot == null)
                return StateActionValidationResult.Fail("Equipped slot entry is null.", 98);

            if (string.IsNullOrWhiteSpace(slot.ItemId))
                return StateActionValidationResult.Fail("Equipped slot is already empty.", 99);

            return StateActionValidationResult.Ok;
        }

        public override void Execute(StateData state)
        {
            CharacterStateData character = state.Characters.Characters[_request.CharacterId];
            EquippedItemStateData slot = character.EquippedItems[_request.SlotIndex];

            state.Inventory.Items ??= new Dictionary<string, int>();
            InventoryItemsOperator.Add(state.Inventory.Items, slot.ItemId);
            slot.ItemId = null;
        }
    }
}
