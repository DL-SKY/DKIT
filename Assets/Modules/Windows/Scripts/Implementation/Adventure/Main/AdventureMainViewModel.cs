using Modules.Windows.Scripts.Base;
using Modules.Windows.Scripts.Implementation.Adventure.Main.Scroll;
using Zenject;
using Zenject.Scripts.Factories;

namespace Modules.Windows.Scripts.Implementation.Adventure.Main
{
    /// <summary>
    /// ViewModel for the main Adventure screen. Owns <see cref="AdventureScrollViewModel"/>.
    /// </summary>
    public class AdventureMainViewModel : ViewModelBase
    {
        [Inject] private readonly ViewModelFactory _viewModelFactory;

        public AdventureScrollViewModel Scroll { get; private set; }

        public void Init()
        {
            if (Scroll != null)
                return;

            Scroll = _viewModelFactory.Create<AdventureScrollViewModel>();
            Scroll.Init();
        }

        public override void Dispose()
        {
            Scroll?.Dispose();
            Scroll = null;
        }
    }
}
