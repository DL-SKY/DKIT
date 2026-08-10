using Modules.State.Scripts.Implementation.Adventure.Actions;
using Modules.State.Scripts.Implementation.Adventure.Logic;
using Zenject;

namespace Modules.RPG.Scripts.Adventure.Choice.Executors
{
    public class AddCharacterParameterChoiceActionExecutor : IChoiceActionExecutor
    {
        [Inject] private readonly AdventureStateLogic _stateLogic;

        private readonly string _parameterKey;
        private readonly int _parameterDelta;

        public AddCharacterParameterChoiceActionExecutor(string parameterKey, int parameterDelta)
        {
            _parameterKey = parameterKey;
            _parameterDelta = parameterDelta;
        }

        public void Execute()
        {
            if (CharacterChoiceActionParameterBlacklist.Contains(_parameterKey))
            {
                UnityEngine.Debug.LogWarning(
                    $"[{nameof(AddCharacterParameterChoiceActionExecutor)}] Skip forbidden character parameter key '{_parameterKey}'.");
                return;
            }

            _stateLogic.ProcessAction(new AddCharacterParameterStateAction(_parameterKey, _parameterDelta));
        }
    }
}
