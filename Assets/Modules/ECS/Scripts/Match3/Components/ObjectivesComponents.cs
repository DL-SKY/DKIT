namespace Modules.ECS.Scripts.Match3.Components
{
    /// <summary>
    /// Компонент данных о количестве оставшихся ходов в раунде.
    /// Используется для отслеживания прогресса выполнения целей раунда.
    /// </summary>
    public struct TurnsData
    {
        public int Turns;
    }

    /// <summary>
    /// Компонент-запрос на изменение количества оставшихся ходов.
    /// Создается для изменения счетчика ходов (например, при выполнении хода игроком).
    /// </summary>
    public struct ChangeTurnsRequest
    {
        /// <summary>
        /// Дельта изменения счетчика оставшихся ходов
        /// -1 - потратить один ход
        /// </summary>
        public int Delta;
    }

    /// <summary>
    /// Типы очков прогресса
    /// </summary>
    public enum ScoreType
    { 
        NA = 0,

        Boost,

        HitPoints,
        ShieldPoints,

        Progress,

        //...
    }

    /// <summary>
    /// Компонент данных об очках, заданиях и прогрессе выполнения целей раунда.
    /// Используется для отслеживания текущего состояния игровых целей и набранных очков.
    /// </summary>
    public struct ScoreData
    {
        public ScoreType Type;
        public int Value;
    }

    /// <summary>
    /// Компонент-запрос на изменение счетчика очков.
    /// </summary>
    public struct ChangeScoreRequest
    {
        /// <summary>
        /// Тип очков
        /// </summary>
        public ScoreType Type;

        /// <summary>
        /// Дельта изменения счетчика
        /// </summary>
        public int Delta;
    }
}
