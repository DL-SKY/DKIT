using Modules.RPG.Scripts.Adventure.Data;
using Modules.Utils.Scripts.Components;
using System.Collections.Generic;
using Zenject;

namespace Modules.Windows.Scripts.Implementation.Adventure.Main.Scroll.Items.Image
{
    /// <summary>
    /// ViewModel for <see cref="SceneContentType.Slideshow"/> — cycles <see cref="SceneContentData.Values"/> on a timer
    /// via <see cref="Updater"/>.
    /// </summary>
    public sealed class AdventureSlideshowContentViewModel : AdventureImageContentViewModelBase
    {
        [Inject] private readonly Updater _updater;

        private IReadOnlyList<string> _paths;
        private float _intervalSeconds = 3f;
        private int _index;
        private float _elapsed;

        public override void Init(SceneContentData data)
        {
            _paths = data?.Values ?? (IReadOnlyList<string>)System.Array.Empty<string>();
            _index = 0;
            _elapsed = 0f;

            InitImage(data, GetInitialPath(data));
            Subscribe();
        }

        /// <summary>
        /// Updates slide interval (e.g. from prefab SerializeField on the View during Init).
        /// </summary>
        public void SetIntervalSeconds(float intervalSeconds)
        {
            _intervalSeconds = intervalSeconds > 0f ? intervalSeconds : 3f;
        }

        public override void Dispose()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            _updater.OnUpdate += OnUpdateHandler;
        }

        private void Unsubscribe()
        {
            _updater.OnUpdate -= OnUpdateHandler;
        }

        private void OnUpdateHandler(float deltaTime)
        {
            if (_paths == null || _paths.Count <= 1)
                return;

            _elapsed += deltaTime;
            if (_elapsed < _intervalSeconds)
                return;

            _elapsed = 0f;
            _index = (_index + 1) % _paths.Count;
            SetPath(_paths[_index], notify: true);
        }

        private static string GetInitialPath(SceneContentData data)
        {
            if (data?.Values == null || data.Values.Count == 0)
            {
                UnityEngine.Debug.LogWarning(
                    $"[{nameof(AdventureSlideshowContentViewModel)}] Values is empty; no image path selected.");
                return string.Empty;
            }

            return data.Values[0];
        }
    }
}
