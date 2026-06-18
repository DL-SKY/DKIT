# Модуль ECS

**Последнее обновление:** 2026-06-18 18:00:00 (+03:00)

## Назначение

`ECS` — модуль с компонентами и системами на `Leopotam.Ecs`, который реализует игровой цикл Match-3 (инициализация поля, ввод, матчи, разрушение, спавн, падение, цели раунда).

## Краткая логика работы

1. `Match3RoundController` подготавливает данные раунда (зона, фишки, цели) и создает `EcsWorld` + `EcsSystems`.
2. Системы создаются через `EcsSystemFactory` (Zenject) и добавляются в пайплайн в строгом порядке.
3. Init-системы (`IEcsInitSystem`) поднимают стартовое состояние: настройки, камеру, клетки, фишки, счетчики целей.
4. Run-системы (`IEcsRunSystem`) на каждом тике обрабатывают ввод/перетаскивание, свап, поиск матчей, экшены, очки/ходы и проверку условий конца раунда.
5. В `Dispose()` контроллер уничтожает `EcsSystems` и `EcsWorld`, очищая ECS-состояние.

## Основные классы

- `Match3RoundController` (в модуле Match3)  
  Точка сборки ECS-пайплайна: создает мир, инстанцирует системы, подписывает `Run()` на `Updater`.

- `EcsSystemFactory`  
  Фабрика создания ECS-систем через `DiContainer.Instantiate<T>()`.

- `Match3GlobalSettingsSystem`, `CenteringOffsetCalculateSystem`, `CameraSetupSystem`  
  Системы подготовительного слоя (глобальные настройки Match-3, смещение поля, настройка камеры).

- `CellsInitSystem`, `GemsInitSystem`, `TurnsInitSystem`, `ScoreInitSystem`, `RoundConditionsInitSystem`  
  Системы начального наполнения ECS-компонентов. `RoundConditionsInitSystem` копирует `VictoryConditions` / `DefeatConditions` из `IObjectivesData` в `RoundEndConditionsData`.

- `RoundEndConditionsData` (`RoundEndComponents.cs`)  
  Хранит копии `List<Restriction>` для победы (`Victory`) и поражения (`Defeat`); источник — `ObjectivesDef` через data-слой Match3.

- `DragStartSystem`, `DragEndSystem`, `SwapSystem`, `SwapAnimationSystem`, `MatchDetectionSystem`, `MatchDestructionSystem`, `GemsSpawnSystem`, `FallSystem`, `FallAnimationSystem`  
  Игровой цикл хода: от пользовательского ввода до стабилизации поля после матчей.

- `ActionProcessSystem`, `MatchScoreRequestProcessSystem`, `TurnsSystem`, `ScoreSystem`, `RoundConditionsCheckSystem`, `CallbackSystem`  
  Слой обработки событий, начислений и проверки победы/поражения.

- `Components/*` (например, `GemComponents`, `MoveComponents`, `ObjectivesComponents`)  
  Набор ECS-компонентов/тегов, которыми обмениваются системы.

## Как добавить новую ECS-систему

1. Создать класс системы в `Assets/Modules/ECS/Scripts/Match3/Systems/<Group>/<NewSystem>.cs`.
2. Реализовать `IEcsInitSystem` или `IEcsRunSystem` и добавить нужные фильтры/внедрение зависимостей.
3. Если системе нужны параметры конструктора, передавать их через `EcsSystemFactory.Create<T>(new object[] { ... })`.
4. Зарегистрировать систему в `Match3RoundController.InitImplementation()`:
   - создать инстанс через `_ecsSystemFactory.Create<NewSystem>(...)`;
   - добавить в `_systems.Add(...)` в корректное место по порядку выполнения.
5. Проверить жизненный цикл:
   - система должна корректно отрабатывать на `Run()`;
   - не ломать порядок соседних систем;
   - не оставлять мусорных сущностей/событий.

