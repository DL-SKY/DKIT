using Modules.Utils.Scripts.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Profiling;
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
        private readonly Dictionary<string, int> _imageUsageCounters = new Dictionary<string, int>();
        private readonly HashSet<string> _inFlightKeys = new HashSet<string>();
        private readonly HashSet<string> _queuedKeys = new HashSet<string>();
        private readonly Queue<RemoteLoadRequest> _downloadQueue = new Queue<RemoteLoadRequest>();
        private string _cacheFolderPath;

        public event Action<string, Sprite> RemoteSpriteReady;

        public bool IsRemoteUrl(string pathOrUrl)
        {
            return TryGetRemoteInfo(pathOrUrl, out _, out _);
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

            if (!TryGetRemoteInfo(url, out string imageKey, out string remoteUrl))
                return false;

            if (_memoryCache.TryGetValue(imageKey, out sprite) && sprite != null)
                return true;

            string cachePath = GetCachePath(remoteUrl);
            if (!File.Exists(cachePath))
                return false;

            if (!TryCreateSpriteFromFile(cachePath, remoteUrl, out sprite))
                return false;

            _memoryCache[imageKey] = sprite;
            return true;
        }

        public string GetImageKey(string pathOrUrl)
        {
            if (string.IsNullOrWhiteSpace(pathOrUrl))
                return string.Empty;

            if (TryGetRemoteInfo(pathOrUrl, out string remoteKey, out _))
                return remoteKey;

            return BuildLocalImageKey(pathOrUrl);
        }

        public void AcquireImageUsage(string imageKey)
        {
            if (string.IsNullOrWhiteSpace(imageKey))
                return;

            _imageUsageCounters.TryGetValue(imageKey, out int count);
            _imageUsageCounters[imageKey] = count + 1;
        }

        public void ReleaseImageUsage(string imageKey)
        {
            if (string.IsNullOrWhiteSpace(imageKey))
                return;

            _imageUsageCounters.TryGetValue(imageKey, out int count);
            _imageUsageCounters[imageKey] = count - 1;
        }

        public void EnsureRemoteLoading(string url, bool prioritize = false)
        {
            if (!TryGetRemoteInfo(url, out string imageKey, out string remoteUrl))
                return;

            if (_memoryCache.ContainsKey(imageKey) || _inFlightKeys.Contains(imageKey))
                return;

            if (TryGetRemoteCached(remoteUrl, out Sprite cached))
            {
                RemoteSpriteReady?.Invoke(imageKey, cached);
                return;
            }

            if (prioritize)
            {
                // Drop from prefetch queue if present, then start regardless of MAX_LOADING_COUNT.
                _queuedKeys.Remove(imageKey);
                StartDownload(imageKey, remoteUrl);
                return;
            }

            if (_queuedKeys.Contains(imageKey))
                return;

            _queuedKeys.Add(imageKey);
            _downloadQueue.Enqueue(new RemoteLoadRequest(imageKey, remoteUrl));
            TryStartQueuedDownloads();
        }

        public void ClearMemoryCache()
        {
            int beforeCount = _memoryCache.Count;
            long beforeBytes = EstimateRuntimeTextureMemoryBytes();

            UnityEngine.Debug.Log(
                $"[{nameof(CachedPathImageService)}] ClearMemoryCache start: " +
                $"memoryCache={beforeCount}, approxTextureMemory={FormatBytes(beforeBytes)}, " +
                $"queued={_downloadQueue.Count}, inFlight={_inFlightKeys.Count}.");

            var keysToRemove = new List<string>();

            int usageCount = 0;
            foreach (KeyValuePair<string, Sprite> pair in _memoryCache)
            {
                _imageUsageCounters.TryGetValue(pair.Key, out usageCount);
                if (usageCount > 0)
                    continue;

                if (pair.Value == null)
                {
                    keysToRemove.Add(pair.Key);
                    continue;
                }

                Texture2D texture = pair.Value.texture;
                UnityEngine.Object.Destroy(pair.Value);
                if (texture != null)
                    UnityEngine.Object.Destroy(texture);

                keysToRemove.Add(pair.Key);
            }

            for (int i = 0; i < keysToRemove.Count; i++)
                _memoryCache.Remove(keysToRemove[i]);

            // Drop queued prefetch requests that have not started yet.
            // In-flight downloads are intentionally kept running.
            _queuedKeys.Clear();
            _downloadQueue.Clear();

            int afterCount = _memoryCache.Count;
            long afterBytes = EstimateRuntimeTextureMemoryBytes();

            UnityEngine.Debug.Log(
                $"[{nameof(CachedPathImageService)}] ClearMemoryCache end: " +
                $"memoryCache={afterCount}, approxTextureMemory={FormatBytes(afterBytes)}, " +
                $"queued={_downloadQueue.Count}, inFlight={_inFlightKeys.Count}.");
        }

        private void TryStartQueuedDownloads()
        {
            while (_downloadQueue.Count > 0 && _inFlightKeys.Count < MAX_LOADING_COUNT)
            {
                RemoteLoadRequest request = _downloadQueue.Dequeue();

                // Stale entry: already promoted via prioritize, or otherwise removed from the set.
                if (!_queuedKeys.Remove(request.ImageKey))
                    continue;

                if (_memoryCache.ContainsKey(request.ImageKey) || _inFlightKeys.Contains(request.ImageKey))
                    continue;

                if (TryGetRemoteCached(request.RemoteUrl, out Sprite cached))
                {
                    RemoteSpriteReady?.Invoke(request.ImageKey, cached);
                    continue;
                }

                StartDownload(request.ImageKey, request.RemoteUrl);
            }
        }

        private void StartDownload(string imageKey, string remoteUrl)
        {
            if (_inFlightKeys.Contains(imageKey))
                return;

            _inFlightKeys.Add(imageKey);
            _coroutineHolder.StartCoroutine(DownloadCoroutine(imageKey, remoteUrl, GetCachePath(remoteUrl)));
        }

        private IEnumerator DownloadCoroutine(string imageKey, string remoteUrl, string cachePath)
        {
            using UnityWebRequest request = UnityWebRequest.Get(remoteUrl);
            yield return request.SendWebRequest();

            try
            {
                if (request.result != UnityWebRequest.Result.Success)
                {
                    UnityEngine.Debug.LogWarning(
                        $"[{nameof(CachedPathImageService)}] Failed to download '{remoteUrl}': {request.error}");
                    RemoteSpriteReady?.Invoke(imageKey, null);
                    yield break;
                }

                byte[] data = request.downloadHandler.data;
                if (data == null || data.Length == 0)
                {
                    RemoteSpriteReady?.Invoke(imageKey, null);
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
                        $"[{nameof(CachedPathImageService)}] Failed to write cache for '{remoteUrl}': {exception.Message}");
                }

                if (!TryCreateSpriteFromBytes(data, remoteUrl, out Sprite sprite))
                {
                    RemoteSpriteReady?.Invoke(imageKey, null);
                    yield break;
                }

                _memoryCache[imageKey] = sprite;
                RemoteSpriteReady?.Invoke(imageKey, sprite);
            }
            finally
            {
                _inFlightKeys.Remove(imageKey);
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

        private static bool TryGetRemoteInfo(string pathOrUrl, out string imageKey, out string remoteUrl)
        {
            imageKey = string.Empty;
            remoteUrl = string.Empty;

            if (string.IsNullOrWhiteSpace(pathOrUrl))
                return false;

            if (!Uri.TryCreate(pathOrUrl, UriKind.Absolute, out Uri uri))
                return false;

            if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
                return false;

            remoteUrl = uri.AbsoluteUri;
            imageKey = BuildRemoteImageKey(remoteUrl);
            return true;
        }

        private static string BuildRemoteImageKey(string remoteUrl)
        {
            return $"url:{ComputeStableFileName(remoteUrl)}";
        }

        private static string BuildLocalImageKey(string localPath)
        {
            string normalizedPath = localPath.Trim().Replace('\\', '/');
            return $"res:{normalizedPath}";
        }

        private readonly struct RemoteLoadRequest
        {
            public readonly string ImageKey;
            public readonly string RemoteUrl;

            public RemoteLoadRequest(string imageKey, string remoteUrl)
            {
                ImageKey = imageKey;
                RemoteUrl = remoteUrl;
            }
        }

        private long EstimateRuntimeTextureMemoryBytes()
        {
            long totalBytes = 0;
            var visitedTextures = new HashSet<Texture2D>();

            foreach (KeyValuePair<string, Sprite> pair in _memoryCache)
            {
                Sprite sprite = pair.Value;
                if (sprite == null)
                    continue;

                Texture2D texture = sprite.texture;
                if (texture == null || !visitedTextures.Add(texture))
                    continue;

                totalBytes += Profiler.GetRuntimeMemorySizeLong(texture);
            }

            return totalBytes;
        }

        private static string FormatBytes(long bytes)
        {
            const float bytesPerMb = 1024f * 1024f;
            float mb = bytes / bytesPerMb;
            return $"{bytes} B ({mb:0.00} MB)";
        }
    }
}
