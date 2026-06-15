using Modules.Cheats.Scripts.Editor.Implementation.Save;
using System;
using System.Collections.Generic;

namespace Modules.Cheats.Scripts.Editor.Core
{
    public static class CheatSectionsProvider
    {
        // Edit this list to control what sections are visible in CheatWindow.
        // Add new section classes here or remove existing ones.
        private static readonly List<Type> RegisteredSectionTypes = new List<Type>
        {
            //typeof(GeneralCheatSection),
            typeof(SaveCheatSection),
        };


        public static List<CheatSectionBase> CreateSections()
        {
            List<CheatSectionBase> sections = new List<CheatSectionBase>();

            for (int i = 0; i < RegisteredSectionTypes.Count; i++)
            {
                Type type = RegisteredSectionTypes[i];
                if (type == null)
                {
                    continue;
                }

                if (!typeof(CheatSectionBase).IsAssignableFrom(type))
                {
                    UnityEngine.Debug.LogError(
                        "[CheatSectionsProvider] Type is not a CheatSectionBase: " + type.FullName);
                    continue;
                }

                if (type.IsAbstract || type.IsGenericTypeDefinition)
                {
                    continue;
                }

                try
                {
                    object instance = Activator.CreateInstance(type);
                    CheatSectionBase section = instance as CheatSectionBase;
                    if (section != null)
                    {
                        sections.Add(section);
                    }
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError(
                        "[CheatSectionsProvider] Failed to create section " + type.FullName + ". Error: " + e);
                }
            }

            sections.Sort(CompareByOrderAndName);
            return sections;
        }

        private static int CompareByOrderAndName(CheatSectionBase left, CheatSectionBase right)
        {
            int orderComparison = left.Order.CompareTo(right.Order);
            if (orderComparison != 0)
            {
                return orderComparison;
            }

            return string.Compare(left.Id, right.Id, StringComparison.Ordinal);
        }
    }
}
