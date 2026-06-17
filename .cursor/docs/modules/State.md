# Модуль State

## Назначение

`State` хранит и управляет профилем игрового состояния (загрузка, сохранение, удаление), опираясь на `Save`-модуль.

## Краткая логика работы

1. Конкретный менеджер (`StateManager<TStateData>` наследник) инициализируется параметрами сохранения или готовым `ISaveManager`.
2. `LoadProfileState(profileId)` пытается прочитать состояние из хранилища.
3. Если состояния нет — создается новое через `CreateNewState(profileId)` и сразу сохраняется.
4. `SaveProfileState(profileId)` сохраняет текущее `State` (с optional light-encryption).
5. `DeleteProfileState(profileId)` удаляет профиль и очищает `State` в памяти.

## Основные классы

- `StateManager<TStateData>`  
  Базовая generic-реализация пайплайна load/save/delete.

- `IStateManager<TStateData>`  
  Интерфейс менеджера состояния.

- `IStateData`  
  Маркерный интерфейс для структуры данных состояния.

- `Match3StateManager`  
  Конкретная реализация для Match-3 с дефолтной инициализацией состояния.

- `AdventureStateManager`  
  Конкретная реализация для Adventure, использующая `Modules.Definitions.Scripts.Implementation.Adventures.DefinitionsManager`.

- `StateData`  
  Корневые данные профиля.
  - Match-3: `Profile`, `Wallet`, `Hangar`, `Storage`.
  - Adventure: `Profile`, `Wallet`, `Characters`, `Adventures`.

- `WalletStateData`
  Общая структура кошелька вынесена в `Modules.State.Scripts.Implementation.Wallet.StateDatas` и переиспользуется в Match-3 и Adventure.

## Как добавить новый state-модуль/профиль

1. Создать класс данных состояния (например, `RpgStateData`), реализующий `IStateData`.
2. Создать менеджер `RpgStateManager : StateManager<RpgStateData>`.
3. Реализовать `CreateNewState(profileId)` с валидными дефолтами домена.
4. Зарегистрировать новый менеджер в DI-инсталлере.
5. На этапе инициализации вызвать:
   - `Init(folder, extension, key)` или `Init(ISaveManager)`;
   - `LoadProfileState(profileId, encryptionFlags...)`.
6. Проверить жизненный цикл:
   - первое создание профиля;
   - загрузка существующего;
   - сохранение изменений и последующая повторная загрузка.

