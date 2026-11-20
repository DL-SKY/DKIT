using System.Collections.Generic;
using Modules.Definitions.Scripts.Defs;

namespace Modules.Space.Scripts.Implementation.Definitions
{
    /// <summary>
    /// A command the player's ship can issue. The file name is the command id.
    /// An empty <see cref="ObjectTypes"/> or <see cref="Relations"/> list shows the command nowhere.
    /// </summary>
    public sealed class SpaceCommandDef : AbstractDefinition
    {
        public string Name;
        public string Description;
        public string IconResourcesPath;
        public int PriorityOrder;
        public List<SpaceObjectType> ObjectTypes;
        public List<SpaceRelation> Relations;

        public bool Allows(SpaceObjectType objectType, SpaceRelation relation)
        {
            return Listed(ObjectTypes, objectType) && Listed(Relations, relation);
        }

        public bool IsUsable()
        {
            return !string.IsNullOrWhiteSpace(Name)
                && !string.IsNullOrWhiteSpace(Description)
                && !string.IsNullOrWhiteSpace(IconResourcesPath)
                && Unique(ObjectTypes)
                && Unique(Relations);
        }

        private static bool Listed<T>(List<T> values, T value)
        {
            if (values == null)
                return false;

            for (var i = 0; i < values.Count; i++)
            {
                if (EqualityComparer<T>.Default.Equals(values[i], value))
                    return true;
            }

            return false;
        }

        private static bool Unique<T>(List<T> values)
        {
            if (values == null || values.Count == 0)
                return false;

            var seen = new HashSet<T>();
            for (var i = 0; i < values.Count; i++)
            {
                if (!seen.Add(values[i]))
                    return false;
            }

            return true;
        }
    }
}
