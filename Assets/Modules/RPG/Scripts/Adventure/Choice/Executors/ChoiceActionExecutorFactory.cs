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

                ChoiceActionType.ModifyVariable => _container.Instantiate<ModifyVariableChoiceActionExecutor>(
                    new object[]
                    {
                        GetRequiredString(actionData.Params.Strings, "key", actionData.Type),
                        GetRequiredInt(actionData.Params.Ints, "delta", actionData.Type),
                    }),

                ChoiceActionType.SetFlag => _container.Instantiate<SetFlagChoiceActionExecutor>(
                    new object[]
                    {
                        GetRequiredString(actionData.Params.Strings, "key", actionData.Type),
                        GetRequiredBool(actionData.Params.Bools, "value", actionData.Type),
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

        private static int GetRequiredInt(Dictionary<string, int> dictionary, string key, ChoiceActionType type)
        {
            if (dictionary == null)
                throw new ArgumentException($"Params.Ints is null for action type '{type}'.");

            if (!dictionary.TryGetValue(key, out int value))
                throw new ArgumentException($"Required int param '{key}' is missing for action type '{type}'.");

            return value;
        }

        private static bool GetRequiredBool(Dictionary<string, bool> dictionary, string key, ChoiceActionType type)
        {
            if (dictionary == null)
                throw new ArgumentException($"Params.Bools is null for action type '{type}'.");

            if (!dictionary.TryGetValue(key, out bool value))
                throw new ArgumentException($"Required bool param '{key}' is missing for action type '{type}'.");

            return value;
        }

        private static float GetRequiredFloat(Dictionary<string, float> dictionary, string key, ChoiceActionType type)
        {
            if (dictionary == null)
                throw new ArgumentException($"Params.Floats is null for action type '{type}'.");

            if (!dictionary.TryGetValue(key, out float value))
                throw new ArgumentException($"Required float param '{key}' is missing for action type '{type}'.");

            return value;
        }
    }
}
