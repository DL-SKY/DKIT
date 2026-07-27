using Modules.State.Scripts.Actions.Core;
using Modules.State.Scripts.Actions.Models;
using Modules.State.Scripts.Implementation.Adventure.Actions.Models;
using Modules.State.Scripts.Implementation.Adventure.StateDatas;

namespace Modules.State.Scripts.Implementation.Adventure.Actions
{
    public class RemoveCharacterEquippedItemStateAction : StateActionBase<StateData>
    {
        public override StateChangeSource Source => StateChangeSource.RemoveCharacterEquippedItem;

        private readonly RemoveCharacterEquippedItemRequestData _request;

        public RemoveCharacterEquippedItemStateAction(RemoveCharacterEquippedItemRequestData request)
        {
            _request = request;
        }

        public override StateActionValidationResult Validate(StateData state)
        {
            if (state?.Characters?.Characters == null)
                return StateActionValidationResult.Fail("Characters dictionary is null.", 129);

            if (_request == null)
                return StateActionValidationResult.Fail("Remove character equipped item request is null.", 130);

            if (_request.CharacterId <= 0)
                return StateActionValidationResult.Fail("Character id must be greater than zero.", 131);

            if (string.IsNullOrWhiteSpace(_request.ItemId))
                return StateActionValidationResult.Fail("Item id is null or empty.", 132);

            if (!state.Characters.Characters.TryGetValue(_request.CharacterId, out CharacterStateData character) || character == null)
                return StateActionValidationResult.Fail("Character is not found by id.", 133);

            if (character.EquippedItems == null)
                return StateActionValidationResult.Fail("Character equipped items are null.", 134);

            if (!TryResolveSlotIndex(character, out int resolvedSlotIndex, out string errorMessage, out int errorCode))
                return StateActionValidationResult.Fail(errorMessage, errorCode);

            EquippedItemStateData slot = character.EquippedItems[resolvedSlotIndex];
            if (slot == null)
                return StateActionValidationResult.Fail("Equipped slot entry is null.", 135);

            if (!string.Equals(slot.ItemId, _request.ItemId, System.StringComparison.Ordinal))
            {
                return StateActionValidationResult.Fail(
                    $"Equipped slot item id mismatch. Expected '{_request.ItemId}', actual '{slot.ItemId}'.",
                    136);
            }

            return StateActionValidationResult.Ok;
        }

        public override void Execute(StateData state)
        {
            CharacterStateData character = state.Characters.Characters[_request.CharacterId];
            TryResolveSlotIndex(character, out int resolvedSlotIndex, out _, out _);
            character.EquippedItems[resolvedSlotIndex].ItemId = null;
        }

        private bool TryResolveSlotIndex(
            CharacterStateData character,
            out int resolvedSlotIndex,
            out string errorMessage,
            out int errorCode)
        {
            resolvedSlotIndex = -1;
            errorMessage = null;
            errorCode = 0;

            if (_request.SlotIndex == -1)
            {
                for (int i = 0; i < character.EquippedItems.Count; i++)
                {
                    EquippedItemStateData slot = character.EquippedItems[i];
                    if (slot == null)
                        continue;

                    if (string.Equals(slot.ItemId, _request.ItemId, System.StringComparison.Ordinal))
                    {
                        resolvedSlotIndex = i;
                        return true;
                    }
                }

                errorMessage = $"Equipped item '{_request.ItemId}' is not found on character.";
                errorCode = 137;
                return false;
            }

            if (_request.SlotIndex < 0 || _request.SlotIndex >= character.EquippedItems.Count)
            {
                errorMessage = "Equipped slot index is out of range.";
                errorCode = 138;
                return false;
            }

            resolvedSlotIndex = _request.SlotIndex;
            return true;
        }
    }
}
