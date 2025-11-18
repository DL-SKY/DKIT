using Leopotam.Ecs;
using Modules.ECS.Scripts.Match3.Components;
using Modules.Match3.Scripts.Helpers;
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
                        _swapInProgressFilter.GetEntity(j).Del<SwapInProgress>();

                    continue;
                }

                if (!swapAnimation.FromEntity.Has<GemView>() || !swapAnimation.ToEntity.Has<GemView>())
                {
                    UnityEngine.Debug.LogWarning("[SwapAnimationSystem] Одна из сущностей не имеет компонента GemView");
                    swapAnimationEntity.Del<SwapAnimation>();
                    // Удаляем состояние ожидания свапа
                    foreach (var j in _swapInProgressFilter)
                        _swapInProgressFilter.GetEntity(j).Del<SwapInProgress>();

                    continue;
                }

                // Вычисляем прогресс анимации (0.0 - 1.0)
                float elapsedTime = Time.time - swapAnimation.StartTime;
                float progress = Mathf.Clamp01(elapsedTime / swapAnimation.Duration);

                // Используем плавную кривую для анимации (ease-in-out)
                float smoothProgress = MoveHelper.SmoothStep(progress);

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

                    // Переименовываем фишки в соответствии с новыми координатами
                    if (swapAnimation.FromEntity.Has<GridPosition>() && swapAnimation.ToEntity.Has<GridPosition>())
                    {
                        ref var fromPosition = ref swapAnimation.FromEntity.Get<GridPosition>();
                        if (fromView.GemVisual != null)
                            fromView.GemVisual.name = GemsHelper.GenerateGemName(fromPosition.X, fromPosition.Y);

                        ref var toPosition = ref swapAnimation.ToEntity.Get<GridPosition>();
                        if (toView.GemVisual != null)
                            toView.GemVisual.name = GemsHelper.GenerateGemName(toPosition.X, toPosition.Y);
                    }

                    UnityEngine.Debug.Log("[SwapAnimationSystem] Анимация свапа завершена");

                    // Удаляем компонент анимации
                    swapAnimationEntity.Del<SwapAnimation>();

                    // Удаляем состояние ожидания свапа
                    foreach (var j in _swapInProgressFilter)
                        _swapInProgressFilter.GetEntity(j).Del<SwapInProgress>();

                    // Создаем запрос на проверку совпадений после завершения свапа
                    var checkMatchesEntity = _world.NewEntity();
                    checkMatchesEntity.Get<CheckMatchesRequest>();
                    UnityEngine.Debug.Log("[SwapAnimationSystem] Создан запрос на проверку совпадений");
                }
            }
        }
    }
}

