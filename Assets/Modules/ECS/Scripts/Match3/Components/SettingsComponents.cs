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
        private const float DEFAULT_DURATION = 0.1f;

        public long SwapAnimationDurationMs;
        public long MatchAnimationDurationMs;
        public long PauseAfterGemDestroyMs;
        public long FallAnimationDurationMs;


        public float GetSwapAnimationDuration()
        {
            return ConvertMsecToSec(SwapAnimationDurationMs);
        }

        public float GetMatchAnimationDuration()
        {
            return ConvertMsecToSec(MatchAnimationDurationMs);
        }

        public float GetPauseAfterGemDestroy()
        { 
            return ConvertMsecToSec(PauseAfterGemDestroyMs);
        }

        public float GetFallAnimationDuration()
        {
            return ConvertMsecToSec(FallAnimationDurationMs);
        }


        private float ConvertMsecToSec(long ms)
        {
            return Mathf.Max(ms / 1000.0f, DEFAULT_DURATION);
        }
    }
}
