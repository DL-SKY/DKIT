using Modules.Initializer.Scripts.Core;
using Modules.Utils.Scripts.Input;
using Modules.Windows.Scripts.Base;
using Modules.Windows.Scripts.Implementation.Adventure.Main;
using Modules.Windows.Scripts.Managers;
using Modules.Windows.Scripts.Settings;
using Zenject;
using Zenject.Scripts.Factories;

namespace Modules.Windows.Scripts.Implementation.Adventure.Preloaders
{
    public class AdventureApplicationLoadViewModel : ViewModelBase
    {
        public const string ON_CHANGE_PROGRESS = "ON_CHANGE_PROGRESS";
        public const string ON_CHANGE_INTERACTABLE = "ON_CHANGE_INTERACTABLE";

        public const string KNOCK_TO_CONTINUE = "KNOCK_TO_CONTINUE";
        public const int MAX_KNOCKS = 3;

        [Inject] private readonly WindowsManager _windowsManager;
        [Inject] private readonly ScreenInputService _screenInput;
        [Inject] private readonly ViewModelFactory _viewModelFactory;

        public float Progress { get; private set; }
        public bool IsInteractable { get; private set; }
        public string HintLocalizationKey => KNOCK_TO_CONTINUE;

        private IProgressable _progressableHolder;
        private int _knockCount;
        private bool _isOpeningAdventure;

        public void Init(IProgressable progressableHolder)
        {
            _progressableHolder = progressableHolder;

            _knockCount = 0;
            _isOpeningAdventure = false;

            Progress = 0.0f;
            SetIsInteractable(false);

            Subscribe();
        }

        protected override Options CreateOptions()
        {
            return new Options(canCloseOnEsc: false, hideInHistory: true, sortingLayer: SortingOrderLayer.PRELOADER);
        }

        private void Subscribe()
        {
            _progressableHolder.OnProgressChange += OnProgressChangeHandler;
            _screenInput.OnInput += OnScreenInputHandler;
        }

        private void Unsubscribe()
        {
            _progressableHolder.OnProgressChange -= OnProgressChangeHandler;
            _screenInput.OnInput -= OnScreenInputHandler;
        }

        private void OnProgressChangeHandler(int currentValue, int maxValue)
        {
            Progress = maxValue > 0 ? currentValue * 1.0f / maxValue : 0.0f;
            SendOnChange(ON_CHANGE_PROGRESS);

            if (currentValue >= maxValue && maxValue > 0)
                SetIsInteractable(true);
        }

        private void OnScreenInputHandler(ScreenInputType inputType)
        {
            if (inputType != ScreenInputType.PointerDown)
                return;

            if (!IsInteractable || _isOpeningAdventure)
                return;

            _knockCount++;

            if (_knockCount >= MAX_KNOCKS)
                OpenAdventureAndClose();
        }

        private void OpenAdventureAndClose()
        {
            _isOpeningAdventure = true;

            var adventureMainViewModel = _viewModelFactory.Create<AdventureMainViewModel>();
            adventureMainViewModel.Init();
            _windowsManager.OpenView<AdventureMainView, AdventureMainViewModel>(
                AdventureMainView.Path,
                adventureMainViewModel);

            _windowsManager.CloseView(ViewHandle);
        }

        private void SetIsInteractable(bool value)
        {
            if (IsInteractable == value)
                return;

            IsInteractable = value;
            SendOnChange(ON_CHANGE_INTERACTABLE);
        }

        public override void Dispose()
        {
            Unsubscribe();
            IsInteractable = false;
            _knockCount = 0;
            _isOpeningAdventure = false;
        }
    }
}
