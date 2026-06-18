# Модуль RPG

**Последнее обновление:** 2026-06-17 20:00:00 (+03:00)

## Назначение

`RPG` — доменный каркас для adventure-геймплея в формате сцен и выборов.  
Модуль задает:

- структуру данных приключения (`AdventureData`);
- структуру сцены и ее контента (`SceneData`, `SceneContentData`);
- структуру выбора игрока и набора действий (`ChoiceData`, `ChoiceActionData`).

На текущем этапе модуль содержит **data-contract слой** и начальный **runtime-слой выполнения действий** (`Choice/Executors`). Оркестраторы приключения пока в заготовочном состоянии.

> **Связь с `Modules.State`:** персистентный прогресс игрока (сейв профиля) живёт в модуле `State` (`AdventureStateManager`, `AdventureStateLogic`, state-actions). Изменения прогресса из choice-executors проходят через `AdventureStateLogic.ProcessAction(...)`, а не напрямую в `StateData`.
>
> Персонажи, отряд, инвентарь и прогресс приключений хранятся в `Modules.State` (`CharactersStateData`, `InventoryStateData`, `AdventuresStateData`). Подробности — в [документации модуля State](State.md).

## Структура модуля

- `Assets/Modules/RPG/Scripts/Adventure`
  - `AdventuresManager` — будущий оркестратор запуска/переходов по приключениям.
  - `IAdventureFlowController` — интерфейс контроллера переходов между сценами/узлами.
  - `Data/*` — модели adventure/scene/content.
  - `Choice/*` — модели выбора и действий по выбору.
  - `Choice/Executors/*` — фабрика и обработчики `ChoiceActionData` (часть executors пишет в `Modules.State` через state-actions).

## Модель данных Adventure

### `AdventureData`

Описывает целое приключение (контентный JSON, модуль `RPG`):

- `Id` — уникальный идентификатор adventure.
- `Tags` — набор тегов для фильтрации/поиска/категоризации.
- `Type` — тип узла (`AdventureType`: `Adventure`, `Chapter`, `Location`).
- `AdventureLinks` — связи с другими adventure-узлами (для карты/иерархии).
- `Title`, `Description` — метаданные и текстовое описание.
- `IgnoredTags` — теги, игнорируемые в контексте этого adventure.
- `Restictions` — список ограничений на доступ к приключению (тип `Restriction` из модуля `Restrictions`).
- `StartScenes` — список стартовых сцен (по `Scene.Id`).
- `Scenes` — словарь `sceneId -> SceneData` со всем графом сцен.

> Не путать с `Modules.State...AdventureStateData` — это **прогресс** конкретного приключения в сейве (`AdventureId`, `SceneId`, `Parameters`).

Важно: поле `Restictions` в коде называется именно так (с опечаткой); при дальнейшей доработке желательно унифицировать нейминг, чтобы снизить вероятность ошибок сериализации/маппинга.

### `SceneData`

Описывает отдельную сцену в приключении:

- `Id` — идентификатор сцены.
- `Tags` — служебные/геймдизайнерские теги сцены.
- `Content` — последовательность контент-элементов (`SceneContentData`).
- `NotClearScene` — флаг особого поведения после показа/прохождения (из названия следует сценарий "не очищать сцену").
- `Choices` — список доступных выборов (`ChoiceData`) для перехода/действий.

### `SceneContentData` и `SceneContentType`

- `SceneContentData.Type` — тип контентного элемента (текст, иллюстрация, звук и т.д. — пока не реализовано).
- `SceneContentData.Value` — payload в строковом виде.
- `SceneContentType` — enum-заготовка (пока без значений).

## Модель данных Choice

### `ChoiceData`

Описывает вариант действия игрока в сцене:

- `Id` — идентификатор выбора.
- `Tags` — теги для аналитики/фильтрации/UI.
- `Text`, `Description` — основной и дополнительный тексты выбора.
- `AlwaysShow` — всегда показывать выбор, даже если ограничения не прошли (ожидаемая интерпретация по названию поля).
- `Restictions` — список ограничений доступности выбора.
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
- ссылки на дефы (класс, происхождение, тип предмета) — `string` (id дефа = имя JSON-файла в `Definitions`).

## Персистентное состояние (`Modules.State`)

RPG-контент (сцены, выборы, действия) описывается в модуле `RPG`, а **сейв профиля** — в `Modules.State.Implementation.Adventure`.

Корневой `StateData` Adventure содержит секции-поля; каждая секция — **отдельный файл** в `StateDatas/`. Дочерние типы секции (например, `CharacterStateData`) — **отдельные классы в том же файле**, не nested class. Подробнее: [State.md — Организация файлов данных состояния](State.md#организация-файлов-данных-состояния).

### Персонажи (`CharactersStateData`, `CharacterStateData`, `EquippedItemStateData`)

- `Characters` — `Dictionary<int, CharacterStateData>`: весь ростер профиля.
- `ActivePartyCharacterIds` — `List<int>`: текущий отряд (до 4 персонажей).
- `CharacterStateData`: `CreateTime`, `Name`, `Ancestry`, `Class`, `Level`, `Experience`, `IsDead`, `DeathTime`, `Parameters`, `SavingThrows`, `Spells`, `StatusEffects`, `EquippedItems`.
- `EquippedItemStateData`: `{ Slot, ItemId }` — слот и id дефа надетого предмета.

### Инвентарь (`InventoryStateData`)

- `Items` — `Dictionary<string, int>`: расходники и стакающиеся предметы (`defId → count`), общий пул отряда.
- Надетая экипировка — `CharacterStateData.EquippedItems` (отдельно от инвентаря).

### Прогресс приключений (`AdventuresStateData`)

- `World.Parameters` — глобальные параметры кампании (`AdventureStateParamsData`).
- `Adventures` — `Dictionary<string, AdventureStateData>`: прогресс по каждому adventure (`AdventureId`, `SceneId`, `Parameters`).
- Формат `Parameters` совпадает с `ChoiceActionParamsData` (`Strings` / `Ints` / `Bools`).

### Создание нового профиля

При первом запуске или отсутствии сейва `AdventureStateManager` создаёт профиль через `IAdventureStateDataFactory` (`DiContainer.Resolve`). Фабрика инициализирует все секции и может использовать дефы (`DefinitionsManager` уже инжектирован). Подробнее — [State.md](State.md).

Изменения прогресса из choice-executors — через `AdventureStateLogic.ProcessAction(...)` и state-actions. Папка `RPG/Scripts/State` удалена.

Лимиты переноски и модификаторы от травм/бафов задаются в дефах и учитываются в runtime-сервисе; в state хранятся источники (статы, `StatusEffects`), а не вычисленный итог.

## Интеграции с другими модулями

- `Restrictions`: `AdventureData` и `ChoiceData` используют `Restriction` для описания условий доступа.
- Остальные интеграции (сохранения, UI, события, инициализация) пока явно не реализованы в коде модуля.

## Целевые runtime-паттерны (следующий этап)

- **Вложенные приключения:** одно adventure может запускать другое через стек контекстов (push/pop), затем возвращать игрока в родительский сценарий.
- **Карта и локации:** глобальная навигация выносится в отдельный world/map JSON слой; adventure остается локальным графом сцен.
- **Стартовый хаб:** рекомендуемая стартовая локация — Таверна, где создается персонаж и выбирается доступное приключение.

## Предполагаемый runtime-поток (по модели данных)

1. Загрузка/получение `AdventureData`.
2. Проверка `AdventureData.Restictions` через `RestrictionsChecker`.
3. Выбор стартовой сцены из `StartScenes`.
4. Рендер `SceneData.Content`.
5. Формирование списка `Choices`:
   - либо по `AlwaysShow`;
   - либо по результату проверки `ChoiceData.Restictions`.
6. Для каждого `ChoiceActionData` из `Actions` фабрика создает `IChoiceActionExecutor` и вызывает `Execute()`.
7. Обновление `StateData.Adventures` и переход к следующей сцене (через контроллеры/менеджеры).

Полный runtime-поток еще не замкнут: `AdventuresManager` не реализован, `IAdventureFlowController` пока только объявлен.

## Текущее состояние реализации

- Реализованы доменные DTO/POCO-модели для adventure-данных и state-root.
- В `Modules.State` реализованы секции Adventure-профиля: `CharactersStateData`, `InventoryStateData`, `AdventuresStateData`; создание нового профиля — через `IAdventureStateDataFactory` (см. [State.md](State.md)).
- `ChoiceActionData` использует контракт `Params` (`Strings` / `Ints` / `Bools`).
- `ChoiceActionType` содержит базовый набор значений для переходов, проверок и боевых/ресурсных эффектов.
- Реализованы `ChoiceActionExecutorFactory`, `IChoiceActionExecutor`, `IChoiceActionExecutorFactory`.
- Реализованы executors: `GoToSceneChoiceActionExecutor`, `SetFlagChoiceActionExecutor`, `ModifyVariableChoiceActionExecutor` (два последних через `AdventureStateLogic`).
- Объявлен интерфейс `IAdventureFlowController`.
- `AdventuresManager` содержит только заготовку и не выполняет бизнес-логику.
- Отсутствуют DI-биндинги фабрики/контроллеров в installer, валидаторы adventure-данных, сериализация и тесты модуля.

## Рекомендации по дальнейшему развитию

1. Заполнить `SceneContentType` конкретными значениями.
2. Добавить executors и маппинг в фабрику для остальных `ChoiceActionType` (`SkillCheck`, `StartCombat` и т.д.).
3. Зарегистрировать в Zenject installer:
   - `IChoiceActionExecutorFactory -> ChoiceActionExecutorFactory`;
   - реализацию `IAdventureFlowController`.
4. Реализовать `AdventuresManager`:
   - запуск приключения;
   - вычисление доступных выборов;
   - применение списка `ChoiceActionData` через фабрику executors.
5. Подключить остальные `ChoiceActionType` к state-actions в `Modules.State`.
6. Добавить state-actions для персонажей (`char.*`) и инвентаря.
7. Добавить валидацию целостности adventure-данных (`StartScenes`, наличие ссылок в `Scenes`, корректность `Actions`).
8. Добавить unit-тесты на:
   - фабрику executors и валидацию `Params`;
   - проверку ограничений;
   - вычисление доступных choices;
   - корректность переходов по сценам и изменения состояния.
