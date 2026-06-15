using Modules.Match3.Scripts.Core;

namespace Modules.ECS.Scripts.Match3.Components
{
    /// <summary>
    /// Компонент-событие, уведомляющее об изменении в оставшихся ходах
    /// </summary>
    public struct TurnsCallback
    {
    }

    /// <summary>
    /// Компонент-событие, уведомляющее об изменении в очках (прогрессе)
    /// </summary>
    public struct ScoreCallback
    {
        public ScoreType Type;
    }

    /// <summary>
    /// Компонент-событие изменения статуса игры/раунда
    /// </summary>
    public struct RoundEndCallback
    {
        public RoundStateType State;
    }
}
