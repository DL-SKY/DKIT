using Modules.Windows.Scripts.Base;
using Modules.Windows.Scripts.Implementation.Adventure.Main.BottomPanel;
using Modules.Windows.Scripts.Implementation.Adventure.Main.Scroll;
using Modules.Windows.Scripts.Implementation.Adventure.Main.TopPanel;
using Modules.Windows.Scripts.Settings;
using Zenject;
using Zenject.Scripts.Factories;

namespace Modules.Windows.Scripts.Implementation.Adventure.Main
{
    /// <summary>
    /// ViewModel for the main Adventure screen.
    /// Owns <see cref="AdventureScrollViewModel"/>, <see cref="AdventureTopPanelViewModel"/>,
    /// and <see cref="AdventureBottomPanelViewModel"/>.
    /// </summary>
    public class AdventureMainViewModel : ViewModelBase
    {
        [Inject] private readonly ViewModelFactory _viewModelFactory;

        public AdventureScrollViewModel Scroll { get; private set; }
        public AdventureTopPanelViewModel TopPanel { get; private set; }
        public AdventureBottomPanelViewModel BottomPanel { get; private set; }

        public void Init()
        {
            Scroll = _viewModelFactory.Create<AdventureScrollViewModel>();
            Scroll.Init();

            TopPanel = _viewModelFactory.Create<AdventureTopPanelViewModel>();
            TopPanel.Init();

            BottomPanel = _viewModelFactory.Create<AdventureBottomPanelViewModel>();
            BottomPanel.Init();
        }

        protected override Options CreateOptions()
        {
            return new Options(canCloseOnEsc: false, hideInHistory: false, sortingLayer: SortingOrderLayer.COMMON);
        }

        public override void Dispose()
        {
            Scroll?.Dispose();
            Scroll = null;

            TopPanel?.Dispose();
            TopPanel = null;

            BottomPanel?.Dispose();
            BottomPanel = null;
        }
    }
}
