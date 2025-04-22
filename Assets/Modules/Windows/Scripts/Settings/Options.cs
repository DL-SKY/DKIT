namespace Modules.Windows.Scripts.Settings
{
    public class Options
    {
        public bool CanCloseOnEsc { get; private set; }
        public bool HideInHistory { get; private set; }
        public SortingOrderLayer SortingLayer { get; private set; }

        public Options()
        {
            CanCloseOnEsc = true;
            HideInHistory = false;
            SortingLayer = SortingOrderLayer.COMMON;
        }

        public Options(bool canCloseOnEsc, bool hideInHistory, SortingOrderLayer sortingLayer)
        {
            CanCloseOnEsc = canCloseOnEsc;
            HideInHistory = hideInHistory;
            SortingLayer = sortingLayer;
        }
    }
}
