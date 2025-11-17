using Leopotam.Ecs;

namespace Modules.ECS.Scripts.Match3.Components
{
    // Компонент для перетаскивания фишек
    public struct Draggable
    {

    }

    // Компонент-событие свапа фишек
    public struct SwapEvent
    {
        public EcsEntity FromEntity;
        public EcsEntity ToEntity;
    }
}
