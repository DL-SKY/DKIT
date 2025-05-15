using Modules.RPG.Scripts.Adventure.Choice.Actions;
using System.Collections.Generic;

namespace Modules.RPG.Scripts.Adventure.Choice
{
    public class ChoiceActionData
    {
        public ChoiceActionType Type;

        public List<string> StringValues;
        public List<int> IntValues;
    }
}
