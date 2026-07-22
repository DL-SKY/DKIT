using Modules.RPG.Scripts.Adventure;
using Modules.RPG.Scripts.Adventure.Data;
using Modules.Windows.Scripts.Base;
using Modules.Windows.Scripts.Implementation.Adventure.Main.Scroll.Items;
using Modules.Windows.Scripts.Implementation.Adventure.Main.Scroll.Items.Image;
using Modules.Windows.Scripts.Implementation.Adventure.Main.Scroll.Items.Item;
using Modules.Windows.Scripts.Implementation.Adventure.Main.Scroll.Items.Text;
using System.Collections.Generic;
using Zenject;

namespace Modules.Windows.Scripts.Implementation.Adventure.Main.Scroll
{
    /// <summary>
    /// ViewModel for adventure scroll: builds content item VMs from <see cref="AdventuresManager"/>
    /// and notifies <see cref="AdventureScrollView"/> to spawn / sequence them.
    /// </summary>
    public class AdventureScrollViewModel : ViewModelBase
    {
        public const string ON_CHANGE_CONTENT = "ON_CHANGE_CONTENT";
        public const string ON_CLEAR_CONTENT = "ON_CLEAR_CONTENT";
        public const string ON_SKIP_ALL_SHOW_ANIMATION = "ON_SKIP_ALL_SHOW_ANIMATION";

        [Inject] private readonly AdventuresManager _adventuresManager;
        [Inject] private readonly DiContainer _container;

        private readonly List<AdventureContentViewModelBase> _contentItems = new List<AdventureContentViewModelBase>();

        private bool _isInitialized;

        public IReadOnlyList<AdventureContentViewModelBase> ContentItems => _contentItems;

        public void Init()
        {
            if (_isInitialized)
                return;

            _isInitialized = true;
            Subscribe();
            RebuildContent();
        }

        /// <summary>
        /// Skips the current appearance animation and reveals all remaining content instantly.
        /// Tap / click wiring is handled outside — call this when a skip is requested.
        /// </summary>
        public void SkipAllShowAnimation()
        {
            SendOnChange(ON_SKIP_ALL_SHOW_ANIMATION);
        }

        public override void Dispose()
        {
            Unsubscribe();
            ClearContentItems();
            _isInitialized = false;
        }

        private void Subscribe()
        {
            _adventuresManager.ChangedContent += OnChangedContentHandler;
        }

        private void Unsubscribe()
        {
            if (_adventuresManager != null)
                _adventuresManager.ChangedContent -= OnChangedContentHandler;
        }

        private void OnChangedContentHandler()
        {
            RebuildContent();
        }

        private void RebuildContent()
        {
            // Tear down visuals first so views unsubscribe while VMs are still alive.
            SendOnChange(ON_CLEAR_CONTENT);
            ClearContentItems();

            var contentData = _adventuresManager.GetCurrentContent();
            if (contentData != null)
            {
                for (int i = 0; i < contentData.Count; i++)
                {
                    var data = contentData[i];
                    if (data == null)
                        continue;

                    var viewModel = CreateContentViewModel(data);
                    if (viewModel != null)
                        _contentItems.Add(viewModel);
                }
            }

            SendOnChange(ON_CHANGE_CONTENT);
        }

        private void ClearContentItems()
        {
            for (int i = 0; i < _contentItems.Count; i++)
                _contentItems[i]?.Dispose();

            _contentItems.Clear();
        }

        private AdventureContentViewModelBase CreateContentViewModel(SceneContentData data)
        {
            AdventureContentViewModelBase viewModel;

            switch (data.Type)
            {
                case SceneContentType.Text:
                    viewModel = _container.Instantiate<AdventureTextContentViewModel>();
                    break;

                case SceneContentType.Image:
                    viewModel = _container.Instantiate<AdventureImageContentViewModel>();
                    break;

                case SceneContentType.RandomImage:
                    viewModel = _container.Instantiate<AdventureRandomImageContentViewModel>();
                    break;

                case SceneContentType.Slideshow:
                    viewModel = _container.Instantiate<AdventureSlideshowContentViewModel>();
                    break;

                case SceneContentType.Splitter:
                    viewModel = _container.Instantiate<AdventureSplitterContentViewModel>();
                    break;

                case SceneContentType.Item:
                    viewModel = _container.Instantiate<AdventureItemContentViewModel>();
                    break;

                default:
                    UnityEngine.Debug.LogWarning(
                        $"[{nameof(AdventureScrollViewModel)}] Unsupported {nameof(SceneContentType)} '{data.Type}'.");
                    return null;
            }

            viewModel.Init(data);
            return viewModel;
        }
    }
}
