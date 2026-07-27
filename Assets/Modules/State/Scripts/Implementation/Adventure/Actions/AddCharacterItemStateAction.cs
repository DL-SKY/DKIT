using Modules.Definitions.Scripts.Implementation.Adventures.Constants;
using Modules.State.Scripts.Actions.Core;
using Modules.State.Scripts.Actions.Models;
using Modules.State.Scripts.Implementation.Adventure.Actions.Models;
using Modules.State.Scripts.Implementation.Adventure.StateDatas;
using System.Collections.Generic;

namespace Modules.State.Scripts.Implementation.Adventure.Actions
{
    public class AddCharacterItemStateAction : StateActionBase<StateData>
    {
        public override StateChangeSource Source => StateChangeSource.AddCharacterItem;

        private readonly AddCharacterItemRequestData _request;

        public AddCharacterItemStateAction(AddCharacterItemRequestData request)
        {
            _request = request;
        }

        public override StateActionValidationResult Validate(StateData state)
        {
            if (state?.Characters?.Characters == null)
                return StateActionValidationResult.Fail("Characters dictionary is null.", 139);

            if (state.Inventory == null)
                return StateActionValidationResult.Fail("Inventory state is null.", 140);

            if (_request == null)
                return StateActionValidationResult.Fail("Add character item request is null.", 141);

            if (_request.CharacterId <= 0)
                return StateActionValidationResult.Fail("Character id must be greater than zero.", 142);

            if (string.IsNullOrWhiteSpace(_request.ItemId))
                return StateActionValidationResult.Fail("Item id is null or empty.", 143);

            if (!state.Characters.Characters.TryGetValue(_request.CharacterId, out CharacterStateData character) || character == null)
                return StateActionValidationResult.Fail("Character is not found by id.", 144);

            if (character.EquippedItems == null)
                return StateActionValidationResult.Fail("Character equipped items are null.", 145);

            return StateActionValidationResult.Ok;
        }

        public override void Execute(StateData state)
        {
            CharacterStateData character = state.Characters.Characters[_request.CharacterId];

            if (TryFindFreeBagSlot(character, out EquippedItemStateData bagSlot))
            {
                // Bag never grants ItemDef.Features (Glossary.Items.GrantsItemFeatures).
                bagSlot.ItemId = _request.ItemId;
                return;
            }

            state.Inventory.Items ??= new Dictionary<string, int>();
            InventoryItemsOperator.TryAdd(state.Inventory.Items, _request.ItemId, 1);
        }

        private static bool TryFindFreeBagSlot(CharacterStateData character, out EquippedItemStateData bagSlot)
        {
            bagSlot = null;
            if (character?.EquippedItems == null)
                return false;

            for (int i = 0; i < character.EquippedItems.Count; i++)
            {
                EquippedItemStateData slot = character.EquippedItems[i];
                if (slot == null)
                    continue;

                if (!string.Equals(slot.Slot, Glossary.Items.SLOT_TYPE_BAG, System.StringComparison.Ordinal))
                    continue;

                if (!string.IsNullOrWhiteSpace(slot.ItemId))
                    continue;

                bagSlot = slot;
                return true;
            }

            return false;
        }
    }
}
