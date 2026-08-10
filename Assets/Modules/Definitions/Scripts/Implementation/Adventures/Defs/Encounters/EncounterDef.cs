using Modules.Definitions.Scripts.Defs;
using System.Collections.Generic;

namespace Modules.Definitions.Scripts.Implementation.Adventures.Defs.Encounters
{
    /// <summary>
    /// Minimal encounter composition for battle startup.
    /// Narrative outcomes and rewards are handled by adventure scenario logic.
    /// </summary>
    public class EncounterDef : AbstractDefinition
    {
        public bool Disabled;

        public List<string> Tags;

        public string Title;
        public string Description;

        /// <summary>
        /// Ordered list of creature definition ids.
        /// Repeat ids to represent multiple enemies of the same type.
        /// </summary>
        public List<string> Creatures;
    }
}
