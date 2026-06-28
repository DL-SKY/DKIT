using Modules.Definitions.Scripts.Implementation.Adventures;
using Modules.Definitions.Scripts.Implementation.Adventures.Defs.Adventures;
using Modules.RPG.Scripts.Adventure.Choice;
using Modules.RPG.Scripts.Adventure.Data;
using Modules.State.Scripts.Actions.Models;
using Modules.State.Scripts.Implementation.Adventure;
using Modules.State.Scripts.Implementation.Adventure.Actions;
using Modules.State.Scripts.Implementation.Adventure.Logic;
using System;
using System.Collections.Generic;
using Zenject;

namespace Modules.RPG.Scripts.Adventure
{
    public class RuntimeSceneData : IDisposable
    {
        [Inject] private readonly DefinitionsManager _definitionsManager;
        [Inject] private readonly AdventureStateManager _stateManager;
        [Inject] private readonly AdventureStateLogic _stateLogic;

        private readonly System.Random _random = new System.Random();

        private string _adventureId;
        private string _sceneId;
        private AdventureDef _adventure;
        private bool _isSyncingState;

        public event Action<string> ChangedAdventure;
        public event Action<string> ChangedScene;
        public event Action ChangedContent;
        public event Action ChangedChoices;

        public void Init()
        {
            if (!TryResolveCurrentScene(out var resolvedAdventureId, out var resolvedSceneId, out var shouldSyncState))
                return;

            if (shouldSyncState)
                WriteCurrentProgressToState(resolvedAdventureId, resolvedSceneId);

            ApplyResolvedIds(resolvedAdventureId, resolvedSceneId);

            Subscribe();
            ChangedScene?.Invoke(_sceneId);
        }

        public void Dispose()
        {
            Unsubscribe();
        }

        public List<SceneContentData> GetCurrentContent()
        {
            if (!TryGetCurrentSceneData(out var sceneData))
                return new List<SceneContentData>();

            return sceneData.Content ?? new List<SceneContentData>();
        }

        public List<ChoiceData> GetCurrentChoices()
        {
            if (!TryGetCurrentSceneData(out var sceneData))
                return new List<ChoiceData>();

            return sceneData.Choices ?? new List<ChoiceData>();
        }

        private void Subscribe()
        {
            _stateLogic.StateChanged += OnStateChangedHandler;
        }

        private void Unsubscribe()
        {
            _stateLogic.StateChanged -= OnStateChangedHandler;
        }

        private void OnStateChangedHandler(StateChangeSource source)
        {
            if (_isSyncingState)
                return;

            switch (source)
            {
                case StateChangeSource.SetCurrentAdventureId:
                case StateChangeSource.SetCurrentAdventureSceneId:
                    SyncFromStateAndNotify();
                    break;

                case StateChangeSource.SetAdventureProgressBool:
                case StateChangeSource.ModifyAdventureProgressInt:
                    ChangedContent?.Invoke();
                    ChangedChoices?.Invoke();
                    break;
            }
        }

        private void SyncFromStateAndNotify()
        {
            if (!TryResolveCurrentScene(out var resolvedAdventureId, out var resolvedSceneId, out var shouldSyncState))
                return;

            if (shouldSyncState)
                WriteCurrentProgressToState(resolvedAdventureId, resolvedSceneId);

            var oldAdventureId = _adventureId;
            var oldSceneId = _sceneId;

            ApplyResolvedIds(resolvedAdventureId, resolvedSceneId);

            if (!string.Equals(oldAdventureId, _adventureId, StringComparison.Ordinal))
                ChangedAdventure?.Invoke(_adventureId);

            if (!string.Equals(oldSceneId, _sceneId, StringComparison.Ordinal))
            {
                ChangedScene?.Invoke(_sceneId);
                ChangedContent?.Invoke();
                ChangedChoices?.Invoke();
            }
        }

        private void ApplyResolvedIds(string resolvedAdventureId, string resolvedSceneId)
        {
            _adventureId = resolvedAdventureId;
            _sceneId = resolvedSceneId;
            _adventure = _definitionsManager.Adventures[_adventureId];
        }

        private bool TryResolveCurrentScene(out string resolvedAdventureId, out string resolvedSceneId, out bool shouldSyncState)
        {
            resolvedAdventureId = null;
            resolvedSceneId = null;
            shouldSyncState = false;

            var adventuresState = _stateManager.State?.Adventures;
            if (adventuresState == null)
            {
                UnityEngine.Debug.LogError("[RuntimeSceneData] Adventures state is null.");
                return false;
            }

            if (TryResolveAdventureFromState(
                adventuresState.CurrentAdventureId,
                adventuresState.CurrentAdventureSceneId,
                out resolvedAdventureId,
                out resolvedSceneId,
                out shouldSyncState))
            {
                return true;
            }

            return TryResolveAdventureFromDefinitions(out resolvedAdventureId, out resolvedSceneId, out shouldSyncState);
        }

        private bool TryResolveAdventureFromState(
            string stateAdventureId,
            string stateSceneId,
            out string resolvedAdventureId,
            out string resolvedSceneId,
            out bool shouldSyncState)
        {
            resolvedAdventureId = null;
            resolvedSceneId = null;
            shouldSyncState = false;

            if (!TryGetAdventure(stateAdventureId, out var adventureDef))
                return false;

            resolvedAdventureId = adventureDef.Id;

            if (IsValidSceneId(adventureDef, stateSceneId))
            {
                resolvedSceneId = stateSceneId;
                return true;
            }

            if (!TrySelectStartSceneId(adventureDef, out resolvedSceneId))
            {
                UnityEngine.Debug.LogError(
                    $"[RuntimeSceneData] Failed to pick start scene for adventure '{adventureDef.Id}'.");
                return false;
            }

            shouldSyncState = true;
            return true;
        }

        private bool TryResolveAdventureFromDefinitions(
            out string resolvedAdventureId,
            out string resolvedSceneId,
            out bool shouldSyncState)
        {
            resolvedAdventureId = null;
            resolvedSceneId = null;
            shouldSyncState = false;

            var startAdventureId = _definitionsManager.RuleSettings?.StartAdventure;
            if (!TryGetAdventure(startAdventureId, out var adventureDef))
            {
                UnityEngine.Debug.LogError(
                    $"[RuntimeSceneData] Invalid start adventure '{startAdventureId}' in RuleSettings.");
                return false;
            }

            if (!TrySelectStartSceneId(adventureDef, out var startSceneId))
            {
                UnityEngine.Debug.LogError(
                    $"[RuntimeSceneData] Adventure '{adventureDef.Id}' has no valid start scenes.");
                return false;
            }

            resolvedAdventureId = adventureDef.Id;
            resolvedSceneId = startSceneId;
            shouldSyncState = true;
            return true;
        }

        private bool TrySelectStartSceneId(AdventureDef adventureDef, out string sceneId)
        {
            sceneId = null;

            if (adventureDef == null || adventureDef.Scenes == null || adventureDef.StartScenes == null)
                return false;

            var validStartScenes = new List<string>();

            for (int i = 0; i < adventureDef.StartScenes.Count; i++)
            {
                var candidateSceneId = adventureDef.StartScenes[i];
                if (IsValidSceneId(adventureDef, candidateSceneId))
                    validStartScenes.Add(candidateSceneId);
            }

            if (validStartScenes.Count == 0)
                return false;

            sceneId = validStartScenes[_random.Next(validStartScenes.Count)];
            return true;
        }

        private bool TryGetAdventure(string adventureId, out AdventureDef adventureDef)
        {
            adventureDef = null;

            if (string.IsNullOrWhiteSpace(adventureId))
                return false;

            var adventures = _definitionsManager.Adventures;
            if (adventures == null)
                return false;

            return adventures.TryGetValue(adventureId, out adventureDef) && adventureDef != null;
        }

        private static bool IsValidSceneId(AdventureDef adventureDef, string sceneId)
        {
            if (adventureDef?.Scenes == null || string.IsNullOrWhiteSpace(sceneId))
                return false;

            return adventureDef.Scenes.ContainsKey(sceneId);
        }

        private bool TryGetCurrentSceneData(out SceneData sceneData)
        {
            sceneData = null;

            if (_adventure?.Scenes == null || string.IsNullOrWhiteSpace(_sceneId))
                return false;

            return _adventure.Scenes.TryGetValue(_sceneId, out sceneData) && sceneData != null;
        }

        private void WriteCurrentProgressToState(string adventureId, string sceneId)
        {
            _isSyncingState = true;

            try
            {
                var setAdventureResult = _stateLogic.ProcessAction(new SetCurrentAdventureIdStateAction(adventureId));
                if (!setAdventureResult.IsValid)
                {
                    UnityEngine.Debug.LogWarning(
                        $"[RuntimeSceneData] Failed to set current adventure id: {setAdventureResult.ErrorMessage}");
                }

                var setSceneResult = _stateLogic.ProcessAction(new SetCurrentAdventureSceneIdStateAction(sceneId));
                if (!setSceneResult.IsValid)
                {
                    UnityEngine.Debug.LogWarning(
                        $"[RuntimeSceneData] Failed to set current scene id: {setSceneResult.ErrorMessage}");
                }
            }
            finally
            {
                _isSyncingState = false;
            }
        }
    }
}
