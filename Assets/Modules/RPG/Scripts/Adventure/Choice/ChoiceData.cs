using Modules.Restrictions.Scripts.Core;
using System.Collections.Generic;

namespace Modules.RPG.Scripts.Adventure.Choice
{
    public enum ChoiceType
    {
        Dafault = 0,
    }


    public class ChoiceData
    {
        public string Id;
        public List<string> Tags;

        public ChoiceType Type;

        //public object ViewOptions;                          //*options... Animation, effects, icons, etc
        //public string Icon;                                 //*options... if EMPTY - use default


        public string Text;
        public string Description;

        public bool AlwaysShow;
        public List<Restriction> Restrictions;

        public List<ChoiceActionData> Actions;
    }
}
