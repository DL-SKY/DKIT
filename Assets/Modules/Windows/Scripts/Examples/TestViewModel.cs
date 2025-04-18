using Modules.Windows.Scripts.Base;
using Modules.Windows.Scripts.Settings;

namespace Modules.Windows.Scripts.Examples
{
    public class TestViewModel : ViewModelBase
    {
        public void OnClick()
        {
            Example.Manager.CloseView(_viewHandle);
        }

        protected override Options CreateOptions()
        {
            //return new Options();
            return new Options(canCloseOnEsc: true, sortingLayer: SortingOrderLayer.COMMON);
        }

        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
