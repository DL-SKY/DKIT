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
        private const float INTERVAL_SECONDS = 1f;

        [Inject] private readonly Updater _updater;

        private IReadOnlyList<string> _paths;
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

        protected override void DisposeImplementation()
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
            if (_elapsed < INTERVAL_SECONDS)
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
