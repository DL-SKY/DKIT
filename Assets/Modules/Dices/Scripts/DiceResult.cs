namespace Modules.Dices.Scripts
{
    public struct DiceResult
    {
        public int Result;
        public DiceResultType Type;

        public DiceResult(int result, DiceResultType type)
        {
            Result = result;
            Type = type;
        }

        public DiceResult(int result)
        {
            Result = result;
            Type = DiceResultType.Common;
        }

        public static implicit operator int(DiceResult result)
        {
            return result.Result;
        }

        public static DiceResult operator +(DiceResult a, DiceResult b)
        {
            return new DiceResult(a.Result + b.Result, a.Type);
        }

        public static DiceResult operator +(DiceResult a, int b)
        {
            return new DiceResult(a.Result + b, a.Type);
        }

        public static DiceResult operator -(DiceResult a, DiceResult b)
        {
            return new DiceResult(a.Result - b.Result, a.Type);
        }

        public static DiceResult operator -(DiceResult a, int b)
        {
            return new DiceResult(a.Result - b, a.Type);
        }

        public static DiceResult operator *(DiceResult a, int b)
        {
            return new DiceResult(a.Result * b, a.Type);
        }

        public static DiceResult operator /(DiceResult a, int b)
        {
            return new DiceResult(a.Result / b, a.Type);
        }
    }
}
