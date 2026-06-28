using Modules.RPG.Scripts.Adventure.Choice;
using Modules.RPG.Scripts.Adventure.Data;
using System;
using System.Collections.Generic;
using Zenject;

namespace Modules.RPG.Scripts.Adventure
{
    public class AdventuresManager : IDisposable
    {
        [Inject] private readonly DiContainer _container;

        private RuntimeSceneData _runtimeSceneData;

        public event Action<string> ChangedAdventure;
        public event Action<string> ChangedScene;
        public event Action ChangedContent;
        public event Action ChangedChoices;

        public void Init()
        {
            UnityEngine.Debug.LogError($"AdventuresManager.Init()");

            _runtimeSceneData = _container.Instantiate<RuntimeSceneData>();
            Subscribe();
            _runtimeSceneData.Init();
        }

        public void Dispose()
        {
            UnityEngine.Debug.LogError($"AdventuresManager.Dispose()");

            Unsubscribe();
            _runtimeSceneData?.Dispose();
            _runtimeSceneData = null;
        }

        public List<SceneContentData> GetCurrentContent()
        {
            if (_runtimeSceneData == null)
                return new List<SceneContentData>();

            return _runtimeSceneData.GetCurrentContent();
        }

        public List<ChoiceData> GetCurrentChoices()
        {
            if (_runtimeSceneData == null)
                return new List<ChoiceData>();

            return _runtimeSceneData.GetCurrentChoices();
        }

        private void Subscribe()
        {
            if (_runtimeSceneData == null)
                return;

            _runtimeSceneData.ChangedAdventure += OnChangedAdventureHandler;
            _runtimeSceneData.ChangedScene += OnChangedSceneHandler;
            _runtimeSceneData.ChangedContent += OnChangedContentHandler;
            _runtimeSceneData.ChangedChoices += OnChangedChoicesHandler;
        }

        private void Unsubscribe()
        {
            if (_runtimeSceneData == null)
                return;

            _runtimeSceneData.ChangedAdventure -= OnChangedAdventureHandler;
            _runtimeSceneData.ChangedScene -= OnChangedSceneHandler;
            _runtimeSceneData.ChangedContent -= OnChangedContentHandler;
            _runtimeSceneData.ChangedChoices -= OnChangedChoicesHandler;
        }

        private void OnChangedAdventureHandler(string adventureId)
        {
            ChangedAdventure?.Invoke(adventureId);
        }

        private void OnChangedSceneHandler(string sceneId)
        {
            ChangedScene?.Invoke(sceneId);
        }

        private void OnChangedContentHandler()
        {
            ChangedContent?.Invoke();
        }

        private void OnChangedChoicesHandler()
        {
            ChangedChoices?.Invoke();
        }
    }
}
