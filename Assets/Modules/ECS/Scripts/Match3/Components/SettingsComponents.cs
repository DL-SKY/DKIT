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
    /// Глобальные настройки
    /// </summary>
    public struct Match3GlobalSettingsData
    {
        public long SwapAnimationDurationMs;

        public float GetSwapAnimationDuration()
        {
            return SwapAnimationDurationMs / 1000.0f;
        }
    }
}
