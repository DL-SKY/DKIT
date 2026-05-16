using Modules.State.Scripts.Implementation.Match3;
using System;

namespace Modules.State.Examples
{
    public static class PlayerProfileExampleData
    {
        public static StateData Create(string profileId)
        {
            return new StateData
            {
                ProfileId = profileId,
                PlayerLevel = 7,
                Coins = 1250,
                Lives = 5,
                CurrentRoundDefId = "RoundExample",
                LastSaveUtcTicks = DateTime.UtcNow.Ticks
            };
        }
    }
}
