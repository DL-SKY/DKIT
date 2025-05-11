using System;

namespace Modules.Dices.Scripts
{
    [Flags]
    public enum DiceOptions
    {
        None = 0,

        Advantage = 1 << 0,
        Disadvantage = 1 << 1,

        Exploding = 1 << 2,

        Hidden = 1 << 3,
    }
}
