using Modules.Space.Scripts.Implementation.Definitions;
using Modules.State.Scripts.Actions.Core;
using Modules.State.Scripts.Actions.Models;

namespace Modules.Space.Scripts.Implementation.State.Actions
{
    /// <summary>
    /// Fits one module into an empty socket. The socket must match the module type and size.
    /// </summary>
    public sealed class InstallSpaceModuleStateAction : StateActionBase<SpaceProfile>
    {
        private readonly SpaceDefinitionsManager _definitions;
        private readonly string _shipId;
        private readonly int _slotIndex;
        private readonly string _moduleId;

        public InstallSpaceModuleStateAction(SpaceDefinitionsManager definitions, string shipId, int slotIndex, string moduleId)
        {
            _definitions = definitions;
            _shipId = shipId;
            _slotIndex = slotIndex;
            _moduleId = moduleId;
        }

        public override StateChangeSource Source => StateChangeSource.Space;

        public override StateActionValidationResult Validate(SpaceProfile state)
        {
            if (state == null)
                return StateActionValidationResult.Fail("Space profile is not loaded.", 941);

            if (_definitions == null)
                return StateActionValidationResult.Fail("Space definitions are not loaded.", 942);

            var ship = FindShip(state, _shipId);
            if (ship == null)
                return StateActionValidationResult.Fail("Ship was not found in the hangar.", 943);

            if (!_definitions.TryGetHull(ship.HullId, out _))
                return StateActionValidationResult.Fail("Hull def was not found.", 944);

            if (!TryGetSlot(ship, _slotIndex, out var slot))
                return StateActionValidationResult.Fail("Socket was not found.", 945);

            if (!string.IsNullOrEmpty(slot.ModuleId))
                return StateActionValidationResult.Fail("Socket is already filled.", 946);

            if (!_definitions.TryGetModule(_moduleId, out var module) || module == null)
                return StateActionValidationResult.Fail("Module def was not found.", 947);

            if (module.SlotType != slot.SlotType)
                return StateActionValidationResult.Fail("Module slot type does not match the socket.", 948);

            if (!module.AllowsSlotSize(slot.SizeType))
                return StateActionValidationResult.Fail("Module does not allow this socket size.", 949);

            return StateActionValidationResult.Ok;
        }

        public override void Execute(SpaceProfile state)
        {
            var ship = FindShip(state, _shipId);
            TryGetSlot(ship, _slotIndex, out var slot);
            _definitions.TryGetModule(_moduleId, out var module);
            slot.ModuleId = string.IsNullOrEmpty(module.Id) ? _moduleId : module.Id;
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
