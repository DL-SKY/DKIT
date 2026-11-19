using Modules.Space.Scripts.Implementation.Definitions;

namespace Modules.Space.Scripts.Implementation.Session
{
    /// <summary>
    /// Shared entry for every space sector. Callers pass a sector definition id, not a scene name.
    /// </summary>
    public static class SpaceSectorLoad
    {
        public static bool TryGet(SpaceDefinitionsManager definitions, string sectorId, out SpaceSectorDef sector)
        {
            sector = null;
            return definitions != null
                && definitions.TryGetSector(sectorId, out sector)
                && sector != null
                && sector.IsUsable();
        }
    }
}
