using UnityEngine;

namespace Modules.Windows.Scripts.Implementation.Adventure.Main.Scroll
{
    /// <summary>
    /// Sub-view for adventure scroll content; attach to a child object inside <c>AdventureMainView</c> prefab.
    /// </summary>
    public class AdventureScrollView : MonoBehaviour
    {
        // [SerializeField] private ... UI element references

        protected AdventureScrollViewModel _viewModel;

        public void Init(AdventureScrollViewModel viewModel)
        {
            _viewModel = viewModel;

            Subscribe();
        }

        private void Subscribe()
        {
            // _viewModel.OnChange += ...
            // _viewModel.OnChangeCustom += ...
        }

        private void Unsubscribe()
        {
            // Remove all subscriptions from Subscribe
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }
    }
}
