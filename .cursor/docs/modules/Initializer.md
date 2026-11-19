# Модуль Initializer

**Последнее обновление:** 2026-11-19

## Назначение

`Initializer` управляет стартовой последовательностью Adventure-сборки: выполняет набор задач загрузки, показывает прогресс на прелоадере и после успеха оставляет экран ждать жест игрока. Открытие `AdventureMainView` делает прелоадер, не callback инициализатора.

Живой пайплайн — один класс: `Modules.Initializer.Scripts.Implementation.Adventure.Initializer`. Отдельной Match3-ветки в `Start()` нет. `Match3StateInitTask` и `DefaultMatch3View` в проекте остаются, но текущий `Initializer` их не запускает.

## Краткая логика работы

1. `Initializer.Start()` берёт из `ProjectContext` `WindowsManager`, `Updater`, `CoroutineHolder`, `ViewModelFactory`.
2. Создаёт `AdventureApplicationLoadViewModel` и список `TaskBase`.
3. `InitializeTasker` выполняет задачи строго последовательно и считает суммарный `Weight`.
4. `loaderViewModel.Init(tasker)` подписывает прелоадер на `OnProgressChange`. Окно открывается до `tasker.Run`.
5. После каждой задачи tasker шлёт прогресс `(current, max)`. На `current >= max` прелоадер становится интерактивным.
6. `tasker.Run(OnCompletedCallback, OnFailedCallback)`:
   - успех — `OnCompletedCallback` только логирует; главное окно приключения здесь не открывается;
   - ошибка — `OnFailedCallback(errorCode)`, очередь останавливается.

Порядок задач в `Initializer.Start()` (веса в конструкторе):

| # | Задача | Weight |
|---|---|---|
| 1 | `DefinitionsInitTask` | 10 |
| 2 | `AdventureStateInitTask` | 10 |
| 3 | `LocalizationInitTask` | 10 |
| 4 | `AdventuresManagerInitTask` | 10 |
| 5 | `PauseTask` 1 с | 1 |
| 6 | `PauseTask` 1 с | 1 |
| 7 | `PauseTask` 0.25 с | 0 |

`LoadSceneTask` и `CloseViewTask` в списке закомментированы. Закрытие прелоадера и открытие `AdventureMainView` — в `AdventureApplicationLoadViewModel` после одного `PointerDown` (`MAX_KNOCKS = 1`). `AdventureMainViewModel.Init()` без аргументов. Подробности UI: [Windows.md](Windows.md) (секция прелоадера).

## Основные классы

- `Initializer`  
  Компоновка стартового пайплайна и callback-ов успеха/ошибки.  
  Файл: `Assets/Modules/Initializer/Scripts/Implementation/Adventure/Initializer.cs`.

- `InitializeTasker` (`IInitializer`, `IProgressable`)  
  Очередь задач: `Run`, `TryStartTask`, `OnProgressChange`, остановка на `Fail`.  
  `Dispose` снимает подписки только с текущей незавершённой задачи.

- `TaskBase`  
  Контракт: `Run`, `Weight`, `Complete()`, `Fail(errorCode)`. События `OnComplete` / `OnError`.

- `DefinitionsInitTask`  
  `DefinitionsManager.InitAsync()` через `CoroutineHolder` (Adventure `DefinitionsManager`). По завершении async — `Complete()`.

- `AdventureStateInitTask`  
  `AdventureStateManager.Init(SaveFolder, FileExtension, EncryptionKey)` и `LoadProfileState(SaveName, EnabledEncryption, EncryptionKey)` из `GlobalSettings`. Сразу `Complete()`.

- `LocalizationInitTask`  
  Язык из Adventure state (`ILocalizationStateDataOwner` / `LocalizationStateData.Language`).  
  `Unknown` или отсутствие state → `LocalizationManager.ResolveSystemLanguageOrDefault()`.  
  `TrySetLanguage`; если не вышло — повтор с `GetDefaultLanguage()`. Если и default не ставится, пишется warning и задача всё равно `Complete()` (пайплайн не падает).  
  Успешный язык пишется обратно через `SetLocalizationLanguageStateAction` и `AdventureStateLogic.ProcessAction(..., forceBatch: true)`.  
  `AdventureStateManager` и `AdventureStateLogic` — `[InjectOptional]`.

- `AdventuresManagerInitTask`  
  `AdventuresManager.Init()` без параметров, после definitions и state.  
  Менеджер создаёт `RuntimeSceneData`, подписывается на него и вызывает `RuntimeSceneData.Init()` (резолв текущей сцены, в конце — `ChangedScene`). Это не подписка менеджера напрямую на `AdventureStateLogic.StateChanged`.

- `PauseTask`, `LoadSceneTask`, `CloseViewTask`, `LogTask`  
  Пауза через `Updater`, загрузка сцены, закрытие окна, лог. В живом списке Adventure используются только `PauseTask`.

- `MainLoadView` / `MainLoadViewModel`  
  Более простой прогресс-бар (`SortingOrderLayer.PRELOADER`, `HideInHistory = false`). Текущий Adventure-`Initializer` его не открывает.

- `AdventureApplicationLoadView` / `AdventureApplicationLoadViewModel`  
  Экран загрузки Adventure. Прогресс, дверь и подсказка `KNOCK_TO_CONTINUE`, затем переход на `AdventureMainView`.

## Как добавить новый task загрузки

1. Создать класс задачи, унаследованный от `TaskBase`.
2. Реализовать `Run()` и гарантированно завершать задачу:
   - на успехе вызвать `Complete()`;
   - на ошибке вызвать `Fail(<errorCode>)`.
3. Выбрать адекватный `weight` в конструкторе, чтобы вклад в прогресс отражал реальную длительность. `Weight == 0` не двигает бар.
4. Если задача требует DI-зависимости, создавать ее через `container.Instantiate<NewTask>(new object[] { weight, ... })`.
5. Добавить задачу в список `tasks` в `Initializer.Start()` в правильной позиции по порядку.
6. Проверить сценарии success/fail:
   - прогресс доходит до 100% и прелоадер становится кликабельным;
   - при `Fail` вызывается `OnFailedCallback` и пайплайн останавливается.
