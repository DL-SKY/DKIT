using Modules.Restrictions.Scripts.Core;
using System.Collections.Generic;

namespace Modules.RPG.Scripts.Adventure.Data
{
    public class AdventureData
    {
        public string Id;
        public List<string> Tags;

        public AdventureType Type;
        public List<string> AdventureLinks;

        public string Title;
        public string Description;
        
        public List<string> IgnoredTags;

        public List<Restriction> Restictions;

        public List<string> StartScenes;                        //Scene.Id
        public Dictionary<string, SceneData> Scenes;

        //...

        //public List<string> Variables;                        //*options... for state Dictionary<string, int> AdventureVariables
    }

    public enum AdventureType
    {
        Adventure = 0,
        Chapter = 1,

        Location = 10,
    }
}
