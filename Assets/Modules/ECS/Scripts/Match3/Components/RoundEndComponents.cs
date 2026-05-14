using Modules.Match3.Scripts.Core;
using Modules.Restrictions.Scripts.Core;
using System.Collections.Generic;

namespace Modules.ECS.Scripts.Match3.Components
{
    /// <summary>
    /// Копии условий победы и поражения раунда для проверки в ECS (источник — ObjectivesData).
    /// </summary>
    public struct RoundEndConditionsData
    {
        public List<Restriction> Victory;
        public List<Restriction> Defeat;
    }

    /// <summary>
    /// Состояние игры
    /// </summary>
    public struct GameState
    {
        public bool IsPaused;
        public RoundStateType State;
    }
}
