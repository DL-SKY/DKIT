using Modules.State.Scripts.Actions.Core;
using Modules.State.Scripts.Actions.Models;
using Modules.State.Scripts.Implementation.Adventure.Actions.Models;
using Modules.State.Scripts.Implementation.Adventure.StateDatas;

namespace Modules.State.Scripts.Implementation.Adventure.Actions
{
    public class MoveEquippedItemBetweenSlotsStateAction : StateActionBase<StateData>
    {
        public override StateChangeSource Source => StateChangeSource.MoveEquippedItemBetweenSlots;

        private readonly MoveEquippedItemBetweenSlotsRequestData _request;

        public MoveEquippedItemBetweenSlotsStateAction(MoveEquippedItemBetweenSlotsRequestData request)
        {
            _request = request;
        }

        public override StateActionValidationResult Validate(StateData state)
        {
            if (state?.Characters?.Characters == null)
                return StateActionValidationResult.Fail("Characters dictionary is null.", 100);

            if (_request == null)
                return StateActionValidationResult.Fail("Move equipped item request is null.", 101);

            if (_request.CharacterId <= 0)
                return StateActionValidationResult.Fail("Character id must be greater than zero.", 102);

            if (_request.FromSlotIndex == _request.ToSlotIndex)
                return StateActionValidationResult.Fail("From and To slot indexes must be different.", 103);

            if (!state.Characters.Characters.TryGetValue(_request.CharacterId, out CharacterStateData character) || character == null)
                return StateActionValidationResult.Fail("Character is not found by id.", 104);

            if (character.EquippedItems == null)
                return StateActionValidationResult.Fail("Character equipped items are null.", 105);

            if (_request.FromSlotIndex < 0 || _request.FromSlotIndex >= character.EquippedItems.Count)
                return StateActionValidationResult.Fail("From slot index is out of range.", 106);

            if (_request.ToSlotIndex < 0 || _request.ToSlotIndex >= character.EquippedItems.Count)
                return StateActionValidationResult.Fail("To slot index is out of range.", 107);

            EquippedItemStateData fromSlot = character.EquippedItems[_request.FromSlotIndex];
            if (fromSlot == null)
                return StateActionValidationResult.Fail("From slot entry is null.", 108);

            if (string.IsNullOrWhiteSpace(fromSlot.ItemId))
                return StateActionValidationResult.Fail("From slot is empty.", 109);

            EquippedItemStateData toSlot = character.EquippedItems[_request.ToSlotIndex];
            if (toSlot == null)
                return StateActionValidationResult.Fail("To slot entry is null.", 110);

            if (string.IsNullOrWhiteSpace(toSlot.Slot))
                return StateActionValidationResult.Fail("To slot type is null or empty.", 111);

            return StateActionValidationResult.Ok;
        }

        public override void Execute(StateData state)
        {
            CharacterStateData character = state.Characters.Characters[_request.CharacterId];
            EquippedItemStateData fromSlot = character.EquippedItems[_request.FromSlotIndex];
            EquippedItemStateData toSlot = character.EquippedItems[_request.ToSlotIndex];

            string movedItemId = fromSlot.ItemId;
            string displacedItemId = toSlot.ItemId;

            toSlot.ItemId = movedItemId;
            fromSlot.ItemId = string.IsNullOrWhiteSpace(displacedItemId) ? null : displacedItemId;
        }
    }
}
