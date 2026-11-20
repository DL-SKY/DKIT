using System.Collections.Generic;
using Modules.Space.Scripts.Implementation.Queues;

namespace Modules.Space.Scripts.Implementation.State
{
    public sealed class SpaceProfile
    {
        public const string STARTER_HULL_ID = "Fighter";
        public const string STARTER_ENGINE_ID = "engine_default";

        public string ActiveHullId;
        public List<string> OwnedHullIds = new List<string>();
        public List<SpaceFittedModule> FittedModules = new List<SpaceFittedModule>();
        public List<SpaceQueueState> Queues = new List<SpaceQueueState>();
    }

    /// <summary>
    /// One module fitted into a hull slot. The pair is stored on the profile, not in the flight world.
    /// </summary>
    public sealed class SpaceFittedModule
    {
        public string SlotId;
        public string ModuleId;
    }
}
