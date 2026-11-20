using System;
using System.Collections.Generic;
using Modules.Cheats.Scripts.Editor.Core;
using Modules.Space.Scripts.Editor.Cheats.Sections;

namespace Modules.Space.Scripts.Editor.Cheats
{
    public static class SpaceCheatSectionsProvider
    {
        private static readonly List<Type> REGISTERED_SECTION_TYPES = new List<Type>
        {
            typeof(SpaceProfileCheatSection),
            typeof(SpaceDefinitionsCheatSection),
            typeof(SpaceSectorCheatSection),
        };

        public static List<CheatSectionBase> CreateSections()
        {
            List<CheatSectionBase> sections = new List<CheatSectionBase>();
            for (int i = 0; i < REGISTERED_SECTION_TYPES.Count; i++)
            {
                Type type = REGISTERED_SECTION_TYPES[i];
                if (type == null || type.IsAbstract || !typeof(CheatSectionBase).IsAssignableFrom(type))
                    continue;

                try
                {
                    if (Activator.CreateInstance(type) is CheatSectionBase section)
                        sections.Add(section);
                }
                catch (Exception exception)
                {
                    UnityEngine.Debug.LogError("[Space] Failed to create cheat section " + type.FullName + ". " + exception.Message);
                }
            }

            sections.Sort(CompareByOrderAndName);
            return sections;
        }

        private static int CompareByOrderAndName(CheatSectionBase left, CheatSectionBase right)
        {
            int orderComparison = left.Order.CompareTo(right.Order);
            if (orderComparison != 0)
                return orderComparison;

            return string.Compare(left.Id, right.Id, StringComparison.Ordinal);
        }
    }
}
