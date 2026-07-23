using System;
using UnityEngine;

namespace Modules.Windows.Scripts.Services
{
    /// <summary>
    /// Shared image path/URL resolver with memory + disk cache for remote images.
    /// </summary>
    public interface IImageCache
    {
        /// <summary>
        /// Raised when a remote image finishes loading. <c>sprite</c> is null on failure.
        /// First argument is a normalized image key (not raw path/URL).
        /// </summary>
        event Action<string, Sprite> RemoteSpriteReady;

        bool IsRemoteUrl(string pathOrUrl);

        /// <summary>
        /// Loads a sprite from Resources (local project asset). Does not touch remote cache.
        /// </summary>
        bool TryLoadLocal(string path, out Sprite sprite);

        /// <summary>
        /// Tries memory cache, then disk cache for a remote URL.
        /// </summary>
        bool TryGetRemoteCached(string url, out Sprite sprite);

        /// <summary>
        /// Returns a normalized image key for path/URL or an empty string for invalid input.
        /// The key is stable and can be used to deduplicate image usage tracking.
        /// </summary>
        string GetImageKey(string pathOrUrl);

        /// <summary>
        /// Increments usage counter for the normalized image key.
        /// Empty/invalid keys are ignored.
        /// </summary>
        void AcquireImageUsage(string imageKey);

        /// <summary>
        /// Decrements usage counter for the normalized image key.
        /// Empty/invalid keys are ignored.
        /// </summary>
        void ReleaseImageUsage(string imageKey);

        /// <summary>
        /// Starts (or joins) a remote download.
        /// Prefetch uses <paramref name="prioritize"/> = false (queued, concurrency limit).
        /// Visible UI (<c>CachedPathImage</c>) uses <paramref name="prioritize"/> = true
        /// (starts immediately, bypassing the limit; promotes if already queued).
        /// Always persists successful bytes to disk.
        /// Fires <see cref="RemoteSpriteReady"/> when finished (including failure with null sprite).
        /// Does not cancel in-flight downloads for other URLs.
        /// </summary>
        void EnsureRemoteLoading(string url, bool prioritize = false);

        /// <summary>
        /// Destroys cached sprites/textures only for images with usage count less than 1,
        /// and clears queued prefetch URLs that have not started yet.
        /// In-flight downloads are not cancelled. Disk files are kept.
        /// </summary>
        void ClearMemoryCache();
    }
}
