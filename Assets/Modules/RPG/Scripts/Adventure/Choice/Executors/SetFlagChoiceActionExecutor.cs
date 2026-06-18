using Modules.State.Scripts.Implementation.Adventure.Actions;
using Modules.State.Scripts.Implementation.Adventure.Logic;

namespace Modules.RPG.Scripts.Adventure.Choice.Executors
{
    public class SetFlagChoiceActionExecutor : IChoiceActionExecutor
    {
        private readonly AdventureStateLogic _stateLogic;
        private readonly string _key;
        private readonly bool _value;
        private readonly string _adventureId;

        public SetFlagChoiceActionExecutor(
            AdventureStateLogic stateLogic,
            string key,
            bool value,
            string adventureId = null)
        {
            _stateLogic = stateLogic;
            _key = key;
            _value = value;
            _adventureId = adventureId;
        }

        public void Execute()
        {
            var result = _stateLogic.ProcessAction(
                new SetAdventureProgressBoolStateAction(_key, _value, _adventureId));

            if (!result.IsValid)
                UnityEngine.Debug.LogWarning($"[SetFlagChoiceActionExecutor] Failed: {result.ErrorMessage}");
        }
    }
}
