using Modules.RPG.Scripts.Adventure.Choice.Actions;
using System;
using System.Collections.Generic;
using Zenject;
using static Modules.Definitions.Scripts.Implementation.Adventures.Constants.Glossary;

namespace Modules.RPG.Scripts.Adventure.Choice.Executors
{
    public class ChoiceActionExecutorFactory : IChoiceActionExecutorFactory
    {
        [Inject] private readonly DiContainer _container;

        public IChoiceActionExecutor Create(ChoiceActionData actionData)
        {
            if (actionData == null)
                throw new ArgumentNullException(nameof(actionData));

            return actionData.Type switch
            {
                ChoiceActionType.GoToAdventure => _container.Instantiate<GoToAdventureChoiceActionExecutor>(
                    new object[]
                    {
                        GetRequiredString(actionData.Params?.Strings, ChoiceActions.ADVENTURE_ID, actionData.Type),
                    }),
                ChoiceActionType.GoToScene => _container.Instantiate<GoToSceneChoiceActionExecutor>(
                    new object[]
                    {
                        GetRequiredString(actionData.Params?.Strings, ChoiceActions.SCENE_ID, actionData.Type),
                    }),
                ChoiceActionType.GoToRandomAdventure => _container.Instantiate<GoToRandomAdventureChoiceActionExecutor>(),
                ChoiceActionType.GoToRandomScene => _container.Instantiate<GoToRandomSceneChoiceActionExecutor>(
                    new object[]
                    {
                        GetRequiredString(actionData.Params?.Strings, ChoiceActions.SCENE_ID, actionData.Type),
                    }),

                ChoiceActionType.OpenWindow => _container.Instantiate<OpenWindowChoiceActionExecutor>(
                new object[]
                {
                        GetRequiredString(actionData.Params?.Strings, ChoiceActions.WINDOW_ID, actionData.Type),
                }),

                ChoiceActionType.SetWorldParams => _container.Instantiate<SetWorldParamsChoiceActionExecutor>(
                    new object[]
                    {
                        EnsureParams(actionData),
                    }),
                ChoiceActionType.SetAdventureParams => _container.Instantiate<SetAdventureParamsChoiceActionExecutor>(
                    new object[]
                    {
                        EnsureParams(actionData),
                    }),
                ChoiceActionType.SetGlobalParams => _container.Instantiate<SetGlobalParamsChoiceActionExecutor>(
                    new object[]
                    {
                        EnsureParams(actionData),
                    }),

                _ => throw new NotImplementedException($"ChoiceActionType '{actionData.Type}' is not supported by factory yet."),
            };
        }

        private static ChoiceActionParamsData EnsureParams(ChoiceActionData actionData)
        {
            actionData.Params ??= new ChoiceActionParamsData();
            actionData.Params.Strings ??= new Dictionary<string, string>();
            actionData.Params.Ints ??= new Dictionary<string, int>();
            actionData.Params.Bools ??= new Dictionary<string, bool>();
            return actionData.Params;
        }

        private static string GetRequiredString(Dictionary<string, string> dictionary, string key, ChoiceActionType type)
        {
            if (dictionary == null)
                throw new ArgumentException($"Params.Strings is null for action type '{type}'.");

            if (!dictionary.TryGetValue(key, out string value) || string.IsNullOrWhiteSpace(value))
                throw new ArgumentException($"Required string param '{key}' is missing for action type '{type}'.");

            return value;
        }

    }
}
