using Modules.Definitions.Scripts.Defs;
using Modules.Restrictions.Scripts.Core;
using System.Collections.Generic;

namespace Modules.RPG.Scripts.Adventure.Data
{
    public enum AdventureType
    {
        Adventure = 0,
        Chapter = 1,

        Location = 10,
    }


    public class AdventureData : AbstractDefinition
    {
        public bool Disabled;

        public List<string> Tags;
        public List<string> IgnoredTags;

        public bool IsRepeatable;
        public AdventureType Type;
        public List<string> AdventureLinks;

        public string Title;
        public string Description;

        public List<Restriction> Restrictions;

        public List<string> StartScenes;
        public Dictionary<string, SceneData> Scenes;

        //...

        //public List<string> Variables;                        //*options... for state Dictionary<string, int> AdventureVariables
    }
}
