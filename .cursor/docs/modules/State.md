# Модуль State

**Последнее обновление:** 2026-06-18 18:00:00 (+03:00)

## Назначение

`State` хранит и управляет профилем игрового состояния (загрузка, сохранение, удаление), опираясь на `Save`-модуль.

Все изменения прогресса игрока в runtime должны проходить через **state-actions** и исполняться через `StateLogic<TStateData>.ProcessAction(...)`.

## Краткая логика работы

### Загрузка и хранение

1. Конкретный менеджер (`StateManager<TStateData>` наследник) инициализируется параметрами сохранения или готовым `ISaveManager`.
2. `LoadProfileState(profileId)` пытается прочитать состояние из хранилища.
3. Если состояния нет — создается новое через `CreateNewState(profileId)` (для Adventure — через `IAdventureStateDataFactory`) и сразу сохраняется.
4. `SaveProfileState(profileId)` сохраняет текущее `State` (с optional light-encryption).
5. `DeleteProfileState(profileId)` удаляет профиль и очищает `State` в памяти.

### Изменение прогресса (state-actions)

1. Геймплейный код создает экшен с входными данными (например, `new ChangeWalletResourceStateAction<StateData>(type, delta)`).
2. Вызывает `stateLogic.ProcessAction(action)` или `ProcessAction(action, forceBatch: true)`.
3. `StateLogic` выполняет пайплайн: `Validate` → `Execute` → (опционально) `SaveState`.
4. Сохранение на диск батчится: не после каждого экшена, а по правилам `batchSize` / `forceBatch` (см. ниже).

## Структура папок

- `Scripts/Core` — `StateManager<TStateData>`.
- `Scripts/Interfaces` — `IStateData`, `IStateManager<TStateData>`.
- `Scripts/Actions` — общие контракты и исполнитель:
  - `Interfaces/IStateAction.cs`, `IStateLogic.cs`
  - `Models/StateActionValidationResult.cs`
  - `Core/StateActionBase.cs`, `StateLogic.cs`
- `Scripts/Implementation/Match3` — Match-3 state, manager, logic, actions.
- `Scripts/Implementation/Adventure` — Adventure state, manager, logic, actions, factories.
- `Scripts/Implementation/Wallet` — общие wallet-структуры, интерфейсы и actions.

## Организация файлов данных состояния

Для любого корневого `StateData : IStateData` действует единое соглашение:

1. **Корневой класс** (`StateData`) — один файл в папке реализации режима, например  
   `Implementation/Adventure/StateData.cs`.
2. **Поля-корни** (`Profile`, `Wallet`, `Characters`, `Inventory`, …) — **отдельный файл на каждую секцию** в подпапке `StateDatas/`, например  
   `StateDatas/CharactersStateData.cs`, `StateDatas/InventoryStateData.cs`.
3. **Вложенные типы секции** (`CharacterStateData`, `EquippedItemStateData`, `AdventureStateParamsData`, …) — **отдельные классы в том же `.cs`-файле и том же `namespace`**, объявленные ниже класса-контейнера секции.  
   Не вкладывать их как `nested class` внутрь контейнера.

Пример структуры для Adventure:

```text
Implementation/Adventure/
  StateData.cs                    ← корень: поля-секции
  AdventureStateManager.cs
  Factories/
    IAdventureStateDataFactory.cs
    AdventureStateDataFactory.cs  ← создание нового профиля
  StateDatas/
    ProfileStateData.cs
    CharactersStateData.cs        ← CharactersStateData + CharacterStateData + EquippedItemStateData
    InventoryStateData.cs
    AdventuresStateData.cs        ← AdventuresStateData + WorldStateData + AdventureStateData + AdventureStateParamsData
Implementation/Wallet/
  StateDatas/WalletStateData.cs   ← общая секция, переиспользуется в режимах
```

При добавлении новой секции:

1. создать `*StateData.cs` в `StateDatas/`;
2. при необходимости описать дочерние POCO в том же файле;
3. добавить поле в корневой `StateData`;
4. инициализировать дефолты в `AdventureStateDataFactory` (или аналогичной фабрике режима).

## Основные классы

- `StateManager<TStateData>`  
  Базовая generic-реализация пайплайна load/save/delete.

- `IStateManager<TStateData>`  
  Интерфейс менеджера состояния.

- `IStateData`  
  Маркерный интерфейс для структуры данных состояния.

- `Match3StateManager` / `AdventureStateManager`  
  Конкретные менеджеры для Match-3 и Adventure. Содержат `SaveState()` — обертку над `SaveProfileState` с настройками из `DefinitionsManager`.  
  `AdventureStateManager.CreateNewState()` делегирует создание профиля фабрике через `DiContainer`.

- `IAdventureStateDataFactory` / `AdventureStateDataFactory`  
  Фабрика начального состояния Adventure-профиля. Создаёт и инициализирует все секции `StateData`.  
  Инжектируется `DefinitionsManager` — для будущего использования дефов при старте нового игрока.  
  Регистрация в DI: `IAdventureStateDataFactory → AdventureStateDataFactory`, `AsTransient()`.

- `StateLogic<TStateData>`  
  Единая точка применения state-actions. Принимает `IStateManager`, callback сохранения и `batchSize`.

- `Match3StateLogic` / `AdventureStateLogic`  
  Наследники `StateLogic<StateData>` для конкретных веток. Зарегистрированы в DI (`ProjectInstaller`).

- `IStateAction<TStateData>` / `StateActionBase<TStateData>`  
  Контракт экшена: `Validate(state)` + `Execute(state)`. В конструктор передаются только входные данные действия, не ссылка на `State`.

- `StateActionValidationResult`  
  Результат валидации (`Ok` / `Fail(message, errorCode)`).

- `StateData`  
  Корневые данные профиля.
  - Match-3: `Profile`, `Wallet`, `Hangar`, `Storage`.
  - Adventure: `Profile`, `Wallet`, `Characters`, `Inventory`, `Adventures`.

- `WalletStateData` / `IWalletStateDataOwner`  
  Общая структура кошелька вынесена в `Implementation/Wallet` и переиспользуется в Match-3 и Adventure. Оба `StateData` реализуют `IWalletStateDataOwner`.

### Adventure: `CharactersStateData` и `CharacterStateData`

Файл: `Implementation/Adventure/StateDatas/CharactersStateData.cs`.

`CharactersStateData` — ростер персонажей профиля и текущий отряд:

| Поле | Тип | Назначение |
|------|-----|------------|
| `NextCharacterId` | `int` | Счётчик для выдачи новых runtime-id персонажей |
| `HeroPoints` | `int` | Очки героя на уровне профиля (ресурс кампании) |
| `Characters` | `Dictionary<int, CharacterStateData>` | Все персонажи профиля (живые и погибшие) |
| `ActivePartyCharacterIds` | `List<int>` | Текущий отряд: упорядоченный список id персонажей |

`CharacterStateData` — данные одного персонажа (в том же файле):

| Поле | Тип | Назначение |
|------|-----|------------|
| `Id` | `int` | Runtime-id; должен совпадать с ключом в `Characters` |
| `CreateTime` | `long` | Время создания персонажа, Unix ms UTC |
| `IsDead` | `bool` | Признак смерти (доска славы, исключение из отряда) |
| `DeathTime` | `long` | Время смерти, Unix ms UTC; `0` — не умер |
| `Name` | `string` | Отображаемое имя персонажа |
| `Ancestry` | `string` | Id дефа происхождения/расы |
| `Class` | `string` | Id дефа класса |
| `Level` | `int` | Уровень персонажа |
| `Experience` | `int` | Опыт персонажа |
| `Parameters` | `Dictionary<string, int>` | Числовые параметры: abilities, skills, HP, speed, feats и т.д. |
| `SavingThrows` | `Dictionary<string, int>` | Спасброски |
| `EquippedItems` | `List<EquippedItemStateData>` | Надетая экипировка |
| `Spells` | `Dictionary<string, int>` | Заклинания |
| `StatusEffects` | `Dictionary<string, int>` | Статусные эффекты: id эффекта → значение |

`EquippedItemStateData` — одна запись экипировки (в том же файле):

| Поле | Тип | Назначение |
|------|-----|------------|
| `Slot` | `string` | Идентификатор слота (`HAND`, `BAG`, …) |
| `ItemId` | `string` | Id дефа предмета (имя JSON-файла дефа) |

**Соглашения по id:**
- runtime-сущности, создаваемые игрой (персонажи) — `int`, выдаются через `NextCharacterId`;
- ссылки на контент из дефов — `string` (имя дефа / id из `Definitions`).

### Adventure: `InventoryStateData`

Файл: `Implementation/Adventure/StateDatas/InventoryStateData.cs`.

`InventoryStateData` — инвентарь отряда (отдельно от персонажей):

| Поле | Тип | Назначение |
|------|-----|------------|
| `Items` | `Dictionary<string, int>` | Предметы и расходники: defId → количество (общий пул отряда) |

**Разделение экипировки:**
- стакающиеся предметы и расходники — в `Inventory.Items`;
- надетая экипировка персонажа — в `CharacterStateData.EquippedItems` (слот + defId предмета).

### Adventure: `AdventuresStateData` и связанные типы

Файл: `Implementation/Adventure/StateDatas/AdventuresStateData.cs`.

`AdventuresStateData` — прогресс приключений и мира:

| Поле | Тип | Назначение |
|------|-----|------------|
| `World` | `WorldStateData` | Глобальные параметры мира/кампании |
| `Adventures` | `Dictionary<string, AdventureStateData>` | Прогресс по отдельным приключениям: adventureId → состояние |

`WorldStateData` (в том же файле):

| Поле | Тип | Назначение |
|------|-----|------------|
| `Parameters` | `AdventureStateParamsData` | Параметры мира (`world.*` и др.) |

`AdventureStateData` (в том же файле) — **прогресс одного приключения в сейве** (не путать с контентным `Modules.RPG.Scripts.Adventure.Data.AdventureData`):

| Поле | Тип | Назначение |
|------|-----|------------|
| `AdventureId` | `string` | Id приключения из контента |
| `SceneId` | `string` | Текущая сцена |
| `Parameters` | `AdventureStateParamsData` | Локальные флаги/переменные приключения |

`AdventureStateParamsData` (в том же файле) — универсальный контейнер параметров:

| Поле | Тип | Назначение |
|------|-----|------------|
| `Strings` | `Dictionary<string, string>` | Строковые значения |
| `Ints` | `Dictionary<string, int>` | Числовые значения |
| `Bools` | `Dictionary<string, bool>` | Булевы флаги |

Формат `AdventureStateParamsData` согласован с `ChoiceActionParamsData` в модуле RPG (`Strings` / `Ints` / `Bools`).

**Резолвинг ключей в state-actions прогресса:**
- `world.*` → `Adventures.World.Parameters`;
- `adventure.*` → `Adventures[adventureId].Parameters` (нужен `adventureId` в state-action / в `Params.Strings.adventureId` у choice-action).

### Adventure: создание нового профиля

`AdventureStateManager` не собирает секции напрямую — делегирует фабрике:

```csharp
protected override StateData CreateNewState(string profileId)
{
    var factory = _container.Resolve<IAdventureStateDataFactory>();
    return factory.Create(profileId);
}
```

Для интерфейса используется `Resolve` (по DI-биндингу), а не `Instantiate` — Zenject не может напрямую инстанцировать абстрактный тип/интерфейс.

`AdventureStateDataFactory.Create(profileId)` возвращает `StateData` с инициализированными секциями:

| Секция | Дефолты при создании |
|--------|----------------------|
| `Profile` | `CreateTime`, `UpdateTime` = текущее Unix ms UTC |
| `Wallet` | пустой `Resources` |
| `Characters` | `NextCharacterId = 1`, `HeroPoints = 0`, пустые `Characters`, `ActivePartyCharacterIds` |
| `Inventory` | пустой `Items` |
| `Adventures` | `World` с пустыми `Parameters`; пустой словарь `Adventures` |

**DI (Adventure `ProjectInstaller`):**

```csharp
Container.Bind<IAdventureStateDataFactory>().To<AdventureStateDataFactory>().AsTransient();
```

`AsTransient()` — при каждом `Resolve` создаётся новый экземпляр фабрики (не singleton). После выхода из `CreateNewState()` ссылок на фабрику нет; Zenject её явно не удаляет — объект собирается GC.

## Батчинг сохранений

`StateLogic` накапливает успешно примененные экшены во внутреннем счетчике `_pendingActionCount`.

Сохранение (`SaveState`) вызывается только если:

1. Счетчик достиг `batchSize` — после сохранения счетчик обнуляется.
2. В `ProcessAction` передан `forceBatch: true` — сохранение выполняется немедленно, счетчик не сбрасывается (если не достигнут `batchSize`).

По умолчанию `forceBatch = false`.

Пример (`batchSize = 10`):

- экшены 1–9 → изменения в памяти, без записи на диск;
- экшен 10 → сохранение + сброс счетчика;
- экшен 3 с `forceBatch: true` → сохранение на 3-м экшене, счетчик остается 3.

Текущий дефолтный `batchSize` в `Match3StateLogic` и `AdventureStateLogic`: `10`.

## Примеры вызова

```csharp
// Обычное изменение (сохранение по batchSize)
stateLogic.ProcessAction(new ChangeWalletResourceStateAction<StateData>(resourceType, delta));

// Принудительное сохранение после экшена
stateLogic.ProcessAction(new SetProfileUpdateTimeStateAction(updateTime), forceBatch: true);
```

## Готовые state-actions

- `ChangeWalletResourceStateAction<TStateData>` — общий экшен кошелька (Match-3 и Adventure).
- `SetProfileUpdateTimeStateAction` — обновление `Profile.UpdateTime` (Adventure).
- `SetAdventureProgressBoolStateAction` — установка bool-параметра в `AdventuresStateData` (`world.*` / `adventure.*`).
- `ModifyAdventureProgressIntStateAction` — изменение int-параметра в `AdventuresStateData` (`world.*` / `adventure.*`).

## Как добавить новый state-action

1. Создать класс в `Implementation/<Mode>/Actions/` (или в `Implementation/Wallet/Actions/` для общих секций).
2. Унаследовать от `StateActionBase<TStateData>`.
3. Принять в конструктор только данные, нужные для изменения.
4. Переопределить `Validate(state)` при необходимости.
5. Реализовать `Execute(state)` — единственное место мутации соответствующей секции состояния.
6. Вызывать через `stateLogic.ProcessAction(new YourAction(...))`.

Для общих секций (например, wallet) можно использовать marker-интерфейс вроде `IWalletStateDataOwner` и generic-экшен с ограничением `where TStateData : IWalletStateDataOwner`.

## Как добавить новый state-модуль/профиль

1. Создать корневой класс данных (например, `RpgStateData`), реализующий `IStateData`, в `Implementation/<Mode>/StateData.cs`.
2. Для каждой секции состояния создать `*StateData.cs` в `StateDatas/`; дочерние POCO — отдельными классами в том же файле (см. раздел «Организация файлов данных состояния»).
3. Создать менеджер `RpgStateManager : StateManager<RpgStateData>`.
4. Создать фабрику начального состояния (по аналогии с `IAdventureStateDataFactory`) и вызывать её из `CreateNewState()` через `DiContainer`.
5. Создать `RpgStateLogic : StateLogic<RpgStateData>` с нужным `batchSize`.
6. Реализовать дефолты секций в фабрике (пустые словари, начальные счётчики `Next*Id`).
7. Добавить папку `Actions/` и state-actions для изменений прогресса.
8. Зарегистрировать менеджер, logic и фабрику в DI-инсталлере.
9. На этапе инициализации вызвать:
   - `Init(folder, extension, key)` или `Init(ISaveManager)`;
   - `LoadProfileState(profileId, encryptionFlags...)`.
10. Проверить жизненный цикл:
   - первое создание профиля;
   - загрузка существующего;
   - применение экшенов и батч-сохранение;
   - `forceBatch: true` в критичных точках (выход, пауза, завершение уровня).

## Текущий статус runtime

- На старте приложения state загружается/создается через `Match3StateInitTask` / `AdventureStateInitTask`.
- Для Adventure новый профиль создаётся через `IAdventureStateDataFactory` (`AdventureStateDataFactory`).
- Реализованы секции Adventure state: `Characters`, `Inventory`, `Adventures` (см. выше).
- Прямых gameplay-мутаций `State` вне state-actions сейчас нет.
- `ProcessAction` пока не вызывается из геймплейного кода — инфраструктура готова к подключению.
