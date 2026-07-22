using Modules.Utils.Scripts.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Zenject;

namespace Modules.Windows.Scripts.Services
{
    /// <summary>
    /// Zenject singleton implementing <see cref="IImageCache"/> with disk cache under
    /// <see cref="CACHE_FOLDER_NAME"/> and downloads via <see cref="CoroutineHolder"/>.
    /// </summary>
    public sealed class CachedPathImageService : IImageCache
    {
        private const string CACHE_FOLDER_NAME = "CachedPathImages";
        private const int MAX_LOADING_COUNT = 5;

        [Inject] private readonly CoroutineHolder _coroutineHolder;

        private readonly Dictionary<string, Sprite> _memoryCache = new Dictionary<string, Sprite>();
        private readonly HashSet<string> _inFlightUrls = new HashSet<string>();
        private readonly HashSet<string> _queuedUrls = new HashSet<string>();
        private readonly Queue<string> _downloadQueue = new Queue<string>();
        private string _cacheFolderPath;

        public event Action<string, Sprite> RemoteSpriteReady;

        public bool IsRemoteUrl(string pathOrUrl)
        {
            if (string.IsNullOrWhiteSpace(pathOrUrl))
                return false;

            return Uri.TryCreate(pathOrUrl, UriKind.Absolute, out Uri uri)
                   && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }

        public bool TryLoadLocal(string path, out Sprite sprite)
        {
            sprite = null;

            if (string.IsNullOrWhiteSpace(path) || IsRemoteUrl(path))
                return false;

            sprite = Resources.Load<Sprite>(path);
            if (sprite != null)
                return true;

            Texture2D texture = Resources.Load<Texture2D>(path);
            if (texture == null)
                return false;

            sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f));
            return sprite != null;
        }

        public bool TryGetRemoteCached(string url, out Sprite sprite)
        {
            sprite = null;

            if (string.IsNullOrWhiteSpace(url) || !IsRemoteUrl(url))
                return false;

            if (_memoryCache.TryGetValue(url, out sprite) && sprite != null)
                return true;

            string cachePath = GetCachePath(url);
            if (!File.Exists(cachePath))
                return false;

            if (!TryCreateSpriteFromFile(cachePath, url, out sprite))
                return false;

            _memoryCache[url] = sprite;
            return true;
        }

        public void EnsureRemoteLoading(string url, bool prioritize = false)
        {
            if (string.IsNullOrWhiteSpace(url) || !IsRemoteUrl(url))
                return;

            if (_memoryCache.ContainsKey(url) || _inFlightUrls.Contains(url))
                return;

            if (TryGetRemoteCached(url, out Sprite cached))
            {
                RemoteSpriteReady?.Invoke(url, cached);
                return;
            }

            if (prioritize)
            {
                // Drop from prefetch queue if present, then start regardless of MAX_LOADING_COUNT.
                _queuedUrls.Remove(url);
                StartDownload(url);
                return;
            }

            if (_queuedUrls.Contains(url))
                return;

            _queuedUrls.Add(url);
            _downloadQueue.Enqueue(url);
            TryStartQueuedDownloads();
        }

        public void ClearMemoryCache()
        {
            foreach (KeyValuePair<string, Sprite> pair in _memoryCache)
            {
                if (pair.Value == null)
                    continue;

                Texture2D texture = pair.Value.texture;
                UnityEngine.Object.Destroy(pair.Value);
                if (texture != null)
                    UnityEngine.Object.Destroy(texture);
            }

            _memoryCache.Clear();
        }

        private void TryStartQueuedDownloads()
        {
            while (_downloadQueue.Count > 0 && _inFlightUrls.Count < MAX_LOADING_COUNT)
            {
                string url = _downloadQueue.Dequeue();

                // Stale entry: already promoted via prioritize, or otherwise removed from the set.
                if (!_queuedUrls.Remove(url))
                    continue;

                if (_memoryCache.ContainsKey(url) || _inFlightUrls.Contains(url))
                    continue;

                if (TryGetRemoteCached(url, out Sprite cached))
                {
                    RemoteSpriteReady?.Invoke(url, cached);
                    continue;
                }

                StartDownload(url);
            }
        }

        private void StartDownload(string url)
        {
            if (_inFlightUrls.Contains(url))
                return;

            _inFlightUrls.Add(url);
            _coroutineHolder.StartCoroutine(DownloadCoroutine(url, GetCachePath(url)));
        }

        private IEnumerator DownloadCoroutine(string url, string cachePath)
        {
            using UnityWebRequest request = UnityWebRequest.Get(url);
            yield return request.SendWebRequest();

            try
            {
                if (request.result != UnityWebRequest.Result.Success)
                {
                    UnityEngine.Debug.LogWarning(
                        $"[{nameof(CachedPathImageService)}] Failed to download '{url}': {request.error}");
                    RemoteSpriteReady?.Invoke(url, null);
                    yield break;
                }

                byte[] data = request.downloadHandler.data;
                if (data == null || data.Length == 0)
                {
                    RemoteSpriteReady?.Invoke(url, null);
                    yield break;
                }

                try
                {
                    EnsureCacheFolder();
                    File.WriteAllBytes(cachePath, data);
                }
                catch (Exception exception)
                {
                    UnityEngine.Debug.LogWarning(
                        $"[{nameof(CachedPathImageService)}] Failed to write cache for '{url}': {exception.Message}");
                }

                if (!TryCreateSpriteFromBytes(data, url, out Sprite sprite))
                {
                    RemoteSpriteReady?.Invoke(url, null);
                    yield break;
                }

                _memoryCache[url] = sprite;
                RemoteSpriteReady?.Invoke(url, sprite);
            }
            finally
            {
                _inFlightUrls.Remove(url);
                TryStartQueuedDownloads();
            }
        }

        private string GetCachePath(string url)
        {
            EnsureCacheFolder();
            return Path.Combine(_cacheFolderPath, ComputeStableFileName(url));
        }

        private void EnsureCacheFolder()
        {
            if (_cacheFolderPath == null)
                _cacheFolderPath = Path.Combine(Application.persistentDataPath, CACHE_FOLDER_NAME);

            if (!Directory.Exists(_cacheFolderPath))
                Directory.CreateDirectory(_cacheFolderPath);
        }

        private static string ComputeStableFileName(string url)
        {
            using SHA256 sha = SHA256.Create();
            byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(url));
            var builder = new StringBuilder(hash.Length * 2);
            for (int i = 0; i < hash.Length; i++)
                builder.Append(hash[i].ToString("x2"));
            return builder.ToString();
        }

        private static bool TryCreateSpriteFromFile(string filePath, string cacheKey, out Sprite sprite)
        {
            sprite = null;

            try
            {
                byte[] data = File.ReadAllBytes(filePath);
                return TryCreateSpriteFromBytes(data, cacheKey, out sprite);
            }
            catch (Exception exception)
            {
                UnityEngine.Debug.LogWarning(
                    $"[{nameof(CachedPathImageService)}] Failed to read cache file '{filePath}': {exception.Message}");
                return false;
            }
        }

        private static bool TryCreateSpriteFromBytes(byte[] data, string spriteName, out Sprite sprite)
        {
            sprite = null;

            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!texture.LoadImage(data))
            {
                UnityEngine.Object.Destroy(texture);
                return false;
            }

            texture.wrapMode = TextureWrapMode.Clamp;
            sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f));
            sprite.name = spriteName;
            return true;
        }
    }
}
