namespace Modules.Windows.Scripts.Settings
{
    public class Options
    {
        public bool CanCloseOnEsc { get; private set; }
        public SortingOrderLayer SortingLayer { get; private set; }

        public Options()
        {
            CanCloseOnEsc = true;
            SortingLayer = SortingOrderLayer.COMMON;
        }

        public Options(bool canCloseOnEsc, SortingOrderLayer sortingLayer)
        {
            CanCloseOnEsc = canCloseOnEsc;
            SortingLayer = sortingLayer;
        }
    }
}
