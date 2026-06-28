# AdventuresManager

**Последнее обновление:** 2026-06-28

## Назначение

`AdventuresManager` — внешний фасад adventure-runtime слоя.

Он:
- создает и владеет `RuntimeSceneData`;
- ретранслирует наружу события изменения runtime-состояния;
- дает публичный доступ к текущему контенту и выборам сцены.

`AdventuresManager` **не работает напрямую** с `AdventureStateLogic.StateChanged` и не содержит своей логики чтения/синхронизации прогресса — это ответственность `RuntimeSceneData`.

## Ключевые классы и роли

- `AdventuresManager` (`Assets/Modules/RPG/Scripts/Adventure/AdventuresManager.cs`)
  - life-cycle: `Init()` / `Dispose()`;
  - создание `RuntimeSceneData` через `DiContainer`;
  - подписка на события `RuntimeSceneData` и ретрансляция своим подписчикам;
  - публичные методы `GetCurrentContent()` и `GetCurrentChoices()`.

- `RuntimeSceneData` (`Assets/Modules/RPG/Scripts/Adventure/RuntimeSceneData.cs`)
  - хранит текущие runtime-поля: adventure id, scene id и `AdventureDef`;
  - читает текущий прогресс из `AdventureStateManager.State`;
  - выполняет fallback к стартовым данным из `RuleSettingsDef.StartAdventure`;
  - при необходимости пишет нормализованные значения обратно в state;
  - подписывается на `AdventureStateLogic.StateChanged` и обновляет runtime-контекст.

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

## Алгоритм инициализации RuntimeSceneData

1. Попытка взять `CurrentAdventureId` и `CurrentAdventureSceneId` из state.
2. Если adventure и scene валидны — используется эта пара.
3. Если adventure валиден, а scene невалидна — выбирается случайная стартовая сцена из `AdventureDef.StartScenes` этого adventure.
4. Если adventure невалиден (или отсутствует) — используется `RuleSettingsDef.StartAdventure`, затем случайная стартовая сцена из его `StartScenes`.
5. Если был fallback/нормализация — значения пишутся обратно в state отдельными state-action.
6. Заполняются runtime-поля класса.
7. Выполняется подписка на `AdventureStateLogic.StateChanged`.
8. Вызывается `ChangedScene`.

## Работа с state

Для обновления активной точки используются отдельные action:
- `SetCurrentAdventureIdStateAction`
- `SetCurrentAdventureSceneIdStateAction`

И отдельные источники изменений:
- `StateChangeSource.SetCurrentAdventureId`
- `StateChangeSource.SetCurrentAdventureSceneId`

`RuntimeSceneData` обрабатывает только выбранные `StateChangeSource` через `switch`.

Также есть защита от реэнтрантности при внутренней синхронизации state, чтобы избежать циклического вызова собственного обработчика `StateChanged`.

## Публичный API AdventuresManager

- `void Init()`
- `void Dispose()`
- `List<SceneContentData> GetCurrentContent()`
- `List<ChoiceData> GetCurrentChoices()`
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

