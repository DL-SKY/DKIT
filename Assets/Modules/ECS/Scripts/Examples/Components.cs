using UnityEngine;

namespace Modules.ECS.Scripts.Examples
{
    // Позиция на сетке
    public struct GridPosition
    {
        public int X;
        public int Y;
    }

    // Тип фишки (цвет)
    public struct GemType
    {
        public int TypeId;
        public Color Color;
    }

    // Флаг для фишки, которую можно перемещать
    public struct Movable { }

    // Флаг для выбранной фишки
    public struct Selected { }

    // Флаг для проверки совпадений
    public struct CheckMatches { }

    // Флаг для фишки, которую нужно уничтожить
    public struct DestroyTag { }

    // Флаг для фишки, которая падает
    public struct Falling { }

    // Компонент для визуального представления
    public struct GemView
    {
        public GameObject GameObject;
    }

    // Новые компоненты для перетаскивания
    public struct Draggable { }
}
