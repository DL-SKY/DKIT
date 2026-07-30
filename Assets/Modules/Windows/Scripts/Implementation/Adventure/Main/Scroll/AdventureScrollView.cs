using Modules.RPG.Scripts.Adventure.Data;
using Modules.Windows.Scripts.Implementation.Adventure.Main.Scroll.Items;
using Modules.Windows.Scripts.Implementation.Adventure.Main.Scroll.Items.Animation;
using Modules.Windows.Scripts.Implementation.Adventure.Main.Scroll.Items.Choice;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.Windows.Scripts.Implementation.Adventure.Main.Scroll
{
    /// <summary>
    /// Sub-view for adventure scroll content; attach to a child object inside <c>AdventureMainView</c> prefab.
    /// Spawns content prefabs for newly appended <see cref="AdventureScrollViewModel.ContentItems"/> and plays
    /// appearance animations sequentially. After the content sequence finishes (or on skip), spawns choice panels
    /// and plays their appearance animations in parallel.
    /// Before appending onto existing content, reserves bottom viewport space (9/10 of viewport height)
    /// and scrolls to the bottom so new items appear in the freed area (skippable with the rest of the
    /// show sequence). Bottom padding is restored to the window baseline after choices are presented.
    /// Existing content panels are removed only on <c>ON_CLEAR_CONTENT</c>.
    /// Choice panels are cleared on <c>ON_CLEAR_CONTENT</c>, <c>ON_APPEND_CONTENT</c>, and <c>ON_CLEAR_CHOICES</c>.
    /// Assign Text / Image / Splitter / Item / Choice prefabs in the inspector.
    /// </summary>
    public class AdventureScrollView : MonoBehaviour
    {
        private const float SPACE_PREP_SCROLL_DURATION = 0.5f;
        private const float SPACE_PREP_BOTTOM_PADDING_VIEWPORT_FRACTION = 3f / 4f;      //9f / 10f;

        [Header("Links")]
        [SerializeField] private Transform _contentRoot;

        [Header("Content Prefabs")]
        [SerializeField] private AdventureContentViewBase _textContentPrefab;
        [SerializeField] private AdventureContentViewBase _imageContentPrefab;
        [SerializeField] private AdventureContentViewBase _splitterContentPrefab;
        [SerializeField] private AdventureContentViewBase _itemContentPrefab;

        [Header("Choice Prefabs")]
        [SerializeField] private AdventureChoiceViewBase _choicePrefab;

        private readonly List<AdventureContentViewBase> _spawnedViews = new List<AdventureContentViewBase>();
        private readonly List<AdventureChoiceViewBase> _spawnedChoiceViews = new List<AdventureChoiceViewBase>();

        private AdventureScrollViewModel _viewModel;
        private Coroutine _sequenceCoroutine;
        private IContentAnimator _currentAnimator;
        private ScrollRect _scrollRect;
        private VerticalLayoutGroup _contentLayoutGroup;
        private int _defaultBottomPadding;
        private bool _hasDefaultBottomPadding;
        private bool _skipAll;
        private bool _choicesPending;
        private bool _needsSpacePrep;

        private void Awake()
        {
            CacheScrollLinks();
        }

        public void Init(AdventureScrollViewModel viewModel)
        {
            Unsubscribe();

            _viewModel = viewModel ?? throw new System.ArgumentNullException(nameof(viewModel));
            CacheScrollLinks();
            Subscribe();
            AppendPendingFromViewModel();
        }

        /// <summary>
        /// Skips the current appearance animation and shows all remaining content / choices immediately.
        /// </summary>
        public void SkipAllShowAnimation()
        {
            if (_skipAll)
                return;

            _skipAll = true;
            _currentAnimator?.Skip();
            SkipChoiceAnimators();
        }

        private void CacheScrollLinks()
        {
            if (_scrollRect == null)
                _scrollRect = GetComponent<ScrollRect>();

            if (_contentLayoutGroup == null && _contentRoot != null)
                _contentLayoutGroup = _contentRoot.GetComponent<VerticalLayoutGroup>();

            // Capture prefab/layout baseline once when the window is created.
            if (!_hasDefaultBottomPadding && _contentLayoutGroup != null)
            {
                _defaultBottomPadding = _contentLayoutGroup.padding.bottom;
                _hasDefaultBottomPadding = true;
            }
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
                ClearSpawnedChoiceViews();
                _choicesPending = false;
                _needsSpacePrep = false;
                RestoreDefaultBottomPadding();
                return;
            }

            if (tag == AdventureScrollViewModel.ON_APPEND_CONTENT)
            {
                // New content replaces any previously shown choice panels until the sequence finishes.
                ClearSpawnedChoiceViews();
                _choicesPending = true;

                // Reserve free space and scroll down before spawning onto an existing feed.
                if (_spawnedViews.Count > 0 && HasPendingItems())
                    _needsSpacePrep = true;

                AppendPendingFromViewModel();
                return;
            }

            if (tag == AdventureScrollViewModel.ON_CLEAR_CHOICES)
            {
                ClearSpawnedChoiceViews();
                return;
            }

            if (tag == AdventureScrollViewModel.ON_REFRESH_CHOICES)
            {
                OnRefreshChoices();
                return;
            }

            if (tag == AdventureScrollViewModel.ON_SKIP_ALL_SHOW_ANIMATION)
                SkipAllShowAnimation();
        }

        private void OnRefreshChoices()
        {
            if (IsSequenceRunning)
            {
                // Present after the current content sequence finishes.
                _choicesPending = true;
                return;
            }

            PresentChoices();
        }

        /// <summary>
        /// Spawns and animates content items that do not yet have a view.
        /// Does not remove already spawned content panels.
        /// </summary>
        private void AppendPendingFromViewModel()
        {
            if (HasPendingItems())
            {
                EnsureSequenceRunning();
                return;
            }

            // No new content to animate — show choices if they were marked pending (e.g. after clear/rebuild).
            if (_choicesPending || _spawnedChoiceViews.Count == 0)
                PresentChoices();
        }

        private bool HasPendingItems()
        {
            if (_viewModel?.ContentItems == null)
                return false;

            return _spawnedViews.Count < _viewModel.ContentItems.Count;
        }

        private bool IsSequenceRunning => _sequenceCoroutine != null;

        private void EnsureSequenceRunning()
        {
            if (_sequenceCoroutine != null)
                return;

            _skipAll = false;
            _sequenceCoroutine = StartCoroutine(PlayPendingSequenceCoroutine());
        }

        private IEnumerator PlayPendingSequenceCoroutine()
        {
            while (HasPendingItems())
            {
                if (_needsSpacePrep)
                {
                    _needsSpacePrep = false;
                    yield return PrepareSpaceForNewContentCoroutine();
                }

                var contentViewModel = _viewModel.ContentItems[_spawnedViews.Count];
                if (contentViewModel == null || contentViewModel.IsDisposed)
                {
                    // Keep list indices aligned with spawned views.
                    _spawnedViews.Add(null);
                    continue;
                }

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

            // Items may have been appended after the last HasPendingItems check.
            if (HasPendingItems())
            {
                EnsureSequenceRunning();
                yield break;
            }

            PresentChoices();
        }

        /// <summary>
        /// Sets bottom padding to 9/10 of the viewport height and scrolls to the bottom so the last
        /// existing content sits in the top tenth, leaving free space for newly appended items.
        /// Padding is restored to the window baseline after choices are presented.
        /// </summary>
        private IEnumerator PrepareSpaceForNewContentCoroutine()
        {
            CacheScrollLinks();

            if (_scrollRect == null || _contentLayoutGroup == null)
                yield break;

            RectTransform viewport = _scrollRect.viewport != null
                ? _scrollRect.viewport
                : _scrollRect.transform as RectTransform;

            if (viewport == null)
                yield break;

            int bottomPadding = Mathf.RoundToInt(viewport.rect.height * SPACE_PREP_BOTTOM_PADDING_VIEWPORT_FRACTION);
            SetBottomPadding(bottomPadding);

            yield return ScrollToBottomCoroutine(SPACE_PREP_SCROLL_DURATION);
        }

        private void RestoreDefaultBottomPadding()
        {
            if (!_hasDefaultBottomPadding)
                return;

            // Revert
            SetBottomPadding(_defaultBottomPadding);
        }

        private void SetBottomPadding(int bottom)
        {
            CacheScrollLinks();

            if (_contentLayoutGroup == null)
                return;

            RectOffset padding = _contentLayoutGroup.padding;
            if (padding.bottom == bottom)
                return;

            padding.bottom = bottom;
            _contentLayoutGroup.padding = padding;

            if (_contentRoot is RectTransform contentRect)
            {
                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
            }
        }

        private IEnumerator ScrollToBottomCoroutine(float duration)
        {
            if (_scrollRect == null)
                yield break;

            const float targetNormalized = 0f;
            float startNormalized = _scrollRect.verticalNormalizedPosition;

            if (_skipAll || duration <= 0f)
            {
                _scrollRect.verticalNormalizedPosition = targetNormalized;
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                if (_skipAll)
                {
                    _scrollRect.verticalNormalizedPosition = targetNormalized;
                    yield break;
                }

                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                _scrollRect.verticalNormalizedPosition = Mathf.Lerp(startNormalized, targetNormalized, t);
                yield return null;
            }

            _scrollRect.verticalNormalizedPosition = targetNormalized;
        }

        private void PresentChoices()
        {
            _choicesPending = false;

            try
            {
                if (_viewModel?.ChoiceItems == null || _viewModel.ChoiceItems.Count == 0)
                    return;

                if (_spawnedChoiceViews.Count > 0)
                    return;

                for (int i = 0; i < _viewModel.ChoiceItems.Count; i++)
                {
                    var choiceViewModel = _viewModel.ChoiceItems[i];
                    if (choiceViewModel == null || choiceViewModel.IsDisposed)
                    {
                        _spawnedChoiceViews.Add(null);
                        continue;
                    }

                    var view = SpawnChoiceView(choiceViewModel);
                    if (view == null)
                        continue;

                    if (_skipAll)
                        view.Animator?.Skip();
                    else
                        view.Animator?.Play();
                }
            }
            finally
            {
                // Return reserved prep space after choices are created (or when there are none).
                RestoreDefaultBottomPadding();
            }
        }

        private void SkipChoiceAnimators()
        {
            for (int i = 0; i < _spawnedChoiceViews.Count; i++)
                _spawnedChoiceViews[i]?.Animator?.Skip();
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
                // Placeholder keeps ContentItems index aligned with _spawnedViews.
                _spawnedViews.Add(null);
                return null;
            }

            var parent = _contentRoot != null ? _contentRoot : transform;
            var instance = Instantiate(prefab, parent);
            instance.Init(contentViewModel);
            _spawnedViews.Add(instance);
            return instance;
        }

        private AdventureChoiceViewBase SpawnChoiceView(AdventureChoiceViewModelBase choiceViewModel)
        {
            if (_choicePrefab == null)
            {
                UnityEngine.Debug.LogWarning(
                    $"[{nameof(AdventureScrollView)}] Choice prefab is not assigned.",
                    this);
                _spawnedChoiceViews.Add(null);
                return null;
            }

            var parent = _contentRoot != null ? _contentRoot : transform;
            var instance = Instantiate(_choicePrefab, parent);
            instance.Init(choiceViewModel);
            _spawnedChoiceViews.Add(instance);
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
                    return _imageContentPrefab;

                case SceneContentType.Splitter:
                    return _splitterContentPrefab;

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

        private void ClearSpawnedChoiceViews()
        {
            for (int i = 0; i < _spawnedChoiceViews.Count; i++)
            {
                var view = _spawnedChoiceViews[i];
                if (view != null)
                    Destroy(view.gameObject);
            }

            _spawnedChoiceViews.Clear();
        }

        private void OnDestroy()
        {
            StopSequence();
            Unsubscribe();
            ClearSpawnedViews();
            ClearSpawnedChoiceViews();
            _viewModel = null;
        }
    }
}
