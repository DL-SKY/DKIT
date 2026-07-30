using Modules.Windows.Scripts.Base;
using Modules.Windows.Scripts.Implementation.Adventure.Main.BottomPanel;
using Modules.Windows.Scripts.Implementation.Adventure.Main.Scroll;
using Modules.Windows.Scripts.Implementation.Adventure.Main.TopPanel;
using UnityEngine;

namespace Modules.Windows.Scripts.Implementation.Adventure.Main
{
    /// <summary>
    /// Main Adventure screen. Prefab must live under Resources at <see cref="Path"/>.
    /// </summary>
    public class AdventureMainView : ViewBase<AdventureMainViewModel>
    {
        public static string Path = "Prefabs/Views/Adventure/AdventureMainView";

        [Header("Subviews")]
        [SerializeField] private AdventureScrollView _scrollView;
        [SerializeField] private AdventureTopPanelView _topPanelView;
        [SerializeField] private AdventureBottomPanelView _bottomPanelView;

        protected override void InitImplementation()
        {
            if (_scrollView != null && _viewModel.Scroll != null)
                _scrollView.Init(_viewModel.Scroll);
            else if (_scrollView == null)
                UnityEngine.Debug.LogWarning(
                    $"[{nameof(AdventureMainView)}] {nameof(AdventureScrollView)} is not assigned on '{name}'.",
                    this);

            if (_topPanelView != null && _viewModel.TopPanel != null)
                _topPanelView.Init(_viewModel.TopPanel);
            else if (_topPanelView == null)
                UnityEngine.Debug.LogWarning(
                    $"[{nameof(AdventureMainView)}] {nameof(AdventureTopPanelView)} is not assigned on '{name}'.",
                    this);

            if (_bottomPanelView != null && _viewModel.BottomPanel != null)
                _bottomPanelView.Init(_viewModel.BottomPanel);
            else if (_bottomPanelView == null)
                UnityEngine.Debug.LogWarning(
                    $"[{nameof(AdventureMainView)}] {nameof(AdventureBottomPanelView)} is not assigned on '{name}'.",
                    this);
        }

        protected override void Subscribe()
        {
            // _viewModel.OnChange += ...
            // _viewModel.OnChangeCustom += ...
        }

        protected override void Unsubscribe()
        {
            // Remove all subscriptions from Subscribe
        }

        public override void Show()
        {
            base.Show();
        }
    }
}
