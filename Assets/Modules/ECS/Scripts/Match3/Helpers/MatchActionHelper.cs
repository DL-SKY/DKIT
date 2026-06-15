using Leopotam.Ecs;
using Modules.Definitions.Scripts.Implementation.Defs.Gems;
using Modules.ECS.Scripts.Match3.Components;

namespace Modules.ECS.Scripts.Match3.Helpers
{
    public static class MatchActionHelper
    {
        /// <summary>
        /// Пытается отправить действия (match-экшены) для указанного типа фишки и количества совпадений.
        /// Проверяет, есть ли в определении фишки действия для данного количества совпавших фишек,
        /// и если есть, создает запросы на выполнение этих действий в ECS мире.
        /// </summary>
        /// <param name="gemDef">Определение фишки, содержащее конфигурацию действий для различных количеств совпадений</param>
        /// <param name="gemCount">Количество совпавших фишек (3, 4, 5+), для которого нужно проверить наличие действий</param>
        /// <param name="world">ECS мир, в котором будут созданы сущности с запросами на выполнение действий</param>
        public static void TrySendMatchActions(GemDef gemDef, int gemCount, EcsWorld world)
        {
            if (gemDef.MatchCountActions.TryGetValue(gemCount, out var data))
            {
                foreach (var action in data.Actions)
                {
                    // Создаем событие об изменении счетчика очков
                    var newAction = action;
                    var requestEntity = world.NewEntity();
                    requestEntity.Get<MatchActionRequest>() = new MatchActionRequest
                    {
                        Action = newAction
                    };
                }
            }
        }
    }
}
