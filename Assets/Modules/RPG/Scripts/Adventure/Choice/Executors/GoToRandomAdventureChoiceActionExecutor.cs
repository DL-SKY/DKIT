using Modules.Definitions.Scripts.Implementation.Adventures;
using Modules.Definitions.Scripts.Implementation.Adventures.Defs.Adventures;
using Modules.State.Scripts.Implementation.Adventure;
using Modules.State.Scripts.Implementation.Adventure.Actions;
using Modules.State.Scripts.Implementation.Adventure.Logic;
using Modules.State.Scripts.Implementation.Adventure.StateDatas;
using System;
using System.Collections.Generic;
using Zenject;
using static Modules.Definitions.Scripts.Implementation.Adventures.Constants.Glossary;

namespace Modules.RPG.Scripts.Adventure.Choice.Executors
{
    public class GoToRandomAdventureChoiceActionExecutor : IChoiceActionExecutor
    {
        [Inject] private readonly AdventureStateLogic _stateLogic;
        [Inject] private readonly AdventureStateManager _stateManager;
        [Inject] private readonly DefinitionsManager _definitionsManager;

        private readonly System.Random _random = new System.Random();

        public void Execute()
        {
            List<string> candidates = CollectCandidateAdventureIds();
            if (candidates.Count == 0)
            {
                UnityEngine.Debug.LogWarning(
                    "[GoToRandomAdventureChoiceActionExecutor] No candidate adventures found.");
                return;
            }

            string adventureId = candidates[_random.Next(candidates.Count)];
            _stateLogic.ProcessAction(new SetCurrentAdventureIdStateAction(adventureId));
        }

        private List<string> CollectCandidateAdventureIds()
        {
            var result = new List<string>();
            Dictionary<string, AdventureDef> adventures = _definitionsManager?.Adventures;
            if (adventures == null || adventures.Count == 0)
                return result;

            AdventuresStateData adventuresState = _stateManager?.State?.Adventures;
            string currentAdventureId = adventuresState?.CurrentAdventureId;

            foreach (KeyValuePair<string, AdventureDef> pair in adventures)
            {
                string adventureId = pair.Key;
                AdventureDef adventure = pair.Value;
                if (string.IsNullOrWhiteSpace(adventureId) || adventure == null)
                    continue;

                if (adventure.Disabled)
                    continue;

                if (string.Equals(adventureId, currentAdventureId, StringComparison.Ordinal))
                    continue;

                if (HasTag(adventure, Adventures.HUB))
                    continue;

                // TODO: exclude completed adventures via StateData.Adventures.Adventures[id].Parameters
                // (completion flag key is not finalized yet).
                // Example:
                // if (IsAdventureCompleted(adventuresState, adventureId))
                //     continue;

                // TODO: filter by suitable party/adventure level once level rules are defined.
                // Example:
                // if (!IsLevelSuitable(adventure, adventuresState))
                //     continue;

                result.Add(adventureId);
            }

            return result;
        }

        private static bool HasTag(AdventureDef adventure, string tag)
        {
            if (adventure?.Tags == null || string.IsNullOrWhiteSpace(tag))
                return false;

            for (int i = 0; i < adventure.Tags.Count; i++)
            {
                if (string.Equals(adventure.Tags[i], tag, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }
    }
}
