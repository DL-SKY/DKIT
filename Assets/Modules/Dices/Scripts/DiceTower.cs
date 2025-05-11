using System.Collections.Generic;

namespace Modules.Dices.Scripts
{
    public class DiceTower
    {
        private Dictionary<DiceType, Dice> _dices = new Dictionary<DiceType, Dice>();

        public DiceResult Roll(DiceType type, DiceOptions options = DiceOptions.None)
        {
            return GetDice(type).Roll(options);
        }

        public DiceResult Roll(DiceType type, int mods, DiceOptions options = DiceOptions.None)
        {
            return GetDice(type).Roll(mods, options);
        }

        public DiceResult Roll(DiceType type, int count, int mods, DiceOptions options = DiceOptions.None)
        {
            return GetDice(type).Roll(count, mods, options);
        }

        private Dice GetDice(DiceType type)
        {
            if (!_dices.ContainsKey(type))
                _dices.Add(type, new Dice(type));

            return _dices[type];
        }
    }
}
