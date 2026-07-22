using Modules.State.Scripts.Implementation.Adventure.Actions;
using Modules.State.Scripts.Implementation.Adventure.Logic;
using System;
using System.Collections.Generic;
using Zenject;
using static Modules.Definitions.Scripts.Implementation.Adventures.Constants.Glossary;

namespace Modules.RPG.Scripts.Adventure.Choice.Executors
{
    public class GoToRandomSceneChoiceActionExecutor : IChoiceActionExecutor
    {
        [Inject] private readonly AdventureStateLogic _stateLogic;

        private readonly System.Random _random = new System.Random();
        private readonly string _sceneIdsRaw;

        public GoToRandomSceneChoiceActionExecutor(string sceneIdsRaw)
        {
            _sceneIdsRaw = sceneIdsRaw;
        }

        public void Execute()
        {
            List<string> sceneIds = ParseSceneIds(_sceneIdsRaw);
            if (sceneIds.Count == 0)
            {
                UnityEngine.Debug.LogWarning(
                    "[GoToRandomSceneChoiceActionExecutor] No scene ids after parsing. " +
                    $"Raw='{_sceneIdsRaw}'.");
                return;
            }

            string sceneId = sceneIds[_random.Next(sceneIds.Count)];
            _stateLogic.ProcessAction(new SetCurrentAdventureSceneIdStateAction(sceneId));
        }

        private static List<string> ParseSceneIds(string raw)
        {
            var result = new List<string>();
            if (string.IsNullOrWhiteSpace(raw))
                return result;

            string[] parts = raw.Split(
                new[] { ChoiceActions.SCENE_IDS_SEPARATOR },
                StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < parts.Length; i++)
            {
                string sceneId = parts[i]?.Trim();
                if (!string.IsNullOrWhiteSpace(sceneId))
                    result.Add(sceneId);
            }

            return result;
        }
    }
}
