using UnityEngine;

namespace Modules.ECS.Scripts.Match3.Components
{
    /// <summary>
    /// Компонент для хранения данных игровой зоны (offset для центрирования).
    /// Создается при инициализации и используется для обновления позиций фишек.
    /// </summary>
    public struct CenterOffsetData
    {
        /// <summary>
        /// Смещение для центрирования игрового поля
        /// </summary>
        public Vector2 Offset;
    }

    /// <summary>
    /// Позиция на сетке
    /// </summary>
    public struct GridPosition
    {
        public int X;
        public int Y;
    }
}
