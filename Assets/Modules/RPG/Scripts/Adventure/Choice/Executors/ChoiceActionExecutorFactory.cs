using Modules.RPG.Scripts.Adventure.Choice.Actions;
using System;
using System.Collections.Generic;
using Zenject;

namespace Modules.RPG.Scripts.Adventure.Choice.Executors
{
    public class ChoiceActionExecutorFactory : IChoiceActionExecutorFactory
    {
        [Inject] private readonly DiContainer _container;

        public IChoiceActionExecutor Create(ChoiceActionData actionData)
        {
            if (actionData == null)
                throw new ArgumentNullException(nameof(actionData));

            if (actionData.Params == null)
                throw new ArgumentException("ChoiceActionData.Params is null.", nameof(actionData));

            return actionData.Type switch
            {
                ChoiceActionType.GoToScene => _container.Instantiate<GoToSceneChoiceActionExecutor>(
                    new object[]
                    {
                        GetRequiredString(actionData.Params.Strings, "sceneId", actionData.Type),
                    }),

                _ => throw new NotImplementedException($"ChoiceActionType '{actionData.Type}' is not supported by factory yet."),
            };
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
