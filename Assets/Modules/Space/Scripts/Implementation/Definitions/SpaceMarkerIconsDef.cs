using System;
using System.Collections.Generic;
using Modules.Definitions.Scripts.Defs;

namespace Modules.Space.Scripts.Implementation.Definitions
{
    /// <summary>
    /// One picture path for a placed object kind. Ships do not use this record.
    /// </summary>
    public sealed class SpaceMarkerKindIcon
    {
        public SpaceObjectKind Kind;
        public string IconResourcesPath;
    }

    /// <summary>
    /// Single record of marker pictures for <see cref="SpaceObjectKind"/>.
    /// A ship marker reads its picture from <see cref="SpaceHullDef.IconResourcesPath"/> instead.
    /// </summary>
    public sealed class SpaceMarkerIconsDef : AbstractDefinition
    {
        public List<SpaceMarkerKindIcon> Icons;

        public bool TryGetIcon(SpaceObjectKind kind, out string path)
        {
            path = null;
            if (Icons == null)
                return false;

            for (var i = 0; i < Icons.Count; i++)
            {
                var entry = Icons[i];
                if (entry == null || entry.Kind != kind || string.IsNullOrWhiteSpace(entry.IconResourcesPath))
                    continue;

                path = entry.IconResourcesPath;
                return true;
            }

            return false;
        }

        public bool IsUsable()
        {
            var kinds = (SpaceObjectKind[])Enum.GetValues(typeof(SpaceObjectKind));
            if (Icons == null || Icons.Count != kinds.Length)
                return false;

            for (var i = 0; i < kinds.Length; i++)
            {
                var matches = 0;
                for (var j = 0; j < Icons.Count; j++)
                {
                    var entry = Icons[j];
                    if (entry == null || entry.Kind != kinds[i])
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
