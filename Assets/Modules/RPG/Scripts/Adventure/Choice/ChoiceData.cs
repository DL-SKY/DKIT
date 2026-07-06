using Modules.Restrictions.Scripts.Core;
using Modules.Dices.Scripts;
using System.Collections.Generic;

namespace Modules.RPG.Scripts.Adventure.Choice
{
    public enum ChoiceType
    {
        Default = 0,
        DiceCheck = 1,
    }

    public class ChoiceDiceCheckData
    {
        public int DifficultyClass;
        public DiceType DiceType;
        public DiceOptions DiceOptions;
        public string DiceCheckParam;

        public List<ChoiceActionData> OnCriticalSuccess;
        public List<ChoiceActionData> OnSuccess;
        public List<ChoiceActionData> OnFailure;
        public List<ChoiceActionData> OnCriticalFailure;
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

        public ChoiceDiceCheckData DiceCheck;

        public List<ChoiceActionData> Actions;
    }
}
