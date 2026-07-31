using Modules.Definitions.Scripts.Defs;
using Modules.Definitions.Scripts.Implementation.Adventures.Constants;
using System.Collections.Generic;

namespace Modules.Definitions.Scripts.Implementation.Adventures.Defs.Single
{
    /// <summary>
    /// Single settings for Adventure profile state defaults.
    /// </summary>
    public class ProfileStateSettingsDef : AbstractDefinition
    {
        /// <summary>
        /// Default values for <c>ProfileStateData.Parameters</c> when creating a new profile.
        /// Missing key in runtime state is treated as 0.
        /// Known keys: <see cref="Glossary.ProfileState.MAX_PARTY_SLOTS"/>,
        /// <see cref="Glossary.ProfileState.MAX_INVENTORY_SLOTS"/>.
        /// </summary>
        public Dictionary<string, int> DefaultParameters;
    }
}
