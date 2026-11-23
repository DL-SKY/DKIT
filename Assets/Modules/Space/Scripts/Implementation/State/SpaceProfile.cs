using System.Collections.Generic;
using Modules.Space.Scripts.Implementation.Definitions;
using Modules.Space.Scripts.Implementation.Queues;
using Modules.State.Scripts.Interfaces;

namespace Modules.Space.Scripts.Implementation.State
{
    public sealed class SpaceProfile : IStateData
    {
        public const string STARTER_HULL_ID = "Fighter";
        public const string STARTER_ENGINE_ID = "engine_default";

        public string ActiveShipId;
        public List<SpacePlayerShipState> Ships = new List<SpacePlayerShipState>();
        public List<SpaceQueueState> Queues = new List<SpaceQueueState>();

        public SpacePlayerShipState ActiveShip()
        {
            var ships = Ships;
            if (ships == null || string.IsNullOrEmpty(ActiveShipId))
                return null;

            for (var i = 0; i < ships.Count; i++)
            {
                var ship = ships[i];
                if (ship != null && ship.Id == ActiveShipId)
                    return ship;
            }

            return null;
        }
    }

    /// <summary>
    /// One hangar ship. <see cref="Id"/> is this instance. <see cref="HullId"/> is the hull def.
    /// <see cref="Name"/> is the player's label and is not read by flight.
    /// </summary>
    public sealed class SpacePlayerShipState
    {
        public string Id;
        public string Name;
        public string HullId;
        public List<SpacePlayerShipSlot> Slots = new List<SpacePlayerShipSlot>();
    }

    /// <summary>
    /// One socket copied from the hull def, plus the installed module id when the socket is filled.
    /// </summary>
    public sealed class SpacePlayerShipSlot
    {
        public SpaceSlotType SlotType;
        public SpaceSizeType SizeType;
        public string ModuleId;
    }
}
