using System.Collections.Generic;

namespace Modules.ECS.Scripts.Match3.Components
{
    /// <summary>
    /// Компонент-запрос на проверку совпадений на игровом поле.
    /// Создается после завершения свапа или других действий, требующих проверки совпадений.
    /// </summary>
    public struct CheckMatchesRequest
    {
    }

    /// <summary>
    /// Компонент-событие, содержащий информацию о найденной группе совпадений.
    /// Создается системой MatchDetectionSystem при обнаружении совпадения из 3+ фишек.
    /// </summary>
    public struct MatchGroup
    {
        /// <summary>
        /// Список координат фишек, входящих в группу совпадений
        /// </summary>
        public List<GridPosition> Positions;

        /// <summary>
        /// Количество фишек в группе (3, 4, 5+)
        /// </summary>
        public int Count;

        /// <summary>
        /// Тип фишки (DefId), которая совпала
        /// </summary>
        public string GemType;

        /// <summary>
        /// Направление совпадения (горизонтальное или вертикальное)
        /// </summary>
        public MatchDirection Direction;
    }

    /// <summary>
    /// Направление совпадения
    /// </summary>
    public enum MatchDirection
    {
        Horizontal,
        Vertical
    }
}

