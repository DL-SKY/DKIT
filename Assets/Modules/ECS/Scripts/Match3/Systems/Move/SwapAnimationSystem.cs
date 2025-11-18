using Leopotam.Ecs;
using Modules.ECS.Scripts.Match3.Components;
using UnityEngine;

namespace Modules.ECS.Scripts.Match3.Systems.Move
{
    /// <summary>
    /// Система анимации свапа фишек.
    /// Плавно перемещает две фишки к их целевым позициям и удаляет состояние ожидания после завершения анимации.
    /// </summary>
    public class SwapAnimationSystem : IEcsRunSystem
    {
        private readonly EcsWorld _world = null;
        private readonly EcsFilter<SwapAnimation> _swapAnimationFilter = null;
        private readonly EcsFilter<SwapInProgress> _swapInProgressFilter = null;

        public void Run()
        {
            foreach (var i in _swapAnimationFilter)
            {
                ref var swapAnimation = ref _swapAnimationFilter.Get1(i);
                ref var swapAnimationEntity = ref _swapAnimationFilter.GetEntity(i);

                // Проверяем, что сущности все еще существуют
                if (swapAnimation.FromEntity.IsNull() || swapAnimation.ToEntity.IsNull())
                {
                    UnityEngine.Debug.LogWarning("[SwapAnimationSystem] Одна из сущностей в анимации свапа не существует");
                    swapAnimationEntity.Del<SwapAnimation>();
                    // Удаляем состояние ожидания свапа
                    foreach (var j in _swapInProgressFilter)
                    {
                        _swapInProgressFilter.GetEntity(j).Del<SwapInProgress>();
                    }
                    continue;
                }

                if (!swapAnimation.FromEntity.Has<GemView>() || !swapAnimation.ToEntity.Has<GemView>())
                {
                    UnityEngine.Debug.LogWarning("[SwapAnimationSystem] Одна из сущностей не имеет компонента GemView");
                    swapAnimationEntity.Del<SwapAnimation>();
                    // Удаляем состояние ожидания свапа
                    foreach (var j in _swapInProgressFilter)
                    {
                        _swapInProgressFilter.GetEntity(j).Del<SwapInProgress>();
                    }
                    continue;
                }

                // Вычисляем прогресс анимации (0.0 - 1.0)
                float elapsedTime = Time.time - swapAnimation.StartTime;
                float progress = Mathf.Clamp01(elapsedTime / swapAnimation.Duration);

                // Используем плавную кривую для анимации (ease-in-out)
                float smoothProgress = SmoothStep(progress);

                // Интерполируем позиции
                ref var fromView = ref swapAnimation.FromEntity.Get<GemView>();
                ref var toView = ref swapAnimation.ToEntity.Get<GemView>();

                if (fromView.GemVisual != null)
                {
                    fromView.GemVisual.transform.position = Vector3.Lerp(
                        swapAnimation.FromStartPosition,
                        swapAnimation.FromTargetPosition,
                        smoothProgress);
                }

                if (toView.GemVisual != null)
                {
                    toView.GemVisual.transform.position = Vector3.Lerp(
                        swapAnimation.ToStartPosition,
                        swapAnimation.ToTargetPosition,
                        smoothProgress);
                }

                // Если анимация завершена
                if (progress >= 1.0f)
                {
                    // Устанавливаем финальные позиции (на случай погрешностей)
                    if (fromView.GemVisual != null)
                    {
                        fromView.GemVisual.transform.position = swapAnimation.FromTargetPosition;
                    }

                    if (toView.GemVisual != null)
                    {
                        toView.GemVisual.transform.position = swapAnimation.ToTargetPosition;
                    }

                    UnityEngine.Debug.Log("[SwapAnimationSystem] Анимация свапа завершена");

                    // Удаляем компонент анимации
                    swapAnimationEntity.Del<SwapAnimation>();

                    // Удаляем состояние ожидания свапа
                    foreach (var j in _swapInProgressFilter)
                    {
                        _swapInProgressFilter.GetEntity(j).Del<SwapInProgress>();
                    }
                }
            }
        }

        /// <summary>
        /// Функция плавного перехода (smoothstep) для более естественной анимации
        /// </summary>
        private float SmoothStep(float t)
        {
            return t * t * (3f - 2f * t);
        }
    }
}

