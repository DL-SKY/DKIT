using Modules.RPG.Scripts.Restrictions;
using System.Collections.Generic;

namespace Modules.RPG.Scripts.Adventure.Data
{
    public class AdventureData
    {
        public string Id;
        public List<string> Tags;

        public string Title;
        public string Description;
        
        //public List<string> IgnoredTags;                      //*options...

        public List<Restriction> Restictions;

        public List<string> StartScenes;                        //Scene.Id
        public Dictionary<string, SceneData> Scenes;

        //...

        //public List<string> Variables;                        //*options... for state Dictionary<string, int> AdventureVariables
    }
}
