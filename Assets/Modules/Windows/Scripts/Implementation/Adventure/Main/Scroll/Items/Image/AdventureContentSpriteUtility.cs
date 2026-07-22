using System;
using UnityEngine;

namespace Modules.Windows.Scripts.Implementation.Adventure.Main.Scroll.Items.Image
{
    /// <summary>
    /// Helpers for resolving adventure content image paths (Resources) vs remote URLs.
    /// </summary>
    public static class AdventureContentSpriteUtility
    {
        public static bool IsRemoteUrl(string pathOrUrl)
        {
            if (string.IsNullOrWhiteSpace(pathOrUrl))
                return false;

            return Uri.TryCreate(pathOrUrl, UriKind.Absolute, out Uri uri)
                   && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }

        /// <summary>
        /// Loads a sprite from Resources. Does not handle remote URLs.
        /// </summary>
        public static bool TryLoadFromResources(string path, out Sprite sprite)
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
    }
}
