using Modules.RPG.Scripts.Adventure.Choice;
using Modules.State.Scripts.Implementation.Adventure.StateDatas;
using System.Collections.Generic;

namespace Modules.State.Scripts.Implementation.Adventure
{
    public static class AdventureStateParamsOperator
    {
        public static AdventureStateParamsData CreateEmpty()
        {
            return new AdventureStateParamsData
            {
                Strings = new Dictionary<string, string>(),
                Ints = new Dictionary<string, int>(),
                Bools = new Dictionary<string, bool>(),
            };
        }

        public static void Merge(AdventureStateParamsData target, ChoiceActionParamsData source)
        {
            if (target == null || source == null)
                return;

            target.Strings ??= new Dictionary<string, string>();
            target.Ints ??= new Dictionary<string, int>();
            target.Bools ??= new Dictionary<string, bool>();

            if (source.Strings != null)
            {
                foreach (KeyValuePair<string, string> pair in source.Strings)
                    target.Strings[pair.Key] = pair.Value;
            }

            if (source.Ints != null)
            {
                foreach (KeyValuePair<string, int> pair in source.Ints)
                    target.Ints[pair.Key] = pair.Value;
            }

            if (source.Bools != null)
            {
                foreach (KeyValuePair<string, bool> pair in source.Bools)
                    target.Bools[pair.Key] = pair.Value;
            }
        }
    }
}
