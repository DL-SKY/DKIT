# Модуль RPG

**Последнее обновление:** 2026-06-15 12:15:00 (+03:00)

## Назначение

`RPG` — доменный каркас для adventure-геймплея в формате сцен и выборов.  
Модуль задает:

- структуру данных приключения (`AdventureData`);
- структуру сцены и ее контента (`SceneData`, `SceneContentData`);
- структуру выбора игрока и набора действий (`ChoiceData`, `ChoiceActionData`);
- базовый контейнер состояния прогресса (`StateData`, `PlayerData`, `AdventureStateData`).

На текущем этапе модуль содержит **data-contract слой** и начальный **runtime-слой выполнения действий** (`Choice/Executors`). Оркестраторы приключения и состояния пока в заготовочном состоянии.

## Структура модуля

- `Assets/Modules/RPG/Scripts/Adventure`
  - `AdventuresManager` — будущий оркестратор запуска/переходов по приключениям.
  - `IAdventureFlowController` — интерфейс контроллера переходов между сценами/узлами.
  - `Data/*` — модели adventure/scene/content.
  - `Choice/*` — модели выбора и действий по выбору.
  - `Choice/Executors/*` — фабрика и обработчики `ChoiceActionData`.
- `Assets/Modules/RPG/Scripts/State`
  - `StateManager` — будущий менеджер состояния RPG.
  - `IRpgVariablesController` — интерфейс изменения int-переменных/статов по строковому ключу.
  - `StateData`, `PlayerData`, `AdventureStateData` — root и секции состояния.

## Модель данных Adventure

### `AdventureData`

Описывает целое приключение:

- `Id` — уникальный идентификатор adventure.
- `Tags` — набор тегов для фильтрации/поиска/категоризации.
- `Title`, `Description` — метаданные и текстовое описание.
- `Restictions` — список ограничений на доступ к приключению (тип `Restriction` из модуля `Restrictions`).
- `StartScenes` — список стартовых сцен (по `Scene.Id`).
- `Scenes` — словарь `sceneId -> SceneData` со всем графом сцен.

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
  - `Params.Floats: Dictionary<string, float>`
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
- `ChoiceActionExecutorFactory` — фабрика на Zenject `DiContainer`, содержит `GetRequiredString/Int/Bool/Float` для валидации `Params`.

Реализованные executors (на текущий момент):

| `ChoiceActionType` | Executor | Обязательные `Params` | Контроллер |
|---|---|---|---|
| `GoToScene` | `GoToSceneChoiceActionExecutor` | `Strings.sceneId` | `IAdventureFlowController.GoToScene` |
| `ModifyVariable` | `ModifyVariableChoiceActionExecutor` | `Strings.key`, `Ints.delta` | `IRpgVariablesController.ModifyInt` |

Остальные значения `ChoiceActionType` объявлены в enum, но пока не подключены к фабрике.

## Ключи статов и параметров (договоренность)

Для персонажей, партии, квестов и мира рекомендуется хранить значения в состоянии через словари по строковым ключам:

- `Dictionary<string, int>`
- `Dictionary<string, string>`
- `Dictionary<string, bool>`
- опционально `Dictionary<string, float>`

Рекомендация по неймингу ключей:
- `char.*` — параметры персонажа (`char.hp`, `char.armor_class`);
- `party.*` — параметры группы;
- `quest.*` — квестовые флаги/счетчики;
- `world.*` — глобальные флаги мира/локаций.

## Слой состояния RPG

### `StateData`

Корневой контейнер состояния RPG:

- `Id` — идентификатор состояния/слота.
- `Player` — блок данных игрока (`PlayerData`).
- `Adventure` — блок данных прогресса adventure (`AdventureStateData`).

### `PlayerData` и `AdventureStateData`

Оба класса пока пустые и служат точками расширения:

- `PlayerData` — ожидаемые направления: статистика персонажа, ресурсы, прогресс мета-игры.
- `AdventureStateData` — ожидаемые направления: текущая сцена, пройденные сцены, флаги и переменные приключения.

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
7. Обновление `StateData.Adventure` и переход к следующей сцене (через контроллеры/менеджеры).

Полный runtime-поток еще не замкнут: `AdventuresManager` и `StateManager` не реализованы, а контроллеры (`IAdventureFlowController`, `IRpgVariablesController`) пока только объявлены как интерфейсы.

## Текущее состояние реализации

- Реализованы доменные DTO/POCO-модели для adventure-данных и state-root.
- `ChoiceActionData` переведен на контракт `Params` (`Strings/Ints/Bools/Floats`).
- `ChoiceActionType` содержит базовый набор значений для переходов, проверок и боевых/ресурсных эффектов.
- Реализованы `ChoiceActionExecutorFactory`, `IChoiceActionExecutor`, `IChoiceActionExecutorFactory`.
- Реализованы executors: `GoToSceneChoiceActionExecutor`, `ModifyVariableChoiceActionExecutor`.
- Объявлены интерфейсы контроллеров: `IAdventureFlowController`, `IRpgVariablesController`.
- `SceneContentType` пока остается точкой расширения без конкретных значений.
- `AdventuresManager` и `StateManager` содержат только конструкторы с `Debug.LogError(...)` и не выполняют бизнес-логику.
- Отсутствуют DI-биндинги фабрики/контроллеров в installer, валидаторы adventure-данных, сериализация и тесты модуля.

## Рекомендации по дальнейшему развитию

1. Заполнить `SceneContentType` конкретными значениями.
2. Добавить executors и маппинг в фабрику для остальных `ChoiceActionType` (`SetFlag`, `SkillCheck`, `StartCombat` и т.д.).
3. Зарегистрировать в Zenject installer:
   - `IChoiceActionExecutorFactory -> ChoiceActionExecutorFactory`;
   - реализации `IAdventureFlowController`, `IRpgVariablesController`.
4. Реализовать `AdventuresManager`:
   - запуск приключения;
   - вычисление доступных выборов;
   - применение списка `ChoiceActionData` через фабрику executors.
5. Расширить `AdventureStateData` и `PlayerData` минимально необходимыми полями прогресса (словари статов/флагов).
6. Добавить валидацию целостности adventure-данных (`StartScenes`, наличие ссылок в `Scenes`, корректность `Actions`).
7. Добавить unit-тесты на:
   - фабрику executors и валидацию `Params`;
   - проверку ограничений;
   - вычисление доступных choices;
   - корректность переходов по сценам и изменения состояния.
