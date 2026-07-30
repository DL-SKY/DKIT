using Modules.Windows.Scripts.Base;
using System.Collections.Generic;
using Zenject;
using Zenject.Scripts.Factories;

namespace Modules.Windows.Scripts.Implementation.Adventure.Main.TopPanel
{
    /// <summary>
    /// ViewModel for the adventure top panel (character, abilities, menu).
    /// Owns <see cref="AdventureParameterViewModel"/> instances.
    /// Owned by <see cref="AdventureMainViewModel"/>.
    /// </summary>
    public class AdventureTopPanelViewModel : ViewModelBase
    {
        [Inject] private readonly ViewModelFactory _viewModelFactory;

        private readonly List<AdventureParameterViewModel> _parameters = new List<AdventureParameterViewModel>();

        private bool _isInitialized;

        public IReadOnlyList<AdventureParameterViewModel> Parameters => _parameters;

        public void Init()
        {
            if (_isInitialized)
                return;

            _isInitialized = true;
            Subscribe();
        }

        /// <summary>
        /// Creates a parameter VM owned by this panel. Call from the view for each
        /// <see cref="AdventureParameterView"/> slot; the view then calls
        /// <see cref="AdventureParameterViewModel.Init"/> with its serialized parameter name.
        /// </summary>
        public AdventureParameterViewModel CreateParameter()
        {
            var viewModel = _viewModelFactory.Create<AdventureParameterViewModel>();
            _parameters.Add(viewModel);
            return viewModel;
        }

        public override void Dispose()
        {
            Unsubscribe();
            DisposeParameters();
            _isInitialized = false;
        }

        private void DisposeParameters()
        {
            for (int i = 0; i < _parameters.Count; i++)
                _parameters[i]?.Dispose();

            _parameters.Clear();
        }

        private void Subscribe()
        {
        }

        private void Unsubscribe()
        {
        }
    }
}
