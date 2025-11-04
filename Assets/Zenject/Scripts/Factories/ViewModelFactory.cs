using Modules.Windows.Scripts.Base;

namespace Zenject.Scripts.Factories
{
    public class ViewModelFactory
    {
        [Inject] private readonly DiContainer _container;

        //var viewModel = _viewModelFactory.Create<AccountBonusInfoViewModel>();
        //var viewModel = _factory.Create<AccountPanelViewModel>(new object[] { _currentSocial, _context.State.Player.Id, !_isShowCaution });
        public T Create<T>(params object[] parameters) where T : IViewModel
        {
            var viewModel = _container.Instantiate<T>(parameters);
            return viewModel;
        }
    }
}
