using System.Collections.Generic;
using Modules.Definitions.Scripts.Defs;

namespace Modules.Space.Scripts.Implementation.Definitions
{
    /// <summary>
    /// One piece of fitted equipment. The file name is the id.
    /// An engine lists only the hull sizes it can be fitted to. A missing size forbids that hull.
    /// A module without size rows allows only its own <see cref="SizeType"/>.
    /// </summary>
    public sealed class SpaceModuleDef : AbstractDefinition
    {
        public SpaceModuleKind Kind;
        public string SlotId;
        public string WeaponGroup;
        public SpaceSizeType SizeType;
        public List<SpaceModuleSizeParameters> SizeParameters;

        public bool TryGetSizeParameters(SpaceSizeType sizeType, out SpaceModuleSizeParameters parameters)
        {
            parameters = null;
            if (SizeParameters == null)
                return false;

            for (var i = 0; i < SizeParameters.Count; i++)
            {
                var row = SizeParameters[i];
                if (row == null || row.SizeType != sizeType)
                    continue;

                parameters = row;
                return true;
            }

            return false;
        }

        public bool AllowsHullSize(SpaceSizeType hullSize)
        {
            if (HasSizeRows())
            {
                if (CountSize(hullSize) != 1 || !TryGetSizeParameters(hullSize, out var row) || row == null)
                    return false;

                return Kind != SpaceModuleKind.Engine || row.IsUsable();
            }

            return SizeType == hullSize;
        }

        public bool IsUsable()
        {
            if (string.IsNullOrWhiteSpace(SlotId))
                return false;

            if (SizeParameters != null)
            {
                var seen = new HashSet<SpaceSizeType>();
                for (var i = 0; i < SizeParameters.Count; i++)
                {
                    var row = SizeParameters[i];
                    if (row == null || !seen.Add(row.SizeType))
                        return false;

                    if (Kind == SpaceModuleKind.Engine && !row.IsUsable())
                        return false;
                }
            }

            if (Kind == SpaceModuleKind.Engine && !HasSizeRows())
                return false;

            return true;
        }

        private bool HasSizeRows()
        {
            if (SizeParameters == null)
                return false;

            for (var i = 0; i < SizeParameters.Count; i++)
            {
                if (SizeParameters[i] != null)
                    return true;
            }

            return false;
        }

        private int CountSize(SpaceSizeType sizeType)
        {
            var count = 0;
            for (var i = 0; i < SizeParameters.Count; i++)
            {
                if (SizeParameters[i] != null && SizeParameters[i].SizeType == sizeType)
                    count++;
            }

            return count;
        }
    }
}
