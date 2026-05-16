using Modules.Save.Scripts.Interfaces;
using Modules.State.Scripts.Core;
using System;

namespace Modules.State.Scripts.Implementation.Match3
{
    public class Match3StateManager : StateManager<StateData>
    {
        protected override StateData CreateNewState(string profileId)
        {
            return new StateData
            {
                ProfileId = profileId,
                PlayerLevel = 1,
                Coins = 0,
                Lives = 5,
                CurrentRoundDefId = "RoundExample",
                LastSaveUtcTicks = DateTime.UtcNow.Ticks
            };
        }
    }
}
