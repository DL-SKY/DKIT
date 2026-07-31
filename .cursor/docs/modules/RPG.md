# Модуль RPG

**Последнее обновление:** 2026-07-31 12:45:00 (+03:00)

## Назначение

`RPG` — доменный каркас для adventure-геймплея в формате сцен и выборов.  
Модуль задает:

- структуру данных приключения (`AdventureData` — доменный контракт в `RPG`; в рантайме загружается как `AdventureDef` из модуля `Definitions`);
- структуру сцены и ее контента (`SceneData`, `SceneContentData`);
- структуру выбора игрока и набора действий (`ChoiceData`, `ChoiceActionData`).

На текущем этапе модуль содержит **data-contract слой** и начальный **runtime-слой выполнения действий** (`Choice/Executors`) для переходов по сценам и записи параметров `World/Adventure` в `State`.

> **Связь с `Modules.Definitions`:** JSON-дефы adventure-проекта загружаются через `DefinitionsManager` (Adventures). `AdventureDef` — единственный деф, который наследует RPG-модель (`AdventureData`); классы, ancestry, предыстории, черты, предметы и заклинания описаны как `ClassDef` / `AncestryDef` / `BackgroundDef` / `FeatDef` / `ItemDef` / `SpellDef` и наследуют `AbstractDefinition` напрямую. Контракт дефов расширяется (`Restrictions`, `Icon`, `Features`, `AncestryDef.Names`, single-каталог `AvatarsDef` и др.); применение механик в state — в планах. Подробности — в [Definitions.md](Definitions.md#adventure-дефы-персонажа-текущий-контракт-и-эволюция).
>
> **Связь с `Modules.State`:** персистентный прогресс игрока (сейв профиля) живёт в модуле `State` (`AdventureStateManager`, `AdventureStateLogic`, state-actions). Изменения прогресса из choice-executors проходят через `AdventureStateLogic.ProcessAction(...)`, а не напрямую в `StateData`.
>
> **Инвариант choice → state:** действия из выборов игрока в приключениях (`IChoiceActionExecutor`) могут вызывать только state-actions, мутирующие секции **`Characters` / `Inventory` / `Adventures`**. Секции `Profile`, `Wallet`, `Localization` через choice-pipeline **запрещены** (их меняют UI/init/cheats и отдельные meta-экшены вне adventure-choices). См. ниже «Инвариант секций State для choice-actions».
>
> Персонажи, отряд, инвентарь и прогресс приключений хранятся в `Modules.State` (`CharactersStateData`, `InventoryStateData`, `AdventuresStateData`). Подробности — в [документации модуля State](State.md).
>
> **Бой:** runtime пошагового боя ещё не начат. Ранний прототип (дефы действий, economy стороны, MAP, таргетинг) — в [Battle.md](Battle.md).

## Структура модуля

- `Assets/Modules/RPG/Scripts/Adventure`
  - `AdventuresManager` — оркестратор приключения (стартовая инициализация, реакция на изменения state). Реализует `IDisposable`.
  - `IAdventureFlowController` — интерфейс контроллера переходов между сценами/узлами (**устаревший**, `[Obsolete]`).
  - `Data/*` — модели adventure/scene/content.
  - `Choice/*` — модели выбора и действий по выбору.
  - `Choice/Executors/*` — фабрика и обработчики `ChoiceActionData` (executors пишут в `Modules.State` через state-actions).

## Модель данных Adventure

### `AdventureData`

Описывает целое приключение (контентный JSON, модуль `RPG`). В `Resources/Definitions` десериализуется в `AdventureDef` (`AdventureDef : AdventureData`).

- `Id` — уникальный идентификатор adventure.
- `Tags` — набор тегов для фильтрации/поиска/категоризации.
- `Type` — тип узла (`AdventureType`: `Adventure`, `Chapter`, `Location`).
- `IsRepeatable` — можно ли повторно проходить узел; для `Location` — `true`, для `Adventure` и `Chapter` — `false`.
- `AdventureLinks` — связи с другими adventure-узлами (для карты/иерархии).
- `Title`, `Description` — метаданные и текстовое описание.
- `IgnoredTags` — теги, игнорируемые в контексте этого adventure.
- `Restrictions` — список ограничений на доступ к приключению (тип `Restriction` из модуля `Restrictions`).
- `StartScenes` — список стартовых сцен (по `Scene.Id`).
- `Scenes` — словарь `sceneId -> SceneData` со всем графом сцен.

> Не путать с `Modules.State...AdventureStateData` — это **прогресс** конкретного приключения в сейве (`AdventureId`, `SceneId`, `Parameters`).

### `AdventureType`

| Значение | Код | `IsRepeatable` (шаблон TEA) | Назначение |
|---|---|---|---|
| `Adventure` | `0` | `false` | Корневое приключение |
| `Chapter` | `1` | `false` | Глава внутри приключения |
| `Location` | `10` | `true` | Повторяемая локация на карте |

В TEA (`AdventureCreateOptionsRegistry`) для каждого типа есть кнопка в блоке `Create Adventure`: `adventure.default`, `adventure.chapter`, `adventure.location` (см. `Assets/Modules/Definitions/Scripts/Editor/Adventures/README.md`).

### `SceneData`

Описывает отдельную сцену в приключении:

- `Id` — идентификатор сцены.
- `Tags` — служебные/геймдизайнерские теги сцены.
- `Content` — последовательность контент-элементов (`SceneContentData`).
- `NotClearScene` — флаг особого поведения после показа/прохождения (из названия следует сценарий "не очищать сцену"). В TEA при создании новой сцены (и стартовой сцены в шаблоне adventure) по умолчанию `true`.
- `Choices` — список доступных выборов (`ChoiceData`) для перехода/действий.

### `SceneContentData` и `SceneContentType`

Оба типа объявлены в `SceneContentData.cs`.

- `SceneContentData.Type` — тип контентного элемента (`SceneContentType`).
- `SceneContentData.Restrictions` — список ограничений видимости элемента (тип `Restriction` из модуля `Restrictions`).
- `SceneContentData.Value` — одиночный payload (текст, путь или URL изображения, id ресурса и т.д.).
- `SceneContentData.Values` — список строк для типов с несколькими значениями (`RandomImage`, `Slideshow`).

`SceneContentType`:

| Значение | Код | Поле данных | Назначение |
|---|---|---|---|
| `Text` | `0` | `Value` | Текстовый блок |
| `Image` | `10` | `Value` | Одно изображение (путь или URL) |
| `RandomImage` | `11` | `Values` | Случайное изображение из списка путей/URL |
| `Slideshow` | `12` | `Values` | Слайдшоу из списка путей/URL |
| `Splitter` | `20` | — | Разделитель / визуальный отступ |
| `Item` | `30` | `Value` | Элемент предмета / иконки |

Проверка `Restrictions` для контента сцены подключена в runtime: `RuntimeSceneData.GetCurrentContent()` возвращает только элементы, прошедшие `RestrictionsChecker` (пустой/`null` список restrictions считается пройденным). Подробнее — [AdventuresManager.md](AdventuresManager.md#фильтрация-content-и-choices).

## Модель данных Choice

### `ChoiceData`

Описывает вариант действия игрока в сцене:

- `Id` — идентификатор выбора.
- `Tags` — теги для аналитики/фильтрации/UI.
- `Type` — тип выбора (`ChoiceType`): `Default` (`0`) или `DiceCheck` (`1`).
- `VisualOptions` — опциональный блок визуальных настроек (`VisualOptions`): иконки и override описания через id параметра.
- `Text`, `Description` — основной и дополнительный тексты выбора (локализационные ключи / текст).
- `AlwaysShow` — всегда включать выбор в `GetCurrentChoices()`, даже если `Restrictions` не прошли. В TEA при создании нового choice по умолчанию `false`.
- `Restrictions` — список ограничений доступности выбора; проверяются в `RuntimeSceneData.GetCurrentChoices()` через `RestrictionsChecker`.
- `DiceCheck` — опциональный блок параметров броска и outcome actions (используется при `Type == DiceCheck`).
- `Actions` — список действий (`ChoiceActionData`), выполняемых при выборе (используется при `Type == Default`).

### `VisualOptions`

Опциональный блок визуальных настроек выбора (`ChoiceData.VisualOptions`):

- `MainIcon` — Resources-path основной иконки выбора; пустая строка — дефолт UI по `ChoiceType` (см. `AdventureChoiceViewModelBase`).
- `DescriptionIcon` — Resources-path иконки описания; используется, если `ParameterOverrideDescription` пуст.
- `ParameterOverrideDescription` — **id параметра** персонажа (например `"Thievery"`, `"STR"`, `"Perception"`), не локализуемый текст. Если задан:
  - описание и иконка берутся из `VisualSettingsDef` (`ParameterLocalizations["{param}.TextParams"]`, `ParameterIcons[param]`);
  - числовое значение параметра активного персонажа (через `CharacterParametersProxy`) уходит в UI как format-arg (`DescriptionParam`).
  Пустая строка — без override: UI использует `ChoiceData.Description` и `DescriptionIcon`.

В TEA блок редактируется в секции `Visual Options` у `Selected Choice`. Шаблоны `choice.default` / `choice.dice_check` создают пустой блок. При duplicate сцены блок копируется. Runtime-резолв: [Windows.md](Windows.md) (секция Choice items).

### `ChoiceType`

| Значение | Код | Назначение |
|---|---|---|
| `Default` | `0` | Стандартный выбор |
| `DiceCheck` | `1` | Выбор-проверка кубика с опциональным блоком `ChoiceData.DiceCheck` |

> Ранее в enum была опечатка `Dafault`; в JSON и коде используется `Default`.

Блок `ChoiceData.DiceCheck` (`ChoiceDiceCheckData`, опциональный) содержит:
- `DifficultyClass` — СЛ проверки;
- `DiceType` (`Modules.Dices.Scripts.DiceType`) — тип кубика;
- `DiceOptions` (`Modules.Dices.Scripts.DiceOptions`) — флаги броска;
- `DiceCheckParam` — строковый ключ проверяемого атрибута/скилла (например, `"Thievery"`); runtime будет использовать его для выбора модификатора персонажа;
- `OnCriticalSuccess` / `OnSuccess` / `OnFailure` / `OnCriticalFailure` — списки `ChoiceActionData` для соответствующих исходов проверки.

Правила TEA-валидации для `ChoiceType`:

| `ChoiceType` | Требование | `Fix` |
|---|---|---|
| `Default` | `DiceCheck == null` | обнулить блок `DiceCheck` |
| `DiceCheck` | `DiceCheck != null`, `DifficultyClass >= 0` | создать шаблон / сбросить отрицательный DC |
| `DiceCheck` | `Actions == null` или пустой | очистить `Actions` |

Пример JSON:

```json
{
  "Id": "pick_lock",
  "Type": "DiceCheck",
  "Text": "Взломать замок",
  "VisualOptions": {
    "MainIcon": "Adventures/Icons/T_086",
    "DescriptionIcon": "",
    "ParameterOverrideDescription": "Thievery"
  },
  "DiceCheck": {
    "DifficultyClass": 18,
    "DiceType": "D20",
    "DiceOptions": "None",
    "DiceCheckParam": "Thievery",
    "OnCriticalSuccess": [ { "Type": "GoToScene", "Params": { "Strings": { "SceneId": "lock_open_fast" } } } ],
    "OnSuccess":         [ { "Type": "GoToScene", "Params": { "Strings": { "SceneId": "lock_open" } } } ],
    "OnFailure":         [ { "Type": "SetAdventureParams", "Params": { "Ints": { "adventure.lock_attempts": 1 } } } ],
    "OnCriticalFailure": [ { "Type": "GoToScene", "Params": { "Strings": { "SceneId": "trap_triggered" } } } ]
  },
  "Actions": []
}
```

> Runtime-обработка `ChoiceType.DiceCheck` (окно броска, расчёт исхода по СЛ, выполнение outcome actions) пока не реализована; контракт данных и TEA-редактор готовы.

### `ChoiceActionData` и `ChoiceActionType`

- `ChoiceActionData.Type` — тип действия при выборе.
- `ChoiceActionData.Params` — именованные параметры действия через типизированные словари:
  - `Params.Strings: Dictionary<string, string>`
  - `Params.Ints: Dictionary<string, int>`
  - `Params.Bools: Dictionary<string, bool>`
- `ChoiceActionType` — enum действий (в процессе переработки, см. ниже).

> В TEA (`AdventureEditorFileRepository`) enum-поля adventure JSON сохраняются строками и читаются в mixed-режиме (строки + legacy-числа).

Текущее состояние `ChoiceActionType`:

| Значение | Код | Статус |
|---|---|---|
| `None` | `0` | Зарезервирован; в фабрике не используется |
| `GoToScene` | `1` | Подключён к `ChoiceActionExecutorFactory` (переход на сцену) |
| `SetWorldParams` | `2` | Подключён к `ChoiceActionExecutorFactory` (запись `Params` в `AdventuresStateData.World.Parameters`) |
| `SetAdventureParams` | `3` | Подключён к `ChoiceActionExecutorFactory` (запись `Params` в `AdventuresStateData.Adventures[currentAdventureId].Parameters`) |
| `SetGlobalParams` | `4` | Подключён к `ChoiceActionExecutorFactory` (запись `Params` в `AdventuresStateData.Global.Parameters`) |
| `GoToAdventure` | `5` | Подключён: `SetCurrentAdventureIdStateAction` (с очисткой `CurrentAdventureSceneId`); стартовую сцену выбирает `RuntimeSceneData` |
| `OpenWindow` | `6` | Stub executor: логирует `WindowId`; открытие окон UI ещё не подключено |
| `GoToRandomAdventure` | `7` | Подключён: фильтр кандидатов (без `HUB`, не текущее, не `Disabled`; TODO: completed/level) → случайный id → `SetCurrentAdventureIdStateAction` |
| `GoToRandomScene` | `8` | Подключён: `Params.Strings[SceneId]` — список id через `;` → случайная сцена → `SetCurrentAdventureSceneIdStateAction` |
| `SetFlag`, `ModifyVariable`, `SkillCheck`, `StartCombat`, `ApplyDamage`, `Heal`, `GrantItem` | `110`–`500` | Временно закомментированы в enum (старый черновик enum) |

Именованные ключи `Params.Strings` для choice-actions задаются в `Glossary.ChoiceActions` (`Modules.Definitions.Scripts.Implementation.Adventures.Constants`):

| Константа | Значение | Назначение |
|---|---|---|
| `Glossary.ChoiceActions.SCENE_ID` | `"SceneId"` | Id целевой сцены для перехода (`ChoiceActionType.GoToScene`); для `GoToRandomScene` — список id через `SCENE_IDS_SEPARATOR` (`;`) |
| `Glossary.ChoiceActions.ADVENTURE_ID` | `"AdventureId"` | Id целевого приключения (`ChoiceActionType.GoToAdventure`) |
| `Glossary.ChoiceActions.WINDOW_ID` | `"WindowId"` | Id окна UI (`ChoiceActionType.OpenWindow`); константы в `Glossary.Windows` |
| `Glossary.ChoiceActions.SCENE_IDS_SEPARATOR` | `";"` | Разделитель id сцен в `Params.Strings[SceneId]` для `GoToRandomScene` |
| `Glossary.Adventures.HUB` | `"HUB"` | Тег хаба; исключается из кандидатов `GoToRandomAdventure` |

Для `SetWorldParams` / `SetAdventureParams` / `SetGlobalParams` ключи `Params` произвольные (рекомендуется префикс `world.*` / `adventure.*` / `global.*`). TEA валидирует только наличие хотя бы одного param; конкретные ключи не фиксируются в `Glossary`.

Примеры JSON для choice-actions:

```json
{
  "Type": "SetWorldParams",
  "Params": {
    "Strings": { "world.last_location": "tavern" },
    "Ints": {},
    "Bools": { "world.tavern_unlocked": true }
  }
}
```

```json
{
  "Type": "SetAdventureParams",
  "Params": {
    "Strings": {},
    "Ints": { "adventure.quest_stage": 2 },
    "Bools": { "adventure.met_innkeeper": true }
  }
}
```

```json
{
  "Type": "SetGlobalParams",
  "Params": {
    "Strings": {},
    "Ints": { "global.tavern_visits": 1, "global.game_launches": 5 },
    "Bools": {}
  }
}
```

Формат с `Params` сохраняет гибкость, но убирает "позиционные" ошибки (`StringValues[0]`, `IntValues[1]`) и делает JSON-контент более читаемым.

Примеры JSON и псевдокод интерпретации `Params` приведены в комментариях класса `ChoiceActionData`.

## Выполнение действий (ChoiceAction Executors)

Для выполнения `ChoiceActionData` используется паттерн **pre-bound command**:

1. `IChoiceActionExecutorFactory.Create(ChoiceActionData)` получает DTO действия.
2. `ChoiceActionExecutorFactory` (через `DiContainer`) парсит `Params`, валидирует обязательные ключи и создает конкретный executor.
3. Executor в конструкторе получает уже распарсенные данные и ссылки на нужные контроллеры/сервисы.
4. Вызов `IChoiceActionExecutor.Execute()` выполняется **без входных параметров и без return**.

Ключевые типы:

- `IChoiceActionExecutor` — `void Execute();`
- `IChoiceActionExecutorFactory` — `IChoiceActionExecutor Create(ChoiceActionData actionData);`
- `ChoiceActionExecutorFactory` — фабрика на Zenject `DiContainer`, содержит `GetRequiredString` для валидации `Params`.

Реализованные executors (на текущий момент):

| `ChoiceActionType` | Executor | `Params` | Куда пишет |
|---|---|---|---|
| `GoToScene` | `GoToSceneChoiceActionExecutor` | `Strings.SceneId` (`Glossary.ChoiceActions.SCENE_ID`) | `AdventureStateLogic.ProcessAction(SetCurrentAdventureSceneIdStateAction)` → `AdventuresStateData.CurrentAdventureSceneId` |
| `GoToAdventure` | `GoToAdventureChoiceActionExecutor` | `Strings.AdventureId` (`Glossary.ChoiceActions.ADVENTURE_ID`) | `SetCurrentAdventureIdStateAction` → `CurrentAdventureId` + `CurrentAdventureSceneId = null`; затем `RuntimeSceneData` резолвит `StartScenes` и дописывает scene id |
| `GoToRandomAdventure` | `GoToRandomAdventureChoiceActionExecutor` | нет обязательных params | Кандидаты из `DefinitionsManager.Adventures` (без `HUB`, не текущее, не `Disabled`; TODO: completed/level) → `SetCurrentAdventureIdStateAction` |
| `GoToRandomScene` | `GoToRandomSceneChoiceActionExecutor` | `Strings.SceneId` — id через `;` | Парсинг списка → случайный id → `SetCurrentAdventureSceneIdStateAction` |
| `OpenWindow` | `OpenWindowChoiceActionExecutor` | `Strings.WindowId` (`Glossary.ChoiceActions.WINDOW_ID`) | Пока stub (warning log); целевой sink — UI/`WindowsManager` |
| `SetWorldParams` | `SetWorldParamsChoiceActionExecutor` | `Strings` / `Ints` / `Bools` | `AdventureStateLogic.ProcessAction(SetWorldParamsStateAction)` → merge в `AdventuresStateData.World.Parameters` |
| `SetAdventureParams` | `SetAdventureParamsChoiceActionExecutor` | `Strings` / `Ints` / `Bools` | `AdventureStateLogic.ProcessAction(SetAdventureParamsStateAction)` → merge в `AdventuresStateData.Adventures[currentAdventureId].Parameters` |
| `SetGlobalParams` | `SetGlobalParamsChoiceActionExecutor` | `Strings` / `Ints` / `Bools` | `AdventureStateLogic.ProcessAction(SetGlobalParamsStateAction)` → merge в `AdventuresStateData.Global.Parameters` |

### Инвариант секций State для choice-actions

Выбор игрока в приключении (`ChoiceData.Actions` / DiceCheck outcomes → `IChoiceActionExecutor`) — **не** произвольный доступ к профилю. Разрешены только мутации:

| Секция `StateData` | Через choice-actions | Комментарий |
|---|---|---|
| `Adventures` | ✅ разрешено | Навигация (`CurrentAdventureId` / `CurrentAdventureSceneId`) и `World` / `Adventure` / `Global` params — уже подключено |
| `Characters` | ✅ разрешено | Будущие `GrantItem` / damage / heal / party и т.п.; сейчас choice-executors Characters не трогают |
| `Inventory` | ✅ разрешено | Будущие выдачи/изъятия предметов; сейчас choice-executors Inventory не трогают |
| `Profile` | ❌ запрещено | В т.ч. `Profile.Parameters` (`MaxPartySlots`, `MaxInventorySlots`) — не через adventure-choices |
| `Wallet` | ❌ запрещено | Meta/UI/cheats, не choice-контент |
| `Localization` | ❌ запрещено | Init / settings |

`OpenWindow` state не меняет (stub UI). `Set*Params` пишут только в `Adventures.*Parameters`, не в `Profile.Parameters`.

При добавлении нового `ChoiceActionType` / executor: вызывать только state-actions из разрешённых секций; иначе — отдельный UI/meta-путь вне choice-pipeline.

Legacy (не используется фабрикой):

| Executor | `Params` | Куда писал |
|---|---|---|
| `ObsoleteGoToSceneChoiceActionExecutor` (**устаревший**, `[Obsolete]`) | `Strings.SceneId` | `IAdventureFlowController.GoToScene` (**устаревший**, `[Obsolete]`) |

Остальные значения `ChoiceActionType` пока не подключены к фабрике.

## Игровой runtime-флоу (целевой цикл)

Целевой цикл работы adventure-runtime:

1. Игрок выбирает действие в UI (choice).
2. `IChoiceActionExecutor` обрабатывает выбор только через state-actions (`StateActionBase<TStateData>`), без прямой мутации `StateData`; целевые секции — только `Characters` / `Inventory` / `Adventures` (см. инвариант выше).
3. `AdventureStateLogic.ProcessAction(...)` выполняет `Validate -> Execute` и публикует `StateChanged` с `StateChangeSource`.
4. `RuntimeSceneData` помечает dirty по `StateChanged`, на следующем кадре (`Updater.OnUpdate`) вызывает `SyncFromStateAndNotify` и уведомляет `AdventuresManager`.
5. `AdventuresManager` публикует собственные события (`ChangedAdventure`, `ChangedScene`, `ChangedContent`, `ChangedChoices`) для UI-слоя; UI читает уже отфильтрованные content/choices.
6. Окна и компоненты приключения перехватывают события, обновляют представление сцены и доступные органы управления (актуальные choice-actions).
7. Игрок делает следующий выбор, цикл повторяется.

Ключевой принцип: все изменения игрового прогресса проходят через state-actions и событийный контур (`StateChanged -> RuntimeSceneData -> AdventuresManager -> UI`), формируя замкнутый игровой цикл.

## `AdventuresManager`

Файл: `Scripts/Adventure/AdventuresManager.cs`.

Подробная и актуальная документация по архитектуре `AdventuresManager` и `RuntimeSceneData` вынесена в отдельный документ: [AdventuresManager.md](AdventuresManager.md).

Оркестратор adventure-runtime. Подписка на `AdventureStateLogic.StateChanged` и sync runtime-контекста живут в `RuntimeSceneData`; `AdventuresManager` владеет им и ретранслирует события наружу.

| Метод / член | Назначение |
|---|---|
| `Init()` | Создаёт `RuntimeSceneData` через `DiContainer` и вызывает его `Init()` |
| `Dispose()` | `Dispose()` у `RuntimeSceneData` (реализация `IDisposable`) |
| `GetCurrentContent()` / `GetCurrentChoices()` | Делегируют в `RuntimeSceneData` (с фильтрацией restrictions) |
| `ChangedAdventure` / `ChangedScene` / `ChangedContent` / `ChangedChoices` | Ретрансляция событий `RuntimeSceneData` |

**DI (Adventure `ProjectInstaller`):**

```csharp
Container.Bind<AdventureStateLogic>().AsSingle().NonLazy();
Container.BindInterfacesAndSelfTo<AdventuresManager>().AsSingle().NonLazy();
```

`BindInterfacesAndSelfTo` регистрирует singleton и интерфейс `IDisposable` для автоматического вызова `Dispose()` при уничтожении контейнера.

**Инициализация:** `AdventuresManagerInitTask` вызывает `Init()` без параметров после загрузки definitions и state (см. [Initializer.md](Initializer.md)). Id стартового приключения (`RuleSettings.StartAdventure`) в менеджер пока не передаётся.

Полная бизнес-логика оркестратора (переходы по сценам, выборы, применение actions) — в разработке; сейчас реализован каркас подписки на изменения state.

## Ключи статов и параметров (договоренность)

Для персонажей, партии, квестов и мира рекомендуется хранить значения в состоянии через словари по строковым ключам:

- `Dictionary<string, int>`
- `Dictionary<string, string>`
- `Dictionary<string, bool>`

В `Modules.State` для персонажа:
- `CharacterStateData.Parameters`, `Spells`, `StatusEffects` — `Dictionary<string, int>`;
- итоговые навыки/perception/`MaxHitPoints` **не** хранятся в `Parameters` как готовое число: в state лежат сырые составляющие (`STR`, `CON`, `Level`, `Athletics.ProfRank`, `MaxHitPoints.PerLevel`, …), а константы ancestry/class HP берутся из дефов; итог считает `CharacterParametersProxy.GetTotalValue` по `RuleDef.ParameterFormulas`;
- модификаторы атаки/урона оружия тоже **не** хранятся итогом: формулы в `ItemDef`, сырые бонусы в `Parameters`, расчёт — `WeaponProxy` (см. [State.md](State.md#adventure-weaponproxy));
- прогресс мира и приключений — `AdventureStateParamsData` (`Strings` / `Ints` / `Bools`) в `AdventuresStateData`.

Рекомендация по неймингу ключей параметров:
- `world.*` — флаги/счётчики мира в рамках кампании (`World.Parameters`); запись через `ChoiceActionType.SetWorldParams`, проверка через `RestrictionType.WorldParams`;
- `adventure.*` — локальные флаги/счётчики текущего приключения (`Adventures[currentAdventureId].Parameters`); запись через `ChoiceActionType.SetAdventureParams`, проверка через `RestrictionType.AdventureParams`;
- `global.*` — долгоживущие метрики игрока (`Global.Parameters`), не сбрасываются при перезапуске приключений; запись через `ChoiceActionType.SetGlobalParams`, проверка через `RestrictionType.GlobalParams`;
- `char.*` — параметры персонажа (будущие state-actions для `CharacterStateData`);
- `party.*` — параметры группы (будущие state-actions).

**Идентификаторы:**
- runtime-сущности (персонаж) — `int`, выдаются игрой (`NextCharacterId`);
- ссылки на дефы (класс, ancestry, предыстория, предмет, заклинание, черта) — `string` (id дефа = имя JSON-файла в `Definitions/_ADVENTURES_/...`; см. [Definitions.md](Definitions.md)).

## Персистентное состояние (`Modules.State`)

RPG-контент (сцены, выборы, действия) описывается в модуле `RPG`, а **сейв профиля** — в `Modules.State.Implementation.Adventure`.

Корневой `StateData` Adventure содержит секции-поля; каждая секция — **отдельный файл** в `StateDatas/`. Дочерние типы секции (например, `CharacterStateData`) — **отдельные классы в том же файле**, не nested class. Подробнее: [State.md — Организация файлов данных состояния](State.md#организация-файлов-данных-состояния).

### Персонажи (`CharactersStateData`, `CharacterStateData`, `EquippedItemStateData`)

- `HeroPoints` — очки героя на уровне профиля (ресурс кампании; дефолт `0` при создании профиля).
- `Characters` — `Dictionary<int, CharacterStateData>`: весь ростер профиля.
- `ActivePartyCharacterIds` — `List<int>`: текущий отряд; лимит — `Profile.Parameters[Glossary.ProfileState.MAX_PARTY_SLOTS]`.
- `CurrentActiveCharacterId` — текущий выбранный персонаж (`0` — не выбран).
- `CharacterStateData`: `CreateTime`, `Name`, `Gender`, `Ancestry`, `Class`, `Background`, `IsDead`, `DeathTime`, `Parameters`, `Spells`, `StatusEffects`, `EquippedItems`. Уровень/опыт и abilities хранятся в `Parameters` (`Glossary.Characters.LEVEL` / `EXPERIENCE`, `STR`, `CON`, …). Итоговые навыки и `MaxHitPoints` читаются через `CharacterParametersProxy.GetTotalValue` по `RuleDef.ParameterFormulas` (см. [State.md](State.md#adventure-characterparametersproxy-read-api)). Мутации сырых `Parameters` — через `CharacterParametersOperator` (Apply / Unapply). `StatusEffects` — таймеры timed Condition-feats (`featId` → оставшиеся ходы); см. [Feats.md](Feats.md).
  - `Gender` — `CharacterGender` (`Male` / `Female`); при генерации имени — вместе с `AncestryDef.Names`; пул аватаров — в `AvatarsDef` по `Ancestry`;
  - `Ancestry`, `Class`, `Background` — id дефов `AncestryDef` / `ClassDef` / `BackgroundDef`;
  - `Spells` — словарь id заклинаний (`SpellDef`) или связанных счётчиков (контракт уточняется при подключении runtime);
  - `EquippedItems` — экипировка персонажа.
- `EquippedItemStateData`: `{ Slot, ItemId }` — тип слота из `Glossary.Items` (`Hand`, `Legs`, `Head`, `Body`, `Bag`; повторяемый список без индексов) и id дефа надетого предмета (пустой `ItemId` — свободный слот). Подробнее — [State.md](State.md#adventure-charactersstatedata-и-characterstatedata).

### Инвентарь (`InventoryStateData`)

- `Items` — `Dictionary<string, int>`: расходники и стакающиеся предметы (`defId → count`), общий пул отряда.
- Слоты конкретного персонажа — `CharacterStateData.EquippedItems` (отдельно от инвентаря).
- Перенос между общим инвентарём и слотами персонажа — через `EquipItemFromInventoryStateAction` / `UnequipItemToInventoryStateAction`.
- Перенос между слотами одного персонажа — через `MoveEquippedItemBetweenSlotsStateAction` (без участия `Inventory.Items`).
- Добавление/удаление в общий инвентарь — `AddInventoryItemsStateAction` / `RemoveInventoryItemsStateAction`.
- Выдача/удаление у персонажа — `AddCharacterItemStateAction` (Bag → иначе inventory) / `RemoveCharacterEquippedItemStateAction` (без возврата в inventory).
- Примеры кейсов — в [State.md](State.md#adventure-inventorystatedata).

### Прогресс приключений (`AdventuresStateData`)

- `CurrentAdventureId` / `CurrentAdventureSceneId` — активная точка приключения для runtime (`AdventuresManager`).
- `World.Parameters` — параметры мира/кампании (`AdventureStateParamsData`).
- `Global.Parameters` — долгоживущие параметры игрока (`AdventureStateParamsData`), не зависят от перезапуска приключений.
- `Adventures` — `Dictionary<string, AdventureStateData>`: прогресс по каждому adventure (`AdventureId`, `SceneId`, `Parameters`). `SceneId` здесь — прогресс конкретного приключения, не активная runtime-сцена.
- Формат `Parameters` совпадает с `ChoiceActionParamsData` (`Strings` / `Ints` / `Bools`).

`World.Parameters` не является указателем текущей точки; он используется для прогресса мира в контексте кампании (например, открытые локации и глобальные события). `Global.Parameters` — отдельное хранилище для метрик игрока (например, число посещений таверны или запусков игры).

### Создание нового профиля

При первом запуске или отсутствии сейва `AdventureStateManager` создаёт профиль через `IAdventureStateDataFactory` (`DiContainer.Resolve`). Фабрика инициализирует все секции и может использовать дефы (`DefinitionsManager` уже инжектирован). Подробнее — [State.md](State.md).

Изменения прогресса профиля применяются через `Modules.State` и state-actions runtime-слоя. Папка `RPG/Scripts/State` удалена.

Лимиты переноски и модификаторы от травм/бафов задаются в дефах и учитываются в runtime-сервисе; в state хранятся источники (статы, `StatusEffects`), а не вычисленный итог.

## Интеграции с другими модулями

- `Definitions`: adventure-контент загружается как JSON-дефы (`AdventureDef`, `ClassDef`, `AncestryDef`, `FeatDef`, `ItemDef`, `SpellDef`). Доменная модель приключения (`AdventureData`, `SceneData`, `ChoiceData`) остаётся в `RPG`; `AdventureDef` — тонкая обёртка для загрузчика. Механика параметров: Write API (`CharacterParametersOperator`) и item Features в equip-экшенах (`CharacterItemFeaturesOperator`) — см. [State.md](State.md) / [Feats.md](Feats.md); UI создания/прокачки — следующий этап.
- `Dices`: `ChoiceDiceCheckData` ссылается на `DiceType` и `DiceOptions` из `Modules.Dices.Scripts`; runtime-интеграция с adventure-flow в разработке.
- `Restrictions`: `AdventureData`, `ChoiceData` и `SceneContentData` используют `Restriction` для описания условий доступа. Для параметров прогресса доступны `RestrictionType.WorldParams`, `RestrictionType.AdventureParams` и `RestrictionType.GlobalParams` (см. [Restrictions.md](Restrictions.md)).
- `State`: персистентный прогресс профиля и ссылки персонажа на id дефов (см. выше).
- Остальные интеграции (UI, события, полный оркестратор приключения) пока явно не реализованы в коде модуля.

## Целевые runtime-паттерны (следующий этап)

- **Вложенные приключения:** одно adventure может запускать другое через стек контекстов (push/pop), затем возвращать игрока в родительский сценарий.
- **Карта и локации:** глобальная навигация выносится в отдельный world/map JSON слой; adventure остается локальным графом сцен.
- **Стартовый хаб:** рекомендуемая стартовая локация — Таверна, где создается персонаж и выбирается доступное приключение.

## Предполагаемый runtime-поток (по модели данных)

1. Загрузка/получение `AdventureDef` (контракт `AdventureData`) из `DefinitionsManager`.
2. Проверка `AdventureData.Restrictions` через `RestrictionsChecker`.
3. Выбор стартовой сцены из `StartScenes`.
4. Рендер `SceneData.Content`: UI берёт список из `RuntimeSceneData.GetCurrentContent()` (уже отфильтрован по `SceneContentData.Restrictions`).
5. Формирование списка `Choices` в `RuntimeSceneData.GetCurrentChoices()`:
   - `AlwaysShow == true` → choice всегда в списке;
   - иначе — только при успешном `RestrictionsChecker.Check(ChoiceData.Restrictions)` (пустой/`null` список проходит).
6. Обработка выбранного `ChoiceData` по типу:
   - `Default` — для каждого `ChoiceActionData` из `Actions` фабрика создаёт `IChoiceActionExecutor` и вызывает `Execute()`;
   - `DiceCheck` — открыть окно броска, взять модификатор по `DiceCheck.DiceCheckParam`, рассчитать исход по `DiceCheck.DifficultyClass`, выполнить actions из соответствующего outcome-списка (`OnCriticalSuccess` / `OnSuccess` / `OnFailure` / `OnCriticalFailure`).
7. Обновление `StateData.Adventures` и переход к следующей сцене (через контроллеры/менеджеры).

Runtime-поток для `ChoiceType.Default` замкнут из UI (`AdventureChoiceViewModel.Select()` → фабрика executors). `DiceCheck` runtime ещё stub. Choice-executors мутируют только `Adventures` (Characters/Inventory — по инварианту разрешены, пока не подключены). Legacy-путь (`ObsoleteGoToSceneChoiceActionExecutor` + `IAdventureFlowController`) помечен `[Obsolete]` и не используется фабрикой.

## Текущее состояние реализации

- Реализованы доменные DTO/POCO-модели для adventure-данных (`AdventureData`, `SceneData`, `ChoiceData` и связанные типы).
- `ChoiceType` расширен значением `DiceCheck`; в `ChoiceData` добавлен опциональный блок `DiceCheck` (`DifficultyClass`, `DiceType`, `DiceOptions`, `DiceCheckParam`, outcome action-списки).
- В `ChoiceData` добавлен опциональный блок `VisualOptions` (`MainIcon`, `DescriptionIcon`, `ParameterOverrideDescription` — id параметра для override описания/иконки через `VisualSettingsDef`).
- Runtime UI choice (`AdventureChoiceViewModelBase`) резолвит иконки/описание/`DescriptionParam` из `VisualOptions` + `VisualSettingsDef` + `CharacterParametersProxy` (см. [Windows.md](Windows.md)).
- В TEA (`AdventureEditorWindow`) для `ChoiceType.DiceCheck` доступен редактор полей `DiceCheck` (включая `DiceCheckParam`) и outcome action-списков; для `Default` — редактор `Actions`; для любого choice — секция `Visual Options`. Валидация TEA обеспечивает взаимную исключительность `DiceCheck` и `Actions` по типу choice (см. `Assets/Modules/Definitions/Scripts/Editor/Adventures/README.md`).
- В `Modules.Definitions` добавлены adventure-дефы персонажа и контента: `ClassDef`, `AncestryDef`, `FeatDef`, `ItemDef`, `SpellDef` (наследуют `AbstractDefinition`); `AdventureDef` наследует `AdventureData`. Загружен стартовый PF2e-ориентированный набор JSON (классы, ancestries, черты, заклинания, предметы).
- Write API параметров персонажа (`CharacterParametersOperator`) и item Features в equip-экшенах (`CharacterItemFeaturesOperator`) реализованы; UI прокачки/создания — следующий этап. Практика: [Feats.md](Feats.md). `WeaponProxy` — инфраструктура оружия.
- `SceneContentType`: `Text`, `Image`, `RandomImage`, `Slideshow`, `Splitter`, `Item`; `SceneContentData` поддерживает `Value`, `Values` и `Restrictions`.
- Поля ограничений унифицированы: `Restrictions` в `AdventureData`, `ChoiceData`, `SceneContentData` (ранее встречалась опечатка `Restictions`).
- В `Modules.State` реализованы секции Adventure-профиля: `CharactersStateData`, `InventoryStateData`, `AdventuresStateData`; создание нового профиля — через `IAdventureStateDataFactory` (см. [State.md](State.md)).
- `ChoiceActionData` использует контракт `Params` (`Strings` / `Ints` / `Bools`).
- `ChoiceActionType`: к фабрике подключены `GoToScene` (`1`), `SetWorldParams` (`2`), `SetAdventureParams` (`3`), `SetGlobalParams` (`4`), `GoToAdventure` (`5`), `OpenWindow` (`6`, stub), `GoToRandomAdventure` (`7`), `GoToRandomScene` (`8`).
- Ключи `Params.Strings`: `Glossary.ChoiceActions.SCENE_ID` / `ADVENTURE_ID` / `WINDOW_ID`; ids окон хаба — `Glossary.Windows.*`.
- Реализованы `ChoiceActionExecutorFactory`, `IChoiceActionExecutor`, `IChoiceActionExecutorFactory`.
- Зафиксирован инвариант: choice-executors могут мутировать только `Characters` / `Inventory` / `Adventures` (не `Profile` / `Wallet` / `Localization`).
- Реализованы executors:
  - `GoToSceneChoiceActionExecutor` → `SetCurrentAdventureSceneIdStateAction`;
  - `GoToAdventureChoiceActionExecutor` → `SetCurrentAdventureIdStateAction` (очищает `CurrentAdventureSceneId`);
  - `GoToRandomAdventureChoiceActionExecutor` → фильтр кандидатов + `SetCurrentAdventureIdStateAction`;
  - `GoToRandomSceneChoiceActionExecutor` → парсинг `SceneId` через `;` + `SetCurrentAdventureSceneIdStateAction`;
  - `OpenWindowChoiceActionExecutor` → stub (warning);
  - `SetWorldParamsChoiceActionExecutor` → `SetWorldParamsStateAction`;
  - `SetAdventureParamsChoiceActionExecutor` → `SetAdventureParamsStateAction`;
  - `SetGlobalParamsChoiceActionExecutor` → `SetGlobalParamsStateAction`.
- `RestrictionType.ActivePartyCount` + `ActivePartyCountRestrictionChecker`: сравнение `ActivePartyCharacterIds.Count` с `IntValues[0]` через `CompareOptions`.
- `RestrictionType.CharacterParams` + `CharacterParamsRestrictionChecker`: сравнение `CharacterStateData.Parameters[key]` с `IntValues[0]`; цель — `CharacterRestrictionContext` (см. [Restrictions.md](Restrictions.md)).
- Legacy executor: `ObsoleteGoToSceneChoiceActionExecutor` (**устаревший**, `[Obsolete]`) — старый путь через `IAdventureFlowController`.
- Объявлен legacy интерфейс: `IAdventureFlowController` (**устаревший**, `[Obsolete]`).
- `AdventuresManager` зарегистрирован в DI (`BindInterfacesAndSelfTo`), инициализируется через `AdventuresManagerInitTask`; владеет `RuntimeSceneData`.
- `RuntimeSceneData`: dirty-coalescing `StateChanged` через `Updater.OnUpdate`; `GetCurrentContent`/`GetCurrentChoices` фильтруют элементы через `RestrictionsChecker` (`AlwaysShow` для choices).
- Отсутствуют DI-биндинги фабрики executors в installer, валидаторы adventure-данных, сериализация и тесты модуля.

## Рекомендации по дальнейшему развитию

1. Расширить dirty-whitelist в `RuntimeSceneData` на `SetWorldParams` / `SetAdventureParams` / `SetGlobalParams` (и позже — изменения партии/персонажей), чтобы UI обновлял content/choices без смены сцены.
2. Довести `OpenWindow` до реального открытия окон через `WindowsManager` / adventure presenter; при закрытии окон — refresh choices (`ChangedChoices`).
3. Реализовать runtime-flow для `ChoiceType.DiceCheck` (окно броска, расчёт исхода, выполнение outcome actions через существующую фабрику executors).
4. Зарегистрировать в Zenject installer:
   - `IChoiceActionExecutorFactory -> ChoiceActionExecutorFactory`.
5. Довести оркестрацию выбора: `DiceCheck` outcomes через ту же фабрику executors (Default уже гоняется из `AdventureChoiceViewModel`).
6. Подключить остальные `ChoiceActionType` к state-actions секций `Characters` / `Inventory` / `Adventures` (`GrantItem`, damage/heal и т.д.); не выводить choice-pipeline на `Profile` / `Wallet` / `Localization`.
7. Добавить валидацию целостности adventure-данных (`StartScenes`, наличие ссылок в `Scenes`, корректность `Actions` и `DiceCheck` outcome actions).
8. Добавить unit-тесты на:
   - фабрику executors и валидацию `Params`;
   - проверку ограничений и фильтрацию content/choices (в т.ч. `ActivePartyCount`);
   - `GoToAdventure` → резолв стартовой сцены;
   - coalescing dirty → один notify за кадр;
   - корректность переходов по сценам и изменения состояния.
