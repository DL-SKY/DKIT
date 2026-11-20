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
    /// Single record of marker presentation. Pictures cover placed body types.
    /// A ship marker reads its picture from <see cref="SpaceHullDef.IconResourcesPath"/> instead.
    /// <see cref="MenuRadius"/> is the pixel distance from the marker center to each ring button.
    /// </summary>
    public sealed class SpaceMarkerSettingsDef : AbstractDefinition
    {
        public float MenuRadius;
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
            if (float.IsNaN(MenuRadius) || float.IsInfinity(MenuRadius) || MenuRadius <= 0f)
                return false;

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
