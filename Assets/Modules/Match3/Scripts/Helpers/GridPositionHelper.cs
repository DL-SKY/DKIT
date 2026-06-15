using UnityEngine;

namespace Modules.Match3.Scripts.Helpers
{
    /// <summary>
    /// Статический хелпер для работы с координатами игрового поля Match3.
    /// Предоставляет методы для конвертации координат сетки в мировые координаты Unity и обратно,
    /// вычисления смещений для центрирования игрового поля, работы с матрицами данных поля
    /// и расчета параметров камеры для корректного отображения игрового поля.
    /// Все методы учитывают центрирование игрового поля относительно начала координат (0, 0, 0).
    /// </summary>
    public static class GridPositionHelper
    {
        public const float CELL_POSITION_Z = 0.00f;
        public const float GEM_POSITION_Z = -0.01f;

        /// <summary>
        /// Вычисляет смещение для центрирования поля относительно камеры.
        /// Центр поля будет находиться в позиции (0, 0, 0).
        /// </summary>
        /// <param name="width">Ширина игрового поля</param>
        /// <param name="height">Высота игрового поля</param>
        /// <returns>Смещение для центрирования (offsetX, offsetY)</returns>
        public static Vector2 CalculateCenteringOffset(int width, int height)
        {
            float offsetX = -(width - 1) / 2f;
            float offsetY = -(height - 1) / 2f;
            return new Vector2(offsetX, offsetY);
        }

        /// <summary>
        /// Вычисляет смещение для центрирования поля относительно камеры на основе маски.
        /// Центр поля будет находиться в позиции (0, 0, 0).
        /// </summary>
        /// <param name="mask">Двумерный массив-маска, где GetLength(0) - количество строк, GetLength(1) - количество столбцов</param>
        /// <returns>Смещение для центрирования (offsetX, offsetY)</returns>
        public static Vector2 CalculateCenteringOffset(int[,] mask)
        {
            // Вычисляем смещение для центрирования поля относительно камеры
            // В маске: GetLength(0) - количество строк, GetLength(1) - количество столбцов
            // В Unity координатах: X - горизонтальная ось (столбцы), Y - вертикальная ось (строки)
            int rows = mask.GetLength(0);  // количество строк в маске
            int cols = mask.GetLength(1);  // количество столбцов в маске
            int width = cols;  // ширина поля в Unity (столбцы)
            int height = rows; // высота поля в Unity (строки)
            
            return CalculateCenteringOffset(width, height);
        }

        /// <summary>
        /// Конвертирует координаты сетки в мировые координаты Unity с учетом центрирования.
        /// </summary>
        /// <param name="gridX">Координата X в сетке</param>
        /// <param name="gridY">Координата Y в сетке</param>
        /// <param name="width">Ширина игрового поля</param>
        /// <param name="height">Высота игрового поля</param>
        /// <param name="z">Z координата в мировом пространстве (по умолчанию 0)</param>
        /// <returns>Мировые координаты Vector3</returns>
        public static Vector3 GridToWorldPosition(int gridX, int gridY, int width, int height, float z)
        {
            var offset = CalculateCenteringOffset(width, height);
            return new Vector3(gridX + offset.x, gridY + offset.y, z);
        }

        /// <summary>
        /// Конвертирует координаты сетки в мировые координаты Unity с использованием предвычисленного смещения.
        /// </summary>
        /// <param name="gridX">Координата X в сетке</param>
        /// <param name="gridY">Координата Y в сетке</param>
        /// <param name="offset">Смещение для центрирования</param>
        /// <param name="z">Z координата в мировом пространстве (по умолчанию 0)</param>
        /// <returns>Мировые координаты Vector3</returns>
        public static Vector3 GridToWorldPosition(int gridX, int gridY, Vector2 offset, float z)
        {
            return new Vector3(gridX + offset.x, gridY + offset.y, z);
        }

        /// <summary>
        /// Конвертирует мировые координаты Unity обратно в координаты сетки.
        /// </summary>
        /// <param name="worldPosition">Мировые координаты</param>
        /// <param name="width">Ширина игрового поля</param>
        /// <param name="height">Высота игрового поля</param>
        /// <returns>Координаты сетки (x, y)</returns>
        public static Vector2Int WorldToGridPosition(Vector3 worldPosition, int width, int height)
        {
            var offset = CalculateCenteringOffset(width, height);
            int gridX = Mathf.RoundToInt(worldPosition.x - offset.x);
            int gridY = Mathf.RoundToInt(worldPosition.y - offset.y);
            return new Vector2Int(gridX, gridY);
        }

        /// <summary>
        /// Конвертирует мировые координаты Unity обратно в координаты сетки с использованием предвычисленного смещения.
        /// </summary>
        /// <param name="worldPosition">Мировые координаты</param>
        /// <param name="offset">Смещение для центрирования</param>
        /// <returns>Координаты сетки (x, y)</returns>
        public static Vector2Int WorldToGridPosition(Vector3 worldPosition, Vector2 offset)
        {
            int gridX = Mathf.RoundToInt(worldPosition.x - offset.x);
            int gridY = Mathf.RoundToInt(worldPosition.y - offset.y);
            return new Vector2Int(gridX, gridY);
        }

        /// <summary>
        /// Получает значение из матрицы по координатам сетки игрового поля.
        /// Выполняет преобразование координат Unity (X - горизонтальная ось, Y - вертикальная ось)
        /// в координаты матрицы (первый индекс - строка, второй индекс - столбец).
        /// Строки матрицы инвертируются: первая строка в JSON соответствует нижней строке на игровом поле.
        /// </summary>
        /// <param name="gridX">Горизонтальная координата в сетке игрового поля (столбец)</param>
        /// <param name="gridY">Вертикальная координата в сетке игрового поля (строка)</param>
        /// <param name="matrix">Двумерный массив, где первый индекс - строка, второй индекс - столбец</param>
        /// <returns>Значение из матрицы по указанным координатам</returns>
        public static int GetGridValueFromMatrix(int gridX, int gridY, int[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int height = rows;
            return matrix[height - 1 - gridY, gridX];
        }

        /// <summary>
        /// Вычисляет размер ортографической камеры для отображения всего игрового поля на экране.
        /// Учитывает соотношение сторон экрана и размеры игрового поля.
        /// </summary>
        /// <param name="width">Ширина игрового поля (количество столбцов)</param>
        /// <param name="height">Высота игрового поля (количество строк)</param>
        /// <param name="padding">Отступ от краев поля (по умолчанию 0.5)</param>
        /// <returns>Размер ортографической камеры (orthographic size)</returns>
        public static float CalculateCameraOrthographicSize(int width, int height, float padding)
        {
            // Соотношение сторон экрана
            float aspectRatio = (float)Screen.width / Screen.height;
            
            // Размеры игрового поля в мировых единицах
            float fieldWidth = width;
            float fieldHeight = height;
            
            // Вычисляем необходимый размер камеры по высоте
            float sizeByHeight = (fieldHeight + padding * 2) / 2f;
            
            // Вычисляем необходимый размер камеры по ширине
            // orthographicSize определяет половину высоты видимой области
            // ширина видимой области = orthographicSize * 2 * aspectRatio
            float sizeByWidth = (fieldWidth + padding * 2) / (2f * aspectRatio);
            
            // Выбираем больший размер, чтобы все поле помещалось на экране
            return Mathf.Max(sizeByHeight, sizeByWidth);
        }
    }
}

