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
    /// Компонент данных об очках, заданиях и прогрессе выполнения целей раунда.
    /// Используется для отслеживания текущего состояния игровых целей и набранных очков.
    /// </summary>
    public struct ScoreData
    { 
        //TODO: ...
        //...
    }

    /// <summary>
    /// Компонент-запрос на начисление очков за обнаруженное совпадение фишек.
    /// Создается системой MatchDetectionSystem при обнаружении группы совпавших фишек.
    /// </summary>
    public struct MatchScoreRequest
    {
        /// <summary>
        /// Количество фишек в группе (3, 4, 5+)
        /// </summary>
        public int Count;

        /// <summary>
        /// Тип фишки (DefId), которая совпала
        /// </summary>
        public string GemType;
    }
}
