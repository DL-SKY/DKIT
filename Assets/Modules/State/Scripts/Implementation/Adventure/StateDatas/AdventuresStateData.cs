using System.Collections.Generic;

namespace Modules.State.Scripts.Implementation.Adventure.StateDatas
{
    public class AdventuresStateData
    {
        public string CurrentAdventureId;
        public string CurrentAdventureSceneId;

        public WorldStateData World;

        public Dictionary<string, AdventureStateData> Adventures;
    }

    public class WorldStateData
    {
        public AdventureStateParamsData Parameters;
    }

    public class AdventureStateData
    {
        public string AdventureId;
        public string SceneId;

        public AdventureStateParamsData Parameters;
    }

    public class AdventureStateParamsData
    {
        public Dictionary<string, string> Strings;
        public Dictionary<string, int> Ints;
        public Dictionary<string, bool> Bools;
    }
}
