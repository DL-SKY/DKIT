using UnityEngine;

namespace Modules.ECS.Scripts.Match3.Components
{
    /// <summary>
    /// Компонент для хранения данных игровой зоны (offset для центрирования).
    /// Создается при инициализации и используется для обновления позиций фишек.
    /// </summary>
    public struct GameZoneData
    {
        /// <summary>
        /// Смещение для центрирования игрового поля
        /// </summary>
        public Vector2 CenteringOffset;
    }
    /*
     * // Сохраняем offset в компоненте для использования другими системами
            var gameZoneEntity = _world.NewEntity();
            gameZoneEntity.Get<GameZoneData>() = new GameZoneData
            {
                CenteringOffset = centeringOffset
            };
     * */


    /*
     * В ECS системе GemsInitSystem происходит создание фишки. Нужно создать компоненты и системы, которые позволят передвигать фишки по следующим правилам:
1. Фишки не двигаются до окончания перетаскивания (нажатие, перемещение курсора/пальца, отпускание нажатия)
2. Начало движения при зажатом пальце/кнопке мыши отпределяет начальную фишку
3. Окончание движения с зажатием - определяет направление, в которм будет происходит движение фишки с заменой соседней фишкой
     * */



    // Позиция на сетке
    public struct GridPosition
    {
        public int X;
        public int Y;
    }
}
