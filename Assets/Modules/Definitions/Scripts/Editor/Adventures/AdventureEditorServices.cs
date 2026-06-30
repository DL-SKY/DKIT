using Modules.RPG.Scripts.Adventure.Choice.Actions;
using Modules.RPG.Scripts.Adventure.Data;
using System;
using System.Collections.Generic;

namespace Modules.Definitions.Scripts.Editor.Adventures
{
    public sealed class AdventureGraphData
    {
        public readonly List<string> Nodes = new List<string>();
        public readonly List<AdventureGraphEdge> Edges = new List<AdventureGraphEdge>();
    }

    public sealed class AdventureGraphEdge
    {
        public string FromSceneId;
        public string ToSceneId;
        public string ChoiceId;
        public string ChoiceText;
    }

    public sealed class AdventureGraphBuilder
    {
        public AdventureGraphData Build(AdventureData adventureData)
        {
            AdventureGraphData graphData = new AdventureGraphData();
            if (adventureData?.Scenes == null)
                return graphData;

            foreach (KeyValuePair<string, SceneData> pair in adventureData.Scenes)
            {
                string sceneId = pair.Key;
                if (string.IsNullOrWhiteSpace(sceneId))
                    continue;

                graphData.Nodes.Add(sceneId);
                SceneData sceneData = pair.Value;
                if (sceneData?.Choices == null)
                    continue;

                for (int choiceIndex = 0; choiceIndex < sceneData.Choices.Count; choiceIndex++)
                {
                    var choice = sceneData.Choices[choiceIndex];
                    if (choice?.Actions == null)
                        continue;

                    for (int actionIndex = 0; actionIndex < choice.Actions.Count; actionIndex++)
                    {
                        var action = choice.Actions[actionIndex];
                        if (action == null || action.Type != ChoiceActionType.GoToScene)
                            continue;

                        string targetSceneId = GetSceneId(action);
                        if (string.IsNullOrWhiteSpace(targetSceneId))
                            continue;

                        graphData.Edges.Add(new AdventureGraphEdge
                        {
                            FromSceneId = sceneId,
                            ToSceneId = targetSceneId,
                            ChoiceId = choice.Id,
                            ChoiceText = choice.Text,
                        });
                    }
                }
            }

            graphData.Nodes.Sort(StringComparer.Ordinal);
            return graphData;
        }

        public static string GetSceneId(Modules.RPG.Scripts.Adventure.Choice.ChoiceActionData actionData)
        {
            if (actionData?.Params?.Strings == null)
                return string.Empty;

            return actionData.Params.Strings.TryGetValue("sceneId", out string value) ? value : string.Empty;
        }
    }

    public interface IAdventureLocalizationKeyCollector
    {
        List<string> Collect(AdventureData adventureData);
    }

    public sealed class DefaultAdventureLocalizationKeyCollector : IAdventureLocalizationKeyCollector
    {
        private const string LOC_PREFIX = "loc:";

        public List<string> Collect(AdventureData adventureData)
        {
            HashSet<string> keys = new HashSet<string>(StringComparer.Ordinal);
            if (adventureData == null)
                return new List<string>();

            CollectFromText(adventureData.Title, keys);
            CollectFromText(adventureData.Description, keys);

            if (adventureData.Scenes == null)
                return ToOrderedList(keys);

            foreach (KeyValuePair<string, SceneData> pair in adventureData.Scenes)
            {
                SceneData sceneData = pair.Value;
                if (sceneData == null)
                    continue;

                if (sceneData.Content != null)
                {
                    for (int contentIndex = 0; contentIndex < sceneData.Content.Count; contentIndex++)
                        CollectFromText(sceneData.Content[contentIndex]?.Value, keys);
                }

                if (sceneData.Choices == null)
                    continue;

                for (int choiceIndex = 0; choiceIndex < sceneData.Choices.Count; choiceIndex++)
                {
                    var choice = sceneData.Choices[choiceIndex];
                    if (choice == null)
                        continue;

                    CollectFromText(choice.Text, keys);
                    CollectFromText(choice.Description, keys);

                    if (choice.Actions == null)
                        continue;

                    for (int actionIndex = 0; actionIndex < choice.Actions.Count; actionIndex++)
                    {
                        var action = choice.Actions[actionIndex];
                        if (action?.Params?.Strings == null)
                            continue;

                        foreach (var stringParam in action.Params.Strings)
                            CollectFromText(stringParam.Value, keys);
                    }
                }
            }

            return ToOrderedList(keys);
        }

        private static void CollectFromText(string value, HashSet<string> keys)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            string trimmed = value.Trim();
            if (!trimmed.StartsWith(LOC_PREFIX, StringComparison.OrdinalIgnoreCase))
                return;

            string key = trimmed.Substring(LOC_PREFIX.Length).Trim();
            if (!string.IsNullOrWhiteSpace(key))
                keys.Add(key);
        }

        private static List<string> ToOrderedList(HashSet<string> keys)
        {
            List<string> result = new List<string>(keys);
            result.Sort(StringComparer.Ordinal);
            return result;
        }
    }

    public sealed class AdventureValidationService
    {
        public List<string> Validate(AdventureData adventureData)
        {
            List<string> errors = new List<string>();
            if (adventureData == null)
            {
                errors.Add("Adventure is null.");
                return errors;
            }

            if (adventureData.Scenes == null || adventureData.Scenes.Count == 0)
                errors.Add("Adventure must contain at least one scene.");

            if (adventureData.StartScenes == null || adventureData.StartScenes.Count == 0)
                errors.Add("Adventure must contain at least one start scene.");

            if (adventureData.StartScenes != null && adventureData.Scenes != null)
            {
                for (int i = 0; i < adventureData.StartScenes.Count; i++)
                {
                    string startSceneId = adventureData.StartScenes[i];
                    if (string.IsNullOrWhiteSpace(startSceneId))
                    {
                        errors.Add("Start scene id cannot be empty.");
                        continue;
                    }

                    if (!adventureData.Scenes.ContainsKey(startSceneId))
                        errors.Add($"Start scene '{startSceneId}' does not exist in Scenes.");
                }
            }

            if (adventureData.Scenes != null)
            {
                foreach (KeyValuePair<string, SceneData> pair in adventureData.Scenes)
                {
                    string sceneId = pair.Key;
                    SceneData sceneData = pair.Value;

                    if (string.IsNullOrWhiteSpace(sceneId))
                        errors.Add("Scene dictionary contains empty scene id.");

                    if (sceneData == null)
                    {
                        errors.Add($"Scene '{sceneId}' is null.");
                        continue;
                    }

                    if (!string.Equals(sceneData.Id, sceneId, StringComparison.Ordinal))
                        errors.Add($"Scene key '{sceneId}' does not match SceneData.Id '{sceneData.Id}'.");

                    if (sceneData.Choices == null)
                        continue;

                    HashSet<string> choiceIds = new HashSet<string>(StringComparer.Ordinal);
                    for (int choiceIndex = 0; choiceIndex < sceneData.Choices.Count; choiceIndex++)
                    {
                        var choice = sceneData.Choices[choiceIndex];
                        if (choice == null)
                        {
                            errors.Add($"Scene '{sceneId}' contains null choice.");
                            continue;
                        }

                        if (!string.IsNullOrWhiteSpace(choice.Id) && !choiceIds.Add(choice.Id))
                            errors.Add($"Scene '{sceneId}' contains duplicated choice id '{choice.Id}'.");

                        if (choice.Actions == null)
                            continue;

                        for (int actionIndex = 0; actionIndex < choice.Actions.Count; actionIndex++)
                        {
                            var action = choice.Actions[actionIndex];
                            if (action == null)
                            {
                                errors.Add($"Scene '{sceneId}' contains null action in choice '{choice.Id}'.");
                                continue;
                            }

                            if (action.Type != ChoiceActionType.GoToScene)
                                continue;

                            string targetSceneId = AdventureGraphBuilder.GetSceneId(action);
                            if (string.IsNullOrWhiteSpace(targetSceneId))
                            {
                                errors.Add($"Choice '{choice.Id}' in scene '{sceneId}' has GoToScene action with empty sceneId.");
                                continue;
                            }

                            if (adventureData.Scenes != null && !adventureData.Scenes.ContainsKey(targetSceneId))
                                errors.Add($"Choice '{choice.Id}' in scene '{sceneId}' points to missing scene '{targetSceneId}'.");
                        }
                    }
                }
            }

            return errors;
        }
    }
}
