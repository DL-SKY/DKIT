using Modules.Windows.Scripts.Base;

namespace Modules.Windows.Scripts.Implementation.Adventure.Main.Scroll
{
    /// <summary>
    /// ViewModel for adventure scroll sub-view: state, service subscriptions, commands for <see cref="AdventureScrollView"/>.
    /// </summary>
    public class AdventureScrollViewModel : ViewModelBase
    {
        public override void Dispose()
        {
            // Unsubscribe from external sources if added outside Subscribe in the View.
        }
    }
}
