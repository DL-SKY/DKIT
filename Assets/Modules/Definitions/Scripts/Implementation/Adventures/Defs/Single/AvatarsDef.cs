using Modules.Definitions.Scripts.Defs;
using System.Collections.Generic;

namespace Modules.Definitions.Scripts.Implementation.Adventures.Defs.Single
{
    /// <summary>
    /// Single catalog of character avatars: free pools by ancestry and paid packs.
    /// Avatar art is resolved by convention from avatar id (e.g. Resources/Adventures/Avatars/{id}).
    /// </summary>
    public class AvatarsDef : AbstractDefinition
    {
        /// <summary>
        /// Free / starter avatars. Key — ancestry def id, value — avatar ids/path.
        /// </summary>
        public Dictionary<string, List<string>> Free;

        /// <summary>
        /// Purchase packs. Key — pack id.
        /// </summary>
        public Dictionary<string, AvatarPackData> Packs;
    }


    /// <summary>
    /// Commercial avatar pack: store product metadata and avatar ids grouped by ancestry.
    /// </summary>
    public class AvatarPackData
    {
        /// <summary>
        /// IAP / store product key.
        /// </summary>
        public string ProductId;

        /// <summary>
        /// Soft-currency price (0 when unused or IAP-only).
        /// </summary>
        public int Price;

        /// <summary>
        /// Avatars unlocked by this pack. Key — ancestry def id, value — avatar ids/paths.
        /// </summary>
        public Dictionary<string, List<string>> Avatars;
    }
}
