using System;

namespace Modules.Dices.Scripts
{
    public class Dice
    {
        public DiceType DiceType => _diceType;
        private readonly DiceType _diceType;

        private Random _random;

        public Dice(DiceType diceType)
        {
            _diceType = diceType;
            _random = new Random(DateTime.UtcNow.Millisecond);
        }

        public DiceResult Roll(DiceOptions options = DiceOptions.None)
        {
            var roll = Roll();

            if (options.HasFlag(DiceOptions.Advantage))
            {
                var advantageRoll = Roll();
                roll = Math.Max(roll, advantageRoll);
            }

            if (options.HasFlag(DiceOptions.Disadvantage))
            {
                var disadvantageRoll = Roll();
                roll = Math.Min(roll, disadvantageRoll);
            }

            return new DiceResult(roll, GetResultType(roll));
        }

        public DiceResult Roll(int mods, DiceOptions options = DiceOptions.None)
        {
            return Roll(options) + mods;
        }

        public DiceResult Roll(int count, int mods, DiceOptions options = DiceOptions.None)
        {
            count = Math.Max(1, count);

            var result = 0;
            for (int i = 0; i < count; i++)
                result += Roll(options);

            return new DiceResult(result + mods);
        }

        private int Roll()
        { 
            return _random.Next(1, (int)DiceType + 1);
        }

        private DiceResultType GetResultType(int rawRoll)
        {
            if (DiceType == DiceType.D20)
            {
                if (rawRoll == 1)
                    return DiceResultType.CriticalFailure;
                else if (rawRoll == 20)
                    return DiceResultType.CriticalHit;
            }

            return DiceResultType.Common;
        }
    }
}
