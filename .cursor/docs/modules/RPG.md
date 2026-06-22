# Модуль RPG

**Последнее обновление:** 2026-06-22 12:27:00 (+03:00)

## Назначение

`RPG` — доменный каркас для adventure-геймплея в формате сцен и выборов.  
Модуль задает:

- структуру данных приключения (`AdventureData` — доменный контракт в `RPG`; в рантайме загружается как `AdventureDef` из модуля `Definitions`);
- структуру сцены и ее контента (`SceneData`, `SceneContentData`);
- структуру выбора игрока и набора действий (`ChoiceData`, `ChoiceActionData`).

На текущем этапе модуль содержит **data-contract слой** и начальный **runtime-слой выполнения действий** (`Choice/Executors`). Оркестраторы приключения пока в заготовочном состоянии.

> **Связь с `Modules.Definitions`:** JSON-дефы adventure-проекта загружаются через `DefinitionsManager` (Adventures). `AdventureDef` — единственный деф, который наследует RPG-модель (`AdventureData`); классы, происхождения, черты, предметы и заклинания описаны как `ClassDef` / `AncestryDef` / `FeatDef` / `ItemDef` / `SpellDef` и наследуют `AbstractDefinition` напрямую. Контракт дефов расширяется (`AncestryDef.MaleNames`/`FemaleNames`, `ItemDef.IsQuestItem`, `FeatDef.Restrictions` и др.); применение механик в state — в планах. Подробности — в [Definitions.md](Definitions.md#adventure-дефы-персонажа-текущий-контракт-и-эволюция).
>
> **Связь с `Modules.State`:** персистентный прогресс игрока (сейв профиля) живёт в модуле `State` (`AdventureStateManager`, `AdventureStateLogic`, state-actions). Изменения прогресса из choice-executors проходят через `AdventureStateLogic.ProcessAction(...)`, а не напрямую в `StateData`.
>
> Персонажи, отряд, инвентарь и прогресс приключений хранятся в `Modules.State` (`CharactersStateData`, `InventoryStateData`, `AdventuresStateData`). Подробности — в [документации модуля State](State.md).

## Структура модуля

- `Assets/Modules/RPG/Scripts/Adventure`
  - `AdventuresManager` — оркестратор приключения (стартовая инициализация, реакция на изменения state). Реализует `IDisposable`.
  - `IAdventureFlowController` — интерфейс контроллера переходов между сценами/узлами.
  - `Data/*` — модели adventure/scene/content.
  - `Choice/*` — модели выбора и действий по выбору.
  - `Choice/Executors/*` — фабрика и обработчики `ChoiceActionData` (часть executors пишет в `Modules.State` через state-actions).

## Модель данных Adventure

### `AdventureData`

Описывает целое приключение (контентный JSON, модуль `RPG`). В `Resources/Definitions` десериализуется в `AdventureDef` (`AdventureDef : AdventureData`).

- `Id` — уникальный идентификатор adventure.
- `Tags` — набор тегов для фильтрации/поиска/категоризации.
- `Type` — тип узла (`AdventureType`: `Adventure`, `Chapter`, `Location`).
- `AdventureLinks` — связи с другими adventure-узлами (для карты/иерархии).
- `Title`, `Description` — метаданные и текстовое описание.
- `IgnoredTags` — теги, игнорируемые в контексте этого adventure.
- `Restrictions` — список ограничений на доступ к приключению (тип `Restriction` из модуля `Restrictions`).
- `StartScenes` — список стартовых сцен (по `Scene.Id`).
- `Scenes` — словарь `sceneId -> SceneData` со всем графом сцен.

> Не путать с `Modules.State...AdventureStateData` — это **прогресс** конкретного приключения в сейве (`AdventureId`, `SceneId`, `Parameters`).

### `SceneData`

Описывает отдельную сцену в приключении:

- `Id` — идентификатор сцены.
- `Tags` — служебные/геймдизайнерские теги сцены.
- `Content` — последовательность контент-элементов (`SceneContentData`).
- `NotClearScene` — флаг особого поведения после показа/прохождения (из названия следует сценарий "не очищать сцену").
- `Choices` — список доступных выборов (`ChoiceData`) для перехода/действий.

### `SceneContentData` и `SceneContentType`

Оба типа объявлены в `SceneContentData.cs`.

- `SceneContentData.Type` — тип контентного элемента (`SceneContentType`).
- `SceneContentData.Restrictions` — список ограничений видимости элемента (тип `Restriction` из модуля `Restrictions`).
- `SceneContentData.Value` — payload в строковом виде (текст, id ресурса и т.д.).

`SceneContentType`:

| Значение | Код | Назначение |
|---|---|---|
| `Text` | `0` | Текстовый блок |
| `Image` | `10` | Изображение |
| `Splitter` | `20` | Разделитель / визуальный отступ |
| `Item` | `30` | Элемент предмета / иконки |

Проверка `Restrictions` для контента сцены пока не подключена в runtime; контракт данных готов для UI/оркестратора.

## Модель данных Choice

### `ChoiceData`

Описывает вариант действия игрока в сцене:

- `Id` — идентификатор выбора.
- `Tags` — теги для аналитики/фильтрации/UI.
- `Text`, `Description` — основной и дополнительный тексты выбора.
- `AlwaysShow` — всегда показывать выбор, даже если ограничения не прошли (ожидаемая интерпретация по названию поля).
- `Restrictions` — список ограничений доступности выбора.
- `Actions` — список действий (`ChoiceActionData`), выполняемых при выборе.

В классе также есть комментарии-заготовки про `Type`, `ViewOptions`, `Icon`; это маркеры планируемого расширения визуальной и семантической модели choice.

### `ChoiceActionData` и `ChoiceActionType`

- `ChoiceActionData.Type` — тип действия при выборе.
- `ChoiceActionData.Params` — именованные параметры действия через типизированные словари:
  - `Params.Strings: Dictionary<string, string>`
  - `Params.Ints: Dictionary<string, int>`
  - `Params.Bools: Dictionary<string, bool>`
- `ChoiceActionType` — базовый enum действий (может расширяться по мере роста механик).

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
- `ChoiceActionExecutorFactory` — фабрика на Zenject `DiContainer`, содержит `GetRequiredString/Int/Bool` для валидации `Params`.

Реализованные executors (на текущий момент):

| `ChoiceActionType` | Executor | `Params` | Куда пишет |
|---|---|---|---|
| `GoToScene` | `GoToSceneChoiceActionExecutor` | `Strings.sceneId` | `IAdventureFlowController.GoToScene` |
| `SetFlag` | `SetFlagChoiceActionExecutor` | `Strings.key`, `Bools.value`, опц. `Strings.adventureId` | `SetAdventureProgressBoolStateAction` → `AdventuresStateData` |
| `ModifyVariable` | `ModifyVariableChoiceActionExecutor` | `Strings.key`, `Ints.delta`, опц. `Strings.adventureId` | `ModifyAdventureProgressIntStateAction` → `AdventuresStateData` |

Ключи с префиксом `world.*` пишутся в `World.Parameters`; `adventure.*` — в `Adventures[adventureId].Parameters` (для них нужен `adventureId` в `Params.Strings` или из runtime-контекста позже).

Остальные значения `ChoiceActionType` объявлены в enum, но пока не подключены к фабрике.

## `AdventuresManager`

Файл: `Scripts/Adventure/AdventuresManager.cs`.

Оркестратор adventure-runtime. **Не наследует** `AdventureStateLogic` — использует композицию: через Zenject инжектируется `AdventureStateLogic` и подписка идёт на его событие `StateChanged`.

| Метод / член | Назначение |
|---|---|
| `Init()` | Подписывается на `AdventureStateLogic.StateChanged` через `Subscribe()` |
| `Dispose()` | Вызывает `Unsubscribe()` (реализация `IDisposable`) |
| `Subscribe()` / `Unsubscribe()` | Подписка / отписка на `AdventureStateLogic.StateChanged` |
| `OnStateChangedHandler(source)` | Обработчик изменений state по `StateChangeSource` |
| `_adventureId` | Зарезервировано для runtime-контекста текущего приключения (пока не задаётся в `Init`) |

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
- `CharacterStateData.Parameters`, `SavingThrows`, `Spells`, `StatusEffects` — `Dictionary<string, int>`;
- прогресс мира и приключений — `AdventureStateParamsData` (`Strings` / `Ints` / `Bools`) в `AdventuresStateData`.

Рекомендация по неймингу ключей для `SetFlag` / `ModifyVariable`:
- `world.*` — глобальные флаги/счётчики мира (`World.Parameters`);
- `adventure.*` — локальные флаги/счётчики приключения (`Adventures[adventureId].Parameters`);
- `char.*` — параметры персонажа (будущие state-actions для `CharacterStateData`);
- `party.*` — параметры группы (будущие state-actions).

**Идентификаторы:**
- runtime-сущности (персонаж) — `int`, выдаются игрой (`NextCharacterId`);
- ссылки на дефы (класс, происхождение, предмет, заклинание, черта) — `string` (id дефа = имя JSON-файла в `Definitions/_ADVENTURES_/...`; см. [Definitions.md](Definitions.md)).

## Персистентное состояние (`Modules.State`)

RPG-контент (сцены, выборы, действия) описывается в модуле `RPG`, а **сейв профиля** — в `Modules.State.Implementation.Adventure`.

Корневой `StateData` Adventure содержит секции-поля; каждая секция — **отдельный файл** в `StateDatas/`. Дочерние типы секции (например, `CharacterStateData`) — **отдельные классы в том же файле**, не nested class. Подробнее: [State.md — Организация файлов данных состояния](State.md#организация-файлов-данных-состояния).

### Персонажи (`CharactersStateData`, `CharacterStateData`, `EquippedItemStateData`)

- `HeroPoints` — очки героя на уровне профиля (ресурс кампании; дефолт `0` при создании профиля).
- `Characters` — `Dictionary<int, CharacterStateData>`: весь ростер профиля.
- `ActivePartyCharacterIds` — `List<int>`: текущий отряд (до 4 персонажей).
- `CharacterStateData`: `CreateTime`, `Name`, `Gender`, `Ancestry`, `Class`, `Level`, `Experience`, `IsDead`, `DeathTime`, `Parameters`, `SavingThrows`, `Spells`, `StatusEffects`, `EquippedItems`.
  - `Gender` — `CharacterGender` (`Male` / `Female`); при генерации имени — вместе с `AncestryDef.MaleNames` / `FemaleNames`;
  - `Ancestry`, `Class` — id дефов `AncestryDef` / `ClassDef`;
  - `Spells` — словарь id заклинаний (`SpellDef`) или связанных счётчиков (контракт уточняется при подключении runtime);
  - `EquippedItems` — экипировка персонажа.
- `EquippedItemStateData`: `{ Slot, ItemId }` — слот и id дефа надетого предмета.

### Инвентарь (`InventoryStateData`)

- `Items` — `Dictionary<string, int>`: расходники и стакающиеся предметы (`defId → count`), общий пул отряда.
- Надетая экипировка — `CharacterStateData.EquippedItems` (отдельно от инвентаря).

### Прогресс приключений (`AdventuresStateData`)

- `CurrentAdventureId` / `CurrentSceneId` — активная точка приключения для runtime (`AdventuresManager`).
- `World.Parameters` — глобальные параметры кампании (`AdventureStateParamsData`).
- `Adventures` — `Dictionary<string, AdventureStateData>`: прогресс по каждому adventure (`AdventureId`, `SceneId`, `Parameters`).
- Формат `Parameters` совпадает с `ChoiceActionParamsData` (`Strings` / `Ints` / `Bools`).

`World.Parameters` не является указателем текущей точки; он используется для долгоживущего прогресса мира (например, открытые локации и глобальные события).

### Создание нового профиля

При первом запуске или отсутствии сейва `AdventureStateManager` создаёт профиль через `IAdventureStateDataFactory` (`DiContainer.Resolve`). Фабрика инициализирует все секции и может использовать дефы (`DefinitionsManager` уже инжектирован). Подробнее — [State.md](State.md).

Изменения прогресса из choice-executors — через `AdventureStateLogic.ProcessAction(...)` и state-actions. Папка `RPG/Scripts/State` удалена.

Лимиты переноски и модификаторы от травм/бафов задаются в дефах и учитываются в runtime-сервисе; в state хранятся источники (статы, `StatusEffects`), а не вычисленный итог.

## Интеграции с другими модулями

- `Definitions`: adventure-контент загружается как JSON-дефы (`AdventureDef`, `ClassDef`, `AncestryDef`, `FeatDef`, `ItemDef`, `SpellDef`). Доменная модель приключения (`AdventureData`, `SceneData`, `ChoiceData`) остаётся в `RPG`; `AdventureDef` — тонкая обёртка для загрузчика. Дефы персонажа пока описывают контентный минимум; применение бонусов/эффектов в `CharacterStateData` — следующий этап (см. [Definitions.md](Definitions.md#adventure-дефы-персонажа-текущий-контракт-и-эволюция)).
- `Restrictions`: `AdventureData` и `ChoiceData` используют `Restriction` для описания условий доступа.
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
4. Рендер `SceneData.Content` с фильтрацией элементов по `SceneContentData.Restrictions` (когда будет подключён runtime).
5. Формирование списка `Choices`:
   - либо по `AlwaysShow`;
   - либо по результату проверки `ChoiceData.Restrictions`.
6. Для каждого `ChoiceActionData` из `Actions` фабрика создает `IChoiceActionExecutor` и вызывает `Execute()`.
7. Обновление `StateData.Adventures` и переход к следующей сцене (через контроллеры/менеджеры).

Полный runtime-поток еще не замкнут: `IAdventureFlowController` пока только объявлен; `AdventuresManager` инициализируется и слушает `StateChanged`, но не управляет сценами и выборами.

## Текущее состояние реализации

- Реализованы доменные DTO/POCO-модели для adventure-данных (`AdventureData`, `SceneData`, `ChoiceData` и связанные типы).
- В `Modules.Definitions` добавлены adventure-дефы персонажа и контента: `ClassDef`, `AncestryDef`, `FeatDef`, `ItemDef`, `SpellDef` (наследуют `AbstractDefinition`); `AdventureDef` наследует `AdventureData`. Загружен стартовый PF2e-ориентированный набор JSON (классы, ancestries, черты, заклинания, предметы).
- Runtime применения механик дефов к персонажу (`CharacterStateData.Parameters` и др.) пока не реализован.
- `SceneContentType` заполнен базовыми значениями (`Text`, `Image`, `Splitter`, `Item`); `SceneContentData` поддерживает `Restrictions`.
- Поля ограничений унифицированы: `Restrictions` в `AdventureData`, `ChoiceData`, `SceneContentData` (ранее встречалась опечатка `Restictions`).
- В `Modules.State` реализованы секции Adventure-профиля: `CharactersStateData`, `InventoryStateData`, `AdventuresStateData`; создание нового профиля — через `IAdventureStateDataFactory` (см. [State.md](State.md)).
- `ChoiceActionData` использует контракт `Params` (`Strings` / `Ints` / `Bools`).
- `ChoiceActionType` содержит базовый набор значений для переходов, проверок и боевых/ресурсных эффектов.
- Реализованы `ChoiceActionExecutorFactory`, `IChoiceActionExecutor`, `IChoiceActionExecutorFactory`.
- Реализованы executors: `GoToSceneChoiceActionExecutor`, `SetFlagChoiceActionExecutor`, `ModifyVariableChoiceActionExecutor` (два последних через `AdventureStateLogic`).
- Объявлен интерфейс `IAdventureFlowController`.
- `AdventuresManager` зарегистрирован в DI (`BindInterfacesAndSelfTo`), инициализируется через `AdventuresManagerInitTask`, подписан на `AdventureStateLogic.StateChanged`.
- Отсутствуют DI-биндинги фабрики/контроллеров в installer, валидаторы adventure-данных, сериализация и тесты модуля.

## Рекомендации по дальнейшему развитию

1. Подключить runtime-фильтрацию `SceneContentData.Restrictions` при рендере сцены.
2. Добавить executors и маппинг в фабрику для остальных `ChoiceActionType` (`SkillCheck`, `StartCombat` и т.д.).
3. Зарегистрировать в Zenject installer:
   - `IChoiceActionExecutorFactory -> ChoiceActionExecutorFactory`;
   - реализацию `IAdventureFlowController`.
4. Расширить `AdventuresManager`:
   - переходы по сценам и выборы;
   - применение списка `ChoiceActionData` через фабрику executors;
   - реакция на `StateChanged` для обновления UI / runtime-контекста.
5. Подключить остальные `ChoiceActionType` к state-actions в `Modules.State`.
6. Добавить state-actions для персонажей (`char.*`) и инвентаря; сервис применения механик из дефов в `CharacterStateData`.
7. Добавить валидацию целостности adventure-данных (`StartScenes`, наличие ссылок в `Scenes`, корректность `Actions`).
8. Добавить unit-тесты на:
   - фабрику executors и валидацию `Params`;
   - проверку ограничений;
   - вычисление доступных choices;
   - корректность переходов по сценам и изменения состояния.
