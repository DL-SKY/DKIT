using Modules.State.Scripts.Actions.Models;
using Modules.State.Scripts.Implementation.Adventure.StateDatas;
using System.Collections.Generic;

namespace Modules.State.Scripts.Implementation.Adventure.Actions
{
    internal static class AdventureProgressStateActionHelper
    {
        private const string WorldPrefix = "world.";
        private const string AdventurePrefix = "adventure.";

        public static StateActionValidationResult TryResolveBoolParameters(
            StateData state,
            string key,
            string adventureId,
            out Dictionary<string, bool> bools)
        {
            bools = null;

            var parametersResult = TryResolveParameters(state, key, adventureId, out AdventureStateParamsData parameters);
            if (!parametersResult.IsValid)
                return parametersResult;

            EnsureBools(parameters);
            bools = parameters.Bools;
            return StateActionValidationResult.Ok;
        }

        public static StateActionValidationResult TryResolveIntParameters(
            StateData state,
            string key,
            string adventureId,
            out Dictionary<string, int> ints)
        {
            ints = null;

            var parametersResult = TryResolveParameters(state, key, adventureId, out AdventureStateParamsData parameters);
            if (!parametersResult.IsValid)
                return parametersResult;

            EnsureInts(parameters);
            ints = parameters.Ints;
            return StateActionValidationResult.Ok;
        }

        private static StateActionValidationResult TryResolveParameters(
            StateData state,
            string key,
            string adventureId,
            out AdventureStateParamsData parameters)
        {
            parameters = null;

            if (state.Adventures == null)
                return StateActionValidationResult.Fail("Adventures state is null.", 30);

            if (string.IsNullOrWhiteSpace(key))
                return StateActionValidationResult.Fail("Progress key is null or empty.", 31);

            if (key.StartsWith(WorldPrefix))
            {
                EnsureWorld(state.Adventures);
                parameters = state.Adventures.World.Parameters;
                return StateActionValidationResult.Ok;
            }

            if (key.StartsWith(AdventurePrefix))
            {
                if (string.IsNullOrWhiteSpace(adventureId))
                    return StateActionValidationResult.Fail("Adventure id is required for adventure-scoped keys.", 32);

                EnsureAdventuresDictionary(state.Adventures);

                if (!state.Adventures.Adventures.TryGetValue(adventureId, out var adventureState) || adventureState == null)
                {
                    adventureState = new AdventureStateData
                    {
                        AdventureId = adventureId,
                        Parameters = CreateEmptyParameters(),
                    };
                    state.Adventures.Adventures[adventureId] = adventureState;
                }

                EnsureParameters(adventureState);
                parameters = adventureState.Parameters;
                return StateActionValidationResult.Ok;
            }

            return StateActionValidationResult.Fail(
                $"Unsupported progress key prefix '{key}'. Expected '{WorldPrefix}' or '{AdventurePrefix}'.",
                33);
        }

        private static void EnsureWorld(AdventuresStateData adventures)
        {
            if (adventures.World == null)
                adventures.World = new WorldStateData();

            EnsureParameters(adventures.World);
        }

        private static void EnsureAdventuresDictionary(AdventuresStateData adventures)
        {
            if (adventures.Adventures == null)
                adventures.Adventures = new Dictionary<string, AdventureStateData>();
        }

        private static void EnsureParameters(WorldStateData world)
        {
            if (world.Parameters == null)
                world.Parameters = CreateEmptyParameters();
        }

        private static void EnsureParameters(AdventureStateData adventure)
        {
            if (adventure.Parameters == null)
                adventure.Parameters = CreateEmptyParameters();
        }

        private static void EnsureBools(AdventureStateParamsData parameters)
        {
            if (parameters.Bools == null)
                parameters.Bools = new Dictionary<string, bool>();
        }

        private static void EnsureInts(AdventureStateParamsData parameters)
        {
            if (parameters.Ints == null)
                parameters.Ints = new Dictionary<string, int>();
        }

        private static AdventureStateParamsData CreateEmptyParameters()
        {
            return new AdventureStateParamsData
            {
                Strings = new Dictionary<string, string>(),
                Ints = new Dictionary<string, int>(),
                Bools = new Dictionary<string, bool>(),
            };
        }
    }
}
