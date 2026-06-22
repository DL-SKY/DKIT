# Модуль Initializer

## Назначение

`Initializer` управляет стартовой последовательностью приложения: выполняет набор задач загрузки, показывает прогресс и после успешной инициализации завершает запуск выбранной ветки (Match3 или Adventure).

## Краткая логика работы

1. `Initializer.Start()` получает зависимости из `ProjectContext` (менеджеры, фабрики, апдейтер).
2. Создает `MainLoadViewModel`, собирает список `TaskBase` (definitions, state, localization и служебные task-ы).
3. Передает список в `InitializeTasker`, который выполняет задачи строго последовательно.
4. `InitializeTasker` считает суммарный `Weight`, после завершения каждой задачи шлет прогресс (`current/max`).
5. `MainLoadViewModel` подписывается на прогресс и обновляет `MainLoadView` (progress bar).
6. После успеха вызывается `OnCompletedCallback()`:  
   - в Match3-ветке инициализируется `Match3RoundController` и открывается `DefaultMatch3View`;  
   - в Adventure-ветке создаётся `AdventureMainViewModel`, вызывается `Init(scenarioName)` и открывается `AdventureMainView`.

## Основные классы

- `Initializer`  
  Компоновка стартового пайплайна и callback-ов успеха/ошибки.

- `Modules.Initializer.Scripts.Implementation.Adventure.Initializer`  
  Adventure-вариант стартового пайплайна с `AdventureStateInitTask`.

- `InitializeTasker`  
  Оркестратор очереди задач (`Run`, `TryStartTask`, `OnProgressChange`, обработка fail).

- `TaskBase`  
  Базовый контракт задач (`Run`, `Weight`, `Complete()`, `Fail(errorCode)`).

- `DefinitionsInitTask`, `Match3StateInitTask`, `AdventureStateInitTask`, `AdventuresManagerInitTask`, `LocalizationInitTask`  
  Ядро стартовой инициализации данных.

- `AdventuresManagerInitTask`  
  Вызывает `AdventuresManager.Init()` (без параметров). Выполняется после `DefinitionsInitTask` и `AdventureStateInitTask`. На этом этапе менеджер только подписывается на `AdventureStateLogic.StateChanged`.

- `LoadSceneTask`, `PauseTask`, `CloseViewTask`  
  Технические задачи для сцены, пауз/филлеров и закрытия окон.

- `MainLoadViewModel` + `MainLoadView`  
  Отображение прогресса загрузки.

- `AdventureMainViewModel` + `AdventureMainView` (Adventure-ветка)  
  Главный экран приключения после загрузки. VM принимает имя сценария через `Init(string scenarioName)` до открытия View.

## Как добавить новый task загрузки

1. Создать класс задачи, унаследованный от `TaskBase`.
2. Реализовать `Run()` и гарантированно завершать задачу:
   - на успехе вызвать `Complete()`;
   - на ошибке вызвать `Fail(<errorCode>)`.
3. Выбрать адекватный `weight` в конструкторе, чтобы вклад в прогресс отражал реальную длительность.
4. Если задача требует DI-зависимости, создавать ее через `container.Instantiate<NewTask>(new object[] { weight, ... })`.
5. Добавить задачу в список `tasks` в `Initializer.Start()` в правильной позиции по порядку.
6. Проверить сценарии success/fail:
   - прогресс доходит до 100%;
   - при ошибке вызывается `OnFailedCallback` и пайплайн останавливается.

