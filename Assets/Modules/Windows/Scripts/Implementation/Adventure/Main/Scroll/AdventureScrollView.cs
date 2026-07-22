using Modules.RPG.Scripts.Adventure.Data;
using Modules.Windows.Scripts.Implementation.Adventure.Main.Scroll.Items;
using Modules.Windows.Scripts.Implementation.Adventure.Main.Scroll.Items.Animation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Modules.Windows.Scripts.Implementation.Adventure.Main.Scroll
{
    /// <summary>
    /// Sub-view for adventure scroll content; attach to a child object inside <c>AdventureMainView</c> prefab.
    /// Spawns content prefabs from <see cref="AdventureScrollViewModel.ContentItems"/> and plays
    /// appearance animations sequentially. Assign Text / Image / Item prefabs in the inspector.
    /// </summary>
    public class AdventureScrollView : MonoBehaviour
    {
        [SerializeField] private Transform _contentRoot;
        [SerializeField] private AdventureContentViewBase _textContentPrefab;
        [SerializeField] private AdventureContentViewBase _imageContentPrefab;
        [SerializeField] private AdventureContentViewBase _itemContentPrefab;

        private readonly List<AdventureContentViewBase> _spawnedViews = new List<AdventureContentViewBase>();

        private AdventureScrollViewModel _viewModel;
        private Coroutine _sequenceCoroutine;
        private IContentAnimator _currentAnimator;
        private bool _skipAll;

        public void Init(AdventureScrollViewModel viewModel)
        {
            Unsubscribe();

            _viewModel = viewModel ?? throw new System.ArgumentNullException(nameof(viewModel));
            Subscribe();
            RebuildFromViewModel();
        }

        /// <summary>
        /// Skips the current appearance animation and shows all remaining content immediately.
        /// </summary>
        public void SkipAllShowAnimation()
        {
            if (_skipAll)
                return;

            _skipAll = true;
            _currentAnimator?.Skip();
        }

        private void Subscribe()
        {
            if (_viewModel == null)
                return;

            _viewModel.OnChangeCustom += OnChangeCustomHandler;
        }

        private void Unsubscribe()
        {
            if (_viewModel != null)
                _viewModel.OnChangeCustom -= OnChangeCustomHandler;
        }

        private void OnChangeCustomHandler(string tag)
        {
            if (tag == AdventureScrollViewModel.ON_CLEAR_CONTENT)
            {
                StopSequence();
                ClearSpawnedViews();
                return;
            }

            if (tag == AdventureScrollViewModel.ON_CHANGE_CONTENT)
            {
                RebuildFromViewModel();
                return;
            }

            if (tag == AdventureScrollViewModel.ON_SKIP_ALL_SHOW_ANIMATION)
                SkipAllShowAnimation();
        }

        private void RebuildFromViewModel()
        {
            StopSequence();
            ClearSpawnedViews();

            if (_viewModel == null || _viewModel.ContentItems == null || _viewModel.ContentItems.Count == 0)
                return;

            _skipAll = false;
            _sequenceCoroutine = StartCoroutine(PlaySequenceCoroutine(_viewModel.ContentItems));
        }

        private IEnumerator PlaySequenceCoroutine(IReadOnlyList<AdventureContentViewModelBase> items)
        {
            for (int i = 0; i < items.Count; i++)
            {
                var contentViewModel = items[i];
                if (contentViewModel == null || contentViewModel.IsDisposed)
                    continue;

                var view = SpawnView(contentViewModel);
                if (view == null)
                    continue;

                if (_skipAll)
                {
                    view.Animator?.Skip();
                    continue;
                }

                yield return WaitUntilContentReady(contentViewModel);

                if (_skipAll)
                {
                    view.Animator?.Skip();
                    continue;
                }

                yield return PlayAnimator(view.Animator);
            }

            _currentAnimator = null;
            _sequenceCoroutine = null;
        }

        private IEnumerator WaitUntilContentReady(AdventureContentViewModelBase contentViewModel)
        {
            if (contentViewModel.IsContentReady)
                yield break;

            bool ready = false;

            void OnContentReady()
            {
                ready = true;
            }

            contentViewModel.ContentReady += OnContentReady;

            try
            {
                while (!ready && !_skipAll && !contentViewModel.IsDisposed)
                {
                    if (contentViewModel.IsContentReady)
                        yield break;

                    yield return null;
                }
            }
            finally
            {
                contentViewModel.ContentReady -= OnContentReady;
            }
        }

        private IEnumerator PlayAnimator(IContentAnimator animator)
        {
            if (animator == null)
                yield break;

            bool completed = false;

            void OnCompleted()
            {
                completed = true;
            }

            _currentAnimator = animator;
            animator.Completed += OnCompleted;
            animator.Play();

            try
            {
                // Play may complete synchronously (empty text, zero duration, missing refs).
                while (!completed)
                    yield return null;
            }
            finally
            {
                animator.Completed -= OnCompleted;
                if (_currentAnimator == animator)
                    _currentAnimator = null;
            }
        }

        private AdventureContentViewBase SpawnView(AdventureContentViewModelBase contentViewModel)
        {
            var prefab = ResolvePrefab(contentViewModel);
            if (prefab == null)
            {
                UnityEngine.Debug.LogWarning(
                    $"[{nameof(AdventureScrollView)}] Prefab is not assigned for {contentViewModel.ContentType}.",
                    this);
                return null;
            }

            var parent = _contentRoot != null ? _contentRoot : transform;
            var instance = Instantiate(prefab, parent);
            instance.Init(contentViewModel);
            _spawnedViews.Add(instance);
            return instance;
        }

        private AdventureContentViewBase ResolvePrefab(AdventureContentViewModelBase contentViewModel)
        {
            switch (contentViewModel.ContentType)
            {
                case SceneContentType.Text:
                    return _textContentPrefab;

                case SceneContentType.Image:
                case SceneContentType.RandomImage:
                case SceneContentType.Slideshow:
                case SceneContentType.Splitter:
                    return _imageContentPrefab;

                case SceneContentType.Item:
                    return _itemContentPrefab;

                default:
                    return null;
            }
        }

        private void StopSequence()
        {
            if (_sequenceCoroutine != null)
            {
                StopCoroutine(_sequenceCoroutine);
                _sequenceCoroutine = null;
            }

            _currentAnimator = null;
            _skipAll = false;
        }

        private void ClearSpawnedViews()
        {
            for (int i = 0; i < _spawnedViews.Count; i++)
            {
                var view = _spawnedViews[i];
                if (view != null)
                    Destroy(view.gameObject);
            }

            _spawnedViews.Clear();
        }

        private void OnDestroy()
        {
            StopSequence();
            Unsubscribe();
            ClearSpawnedViews();
            _viewModel = null;
        }
    }
}
