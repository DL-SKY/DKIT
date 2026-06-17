# Модуль State

## Назначение

`State` хранит и управляет профилем игрового состояния (загрузка, сохранение, удаление), опираясь на `Save`-модуль.

Все изменения прогресса игрока в runtime должны проходить через **state-actions** и исполняться через `StateLogic<TStateData>.ProcessAction(...)`.

## Краткая логика работы

### Загрузка и хранение

1. Конкретный менеджер (`StateManager<TStateData>` наследник) инициализируется параметрами сохранения или готовым `ISaveManager`.
2. `LoadProfileState(profileId)` пытается прочитать состояние из хранилища.
3. Если состояния нет — создается новое через `CreateNewState(profileId)` и сразу сохраняется.
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
- `Scripts/Implementation/Adventure` — Adventure state, manager, logic, actions.
- `Scripts/Implementation/Wallet` — общие wallet-структуры, интерфейсы и actions.

## Основные классы

- `StateManager<TStateData>`  
  Базовая generic-реализация пайплайна load/save/delete.

- `IStateManager<TStateData>`  
  Интерфейс менеджера состояния.

- `IStateData`  
  Маркерный интерфейс для структуры данных состояния.

- `Match3StateManager` / `AdventureStateManager`  
  Конкретные менеджеры для Match-3 и Adventure. Содержат `SaveState()` — обертку над `SaveProfileState` с настройками из `DefinitionsManager`.

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
  - Adventure: `Profile`, `Wallet`, `Characters`, `Adventures`.

- `WalletStateData` / `IWalletStateDataOwner`  
  Общая структура кошелька вынесена в `Implementation/Wallet` и переиспользуется в Match-3 и Adventure. Оба `StateData` реализуют `IWalletStateDataOwner`.

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
- `SetProfileUpdateTimeStateAction` — обновление `Profile.UpdateTime` (отдельные реализации для Match-3 и Adventure).

## Как добавить новый state-action

1. Создать класс в `Implementation/<Mode>/Actions/` (или в `Implementation/Wallet/Actions/` для общих секций).
2. Унаследовать от `StateActionBase<TStateData>`.
3. Принять в конструктор только данные, нужные для изменения.
4. Переопределить `Validate(state)` при необходимости.
5. Реализовать `Execute(state)` — единственное место мутации соответствующей секции состояния.
6. Вызывать через `stateLogic.ProcessAction(new YourAction(...))`.

Для общих секций (например, wallet) можно использовать marker-интерфейс вроде `IWalletStateDataOwner` и generic-экшен с ограничением `where TStateData : IWalletStateDataOwner`.

## Как добавить новый state-модуль/профиль

1. Создать класс данных состояния (например, `RpgStateData`), реализующий `IStateData`.
2. Создать менеджер `RpgStateManager : StateManager<RpgStateData>`.
3. Создать `RpgStateLogic : StateLogic<RpgStateData>` с нужным `batchSize`.
4. Реализовать `CreateNewState(profileId)` с валидными дефолтами домена.
5. Добавить папку `Actions/` и state-actions для изменений прогресса.
6. Зарегистрировать менеджер и logic в DI-инсталлере.
7. На этапе инициализации вызвать:
   - `Init(folder, extension, key)` или `Init(ISaveManager)`;
   - `LoadProfileState(profileId, encryptionFlags...)`.
8. Проверить жизненный цикл:
   - первое создание профиля;
   - загрузка существующего;
   - применение экшенов и батч-сохранение;
   - `forceBatch: true` в критичных точках (выход, пауза, завершение уровня).

## Текущий статус runtime

- На старте приложения state загружается/создается через `Match3StateInitTask` / `AdventureStateInitTask`.
- Прямых gameplay-мутаций `State` вне state-actions сейчас нет.
- `ProcessAction` пока не вызывается из геймплейного кода — инфраструктура готова к подключению.
