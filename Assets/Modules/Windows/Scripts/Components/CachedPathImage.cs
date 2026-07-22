using Modules.Windows.Scripts.Services;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Modules.Windows.Scripts.Components
{
    /// <summary>
    /// Binds a path or URL to a <see cref="Image"/> via <see cref="IImageCache"/>.
    /// Local Resources paths load immediately; remote URLs may show placeholder/throbber while downloading.
    /// In-flight downloads are not cancelled when <see cref="SetPath"/> changes — completed
    /// downloads are still cached, but applied to the Image only if the path is still current.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public sealed class CachedPathImage : MonoBehaviour
    {
        [SerializeField] private Image _image;
        [SerializeField] private GameObject _placeholder;
        [SerializeField] private GameObject _throbber;

        [Inject] private IImageCache _imageCache;

        private string _currentPath = string.Empty;
        private bool _subscribed;

        private void Awake()
        {
            if (_image == null)
                _image = GetComponent<Image>();

            // Prefabs opened via Resources.Instantiate are not always Zenject-injected.
            if (_imageCache == null && ProjectContext.HasInstance)
                _imageCache = ProjectContext.Instance.Container.Resolve<IImageCache>();
        }

        private void OnEnable()
        {
            EnsureResolved();
            if (_imageCache == null || _subscribed)
                return;

            _imageCache.RemoteSpriteReady += OnRemoteSpriteReady;
            _subscribed = true;
        }

        private void OnDisable()
        {
            if (!_subscribed || _imageCache == null)
                return;

            _imageCache.RemoteSpriteReady -= OnRemoteSpriteReady;
            _subscribed = false;
        }

        /// <summary>
        /// Shows the image at <paramref name="pathOrUrl"/> (Resources path or http(s) URL).
        /// </summary>
        public void SetPath(string pathOrUrl)
        {
            EnsureResolved();
            _currentPath = pathOrUrl ?? string.Empty;

            if (_image == null)
            {
                UnityEngine.Debug.LogWarning(
                    $"[{nameof(CachedPathImage)}] Image is not assigned on '{name}'.",
                    this);
                return;
            }

            if (_imageCache == null)
            {
                UnityEngine.Debug.LogWarning(
                    $"[{nameof(CachedPathImage)}] {nameof(IImageCache)} is not available on '{name}'.",
                    this);
                return;
            }

            if (string.IsNullOrWhiteSpace(_currentPath))
            {
                ClearImage();
                SetLoadingVisuals(showPlaceholder: true, showThrobber: false);
                return;
            }

            if (_imageCache.IsRemoteUrl(_currentPath))
            {
                TryShowRemote(_currentPath);
                return;
            }

            TryShowLocal(_currentPath);
        }

        private void OnRemoteSpriteReady(string url, Sprite sprite)
        {
            if (_currentPath != url)
                return;

            if (sprite == null)
            {
                ClearImage();
                SetLoadingVisuals(showPlaceholder: true, showThrobber: false);
                return;
            }

            ApplySprite(sprite);
            SetLoadingVisuals(showPlaceholder: false, showThrobber: false);
        }

        private void TryShowLocal(string path)
        {
            if (_imageCache.TryLoadLocal(path, out Sprite sprite))
            {
                ApplySprite(sprite);
                SetLoadingVisuals(showPlaceholder: false, showThrobber: false);
                return;
            }

            UnityEngine.Debug.LogWarning(
                $"[{nameof(CachedPathImage)}] Failed to load local sprite '{path}'.",
                this);
            ClearImage();
            SetLoadingVisuals(showPlaceholder: true, showThrobber: false);
        }

        private void TryShowRemote(string url)
        {
            if (_imageCache.TryGetRemoteCached(url, out Sprite cachedSprite))
            {
                ApplySprite(cachedSprite);
                SetLoadingVisuals(showPlaceholder: false, showThrobber: false);
                return;
            }

            ClearImage();
            SetLoadingVisuals(showPlaceholder: true, showThrobber: true);
            _imageCache.EnsureRemoteLoading(url, prioritize: true);
        }

        private void ApplySprite(Sprite sprite)
        {
            _image.sprite = sprite;
            _image.enabled = sprite != null;
        }

        private void ClearImage()
        {
            _image.sprite = null;
            _image.enabled = false;
        }

        private void SetLoadingVisuals(bool showPlaceholder, bool showThrobber)
        {
            if (_placeholder != null)
                _placeholder.SetActive(showPlaceholder);

            if (_throbber != null)
                _throbber.SetActive(showThrobber);
        }

        private void EnsureResolved()
        {
            if (_imageCache != null)
                return;

            if (ProjectContext.HasInstance)
                _imageCache = ProjectContext.Instance.Container.Resolve<IImageCache>();
        }
    }
}
