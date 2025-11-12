using UnityEngine;

namespace Modules.ECS.Scripts.Match3.Components
{
    // Позиция клетки на сетке
    public struct CellPosition
    {
        public int X;
        public int Y;
    }

    // Компонент для визуального представления клетки
    public struct CellView
    {
        public GameObject GameObject;
    }
}

