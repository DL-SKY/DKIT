using System.Collections.Generic;
using Modules.Restrictions.Scripts.Core;

namespace Modules.Definitions.Scripts.Editor.Adventures.Restrictions
{
    /// <summary>
    /// Editable restriction fields in TEA. Types without an explicit profile use <see cref="Default"/>.
    /// </summary>
    [System.Flags]
    public enum RestrictionEditorField
    {
        None = 0,
        CompareOptions = 1 << 0,
        StringValues = 1 << 1,
        IntValues = 1 << 2,
        LongValues = 1 << 3,
        BoolValues = 1 << 4,

        Default = CompareOptions | StringValues | IntValues | LongValues | BoolValues,
    }

    public interface IRestrictionEditorFieldProfilesRegistry
    {
        RestrictionEditorField GetVisibleFields(RestrictionType type);
    }

    /// <summary>
    /// Maps <see cref="RestrictionType"/> to visible editor fields in TEA.
    /// Add a new entry here when a restriction type needs a narrower field set.
    /// </summary>
    public sealed class RestrictionEditorFieldProfilesRegistry : IRestrictionEditorFieldProfilesRegistry
    {
        private readonly Dictionary<RestrictionType, RestrictionEditorField> _profilesByType;

        public RestrictionEditorFieldProfilesRegistry()
        {
            _profilesByType = new Dictionary<RestrictionType, RestrictionEditorField>
            {
                {
                    RestrictionType.TimeNow,
                    RestrictionEditorField.CompareOptions | RestrictionEditorField.LongValues
                },
            };
        }

        public RestrictionEditorField GetVisibleFields(RestrictionType type)
        {
            return _profilesByType.TryGetValue(type, out RestrictionEditorField fields)
                ? fields
                : RestrictionEditorField.Default;
        }
    }
}
