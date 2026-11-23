using System;
using System.Collections.Generic;
using Modules.Space.Scripts.Implementation.Definitions;
using Modules.State.Scripts.Actions.Core;
using Modules.State.Scripts.Actions.Models;

namespace Modules.Space.Scripts.Implementation.State.Actions
{
    /// <summary>
    /// Keeps hangar instances, fills a missing active ship, and rebuilds sockets from the hull def.
    /// </summary>
    public sealed class EnsureSpaceShipStateAction : StateActionBase<SpaceProfile>
    {
        private readonly SpaceDefinitionsManager _definitions;

        public EnsureSpaceShipStateAction(SpaceDefinitionsManager definitions)
        {
            _definitions = definitions;
        }

        public override StateChangeSource Source => StateChangeSource.Space;

        public override StateActionValidationResult Validate(SpaceProfile state)
        {
            if (state == null)
                return StateActionValidationResult.Fail("Space profile is not loaded.", 911);

            if (_definitions == null)
                return StateActionValidationResult.Fail("Space definitions are not loaded.", 912);

            if (HasKnownShip(state) || _definitions.TryGetHull(SpaceProfile.STARTER_HULL_ID, out _))
                return StateActionValidationResult.Ok;

            return StateActionValidationResult.Fail("Space profile has no ship and the starter hull is missing.", 913);
        }

        public override void Execute(SpaceProfile state)
        {
            if (state.Ships == null)
                state.Ships = new List<SpacePlayerShipState>();

            state.Ships = KeepKnownShips(state.Ships);
            if (Find(state.Ships, state.ActiveShipId) == null)
            {
                if (state.Ships.Count == 0)
                    state.Ships.Add(CreateShip(SpaceProfile.STARTER_HULL_ID));

                state.ActiveShipId = state.Ships[0].Id;
            }

            for (var i = 0; i < state.Ships.Count; i++)
            {
                var ship = state.Ships[i];
                if (!_definitions.TryGetHull(ship.HullId, out var hull))
                    continue;

                ship.Slots = RebuildSlots(hull, ship.Slots);
                EnsureDefaultEngine(ship);
            }
        }

        private bool HasKnownShip(SpaceProfile state)
        {
            var ships = state.Ships;
            if (ships == null)
                return false;

            for (var i = 0; i < ships.Count; i++)
            {
                var ship = ships[i];
                if (ship != null && !string.IsNullOrEmpty(ship.HullId) && _definitions.TryGetHull(ship.HullId, out _))
                    return true;
            }

            return false;
        }

        private SpacePlayerShipState CreateShip(string hullId)
        {
            return new SpacePlayerShipState
            {
                Id = Guid.NewGuid().ToString("N"),
                Name = string.Empty,
                HullId = hullId
            };
        }

        private List<SpacePlayerShipState> KeepKnownShips(List<SpacePlayerShipState> ships)
        {
            var kept = new List<SpacePlayerShipState>();
            for (var i = 0; i < ships.Count; i++)
            {
                var ship = ships[i];
                if (ship == null || string.IsNullOrEmpty(ship.HullId) || !_definitions.TryGetHull(ship.HullId, out _))
                    continue;

                if (string.IsNullOrEmpty(ship.Id) || Find(kept, ship.Id) != null)
                    ship.Id = Guid.NewGuid().ToString("N");

                if (ship.Name == null)
                    ship.Name = string.Empty;

                kept.Add(ship);
            }

            return kept;
        }

        private List<SpacePlayerShipSlot> RebuildSlots(SpaceHullDef hull, List<SpacePlayerShipSlot> saved)
        {
            var slots = new List<SpacePlayerShipSlot>();
            if (hull?.Slots == null)
                return slots;

            var used = new bool[saved != null ? saved.Count : 0];
            for (var i = 0; i < hull.Slots.Count; i++)
            {
                var defined = hull.Slots[i];
                if (defined == null)
                    continue;

                slots.Add(new SpacePlayerShipSlot
                {
                    SlotType = defined.SlotType,
                    SizeType = defined.SizeType,
                    ModuleId = TakeInstalledModule(saved, used, defined)
                });
            }

            return slots;
        }

        private string TakeInstalledModule(List<SpacePlayerShipSlot> saved, bool[] used, SpaceHullSlotDef defined)
        {
            if (saved == null)
                return null;

            for (var i = 0; i < saved.Count; i++)
            {
                if (used[i])
                    continue;

                var previous = saved[i];
                if (previous == null || previous.SlotType != defined.SlotType || previous.SizeType != defined.SizeType)
                    continue;

                used[i] = true;
                if (string.IsNullOrEmpty(previous.ModuleId))
                    return null;

                if (!_definitions.TryGetModule(previous.ModuleId, out var module))
                    return null;

                if (module.SlotType != defined.SlotType || !module.AllowsSlotSize(defined.SizeType))
                    return null;

                return module.Id;
            }

            return null;
        }

        private void EnsureDefaultEngine(SpacePlayerShipState ship)
        {
            if (ship?.Slots == null || !_definitions.TryGetModule(SpaceProfile.STARTER_ENGINE_ID, out var engine))
                return;

            if (engine.SlotType != SpaceSlotType.Engine || !engine.IsUsable())
                return;

            for (var i = 0; i < ship.Slots.Count; i++)
            {
                var slot = ship.Slots[i];
                if (slot == null || slot.SlotType != SpaceSlotType.Engine || !string.IsNullOrEmpty(slot.ModuleId))
                    continue;

                if (!engine.AllowsSlotSize(slot.SizeType))
                    continue;

                slot.ModuleId = engine.Id;
                return;
            }
        }

        private static SpacePlayerShipState Find(List<SpacePlayerShipState> ships, string shipId)
        {
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
    }
}
