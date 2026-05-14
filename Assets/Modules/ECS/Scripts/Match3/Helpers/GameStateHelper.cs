using Leopotam.Ecs;
using Modules.ECS.Scripts.Match3.Components;
using Modules.Match3.Scripts.Core;

namespace Modules.ECS.Scripts.Match3.Helpers
{
    public static class GameStateHelper
    {
        public static bool IsGameStopped(EcsFilter<GameState> filter)
        {
            if (filter.GetEntitiesCount() == 0)
            {
                return true;
            }

            ref var gameState = ref filter.Get1(0);
            if (gameState.IsPaused
                || gameState.State == RoundStateType.Win
                || gameState.State == RoundStateType.Lose)
            {
                return true;
            }

            return false;
        }
    }
}
