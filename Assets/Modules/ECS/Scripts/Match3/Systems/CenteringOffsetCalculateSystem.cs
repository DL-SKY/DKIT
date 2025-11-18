using Leopotam.Ecs;
using Modules.ECS.Scripts.Match3.Components;
using Modules.Match3.Scripts.Helpers;
using Modules.Match3.Scripts.Interfaces;

namespace Modules.ECS.Scripts.Match3.Systems
{
    /// <summary>
    /// Система инициализации, вычисляющая смещение для центрирования игрового поля относительно камеры.
    /// Выполняется при старте игры и сохраняет вычисленное смещение в компоненте CenterOffsetData,
    /// который затем используется другими системами для корректного позиционирования элементов игрового поля.
    /// </summary>
    public class CenteringOffsetCalculateSystem : IEcsInitSystem
    {
        private readonly EcsWorld _world = null;

        private readonly IGameZoneData _gameZoneData;

        public CenteringOffsetCalculateSystem(IGameZoneData gameZoneData)
        {
            _gameZoneData = gameZoneData;
        }

        public void Init()
        {
            if (_gameZoneData == null)
            {
                UnityEngine.Debug.LogError($"[CenteringOffsetCalculateSystem] GameZoneData is null!");
                return;
            }

            var mask = _gameZoneData.GetMask();
            if (mask == null)
            {
                UnityEngine.Debug.LogError($"[CenteringOffsetCalculateSystem] Mask is null!");
                return;
            }

            // Вычисляем смещение для центрирования поля относительно камеры
            var centeringOffset = GridPositionHelper.CalculateCenteringOffset(mask);

            var entity = _world.NewEntity();
            entity.Get<CenterOffsetData>();
            entity.Get<CenterOffsetData>() = new CenterOffsetData { Offset = centeringOffset };
        }
    }
}
