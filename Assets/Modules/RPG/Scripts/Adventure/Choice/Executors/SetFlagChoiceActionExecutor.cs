using Modules.RPG.Scripts.State;

namespace Modules.RPG.Scripts.Adventure.Choice.Executors
{
    public class SetFlagChoiceActionExecutor : IChoiceActionExecutor
    {
        private readonly IRpgFlagsController _rpgFlagsController;
        private readonly string _key;
        private readonly bool _value;

        public SetFlagChoiceActionExecutor(
            IRpgFlagsController rpgFlagsController,
            string key,
            bool value)
        {
            _rpgFlagsController = rpgFlagsController;
            _key = key;
            _value = value;
        }

        public void Execute()
        {
            _rpgFlagsController.SetBool(_key, _value);
        }
    }
}
