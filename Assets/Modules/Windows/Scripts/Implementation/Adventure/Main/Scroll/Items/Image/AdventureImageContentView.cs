using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Modules.Windows.Scripts.Implementation.Adventure.Main.Scroll.Items.Image
{
    /// <summary>
    /// Shared view for Image / RandomImage / Slideshow / Splitter content.
    /// Pair with <see cref="Animation.FadeInContentAnimator"/> on the same GameObject.
    /// Loads sprites from <see cref="AdventureImageContentViewModelBase.CurrentPath"/> (Resources or URL).
    /// </summary>
    public sealed class AdventureImageContentView : AdventureContentViewBase<AdventureImageContentViewModelBase>
    {
        [SerializeField] private UnityEngine.UI.Image _image;
        [SerializeField] private float _slideshowIntervalSeconds = 3f;

        private Coroutine _loadCoroutine;
        private string _loadingPath;

        /// <summary>
        /// Interval used when the bound VM is <see cref="AdventureSlideshowContentViewModel"/>.
        /// Applied to the VM during <see cref="InitImplementation"/>.
        /// </summary>
        public float SlideshowIntervalSeconds => _slideshowIntervalSeconds;

        protected override void InitImplementation()
        {
            if (_viewModel is AdventureSlideshowContentViewModel slideshow)
                slideshow.SetIntervalSeconds(_slideshowIntervalSeconds);

            LoadAndApplyCurrentPath();
        }

        protected override void Subscribe()
        {
            _viewModel.OnChangeCustom += OnChangeCustomHandler;
        }

        protected override void Unsubscribe()
        {
            if (_viewModel != null)
                _viewModel.OnChangeCustom -= OnChangeCustomHandler;

            StopRemoteLoad();
        }

        private void OnChangeCustomHandler(string tag)
        {
            if (tag == AdventureImageContentViewModelBase.ON_CHANGE_PATH)
                LoadAndApplyCurrentPath();
        }

        private void LoadAndApplyCurrentPath()
        {
            if (_image == null)
            {
                UnityEngine.Debug.LogWarning(
                    $"[{nameof(AdventureImageContentView)}] Image is not assigned on '{name}'.",
                    this);
                return;
            }

            string path = _viewModel.CurrentPath;
            StopRemoteLoad();

            if (string.IsNullOrWhiteSpace(path))
            {
                ApplySprite(null);
                return;
            }

            if (AdventureContentSpriteUtility.IsRemoteUrl(path))
            {
                ApplySprite(null);
                _loadingPath = path;
                _loadCoroutine = StartCoroutine(LoadRemoteSpriteCoroutine(path));
                return;
            }

            if (AdventureContentSpriteUtility.TryLoadFromResources(path, out Sprite sprite))
                ApplySprite(sprite);
            else
            {
                UnityEngine.Debug.LogWarning(
                    $"[{nameof(AdventureImageContentView)}] Failed to load sprite from Resources path '{path}'.",
                    this);
                ApplySprite(null);
            }
        }

        private void ApplySprite(Sprite sprite)
        {
            _image.sprite = sprite;
            _image.enabled = sprite != null;
        }

        private IEnumerator LoadRemoteSpriteCoroutine(string url)
        {
            using UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
            yield return request.SendWebRequest();

            _loadCoroutine = null;

            if (_viewModel == null)
                yield break;

            if (_viewModel.CurrentPath != url || _loadingPath != url)
                yield break;

            if (request.result != UnityWebRequest.Result.Success)
            {
                UnityEngine.Debug.LogWarning(
                    $"[{nameof(AdventureImageContentView)}] Failed to load image '{url}': {request.error}",
                    this);
                ApplySprite(null);
                yield break;
            }

            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f));

            ApplySprite(sprite);
        }

        private void StopRemoteLoad()
        {
            _loadingPath = null;

            if (_loadCoroutine == null)
                return;

            StopCoroutine(_loadCoroutine);
            _loadCoroutine = null;
        }
    }
}
