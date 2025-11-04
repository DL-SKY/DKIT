using Leopotam.Ecs;
using UnityEngine;

namespace Modules.ECS.Scripts.Examples
{
    // Событие свапа фишек
    public struct SwapEvent
    {
        public EcsEntity FromEntity;
        public EcsEntity ToEntity;
    }

    public struct DragStartEvent
    {
        public Vector2 ScreenPosition;
        public EcsEntity GemEntity;
    }

    public struct DragEndEvent
    {
        public Vector2 ScreenPosition;
        public EcsEntity GemEntity;
    }
}
