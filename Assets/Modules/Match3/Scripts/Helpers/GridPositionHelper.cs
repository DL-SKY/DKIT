using UnityEngine;

namespace Modules.Match3.Scripts.Helpers
{
    /// <summary>
    /// Хелпер для конвертации координат сетки игрового поля в мировые координаты Unity
    /// и обратно. Обеспечивает центрирование игрового поля относительно камеры.
    /// </summary>
    public static class GridPositionHelper
    {
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
        /// Конвертирует координаты сетки в мировые координаты Unity с учетом центрирования.
        /// </summary>
        /// <param name="gridX">Координата X в сетке</param>
        /// <param name="gridY">Координата Y в сетке</param>
        /// <param name="width">Ширина игрового поля</param>
        /// <param name="height">Высота игрового поля</param>
        /// <param name="z">Z координата в мировом пространстве (по умолчанию 0)</param>
        /// <returns>Мировые координаты Vector3</returns>
        public static Vector3 GridToWorldPosition(int gridX, int gridY, int width, int height, float z = 0f)
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
        public static Vector3 GridToWorldPosition(int gridX, int gridY, Vector2 offset, float z = 0f)
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
    }
}

