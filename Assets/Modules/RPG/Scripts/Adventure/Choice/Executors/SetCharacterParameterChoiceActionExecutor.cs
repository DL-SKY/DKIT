using Modules.State.Scripts.Implementation.Adventure.Actions;
using Modules.State.Scripts.Implementation.Adventure.Logic;
using Zenject;

namespace Modules.RPG.Scripts.Adventure.Choice.Executors
{
    public class SetCharacterParameterChoiceActionExecutor : IChoiceActionExecutor
    {
        [Inject] private readonly AdventureStateLogic _stateLogic;

        private readonly string _parameterKey;
        private readonly int _parameterValue;

        public SetCharacterParameterChoiceActionExecutor(string parameterKey, int parameterValue)
        {
            _parameterKey = parameterKey;
            _parameterValue = parameterValue;
        }

        public void Execute()
        {
            if (CharacterChoiceActionParameterBlacklist.Contains(_parameterKey))
            {
                UnityEngine.Debug.LogWarning(
                    $"[{nameof(SetCharacterParameterChoiceActionExecutor)}] Skip forbidden character parameter key '{_parameterKey}'.");
                return;
            }

            _stateLogic.ProcessAction(new SetCharacterParameterStateAction(_parameterKey, _parameterValue));
        }
    }
}
