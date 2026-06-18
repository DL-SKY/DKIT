using Modules.State.Scripts.Implementation.Adventure.Actions;
using Modules.State.Scripts.Implementation.Adventure.Logic;

namespace Modules.RPG.Scripts.Adventure.Choice.Executors
{
    public class ModifyVariableChoiceActionExecutor : IChoiceActionExecutor
    {
        private readonly AdventureStateLogic _stateLogic;
        private readonly string _key;
        private readonly int _delta;
        private readonly string _adventureId;

        public ModifyVariableChoiceActionExecutor(
            AdventureStateLogic stateLogic,
            string key,
            int delta,
            string adventureId = null)
        {
            _stateLogic = stateLogic;
            _key = key;
            _delta = delta;
            _adventureId = adventureId;
        }

        public void Execute()
        {
            var result = _stateLogic.ProcessAction(
                new ModifyAdventureProgressIntStateAction(_key, _delta, _adventureId));

            if (!result.IsValid)
                UnityEngine.Debug.LogWarning($"[ModifyVariableChoiceActionExecutor] Failed: {result.ErrorMessage}");
        }
    }
}
