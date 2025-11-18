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

    /// <summary>
    /// Компонент состояния удаления фишек. Создается при начале процесса удаления и удаляется после завершения.
    /// Пока этот компонент существует, игрок не может перетаскивать фишки, а другие системы игрового поля заблокированы.
    /// </summary>
    public struct MatchDestructionInProgress
    {
    }

    /// <summary>
    /// Компонент данных анимации удаления фишек. Содержит информацию о фишках, которые удаляются, и время начала анимации.
    /// </summary>
    public struct MatchDestructionAnimation
    {
        /// <summary>
        /// Список сущностей фишек, которые нужно удалить
        /// </summary>
        public System.Collections.Generic.List<Leopotam.Ecs.EcsEntity> Entities;

        /// <summary>
        /// Время начала анимации
        /// </summary>
        public float StartTime;

        /// <summary>
        /// Длительность анимации в секундах
        /// </summary>
        public float Duration;
    }
}

