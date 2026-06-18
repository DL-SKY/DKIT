# Модуль Match3

**Последнее обновление:** 2026-06-18 18:00:00 (+03:00)

## Назначение

`Match3` формирует доменную модель раунда (поле, набор фишек, цели) и запускает игровой цикл через ECS-пайплайн.

## Краткая логика работы

1. `Match3RoundController.Init(roundDefId)` получает `RoundDef` из `DefinitionsManager`.
2. По ссылкам из `RoundDef` подтягиваются связанные дефы: `GameZoneDef`, `GameZoneGemsDef`, `ObjectivesDef`.
3. На основе дефов создаются адаптеры данных:
   - `GameZoneData : IGameZoneData`;
   - `GemsData : IGemsData`;
   - `ObjectivesData : IObjectivesData`.
4. `RoundControllerBase.InitBase(...)` сохраняет эти контракты и вызывает инициализацию реализации.
5. `Match3RoundController.InitImplementation()` поднимает `EcsWorld`/`EcsSystems`, создает и регистрирует системы (инициализация поля, ввод, матчи, спавн, падение, подсчет очков/ходов, проверка конца раунда).
6. На каждом `Updater.OnUpdate` вызывается `_systems.Run()`, а в `Dispose()` корректно уничтожаются ECS-объекты и отписки.

## Основные классы

- `RoundControllerBase`  
  Абстрактная база контроллера раунда: общий lifecycle (`InitBase`, `Subscribe`, `Dispose`) и хранение интерфейсных данных раунда.

- `Match3RoundController`  
  Главный orchestrator Match3-раунда: проверяет наличие нужных дефов, собирает data-слой, конфигурирует ECS-системы и запускает tick-цикл.

- `IGameZoneData`, `IGemsData`, `IObjectivesData`  
  Контракты доступа к конфигурации поля, генерации фишек и условиям раунда.

- `ObjectivesData`  
  Реализация `IObjectivesData` поверх `ObjectivesDef`; предоставляет стартовые счётчики и списки `VictoryConditions` / `DefeatConditions` (`List<Restriction>`) через методы `GetVictoryConditions()` / `GetDefeatConditions()`.

- `GameZoneData`, `GemsData`  
  Реализации интерфейсов поверх def-объектов из `Definitions`.

- `GridPositionHelper`  
  Утилиты для преобразования координат сетки/мира, вычисления центрирования и параметров камеры.

- `GemsHelper`  
  Создание gem-сущности ECS + визуального `GemVisual` из prefab-пути в `GemDef`.

- `GemVisual`, `CellVisual`  
  MonoBehaviour-представление элементов игрового поля.

- `RoundStateType`  
  Состояния раунда (`Loading`, `Win`, `Lose`).

## Добавление новых def для Match3 (явный чек-лист)

1. Добавить или обновить def-файлы в `Definitions`:
   - `GameZones/<NewZone>.json`;
   - `GameZoneGems/<NewGemsSet>.json`;
   - `Objectives/<NewObjectives>.json` с ключами `StartScores`, `VictoryConditions`, `DefeatConditions`;
   - `Rounds/<NewRound>.json` со ссылками на три дефа выше.
2. Убедиться, что имена файлов совпадают с id, которые используются в ссылках `RoundDef`.
3. Проверить, что для всех gem-id из `GameZoneGems` существуют соответствующие `GemDef` и валидные `PrefabPath`.
4. Запустить раунд через `Match3RoundController.Init("<NewRound>")` и проверить:
   - все `TryGetValue` на дефы проходят без ошибок;
   - поле создается с нужной маской;
   - фишки спавнятся из ожидаемого пула;
   - условия победы/поражения применяются корректно.

## Как расширять модуль Match3 кодом

1. Новую игровую механику оформлять отдельной ECS-системой (обычно в `Assets/Modules/ECS/Scripts/Match3/Systems/...`).
2. Добавлять систему в `Match3RoundController.InitImplementation()` в правильную фазу пайплайна (init, processing, move, objectives).
3. Если системе нужны данные раунда, передавать их через `EcsSystemFactory.Create<T>(new object[] { ... })`.
4. Проверять, что новая система не ломает порядок соседних систем и не оставляет неочищенные ECS-события/сущности.
