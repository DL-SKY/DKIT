using Leopotam.Ecs;
using UnityEngine;

namespace Modules.ECS.Scripts.Match3.Components
{
    /// <summary>
    /// Компонент для перетаскивания фишек
    /// </summary>
    public struct Draggable
    {

    }

    /// <summary>
    /// Компонент состояния перетаскивания. Создается при начале перетаскивания и удаляется при окончании.
    /// </summary>
    public struct DragState
    {
        /// <summary>
        /// Сущность фишки, которая перетаскивается
        /// </summary>
        public EcsEntity DraggedEntity;
        
        /// <summary>
        /// Начальная позиция в мировых координатах (где началось перетаскивание)
        /// </summary>
        public Vector3 StartWorldPosition;
        
        /// <summary>
        /// Начальная позиция на сетке
        /// </summary>
        public GridPosition StartGridPosition;
    }

    /// <summary>
    /// Компонент-событие запроса свапа фишек. Создается при окончании перетаскивания.
    /// </summary>
    public struct SwapRequest
    {
        /// <summary>
        /// Сущность первой фишки (начальная)
        /// </summary>
        public EcsEntity FromEntity;
        
        /// <summary>
        /// Сущность второй фишки (целевая, соседняя)
        /// </summary>
        public EcsEntity ToEntity;
        
        /// <summary>
        /// Направление свапа (для логирования/отладки)
        /// </summary>
        public SwapDirection Direction;
    }

    /// <summary>
    /// Направление свапа
    /// </summary>
    public enum SwapDirection
    {
        None = 0,

        Up,
        Down,
        Left,
        Right        
    }
}
