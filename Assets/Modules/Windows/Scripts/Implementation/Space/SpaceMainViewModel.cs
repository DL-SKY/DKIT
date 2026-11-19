using Modules.Windows.Scripts.Base;
using Modules.Windows.Scripts.Settings;

namespace Modules.Windows.Scripts.Implementation.Space
{
    public sealed class SpaceMainViewModel : ViewModelBase
    {
        public void Init()
        {
        }

        protected override Options CreateOptions()
        {
            return new Options(canCloseOnEsc: false, hideInHistory: false, sortingLayer: SortingOrderLayer.COMMON);
        }

        public override void Dispose()
        {
        }
    }
}
