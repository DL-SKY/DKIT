using Modules.RPG.Scripts.State;

namespace Modules.RPG.Scripts.Adventure.Choice.Executors
{
    public class ModifyVariableChoiceActionExecutor : IChoiceActionExecutor
    {
        private readonly IRpgVariablesController _rpgVariablesController;
        private readonly string _key;
        private readonly int _delta;

        public ModifyVariableChoiceActionExecutor(
            IRpgVariablesController rpgVariablesController,
            string key,
            int delta)
        {
            _rpgVariablesController = rpgVariablesController;
            _key = key;
            _delta = delta;
        }

        public void Execute()
        {
            _rpgVariablesController.ModifyInt(_key, _delta);
        }
    }
}
