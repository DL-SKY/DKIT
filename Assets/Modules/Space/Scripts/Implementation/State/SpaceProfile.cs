using System.Collections.Generic;
using Modules.Space.Scripts.Implementation.Definitions;
using Modules.Space.Scripts.Implementation.Queues;

namespace Modules.Space.Scripts.Implementation.State
{
    public sealed class SpaceProfile
    {
        public const string STARTER_HULL_ID = "Fighter";
        public const string STARTER_ENGINE_ID = "engine_default";

        public string ActiveHullId;
        public List<SpacePlayerShipState> Ships = new List<SpacePlayerShipState>();
        public List<SpaceQueueState> Queues = new List<SpaceQueueState>();
    }

    /// <summary>
    /// One owned hull and the modules sitting in its sockets.
    /// </summary>
    public sealed class SpacePlayerShipState
    {
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
