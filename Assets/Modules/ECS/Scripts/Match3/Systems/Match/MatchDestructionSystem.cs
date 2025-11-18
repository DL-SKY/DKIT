using Leopotam.Ecs;
using Modules.ECS.Scripts.Match3.Components;
using System.Collections.Generic;
using UnityEngine;

namespace Modules.ECS.Scripts.Match3.Systems.Match
{
    /// <summary>
    /// Система удаления фишек после обнаружения совпадений.
    /// Обрабатывает MatchGroup, запускает анимацию удаления и удаляет фишки после завершения анимации.
    /// </summary>
    public class MatchDestructionSystem : IEcsRunSystem
    {
        private readonly EcsWorld _world = null;
        private readonly EcsFilter<MatchGroup> _matchGroupFilter = null;
        private readonly EcsFilter<MatchDestructionAnimation> _destructionAnimationFilter = null;
        private readonly EcsFilter<MatchDestructionInProgress> _destructionInProgressFilter = null;
        private readonly EcsFilter<Match3GlobalSettingsData> _globalSettingsFilter = null;
        private readonly EcsFilter<GridPosition, GemView> _gemsFilter = null;

        public void Run()
        {
            // Обрабатываем анимацию удаления, если она идет
            ProcessDestructionAnimation();

            // Если уже идет процесс удаления, не обрабатываем новые совпадения
            if (_destructionInProgressFilter.GetEntitiesCount() > 0)
            {
                return;
            }

            // Проверяем наличие совпадений для обработки
            if (_matchGroupFilter.GetEntitiesCount() == 0)
            {
                return;
            }

            // Собираем все уникальные позиции из всех групп совпадений
            var positionsToDestroy = new HashSet<GridPosition>();
            var matchGroups = new List<MatchGroup>();

            foreach (var i in _matchGroupFilter)
            {
                ref var matchGroup = ref _matchGroupFilter.Get1(i);
                matchGroups.Add(matchGroup);

                // Добавляем все позиции из группы
                if (matchGroup.Positions != null)
                {
                    foreach (var pos in matchGroup.Positions)
                    {
                        positionsToDestroy.Add(pos);
                    }
                }

                // Удаляем компонент MatchGroup после обработки
                _matchGroupFilter.GetEntity(i).Del<MatchGroup>();
            }

            if (positionsToDestroy.Count == 0)
            {
                return;
            }

            // Находим все сущности фишек по координатам
            var entitiesToDestroy = new List<EcsEntity>();
            var viewsToDestroy = new List<GemView>();
            foreach (var i in _gemsFilter)
            {
                ref var gridPosition = ref _gemsFilter.Get1(i);
                if (positionsToDestroy.Contains(gridPosition))
                {
                    entitiesToDestroy.Add(_gemsFilter.GetEntity(i));
                    viewsToDestroy.Add(_gemsFilter.Get2(i));
                }
            }

            if (entitiesToDestroy.Count == 0)
            {
                UnityEngine.Debug.LogWarning("[MatchDestructionSystem] Не найдены сущности для удаления");
                return;
            }

            // Получаем длительность анимации из настроек
            var duration = 0.0f;
            foreach (var j in _globalSettingsFilter)
            {
                ref var settings = ref _globalSettingsFilter.Get1(j);
                duration = settings.GetMatchAnimationDuration();
                break;
            }

            // Создаем состояние удаления
            var destructionInProgressEntity = _world.NewEntity();
            destructionInProgressEntity.Get<MatchDestructionInProgress>();

            // Создаем компонент анимации удаления
            var destructionAnimationEntity = _world.NewEntity();
            destructionAnimationEntity.Get<MatchDestructionAnimation>() = new MatchDestructionAnimation
            {
                Entities = entitiesToDestroy,
                StartTime = Time.time,
                Duration = duration
            };

            // Запускаем у фишек анимацию
            foreach (var j in viewsToDestroy)
            {
                j.GemVisual?.StartDestroyAnimation();
            }

            UnityEngine.Debug.Log($"[MatchDestructionSystem] Запущена анимация удаления {entitiesToDestroy.Count} фишек. Длительность: {duration}с");
        }

        /// <summary>
        /// Обрабатывает анимацию удаления фишек
        /// </summary>
        public void ProcessDestructionAnimation()
        {
            foreach (var i in _destructionAnimationFilter)
            {
                ref var destructionAnimation = ref _destructionAnimationFilter.Get1(i);
                ref var destructionAnimationEntity = ref _destructionAnimationFilter.GetEntity(i);

                // Вычисляем прогресс анимации (0.0 - 1.0)
                float elapsedTime = Time.time - destructionAnimation.StartTime;
                float progress = Mathf.Clamp01(elapsedTime / destructionAnimation.Duration);

                // Если анимация завершена
                if (progress >= 1.0f)
                {
                    // Удаляем GameObject и сущности
                    if (destructionAnimation.Entities != null)
                    {
                        foreach (var entity in destructionAnimation.Entities)
                        {
                            if (entity.IsNull())
                                continue;



                            // Удаляем GameObject, если он существует
                            if (entity.Has<GemView>())
                            {
                                ref var gemView = ref entity.Get<GemView>();
                                if (gemView.GemVisual != null && gemView.GemVisual.gameObject != null)
                                {
                                    Object.Destroy(gemView.GemVisual.gameObject);
                                }
                            }

                            // Удаляем сущность
                            entity.Destroy();
                        }
                    }

                    UnityEngine.Debug.Log($"[MatchDestructionSystem] Удалено {destructionAnimation.Entities?.Count ?? 0} фишек");

                    // Удаляем компонент анимации
                    destructionAnimationEntity.Del<MatchDestructionAnimation>();

                    // Удаляем состояние удаления
                    foreach (var j in _destructionInProgressFilter)
                    {
                        _destructionInProgressFilter.GetEntity(j).Del<MatchDestructionInProgress>();
                    }

                    // Создаем запрос на падение фишек после удаления
                    var fallRequestEntity = _world.NewEntity();
                    fallRequestEntity.Get<FallRequest>();
                    UnityEngine.Debug.Log("[MatchDestructionSystem] Создан запрос на падение фишек");
                }
            }
        }
    }
}

