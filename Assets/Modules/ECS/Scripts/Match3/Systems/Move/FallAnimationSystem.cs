using Leopotam.Ecs;
using Modules.ECS.Scripts.Match3.Components;
using Modules.Match3.Scripts.Helpers;
using UnityEngine;

namespace Modules.ECS.Scripts.Match3.Systems.Move
{
    /// <summary>
    /// Система анимации падения фишек.
    /// Плавно перемещает фишки к их целевым позициям и удаляет состояние ожидания после завершения анимации.
    /// </summary>
    public class FallAnimationSystem : IEcsRunSystem
    {
        private readonly EcsWorld _world = null;
        private readonly EcsFilter<FallAnimation> _fallAnimationFilter = null;
        private readonly EcsFilter<FallInProgress> _fallInProgressFilter = null;

        public void Run()
        {
            foreach (var i in _fallAnimationFilter)
            {
                ref var fallAnimation = ref _fallAnimationFilter.Get1(i);
                ref var fallAnimationEntity = ref _fallAnimationFilter.GetEntity(i);

                // Проверяем, что сущность все еще существует
                if (fallAnimation.Entity.IsNull())
                {
                    UnityEngine.Debug.LogWarning("[FallAnimationSystem] Сущность в анимации падения не существует");
                    fallAnimationEntity.Del<FallAnimation>();
                    continue;
                }

                if (!fallAnimation.Entity.Has<GemView>())
                {
                    UnityEngine.Debug.LogWarning("[FallAnimationSystem] Сущность не имеет компонента GemView");
                    fallAnimationEntity.Del<FallAnimation>();
                    continue;
                }

                // Вычисляем прогресс анимации (0.0 - 1.0)
                float elapsedTime = Time.time - fallAnimation.StartTime;
                float progress = Mathf.Clamp01(elapsedTime / fallAnimation.Duration);

                // Используем плавную кривую для анимации (ease-in-out)
                float smoothProgress = MoveHelper.SmoothStep(progress);

                // Интерполируем позицию
                ref var gemView = ref fallAnimation.Entity.Get<GemView>();

                if (gemView.GemVisual != null)
                {
                    gemView.GemVisual.transform.position = Vector3.Lerp(
                        fallAnimation.StartPosition,
                        fallAnimation.TargetPosition,
                        smoothProgress);
                }

                // Если анимация завершена
                if (progress >= 1.0f)
                {
                    // Устанавливаем финальную позицию (на случай погрешностей)
                    if (gemView.GemVisual != null)
                    {
                        gemView.GemVisual.transform.position = fallAnimation.TargetPosition;
                    }

                    // Переименовываем фишку в соответствии с новыми координатами
                    if (fallAnimation.Entity.Has<GridPosition>())
                    {
                        ref var gridPosition = ref fallAnimation.Entity.Get<GridPosition>();
                        if (gemView.GemVisual != null)
                            gemView.GemVisual.name = GemsHelper.GenerateGemName(gridPosition.X, gridPosition.Y);
                    }

                    UnityEngine.Debug.Log($"[FallAnimationSystem] Анимация падения завершена: ({fallAnimation.StartGridPosition.X}, {fallAnimation.StartGridPosition.Y}) -> ({fallAnimation.TargetGridPosition.X}, {fallAnimation.TargetGridPosition.Y})");

                    // Удаляем компонент анимации
                    fallAnimationEntity.Del<FallAnimation>();
                }
            }

            // Если все анимации завершены, удаляем состояние падения
            if (_fallAnimationFilter.GetEntitiesCount() == 0 && _fallInProgressFilter.GetEntitiesCount() > 0)
            {
                foreach (var j in _fallInProgressFilter)
                {
                    _fallInProgressFilter.GetEntity(j).Del<FallInProgress>();
                }

                // Создаем запрос на проверку совпадений после завершения падения
                var checkMatchesEntity = _world.NewEntity();
                checkMatchesEntity.Get<CheckMatchesRequest>();
                UnityEngine.Debug.Log("[FallAnimationSystem] Создан запрос на проверку совпадений после падения");
            }
        }
    }
}

