using Modules.State.Scripts.Actions.Core;
using Modules.State.Scripts.Actions.Models;

namespace Modules.Space.Scripts.Implementation.State.Actions
{
    /// <summary>
    /// Clears one filled socket. Does not check whether the module def still exists.
    /// </summary>
    public sealed class RemoveSpaceModuleStateAction : StateActionBase<SpaceProfile>
    {
        private readonly string _shipId;
        private readonly int _slotIndex;

        public RemoveSpaceModuleStateAction(string shipId, int slotIndex)
        {
            _shipId = shipId;
            _slotIndex = slotIndex;
        }

        public override StateChangeSource Source => StateChangeSource.Space;

        public override StateActionValidationResult Validate(SpaceProfile state)
        {
            if (state == null)
                return StateActionValidationResult.Fail("Space profile is not loaded.", 961);

            var ship = FindShip(state, _shipId);
            if (ship == null)
                return StateActionValidationResult.Fail("Ship was not found in the hangar.", 962);

            if (!TryGetSlot(ship, _slotIndex, out var slot))
                return StateActionValidationResult.Fail("Socket was not found.", 963);

            if (string.IsNullOrEmpty(slot.ModuleId))
                return StateActionValidationResult.Fail("Socket is already empty.", 964);

            return StateActionValidationResult.Ok;
        }

        public override void Execute(SpaceProfile state)
        {
            var ship = FindShip(state, _shipId);
            TryGetSlot(ship, _slotIndex, out var slot);
            slot.ModuleId = null;
        }

        private static SpacePlayerShipState FindShip(SpaceProfile state, string shipId)
        {
            var ships = state?.Ships;
            if (ships == null || string.IsNullOrEmpty(shipId))
                return null;

            for (var i = 0; i < ships.Count; i++)
            {
                var ship = ships[i];
                if (ship != null && ship.Id == shipId)
                    return ship;
            }

            return null;
        }

        private static bool TryGetSlot(SpacePlayerShipState ship, int slotIndex, out SpacePlayerShipSlot slot)
        {
            slot = null;
            var slots = ship?.Slots;
            if (slots == null || slotIndex < 0 || slotIndex >= slots.Count)
                return false;

            slot = slots[slotIndex];
            return slot != null;
        }
    }
}
