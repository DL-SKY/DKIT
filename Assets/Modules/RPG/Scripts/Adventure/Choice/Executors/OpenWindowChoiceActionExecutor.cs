using Modules.Definitions.Scripts.Implementation.Adventures.Constants;
using Modules.Windows.Scripts.Implementation.Adventure.Characters.CreateCharacter;
using Modules.Windows.Scripts.Managers;
using Zenject;
using Zenject.Scripts.Factories;

namespace Modules.RPG.Scripts.Adventure.Choice.Executors
{
    /// <summary>
    /// Opens hub UI windows (create/select character, trade, party, adventure list).
    /// </summary>
    public class OpenWindowChoiceActionExecutor : IChoiceActionExecutor
    {
        [Inject] private readonly WindowsManager _windowsManager;
        [Inject] private readonly ViewModelFactory _viewModelFactory;

        private readonly string _windowId;

        public OpenWindowChoiceActionExecutor(string windowId)
        {
            _windowId = windowId;
        }

        public void Execute()
        {
            switch (_windowId)
            {
                case Glossary.Windows.CREATE_CHARACTER:
                    OpenCreateCharacter();
                    break;

                default:
                    UnityEngine.Debug.LogWarning($"[OpenWindowChoiceActionExecutor] OpenWindow is not wired yet. WindowId='{_windowId}'.");
                    break;
            }
        }

        private void OpenCreateCharacter()
        {
            var viewModel = _viewModelFactory.Create<CreateCharacterViewModel>();
            viewModel.Init(addToActiveParty: true);
            _windowsManager.OpenView<CreateCharacterView, CreateCharacterViewModel>(
                CreateCharacterView.Path,
                viewModel);
        }
    }
}
