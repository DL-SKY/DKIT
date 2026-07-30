using Modules.RPG.Scripts.Adventure;
using Modules.RPG.Scripts.Adventure.Choice;
using Modules.RPG.Scripts.Adventure.Data;
using Modules.Utils.Scripts.Input;
using Modules.Windows.Scripts.Base;
using Modules.Windows.Scripts.Implementation.Adventure.Main.Scroll.Items;
using Modules.Windows.Scripts.Implementation.Adventure.Main.Scroll.Items.Choice;
using Modules.Windows.Scripts.Implementation.Adventure.Main.Scroll.Items.Image;
using Modules.Windows.Scripts.Implementation.Adventure.Main.Scroll.Items.Item;
using Modules.Windows.Scripts.Implementation.Adventure.Main.Scroll.Items.Text;
using System.Collections.Generic;
using Zenject;

namespace Modules.Windows.Scripts.Implementation.Adventure.Main.Scroll
{
    /// <summary>
    /// ViewModel for adventure scroll: appends content item VMs from <see cref="AdventuresManager"/>
    /// and maintains choice panel VMs from <see cref="AdventuresManager.GetCurrentChoices"/>.
    /// Notifies <see cref="AdventureScrollView"/> to spawn / sequence content and present choices.
    /// Existing content panels are removed only via <see cref="ClearContent"/>.
    /// </summary>
    public class AdventureScrollViewModel : ViewModelBase
    {
        public const string ON_APPEND_CONTENT = "ON_APPEND_CONTENT";
        public const string ON_CLEAR_CONTENT = "ON_CLEAR_CONTENT";
        public const string ON_SKIP_ALL_SHOW_ANIMATION = "ON_SKIP_ALL_SHOW_ANIMATION";
        public const string ON_CLEAR_CHOICES = "ON_CLEAR_CHOICES";
        public const string ON_REFRESH_CHOICES = "ON_REFRESH_CHOICES";

        [Inject] private readonly AdventuresManager _adventuresManager;
        [Inject] private readonly ScreenInputService _screenInput;
        [Inject] private readonly DiContainer _container;

        private readonly List<AdventureContentViewModelBase> _contentItems = new List<AdventureContentViewModelBase>();
        private readonly List<AdventureChoiceViewModelBase> _choiceItems = new List<AdventureChoiceViewModelBase>();

        private bool _isInitialized;

        public IReadOnlyList<AdventureContentViewModelBase> ContentItems => _contentItems;

        public IReadOnlyList<AdventureChoiceViewModelBase> ChoiceItems => _choiceItems;

        public void Init()
        {
            if (_isInitialized)
                return;

            _isInitialized = true;
            Subscribe();
            AppendContent();
            RebuildChoices();
        }

        /// <summary>
        /// Skips the current appearance animation and reveals all remaining content instantly.
        /// Also invoked automatically on any <see cref="ScreenInputType.PointerDown"/>.
        /// </summary>
        public void SkipAllShowAnimation()
        {
            SendOnChange(ON_SKIP_ALL_SHOW_ANIMATION);
        }

        /// <summary>
        /// Removes all content and choice item VMs and asks the view to destroy spawned panels.
        /// </summary>
        public void ClearContent()
        {
            // Tear down visuals first so views unsubscribe while VMs are still alive.
            SendOnChange(ON_CLEAR_CONTENT);
            DisposeContentItems();
            DisposeChoiceItems();
        }

        public override void Dispose()
        {
            Unsubscribe();
            ClearContent();
            _isInitialized = false;
        }

        private void Subscribe()
        {
            _adventuresManager.ChangedScene += OnChangedSceneHandler;
            _adventuresManager.ChangedContent += OnChangedContentHandler;
            _adventuresManager.ChangedChoices += OnChangedChoicesHandler;
            _screenInput.OnInput += OnScreenInputHandler;
        }

        private void Unsubscribe()
        {
            if (_adventuresManager != null)
            {
                _adventuresManager.ChangedScene -= OnChangedSceneHandler;
                _adventuresManager.ChangedContent -= OnChangedContentHandler;
                _adventuresManager.ChangedChoices -= OnChangedChoicesHandler;
            }

            if (_screenInput != null)
                _screenInput.OnInput -= OnScreenInputHandler;
        }

        private void OnScreenInputHandler(ScreenInputType inputType)
        {
            if (inputType == ScreenInputType.PointerDown)
                SkipAllShowAnimation();
        }

        private void OnChangedSceneHandler(string sceneId)
        {
            if (_adventuresManager.ShouldClearContent())
                ClearContent();
        }

        private void OnChangedContentHandler()
        {
            AppendContent();
        }

        private void OnChangedChoicesHandler()
        {
            RebuildChoices();
        }

        private void AppendContent()
        {
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

            SendOnChange(ON_APPEND_CONTENT);
        }

        private void RebuildChoices()
        {
            // Tear down choice visuals first so views unsubscribe while VMs are still alive.
            SendOnChange(ON_CLEAR_CHOICES);
            DisposeChoiceItems();

            var choicesData = _adventuresManager.GetCurrentChoices();
            if (choicesData != null)
            {
                for (int i = 0; i < choicesData.Count; i++)
                {
                    var data = choicesData[i];
                    if (data == null)
                        continue;

                    var viewModel = CreateChoiceViewModel(data);
                    if (viewModel != null)
                        _choiceItems.Add(viewModel);
                }
            }

            SendOnChange(ON_REFRESH_CHOICES);
        }

        private void DisposeContentItems()
        {
            for (int i = 0; i < _contentItems.Count; i++)
                _contentItems[i]?.Dispose();

            _contentItems.Clear();
        }

        private void DisposeChoiceItems()
        {
            for (int i = 0; i < _choiceItems.Count; i++)
                _choiceItems[i]?.Dispose();

            _choiceItems.Clear();
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

        private AdventureChoiceViewModelBase CreateChoiceViewModel(ChoiceData data)
        {
            var viewModel = _container.Instantiate<AdventureChoiceViewModel>();
            viewModel.Init(data);
            return viewModel;
        }
    }
}
