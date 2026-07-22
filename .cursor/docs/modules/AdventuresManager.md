# AdventuresManager

**Последнее обновление:** 2026-07-22 17:47:15 (+03:00)

## Назначение

`AdventuresManager` — внешний фасад adventure-runtime слоя.

Он:
- создает и владеет `RuntimeSceneData`;
- ретранслирует наружу события изменения runtime-состояния;
- дает публичный доступ к текущему контенту и выборам сцены.

`AdventuresManager` **не работает напрямую** с `AdventureStateLogic.StateChanged` и не содержит своей логики чтения/синхронизации прогресса — это ответственность `RuntimeSceneData`.

## Игровой цикл событий

1. UI вызывает `IChoiceActionExecutor` для выбранного игроком действия.
2. Executor меняет состояние через state-action (`StateActionBase<TStateData>`) и `AdventureStateLogic.ProcessAction(...)`.
3. После успешного `Execute` logic публикует `AdventureStateLogic.StateChanged`.
4. `RuntimeSceneData` помечает runtime как dirty (см. ниже) и на следующем кадре синхронизирует контекст, поднимая события контента/сцены.
5. `AdventuresManager` ретранслирует эти события наружу (`ChangedAdventure`, `ChangedScene`, `ChangedContent`, `ChangedChoices`).
6. Окна и UI-компоненты приключения обновляют интерфейс через `GetCurrentContent()` / `GetCurrentChoices()` (уже с фильтрацией по `Restrictions`).
7. Следующий выбор снова запускает этот цикл.

Так формируется runtime-loop: `Choice -> StateAction -> StateChanged -> (dirty) -> next frame Sync -> AdventuresManager -> UI -> Choice`.

> Оркестрация «клик choice → прогон списка `Actions` через фабрику executors» со стороны UI пока может быть не полностью замкнута; переход по сцене и запись params через executors уже реализованы.

> **UI (Windows):** `AdventureScrollViewModel` подписан на `ChangedContent` / `GetCurrentContent()`, создаёт content VM и через `AdventureScrollView` секвенциально показывает item’ы (`IContentAnimator`, `SkipAllShowAnimation`). Choices UI — ещё впереди. Подробности: [Windows.md](Windows.md) (секция Adventure runtime UI).

## Ключевые классы и роли

- `AdventuresManager` (`Assets/Modules/RPG/Scripts/Adventure/AdventuresManager.cs`)
  - life-cycle: `Init()` / `Dispose()`;
  - создание `RuntimeSceneData` через `DiContainer`;
  - подписка на события `RuntimeSceneData` и ретрансляция своим подписчикам;
  - публичные методы `GetCurrentContent()` и `GetCurrentChoices()` (делегируют в `RuntimeSceneData`).

- `RuntimeSceneData` (`Assets/Modules/RPG/Scripts/Adventure/RuntimeSceneData.cs`)
  - хранит текущие runtime-поля: adventure id, scene id и `AdventureDef`;
  - читает текущий прогресс из `AdventureStateManager.State`;
  - выполняет fallback к стартовым данным из `RuleSettingsDef.StartAdventure`;
  - при необходимости пишет нормализованные значения обратно в state;
  - подписывается на `AdventureStateLogic.StateChanged` и на покадровый `Updater.OnUpdate`;
  - фильтрует content/choices через `RestrictionsChecker`.

## Событийный контракт

`RuntimeSceneData` публикует:
- `event Action<string> ChangedAdventure`
- `event Action<string> ChangedScene`
- `event Action ChangedContent`
- `event Action ChangedChoices`

`AdventuresManager` публикует те же события как внешний API, ретранслируя их своим подписчикам.

### Поведение на старте

В конце `RuntimeSceneData.Init()` вызывается только `ChangedScene`.

Это осознанно: считается, что на запуске системы-потребители сами опросят нужные данные (`GetCurrentContent()`, `GetCurrentChoices()` и т.д.).

### Coalescing `StateChanged` (dirty-flag)

Чтобы пачка синхронных `StateChanged` за один кадр не вызывала многократный rebuild UI:

1. `OnStateChangedHandler` при релевантном `StateChangeSource` только ставит `_isDirty = true` (без немедленного sync).
2. На `Updater.OnUpdate` (проектный покадровый цикл, `Modules.Utils.Scripts.Components.Updater`): если dirty — сбрасывает флаг и вызывает `SyncFromStateAndNotify()`.
3. Эхо от собственной записи в state при нормализации отсекается флагом `_isSyncingState`.

Текущий whitelist источников dirty:

| `StateChangeSource` | Помечает dirty |
|---|---|
| `SetCurrentAdventureId` | да |
| `SetCurrentAdventureSceneId` | да |
| остальные | нет (пока) |

> `SetWorldParams` / `SetAdventureParams` / `SetGlobalParams` пока **не** входят в whitelist. После их записи UI не получит `ChangedContent`/`ChangedChoices` автоматически, пока не сменится adventure/scene id. Расширение whitelist — ожидаемый следующий шаг, когда UI должен реагировать на params без смены сцены.

### Поведение `SyncFromStateAndNotify`

1. Резолв текущей пары adventure/scene (с fallback при необходимости).
2. При нормализации — запись в state через `SetCurrentAdventureIdStateAction` / `SetCurrentAdventureSceneIdStateAction`.
3. Обновление runtime-полей.
4. `ChangedAdventure` — только если изменился adventure id.
5. `ChangedScene` — только если изменился scene id.
6. **Всегда** на flush: `ChangedContent` и `ChangedChoices` (чтобы UI перечитал списки с учётом restrictions).

## Фильтрация content и choices

`GetCurrentContent()` и `GetCurrentChoices()` **не** возвращают сырой список из дефа: элементы проходят через `RestrictionsChecker`.

### Content (`SceneContentData`)

- `Restrictions == null` или пустой список → элемент доступен.
- Иначе → в результат только если `RestrictionsChecker.Check(restrictions) == true`.

### Choices (`ChoiceData`)

- `AlwaysShow == true` → choice попадает в результат **даже если** restrictions не прошли.
- Иначе — та же логика, что у content: пустые restrictions проходят; иначе нужен успешный `Check`.

Зависимости `RuntimeSceneData` (Zenject inject):

- `DefinitionsManager`
- `AdventureStateManager`
- `AdventureStateLogic`
- `Updater`
- `RestrictionsChecker` (уже биндится в Adventure `ProjectInstaller`)

## Алгоритм инициализации RuntimeSceneData

1. Попытка взять `CurrentAdventureId` и `CurrentAdventureSceneId` из state.
2. Если adventure и scene валидны — используется эта пара.
3. Если adventure валиден, а scene невалидна — выбирается случайная стартовая сцена из `AdventureDef.StartScenes` этого adventure.
4. Если adventure невалиден (или отсутствует) — используется `RuleSettingsDef.StartAdventure`, затем случайная стартовая сцена из его `StartScenes`.
5. Если был fallback/нормализация — значения пишутся обратно в state отдельными state-action.
6. Заполняются runtime-поля класса.
7. Подписка на `AdventureStateLogic.StateChanged` и `Updater.OnUpdate`.
8. Вызывается `ChangedScene`.

## Работа с state

Для обновления активной точки используются отдельные action:

- `SetCurrentAdventureIdStateAction`
- `SetCurrentAdventureSceneIdStateAction`

И отдельные источники изменений:

- `StateChangeSource.SetCurrentAdventureId`
- `StateChangeSource.SetCurrentAdventureSceneId`

Также есть защита от реэнтрантности при внутренней синхронизации state (`_isSyncingState`), чтобы избежать циклического вызова собственного обработчика `StateChanged`.

## Публичный API AdventuresManager

- `void Init()`
- `void Dispose()`
- `List<SceneContentData> GetCurrentContent()` — уже отфильтрованный по restrictions список
- `List<ChoiceData> GetCurrentChoices()` — с учётом restrictions и `AlwaysShow`
- события:
  - `ChangedAdventure`
  - `ChangedScene`
  - `ChangedContent`
  - `ChangedChoices`

Если внутренний `RuntimeSceneData` еще не инициализирован, методы чтения возвращают пустые списки.

## Ограничения и допущения

- Выбор стартовой сцены выполняется через `System.Random`.
- Система ожидает, что `DefinitionsManager` и `AdventureStateManager` уже инициализированы до `AdventuresManager.Init()`.
- При невалидных дефах (нет стартового adventure или валидных стартовых сцен) `RuntimeSceneData` логирует ошибку и не завершает корректную инициализацию runtime-контекста.
- Фильтрация `AdventureData.Restrictions` (доступ ко всему приключению) в `RuntimeSceneData` не выполняется — только content/choices текущей сцены.
- Для гейта «в отряде есть герой» используйте `RestrictionType.ActivePartyCount` (`CompareOptions` + `IntValues[0]`, например `MoreEqual` / `1`).
