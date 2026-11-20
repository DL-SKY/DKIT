using System;
using System.Collections.Generic;
using Modules.Definitions.Scripts.Defs;

namespace Modules.Space.Scripts.Implementation.Definitions
{
    /// <summary>
    /// One picture path for a placed object type. <see cref="SpaceObjectType.Ship"/> is not pictured here.
    /// </summary>
    public sealed class SpaceMarkerKindIcon
    {
        public SpaceObjectType ObjectType;
        public string IconResourcesPath;
    }

    /// <summary>
    /// Single record of marker pictures for placed <see cref="SpaceObjectType"/> values.
    /// A ship marker reads its picture from <see cref="SpaceHullDef.IconResourcesPath"/> instead.
    /// </summary>
    public sealed class SpaceMarkerIconsDef : AbstractDefinition
    {
        public List<SpaceMarkerKindIcon> Icons;

        public bool TryGetIcon(SpaceObjectType objectType, out string path)
        {
            path = null;
            if (Icons == null || objectType == SpaceObjectType.Ship)
                return false;

            for (var i = 0; i < Icons.Count; i++)
            {
                var entry = Icons[i];
                if (entry == null || entry.ObjectType != objectType || string.IsNullOrWhiteSpace(entry.IconResourcesPath))
                    continue;

                path = entry.IconResourcesPath;
                return true;
            }

            return false;
        }

        public bool IsUsable()
        {
            var types = (SpaceObjectType[])Enum.GetValues(typeof(SpaceObjectType));
            var pictured = 0;
            for (var i = 0; i < types.Length; i++)
            {
                if (types[i] != SpaceObjectType.Ship)
                    pictured++;
            }

            if (Icons == null || Icons.Count != pictured)
                return false;

            for (var i = 0; i < types.Length; i++)
            {
                if (types[i] == SpaceObjectType.Ship)
                    continue;

                var matches = 0;
                for (var j = 0; j < Icons.Count; j++)
                {
                    var entry = Icons[j];
                    if (entry == null || entry.ObjectType != types[i])
                        continue;

                    if (string.IsNullOrWhiteSpace(entry.IconResourcesPath))
                        return false;

                    matches++;
                }

                if (matches != 1)
                    return false;
            }

            return true;
        }
    }
}
