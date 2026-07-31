using System.Collections.Generic;

namespace Modules.State.Scripts.Implementation.Adventure.StateDatas
{
    public class ProfileStateData
    {
        public long CreateTime;
        public long UpdateTime;

        /// <summary>
        /// Profile-level counters/flags: purchases, unlocks, slot counts, etc.
        /// Missing key == 0. Bool flags as 0 / non-zero.
        /// </summary>
        public Dictionary<string, int> Parameters;
    }
}
