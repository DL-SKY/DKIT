using Modules.RPG.Scripts.Restrictions;
using System.Collections.Generic;

namespace Modules.RPG.Scripts.Adventure.Choice
{
    public class ChoiceData
    {
        public string Id;
        public List<string> Tags;


        //public object Type;                                 //*options... !!! TODO: enum
        //public object ViewOptions;                          //*options... Animation, effects, icons, etc
        //public string Icon;                                 //*options... if EMPTY - use default


        public string Text;
        public string Description;

        public bool AlwaysShow;
        public List<Restriction> Restictions;

        public List<ChoiceActionData> Actions;
    }
}
