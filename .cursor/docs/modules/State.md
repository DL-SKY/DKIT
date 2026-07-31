# Модуль State

**Последнее обновление:** 2026-07-31 12:45:00 (+03:00)

## Назначение

`State` хранит и управляет профилем игрового состояния (загрузка, сохранение, удаление), опираясь на `Save`-модуль.

Все изменения прогресса игрока в runtime должны проходить через **state-actions** и исполняться через `StateLogic<TStateData>.ProcessAction(...)`.

> **Бой (план):** персистентные итоги боя (HP, устойчивые статусы) остаются в `CharacterStateData`; эфемерные счётчики раунда (MAP, AP) — в runtime session, не в сейве. См. [Battle.md](Battle.md).
>
> **Choice-actions (Adventure):** executors выборов игрока в приключениях могут мутировать только секции **`Characters` / `Inventory` / `Adventures`**. `Profile` / `Wallet` / `Localization` — вне choice-pipeline (UI, init, cheats, meta). См. [RPG.md — Инвариант секций State для choice-actions](RPG.md#инвариант-секций-state-для-choice-actions).

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
3. `StateLogic` выполняет пайплайн: `Validate` → `Execute` → `StateChanged` → (опционально) `SaveState`.
4. После успешного `Execute` подписчики получают `StateChanged` с `StateChangeSource` из экшена.
5. Сохранение на диск батчится: не после каждого экшена, а по правилам `batchSize` / `forceBatch` (см. ниже).

## Структура папок

- `Scripts/Core` — `StateManager<TStateData>`.
- `Scripts/Interfaces` — `IStateData`, `IStateManager<TStateData>`.
- `Scripts/Actions` — общие контракты и исполнитель:
  - `Interfaces/IStateAction.cs`, `IStateLogic.cs`
  - `Models/StateActionValidationResult.cs`, `StateChangeSource.cs`
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
  CharacterParametersProxy.cs     ← Read API: GetRawValue / GetTotalValue
  CharacterParametersOperator.cs  ← Write API: Apply/Unapply patch + Feat (+ StatusEffects timer for Condition); CreateCharacterRequestSnapshot
  CharacterItemFeaturesOperator.cs ← ItemDef.Features Apply/Unapply с правилом Bag (GrantsItemFeatures)
  CreatureCombatantFactory.cs     ← CreatureDef → CharacterStateData (+ CloneForBattle для копий партии)
  WeaponProxy.cs                  ← attack/damage modifiers по ItemDef формулам
  Actions/
    EndCharacterTurnStateAction.cs ← декремент таймеров Condition / Unapply при 0
    Models/EndCharacterTurnRequestData.cs
  Factories/
    IAdventureStateDataFactory.cs
    AdventureStateDataFactory.cs  ← создание нового профиля
  StateDatas/
    ProfileStateData.cs
    CharactersStateData.cs        ← CharactersStateData + CharacterGender + ProficiencyType + CharacterStateData + EquippedItemStateData
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
  Регистрация в DI: `IAdventureStateDataFactory → AdventureStateDataFactory`, `AsTransient()`.

- `CharacterParametersProxy`  
  **Read API** параметров персонажа: сырые значения из `CharacterStateData.Parameters` и вычисляемые по `RuleDef.ParameterFormulas` (навыки, `MaxHitPoints`).  
  Зависит от `DefinitionsManager` (Adventures) для `ANCESTRY_HP` / `CLASS_HP`. Конвенция: читать через proxy (`GetRawValue` / `GetTotalValue`); мутации — через `CharacterParametersOperator` (Apply / Unapply). Подробнее — [ниже](#adventure-characterparametersproxy-read-api).

- `CharacterParametersOperator`  
  **Write API** сырых `Parameters`: `ApplyPatch` / `UnapplyPatch`, `ApplyFeat` / `UnapplyFeat`, слепок `CreateCharacterRequestSnapshot`. Подробнее — [ниже](#adventure-write-api-параметров-apply--unapply).

- `CharacterItemFeaturesOperator`  
  Apply / Unapply `ItemDef.Features` с правилом Bag (`Glossary.Items.GrantsItemFeatures`). Используется из equip/unequip/move/remove state-actions.

- `CreatureCombatantFactory`  
  `CreatureDef` → валидный `CharacterStateData` для боя (`CreateFromCreature`: отрицательный session id, запекание статблока, `ApplyFeat` / item Features). `CloneForBattle` — копии party в session. Не пишет в профиль. Практика: [Creatures.md](Creatures.md); также [Battle.md](Battle.md).

- `WeaponProxy`  
  Прокси модификаторов оружия по формулам `ItemDef.AttackModifierFormula` / `DamageModifierFormula`.  
  Читает сырые ключи из `CharacterStateData.Parameters` (профа по `Type`, item/group bonuses). Подробнее — [ниже](#adventure-weaponproxy).

- `StateLogic<TStateData>`  
  Единая точка применения state-actions. Принимает `IStateManager`, callback сохранения и `batchSize`.  
  После успешного `Execute` вызывает `StateChanged` с `action.Source`.

- `Match3StateLogic` / `AdventureStateLogic`  
  Наследники `StateLogic<StateData>` для конкретных веток. Зарегистрированы в DI (`ProjectInstaller`).

- `IStateLogic<TStateData>`  
  Контракт logic: `ProcessAction(...)` и событие `StateChanged`.

- `StateChangeSource`  
  Enum категорий областей стейта, которые мутировал экшен (не имя конкретного action).  
  Потребители фильтруют по категории. Multi-section экшены (equip и т.п.) используют `CharactersAndInventory`.

  | Категория | Секция / смысл | Типичные экшены |
  |---|---|---|
  | `Profile` | `Profile` (UpdateTime, Parameters) | `SetProfileUpdateTime*`, `Add/SetProfileParameter` |
  | `Wallet` | `Wallet` | `Change/SetWalletResource` |
  | `Localization` | `Localization` | `SetLocalizationLanguage` |
  | `Characters` | `Characters` (sheet, party, active, HP, turn, slots) | `Create/UpdateCharacter`, party/active, `EndCharacterTurn`, `ChangeHeroPoints`, `RemoveCharacterEquippedItem` |
  | `Inventory` | `Inventory.Items` | `Add/RemoveInventoryItems` |
  | `CharactersAndInventory` | обе секции | `Equip/Unequip/MoveEquipped*`, `AddCharacterItem` |
  | `AdventuresNavigation` | `CurrentAdventureId` / `CurrentAdventureSceneId` | `SetCurrentAdventureId/SceneId` |
  | `AdventuresParams` | World / Adventure / Global params | `SetWorld/Adventure/GlobalParams` |

- `IStateAction<TStateData>` / `StateActionBase<TStateData>`  
  Контракт экшена: read-only `Source`, `Validate(state)`, `Execute(state)`. В конструктор передаются только входные данные действия, не ссылка на `State`.

- `CreateCharacterRequestData`  
  DTO входных данных для `CreateCharacterStateAction` (единый буфер для ручного создания и выбора прегена): персонажные поля (`Name`, `Avatar`, `Gender`, `Ancestry`, `Class`, `Background`), блок `CharacterData` (`CharacterRequestData`) и флаг `AddToActiveParty`.

- `CharacterRequestData`  
  Переиспользуемый блок изменяемых данных персонажа: `Parameters`, `EquippedItems`, `Spells`, `StatusEffects`. Используется в `CreateCharacterRequestData` и `UpdateCharacterRequestData`.  
  Методы `ApplyFeat` / `UnapplyFeat` / `ApplyPatch` / `UnapplyPatch` делегируют в `CharacterParametersOperator`.  
  `ApplyFeat` / `UnapplyFeat` на DTO передают и `Parameters`, и `StatusEffects` (нужно для timed Condition).

- `EndCharacterTurnRequestData`  
  DTO для `EndCharacterTurnStateAction`: `CharacterId`. Декремент таймеров Condition в `StatusEffects`, тик ongoing damage, `UnapplyFeat` при `0` (см. [Feats.md](Feats.md)).

- `UpdateCharacterRequestData`  
  DTO для `UpdateCharacterStateAction`: `CharacterId` и `CharacterData` (`CharacterRequestData`).  
  **Контракт:** только прокачка (новый уровень). UI: `CreateCharacterRequestSnapshot` → последовательные Apply на DTO → `ProcessAction(UpdateCharacter…)`. Тонкие делегаты `ApplyFeat` / … на `CharacterData`.

- `EquipItemFromInventoryRequestData`  
  DTO для `EquipItemFromInventoryStateAction`: `CharacterId`, `SlotIndex` (индекс в `EquippedItems`), `ItemId` (предмет из `Inventory.Items`).

- `UnequipItemToInventoryRequestData`  
  DTO для `UnequipItemToInventoryStateAction`: `CharacterId`, `SlotIndex`.

- `MoveEquippedItemBetweenSlotsRequestData`  
  DTO для `MoveEquippedItemBetweenSlotsStateAction`: `CharacterId`, `FromSlotIndex`, `ToSlotIndex` (оба — индексы в `EquippedItems`).

- `AddInventoryItemsRequestData`  
  DTO для `AddInventoryItemsStateAction`: `ItemId`, `Count` (1..N).

- `RemoveInventoryItemsRequestData`  
  DTO для `RemoveInventoryItemsStateAction`: `ItemId`, `Count` (1..N). Валидация требует, чтобы предмет **был** в инвентаре (`count > 0`); удаление большего количества, чем есть, допустимо и clamp-ится до 0.

- `AddCharacterItemRequestData`  
  DTO для `AddCharacterItemStateAction`: `CharacterId`, `ItemId` (поштучно). Сначала свободный `Bag`-слот, иначе общий инвентарь.

- `RemoveCharacterEquippedItemRequestData`  
  DTO для `RemoveCharacterEquippedItemStateAction`: `CharacterId`, `SlotIndex`, `ItemId`. `SlotIndex = -1` — найти первый слот с этим `ItemId`. Удаление без возврата в `Inventory.Items`.

- `InventoryItemsOperator`  
  Вспомогательный оператор общего инвентаря: `GetCount`, `TryConsume`, `TryAdd` (возвращает фактическое добавленное количество; capacity stub пока = requested), `Add`, `RemoveUpTo` (`Max(0, current - amount)`).

- `StateActionValidationResult`  
  Результат валидации (`Ok` / `Fail(message, errorCode)`).

- `StateData`  
  Корневые данные профиля.
  - Match-3: `Profile`, `Wallet`, `Hangar`, `Storage`.
  - Adventure: `Profile`, `Wallet`, `Localization`, `Characters`, `Inventory`, `Adventures`.

- `WalletStateData` / `IWalletStateDataOwner`  
  Общая структура кошелька вынесена в `Implementation/Wallet` и переиспользуется в Match-3 и Adventure. Оба `StateData` реализуют `IWalletStateDataOwner`.

- `LocalizationStateData` / `ILocalizationStateDataOwner`  
  Секция выбранного языка для Adventure-профиля.  
  Поле: `Language` (`SystemLanguage`, по умолчанию `Unknown`).  
  Сериализация в save выполняется строковым именем enum (`"Russian"`, `"English"`), а не числовым кодом.

### Adventure: `ProfileStateData`

Файл: `Implementation/Adventure/StateDatas/ProfileStateData.cs`.

| Поле | Тип | Назначение |
|------|-----|------------|
| `CreateTime` / `UpdateTime` | `long` | Unix ms UTC |
| `Parameters` | `Dictionary<string, int>` | Счётчики/флаги/лимиты профиля; нет ключа → `0` |

Ключи `Parameters` — `Glossary.ProfileState` (`Modules.Definitions`):

| Константа | Значение | Назначение |
|-----------|----------|------------|
| `MAX_PARTY_SLOTS` | `"MaxPartySlots"` | Макс. слотов активной партии (`AddCharacterToActivePartyStateAction`) |
| `MAX_INVENTORY_SLOTS` | `"MaxInventorySlots"` | Макс. слотов общего инвентаря |

Дефолты при создании профиля — из `ProfileStateSettingsDef.DefaultParameters` (`Definitions/_ADVENTURES_/ProfileStateSettings/ProfileStateSettings.json`; сейчас `MaxPartySlots: 4`, `MaxInventorySlots: 20`). Мутация runtime — `SetProfileParameterStateAction` / `AddProfileParameterStateAction` (**не** из adventure choice-executors).

### Adventure: `CharactersStateData` и `CharacterStateData`

Файл: `Implementation/Adventure/StateDatas/CharactersStateData.cs`.

`CharactersStateData` — ростер персонажей профиля и текущий отряд:

| Поле | Тип | Назначение |
|------|-----|------------|
| `NextCharacterId` | `int` | Счётчик для выдачи новых runtime-id персонажей |
| `HeroPoints` | `int` | Очки героя на уровне профиля (ресурс кампании); мутация через `ChangeHeroPointsStateAction` |
| `Characters` | `Dictionary<int, CharacterStateData>` | Все персонажи профиля (живые и погибшие) |
| `CurrentActiveCharacterId` | `int` | Текущий выбранный персонаж UI/геймплея; `0` — не выбран; мутация через `SetCurrentActiveCharacterIdStateAction` |
| `ActivePartyCharacterIds` | `List<int>` | Текущий отряд: упорядоченный список id; лимит — `Profile.Parameters[MAX_PARTY_SLOTS]`; `AddCharacterToActiveParty` / `RemoveCharacterFromActiveParty` |

`CharacterGender` — пол персонажа (в том же файле):

| Значение | Описание |
|----------|----------|
| `Male` | Мужской |
| `Female` | Женский |

`ProficiencyType` — ранг владения навыком (в том же файле; значение хранится в `Parameters` по ключу `<SkillId> + Glossary.Characters.PROFICIENCY_SUFFIX`, например `Athletics.ProfRank`):

| Значение | Описание |
|----------|----------|
| `Untrained` | Не изучено (бонус владения `0`) |
| `Trained` | Обучен (`Level + 2`) |
| `Expert` | Эксперт (`Level + 4`) |
| `Master` | Мастер (`Level + 6`) |
| `Legendary` | Легендарный (`Level + 8`) |

`CharacterStateData` — данные одного персонажа (в том же файле):

| Поле | Тип | Назначение |
|------|-----|------------|
| `Id` | `int` | Runtime-id; должен совпадать с ключом в `Characters` |
| `CreateTime` | `long` | Время создания персонажа, Unix ms UTC |
| `IsDead` | `bool` | Признак смерти (доска славы, исключение из отряда) |
| `DeathTime` | `long` | Время смерти, Unix ms UTC; `0` — не умер |
| `Name` | `string` | Отображаемое имя персонажа |
| `Gender` | `CharacterGender` | Пол персонажа; при генерации имени используется с `AncestryDef.Names`; аватары — каталог `AvatarsDef` по `Ancestry` |
| `Ancestry` | `string` | Id дефа ancestry (`AncestryDef`) |
| `Class` | `string` | Id дефа класса (`ClassDef`) |
| `Background` | `string` | Id дефа предыстории (`BackgroundDef`) |
| `Parameters` | `Dictionary<string, int>` | Сырое хранилище: abilities, level/experience, proficiency ranks, item/per-level/flat bonuses, текущие HP, speed, feat-флаги и т.д. Итоги (`MaxHitPoints`, навыки) **не** хранятся — `CharacterParametersProxy.GetTotalValue`. Чтение — через proxy; запись — через Write API (Apply / Unapply) |
| `EquippedItems` | `List<EquippedItemStateData>` | Надетая экипировка |
| `Spells` | `Dictionary<string, int>` | Заклинания |
| `StatusEffects` | `Dictionary<string, int>` | Таймеры timed Condition-feats: **ключ = id `FeatDef`**, **value = оставшиеся ходы**. Пишется `CharacterParametersOperator` при Apply Condition с `ConditionDuration > 0`; декремент — `EndCharacterTurnStateAction` |

`EquippedItemStateData` — одна запись экипировки (в том же файле):

| Поле | Тип | Назначение |
|------|-----|------------|
| `Slot` | `string` | Тип слота из `Glossary.Items` (`Hand`, `Legs`, `Head`, `Body`, `Bag`, `Finger`, `Neck`, `Tail`). Несколько одинаковых типов допустимы (список повторяемых записей, без индексов) |
| `ItemId` | `string` | Id дефа предмета (`ItemDef`, имя JSON-файла); пустой/`null` — свободный слот |

Базовый набор слотов обычно собирается из `ClassDef.EquippedItems` (оружие/броня/`Bag`) и `AncestryDef.EquippedItems` (украшения: `Finger` / `Neck` / `Tail` и т.п.); также может задаваться в `PregeneratedCharacterDef.EquippedItems`. Дополнительные слоты могут выдавать черты через `FeatDef.AdditionalSlots`. Предмет можно положить в слот только если тип слота есть в `ItemDef.AvailableSlots`.

**`ItemDef.Features` и слоты (правило Bag):**
- Features применяются только если предмет лежит в слоте, для которого `Glossary.Items.GrantsItemFeatures(slot) == true` (все слоты **кроме** `Bag`);
- в `Bag` / общем `Inventory.Items` Features **не** применяются;
- перенос из носимого слота (`Hand`, `Finger`, …) в `Bag` → **Unapply** Features; из `Bag` в носимый → **Apply**;
- Move между двумя носимыми слотами → Features не трогать (предмет остаётся «надет»).

**Соглашения по id:**
- runtime-сущности party — `int` > 0 через `NextCharacterId`;
- session NPC / противники в бою — `int` < 0 (не инкрементят `NextCharacterId`, не пишутся в профиль);
- ссылки на контент из дефов — `string` (имя дефа / id из `Definitions`; типы дефов — в [Definitions.md](Definitions.md)).

**`Parameters` (сырое хранилище):**
- ключи abilities/skills/level согласованы с `Glossary.Characters` (`STR`, `DEX`, `CON`, `Level`, `Athletics.ProfRank`, `Athletics.ItemsBonus`, `MaxHitPoints.PerLevel`, `MaxHitPoints.Bonus`, …);
- оружейные сырые ключи: `Weapon.Martial.ProfRank` (и аналоги по `Glossary.Weapons` типам), `<ItemId>.Attack.ItemsBonus` / `<ItemId>.Damage.ItemsBonus`, `<Group>.Attack.Group.Bonus` / `<Group>.Damage.Group.Bonus`;
- bool-флаги кодируются как `0` / ненулевое значение;
- итоговый модификатор навыка и `MaxHitPoints` **не пишутся** в `Parameters` — их даёт `CharacterParametersProxy.GetTotalValue`;
- итоговые attack/damage modifiers оружия тоже **не пишутся** — их даёт `WeaponProxy`;
- **чтение:** конвенция — через `CharacterParametersProxy` (пока не enforced компилятором);
- **запись:** через `CharacterParametersOperator` — Apply / Unapply `CharacterParamsPatchData` / `FeatDef`.

**Defs → State (планируемый поток):**
- дефы (`ClassDef`, `AncestryDef`, `BackgroundDef`, `FeatDef`, `ItemDef`, `SpellDef`) описывают статический контент;
- `AncestryDef.HitPoints` и `ClassDef.HitPointsPerLevel` — константы для формулы Max HP (не копируются в `Parameters`);
- для черт механика задаётся в `FeatDef.Apply` (`CharacterParamsPatchData`: `Add`, `Set`, `AlsoApplyFeatIds`, `ConditionDuration`; см. [Definitions.md](Definitions.md));
- при создании/прокачке / экипировке runtime применяет патчи в **сырые** ключи `Parameters` (и связанные поля) через Write API;
- для `FeatType.Condition` с `ConditionDuration > 0` дополнительно пишется таймер в `StatusEffects[featId]`;
- `UpdateCharacter` — батч прокачки (слепок + последовательный Apply feats/ручных правок нового уровня), не произвольный edit.

### Adventure: `CharacterParametersProxy` (Read API)

Файл: `Implementation/Adventure/CharacterParametersProxy.cs`.

Прокси **чтения** параметров персонажа поверх `CharacterStateData.Parameters`, `RuleDef.ParameterFormulas` и дефов из `DefinitionsManager`. Целевая точка чтения raw/total; обязательность в gameplay пока закреплена документацией и комментариями, не кодом.

Конструктор: `(CharacterStateData characterState, RuleDef ruleDef, DefinitionsManager definitionsManager)`.

| Метод | Поведение |
|-------|-----------|
| `GetTotalValue(key)` | Если `key` есть в `RuleDef.ParameterFormulas` — вычисляет выражение (`+`, `*`, скобки); иначе возвращает сырое значение из `Parameters` (или `0`) |
| `GetRawValue(key)` | Всегда читает только `CharacterStateData.Parameters` (или `0`) |

Ключевые слова формулы:
- `PROFICIENCY` → `<requestedKey> + PROFICIENCY_SUFFIX` (например `Athletics.ProfRank`) → `ProficiencyType`; для `Untrained` бонус `0`, иначе `Level + rankBonus` (2/4/6/8);
- `ITEMS` → `<requestedKey> + ITEMS_SUFFIX` (например `Athletics.ItemsBonus`);
- `ANCESTRY_HP` → `AncestryDef.HitPoints` по `CharacterStateData.Ancestry`;
- `CLASS_HP` → `ClassDef.HitPointsPerLevel` по `CharacterStateData.Class`;
- `PER_LEVEL` → `<requestedKey> + PER_LEVEL_SUFFIX` (например `MaxHitPoints.PerLevel`);
- `BONUS` → `<requestedKey> + BONUS_SUFFIX` (например `MaxHitPoints.Bonus`);
- остальные токены (`STR`, `DEX`, `CON`, `Level`, …) — сырые ключи из `Parameters`.

Примеры:
- `GetTotalValue("Athletics")` при `STR+PROFICIENCY+ITEMS`;
- `GetTotalValue("MaxHitPoints")` при `ANCESTRY_HP+(CLASS_HP+CON+PER_LEVEL)*Level+BONUS`  
  (дварф-воин 5 ур., CON+3, без доп. бонусов → `10+(10+3+0)*5+0 = 75`).

Формулы навыков и `MaxHitPoints` заданы в `GeneralRule.ParameterFormulas` (см. [Definitions.md](Definitions.md)).

### Adventure: Write API параметров (Apply / Unapply)

Файл: `Implementation/Adventure/CharacterParametersOperator.cs`.

Единая точка **записи** в `CharacterStateData.Parameters` (и в слепок `CharacterRequestData.Parameters`). Для timed Condition также пишет/снимает таймер в `StatusEffects`. Вход — `CharacterParamsPatchData` из дефов (`FeatDef.Apply`) либо опосредованно через `CharacterItemFeaturesOperator` (`ItemDef.Features` → feat ids).

| Метод | Поведение |
|-------|-----------|
| `ApplyPatch` / `UnapplyPatch` | патч `Add` / `Set` / `AlsoApplyFeatIds` |
| `ApplyFeat` / `UnapplyFeat` | резолв `FeatDef` по id → Apply/Unapply его `Apply`; для Condition + `ConditionDuration > 0` — регистрация / refresh / снятие таймера в `StatusEffects`; `AdditionalSlots` — добавление/удаление слотов в `EquippedItems` с переносом предмета в `Bag`/инвентарь при снятии слота |
| `CreateCharacterRequestSnapshot` | полный слепок mutable-блока для прокачки |
| Overloads на `CharacterStateData` | `Parameters` / `StatusEffects ??= new()`, делегируют в словари |

| Операция патча | Apply | Unapply |
|----------------|-------|---------|
| `Add` | `current += value` | `current += -value` |
| `Set` | `current = value` | если патч ставил **≠ 0** → `0`; если патч ставил **0** → `1` (инверсия флага) |
| `AlsoApplyFeatIds` | каскадный Apply (прямой порядок) | каскадный Unapply (обратный порядок) |
| `ConditionDuration` | Condition + `> 0` → `StatusEffects[featId] = Duration` (повторный Apply = refresh без повторного Add) | удалить `StatusEffects[featId]` |

Циклы / дубликаты feat id в каскаде — warning и skip. Нет feat / `Apply == null` — warning и no-op.

Итоги (`MaxHitPoints`, навыки) отдельно не пересчитываются: меняются только сырые ключи; итог даёт Read API.

Обёртки на DTO: `CharacterRequestData` / `UpdateCharacterRequestData` — методы `ApplyFeat` / `UnapplyFeat` / `ApplyPatch` / `UnapplyPatch`.

FAQ по `AdditionalSlots`:

- **Если снимается дополнительный слот и он занят:** оператор сначала пытается перенести предмет в свободный `Bag` этого персонажа.
- **Если свободного `Bag` нет:** при наличии `inventoryItems` предмет переносится в общий инвентарь.
- **Если нет и `Bag`, и `inventoryItems`:** слот не удаляется, пишется warning (защита от потери предмета).
- **Если удаляемый слот сам `Bag`:** сначала ищется другой свободный `Bag`, иначе предмет уходит в общий инвентарь.
- **Фичи предмета (`ItemDef.Features`):** при уходе из носимого слота снимаются автоматически; при `Bag` → `Bag` не меняются.

Практические сценарии (создание, уровень, экипировка, бой/статусы): [Feats.md](Feats.md) («Как использовать Feats»).

Пример прокачки:

```csharp
var request = new UpdateCharacterRequestData
{
    CharacterId = characterId,
    CharacterData = CharacterParametersOperator.CreateCharacterRequestSnapshot(character),
};
request.ApplyFeat("_SomeLevelFeat", definitionsManager);
request.ApplyPatch(manualBoostPatch, definitionsManager);
stateLogic.ProcessAction(new UpdateCharacterStateAction(request));
```

`UpdateCharacterStateAction` персистит готовый слепок wholesale (не крутит Apply сам).

Пример конца хода (таймеры Condition):

```csharp
stateLogic.ProcessAction(new EndCharacterTurnStateAction(
    new EndCharacterTurnRequestData { CharacterId = characterId },
    definitionsManager));
```

Планируемые следующие вызывающие сценарии:
- UI создания / прокачки персонажа (Apply feats на слепке до Create/Update).
- Turn-loop боя: вызов `EndCharacterTurn` для актёра (см. [Feats.md](Feats.md) / [Battle.md](Battle.md)).

**`ItemDef.Features` в inventory state-actions:** сделано. Конструктор Equip/Unequip/Move/Remove принимает `DefinitionsManager`. `AddCharacterItem` кладёт только в `Bag` / общий инвентарь — Features не применяет.

### Adventure: `WeaponProxy`

Файл: `Implementation/Adventure/WeaponProxy.cs`.

Прокси модификаторов атаки и урона оружия поверх `CharacterStateData.Parameters` и формул конкретного `ItemDef`. Пока не подключён в gameplay/DI как обязательный API.

Конструктор: `(CharacterStateData characterState)`.

| Метод | Поведение |
|-------|-----------|
| `GetAttackModifier(itemDef)` | Вычисляет `itemDef.AttackModifierFormula`; пустая формула / `null` → `0` |
| `GetDamageModifier(itemDef)` | Вычисляет `itemDef.DamageModifierFormula` (**без** броска костей); пустая формула / `null` → `0` |
| `GetDamageDice(itemDef)` | Возвращает `itemDef.DamageDice` как метаданные для будущего броска (прокси не роллит) |

Ключевые слова формулы:
- `PROFICIENCY` → `itemDef.Type + ".ProfRank"` (например `Weapon.Martial.ProfRank`) → `ProficiencyType`; `Untrained` → `0`, иначе `Level + rankBonus` (2/4/6/8);
- `ITEMS` → в контексте атаки `<ItemId>.Attack.ItemsBonus`, в контексте урона `<ItemId>.Damage.ItemsBonus`;
- `GROUP_ATTACK_BONUS` → `<Group>.Attack.Group.Bonus` (например `Bow.Attack.Group.Bonus`);
- `GROUP_DAMAGE_BONUS` → `<Group>.Damage.Group.Bonus`;
- `ABILITY_BEST` / `ATTACK_ABILITY_BEST` / `DAMAGE_ABILITY_BEST` → максимум среди `itemDef.AbilityDependencies`;
- остальные токены (`STR`, `DEX`, …) и числовые литералы (`+2`, `+8`) — напрямую.

Пример: `GetAttackModifier` при `ATTACK_ABILITY_BEST+PROFICIENCY+ITEMS+GROUP_ATTACK_BONUS`  
= max(`AbilityDependencies`) + prof(`Type`) + item attack bonus + group attack bonus.

Контракт полей оружия и стартовые JSON — в [Definitions.md](Definitions.md) (`ItemDef`).

### Adventure: `CreatureCombatantFactory`

Файл: `Implementation/Adventure/CreatureCombatantFactory.cs`.

Materialize статблока противника в тот же shape, что у игрока (`CharacterStateData`), чтобы Apply/Unapply статусов и proxies работали одинаково для обеих сторон.

Практическое руководство с полным walkthrough: [Creatures.md](Creatures.md).

| Метод | Поведение |
|-------|-----------|
| `CreateFromCreature(def, instanceId, definitionsManager)` | Собирает combatant: `Name`←`Title`, `Avatar`←`Icon`; запекает `Level` / `ChallengeRating` / `AC` / `Speed` / abilities / HP / Perception / saves / skills; мержит escape-hatch `Parameters`; `ApplyFeat` по `Features`; Apply item Features на носимых слотах. `instanceId` должен быть **&lt; 0**. |
| `CloneForBattle(source, overrideId?)` | Копия party-персонажа для session |

Запекание итогов статблока:
- `MaxHitPoints`: без Class/Ancestry формула даёт `(CON)*Level + Bonus` → factory ставит `MaxHitPoints.Bonus` так, чтобы `GetTotalValue(MaxHitPoints)` = `HitPoints`;
- Perception / skills: `*.ItemsBonus = final − ability`, чтобы `GetTotalValue` совпал с числом в статблоке (Untrained);
- saves (`Fortitude` / `Reflex` / `Will`) и `AC` — сырые ключи (формул в `GeneralRule` пока нет).

`BattleActionIds` на `CharacterStateData` не копируются — остаются на `CreatureDef`.

**Пример:**

```csharp
CreatureDef goblinDef = definitionsManager.Creatures["_GoblinWarrior"];
CharacterStateData goblin = CreatureCombatantFactory.CreateFromCreature(
    goblinDef,
    instanceId: -1,
    definitionsManager);

CharacterStateData heroCopy = CreatureCombatantFactory.CloneForBattle(
    state.Characters.Characters[heroId]);

// Один Write API статусов для обеих сторон
CharacterParametersOperator.ApplyFeat(goblin, "_ConditionFrightened", definitionsManager);
CharacterParametersOperator.ApplyFeat(heroCopy, "_ConditionFrightened", definitionsManager);

List<string> actions = goblinDef.BattleActionIds; // e.g. "_Strike"
```

### Adventure: `InventoryStateData`

Файл: `Implementation/Adventure/StateDatas/InventoryStateData.cs`.

`InventoryStateData` — инвентарь отряда (отдельно от персонажей):

| Поле | Тип | Назначение |
|------|-----|------------|
| `Items` | `Dictionary<string, int>` | Предметы и расходники: defId → количество (общий пул отряда) |

**Разделение экипировки:**
- общий пул отряда — в `Inventory.Items` (`ItemDef` id → количество);
- слоты конкретного персонажа — в `CharacterStateData.EquippedItems` (тип слота + `ItemId`).

**Перенос предметов** выполняется только через dedicated state-actions:
- `EquipItemFromInventoryStateAction` — из `Inventory.Items` в слот (`SlotIndex`); если слот был занят, старый предмет возвращается в инвентарь (swap);
- `UnequipItemToInventoryStateAction` — из слота обратно в `Inventory.Items`, слот очищается (`ItemId = null`);
- `MoveEquippedItemBetweenSlotsStateAction` — перенос между двумя слотами **одного** персонажа (`FromSlotIndex` → `ToSlotIndex`) **без** участия `Inventory.Items`; если целевой слот занят — swap `ItemId`.

**Добавление / удаление предметов** (не перенос):
- `AddInventoryItemsStateAction` — добавить `Count` штук `ItemId` в `Inventory.Items` (через `InventoryItemsOperator.TryAdd`; capacity stub пока пропускает всё количество);
- `RemoveInventoryItemsStateAction` — удалить до `Count` штук `ItemId` из `Inventory.Items` (`RemoveUpTo` / `Max(0, …)`). Валидация: предмет должен существовать (`count > 0`), даже если `Count` больше текущего количества;
- `AddCharacterItemStateAction` — выдать 1 предмет персонажу: свободный слот `Glossary.Items.SLOT_TYPE_BAG`, иначе в общий инвентарь;
- `RemoveCharacterEquippedItemStateAction` — удалить предмет из слота персонажа (без возврата в инвентарь). `SlotIndex = -1` ищет первый слот с указанным `ItemId`.

`CreateCharacterStateAction` / `UpdateCharacterStateAction` **не** трогают `Inventory.Items`: они работают только с локальными данными персонажа.

**Важно про `SlotIndex`:** это индекс в списке `CharacterStateData.EquippedItems`, а не “тип слота”.  
Пример набора слотов у персонажа:

| SlotIndex | Slot | ItemId (пример) |
|-----------|------|-----------------|
| `0` | `Hand` | `"_Longsword"` |
| `1` | `Hand` | `null` |
| `2` | `Bag` | `null` |
| `3` | `Bag` | `"_Torch"` |
| `4` | `Body` | `"_LeatherArmor"` |

#### Примеры использования inventory-экшенов

Ниже `characterId = 1`, `stateLogic` — `AdventureStateLogic`, `definitionsManager` — Adventures `DefinitionsManager` (нужен для Apply/Unapply `ItemDef.Features`).

**1) Перенести предмет из руки персонажа в его мешок**  
Атомарно, без участия `Inventory.Items` (Features с `Hand` снимаются — правило Bag):

До: `EquippedItems[0] = Hand/_Longsword`, `EquippedItems[2] = Bag/null`.

```csharp
stateLogic.ProcessAction(new MoveEquippedItemBetweenSlotsStateAction(
    new MoveEquippedItemBetweenSlotsRequestData
    {
        CharacterId = 1,
        FromSlotIndex = 0,      // Hand
        ToSlotIndex = 2,        // Bag
    },
    definitionsManager));
```

После: `EquippedItems[0] = Hand/null`, `EquippedItems[2] = Bag/_Longsword`.  
`Inventory.Items` не меняется.

**1b) Поменять местами предметы между двумя слотами персонажа (swap)**

До: `EquippedItems[0] = Hand/_Longsword`, `EquippedItems[3] = Bag/_Torch`.

```csharp
stateLogic.ProcessAction(new MoveEquippedItemBetweenSlotsStateAction(
    new MoveEquippedItemBetweenSlotsRequestData
    {
        CharacterId = 1,
        FromSlotIndex = 0,      // Hand/_Longsword
        ToSlotIndex = 3,        // Bag/_Torch
    },
    definitionsManager));
```

После: `EquippedItems[0] = Hand/_Torch`, `EquippedItems[3] = Bag/_Longsword`.  
`Inventory.Items` не меняется. Features: Unapply у `_Longsword` (Hand→Bag), Apply у `_Torch` если у него есть Features и `Hand` их даёт.

**2) Перенести из глобального хранилища в мешок персонажа**

До: `Inventory.Items["_Potion"] = 3`, `EquippedItems[2] = Bag/null`.

```csharp
stateLogic.ProcessAction(new EquipItemFromInventoryStateAction(
    new EquipItemFromInventoryRequestData
    {
        CharacterId = 1,
        SlotIndex = 2,          // Bag
        ItemId = "_Potion",
    },
    definitionsManager));
```

После: `Inventory.Items["_Potion"] = 2`, `EquippedItems[2] = Bag/_Potion` (Features не применяются — Bag).

**3) Перенести из глобального хранилища в руку персонажа**

До: `Inventory.Items["_Dagger"] = 1`, `EquippedItems[1] = Hand/null`.

```csharp
stateLogic.ProcessAction(new EquipItemFromInventoryStateAction(
    new EquipItemFromInventoryRequestData
    {
        CharacterId = 1,
        SlotIndex = 1,          // вторая Hand
        ItemId = "_Dagger",
    },
    definitionsManager));
```

После: ключ `"_Dagger"` удалён из `Inventory.Items` (count стал 0), `EquippedItems[1] = Hand/_Dagger` (+ Apply Features, если есть).

**4) Снять предмет с персонажа в глобальное хранилище**

До: `EquippedItems[4] = Body/_LeatherArmor`.

```csharp
stateLogic.ProcessAction(new UnequipItemToInventoryStateAction(
    new UnequipItemToInventoryRequestData
    {
        CharacterId = 1,
        SlotIndex = 4,          // Body
    },
    definitionsManager));
```

После: `EquippedItems[4].ItemId = null`, `Inventory.Items["_LeatherArmor"]` увеличен на 1 (+ Unapply Features с Body).

**5) Надеть предмет в уже занятый слот (авто-swap)**

До: `EquippedItems[0] = Hand/_Dagger`, `Inventory.Items["_Longsword"] = 1`, `Inventory.Items["_Dagger"]` отсутствует.

```csharp
stateLogic.ProcessAction(new EquipItemFromInventoryStateAction(
    new EquipItemFromInventoryRequestData
    {
        CharacterId = 1,
        SlotIndex = 0,
        ItemId = "_Longsword",
    },
    definitionsManager));
```

После: `EquippedItems[0] = Hand/_Longsword`, `Inventory.Items["_Longsword"]` уменьшен на 1, `Inventory.Items["_Dagger"]` увеличен на 1 (старый предмет вернулся в хранилище; Unapply `_Dagger`, Apply `_Longsword`).

**6) Ошибка: предмета нет в глобальном хранилище**

```csharp
var result = stateLogic.ProcessAction(new EquipItemFromInventoryStateAction(
    new EquipItemFromInventoryRequestData
    {
        CharacterId = 1,
        SlotIndex = 0,
        ItemId = "_MissingItem",
    },
    definitionsManager));

// result.IsValid == false
// персонаж и Inventory не изменены
```

**7) Ошибка: снять пустой слот**

```csharp
var result = stateLogic.ProcessAction(new UnequipItemToInventoryStateAction(
    new UnequipItemToInventoryRequestData
    {
        CharacterId = 1,
        SlotIndex = 1, // Hand, но ItemId уже null
    },
    definitionsManager));

// result.IsValid == false ("Equipped slot is already empty.")
```

**8) Добавить предметы в общий инвентарь (1..N)**

До: `Inventory.Items["_Apple"] = 2`.

```csharp
stateLogic.ProcessAction(new AddInventoryItemsStateAction(
    new AddInventoryItemsRequestData
    {
        ItemId = "_Apple",
        Count = 3,
    }));
```

После: `Inventory.Items["_Apple"] = 5`.  
`TryAdd` возвращает фактическое добавленное количество (сейчас stub = requested).

**9) Удалить предметы из общего инвентаря (clamp до 0)**

До: `Inventory.Items["_Apple"] = 5`.

```csharp
stateLogic.ProcessAction(new RemoveInventoryItemsStateAction(
    new RemoveInventoryItemsRequestData
    {
        ItemId = "_Apple",
        Count = 10, // больше, чем есть — валидация OK, т.к. яблоки существуют
    }));
```

После: ключ `"_Apple"` удалён (`Max(0, 5-10) = 0`).

**10) Выдать предмет персонажу (Bag, иначе общий инвентарь)**

```csharp
stateLogic.ProcessAction(new AddCharacterItemStateAction(
    new AddCharacterItemRequestData
    {
        CharacterId = 1,
        ItemId = "_Potion",
    }));
```

Если есть свободный `Bag`-слот — предмет туда (**без** Apply Features); иначе `Inventory.Items["_Potion"] += 1`.

**11) Удалить предмет из слота персонажа (без возврата в инвентарь)**

По индексу:

```csharp
stateLogic.ProcessAction(new RemoveCharacterEquippedItemStateAction(
    new RemoveCharacterEquippedItemRequestData
    {
        CharacterId = 1,
        SlotIndex = 3,          // Bag/_Torch
        ItemId = "_Torch",
    },
    definitionsManager));
```

Или поиск по `ItemId` (`SlotIndex = -1`):

```csharp
stateLogic.ProcessAction(new RemoveCharacterEquippedItemStateAction(
    new RemoveCharacterEquippedItemRequestData
    {
        CharacterId = 1,
        SlotIndex = -1,
        ItemId = "_Torch",
    },
    definitionsManager));
```

После: слот очищен (`ItemId = null`), `Inventory.Items` не меняется (Unapply Features только если слот был носимым).


### Adventure: `AdventuresStateData` и связанные типы

Файл: `Implementation/Adventure/StateDatas/AdventuresStateData.cs`.

`AdventuresStateData` — прогресс приключений и мира:

| Поле | Тип | Назначение |
|------|-----|------------|
| `CurrentAdventureId` | `string` | Id активного приключения (runtime-точка входа/продолжения) |
| `CurrentAdventureSceneId` | `string` | Id активной сцены в рамках текущего приключения (runtime-точка для `AdventuresManager`) |
| `World` | `WorldStateData` | Параметры мира/кампании в рамках текущего прогресса |
| `Global` | `GlobalStateData` | Параметры игрока, не сбрасываемые при перезапуске приключений |
| `Adventures` | `Dictionary<string, AdventureStateData>` | Прогресс по отдельным приключениям: adventureId → состояние |

`WorldStateData` (в том же файле):

| Поле | Тип | Назначение |
|------|-----|------------|
| `Parameters` | `AdventureStateParamsData` | Параметры мира (`world.*` и др.) |

`GlobalStateData` (в том же файле; **не наследует** `WorldStateData`):

| Поле | Тип | Назначение |
|------|-----|------------|
| `Parameters` | `AdventureStateParamsData` | Долгоживущие параметры игрока (`global.*` и др.): число посещений таверны, число запусков игры и т.п. |

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
- `world.*` → `Adventures.World.Parameters` (`SetWorldParamsStateAction`);
- `adventure.*` → `Adventures[currentAdventureId].Parameters` (`SetAdventureParamsStateAction`; `currentAdventureId` берётся из `AdventuresStateData.CurrentAdventureId`, отдельный ключ в `Params` не нужен);
- `global.*` → `Adventures.Global.Parameters` (`SetGlobalParamsStateAction`).

`SetWorldParamsStateAction`, `SetAdventureParamsStateAction` и `SetGlobalParamsStateAction` принимают `ChoiceActionParamsData` и **merge**-ят значения в целевой `AdventureStateParamsData` (перезаписывают ключи из `Strings` / `Ints` / `Bools`, остальные ключи не трогают). Если записи приключения ещё нет, `SetAdventureParamsStateAction` создаёт `AdventureStateData` с `AdventureId = CurrentAdventureId`.

**Разделение `World` и `Global`:** `World.Parameters` хранит прогресс мира/кампании в контексте текущего игрового цикла; `Global.Parameters` — метрики и флаги на уровне игрока, которые сохраняются независимо от перезапуска или смены приключения.

`CurrentAdventureId` / `CurrentAdventureSceneId` не относятся к словарям прогресса `world.*` и `adventure.*`; это отдельный контракт активной runtime-точки. Не путать с `AdventureStateData.SceneId` в `Adventures[adventureId]` — там хранится прогресс конкретного приключения.

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
| `Profile` | `CreateTime`, `UpdateTime` = текущее Unix ms UTC; `Parameters` — копия `ProfileStateSettings.DefaultParameters` (`MaxPartySlots`, `MaxInventorySlots`, …) |
| `Wallet` | пустой `Resources` |
| `Localization` | `Language = SystemLanguage.Unknown` |
| `Characters` | `NextCharacterId = 1`, `HeroPoints = 0`, пустые `Characters`, `ActivePartyCharacterIds` |
| `Inventory` | пустой `Items` |
| `Adventures` | `World` и `Global` с пустыми `Parameters`; пустой словарь `Adventures` |

**DI (Adventure `ProjectInstaller`):**

```csharp
Container.Bind<AdventureStateManager>().AsSingle().NonLazy();
Container.Bind<AdventureStateLogic>().AsSingle().NonLazy();
Container.BindInterfacesAndSelfTo<AdventuresManager>().AsSingle().NonLazy();
Container.Bind<IAdventureStateDataFactory>().To<AdventureStateDataFactory>().AsTransient();
```

`AdventuresManager` (`Modules.RPG`) — отдельный singleton, не наследник `AdventureStateLogic`. Подписывается на `AdventureStateLogic.StateChanged` в `Init()` и отписывается в `Dispose()` (см. [RPG.md](RPG.md)).

`AsTransient()` для фабрики — при каждом `Resolve` создаётся новый экземпляр (не singleton). После выхода из `CreateNewState()` ссылок на фабрику нет; Zenject её явно не удаляет — объект собирается GC.

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

// Подписка на изменения state (после успешного Execute)
stateLogic.StateChanged += source =>
{
    if (source == StateChangeSource.Wallet)
        RefreshWalletUi();
};
```

## Готовые state-actions

- `ChangeWalletResourceStateAction<TStateData>` — общий экшен кошелька (Match-3 и Adventure).
- `SetWalletResourceStateAction<TStateData>` — установка ресурса кошелька (Match-3 и Adventure).
- `SetProfileUpdateTimeStateAction` — обновление `Profile.UpdateTime` (Match-3 и Adventure).
- `SetCurrentAdventureIdStateAction` — установка `Adventures.CurrentAdventureId` и сброс `Adventures.CurrentAdventureSceneId` в `null` (чтобы `RuntimeSceneData` заново выбрал сцену из `StartScenes` целевого приключения; защита от коллизии одинаковых scene id между adventure).
- `SetCurrentAdventureSceneIdStateAction` — установка `Adventures.CurrentAdventureSceneId` (Adventure).
- `SetLocalizationLanguageStateAction<TStateData>` — установка `Localization.Language` (Adventure).
- `SetWorldParamsStateAction` — merge `ChoiceActionParamsData` в `Adventures.World.Parameters` (Adventure).
- `SetAdventureParamsStateAction` — merge `ChoiceActionParamsData` в `Adventures.Adventures[CurrentAdventureId].Parameters` (Adventure).
- `SetGlobalParamsStateAction` — merge `ChoiceActionParamsData` в `Adventures.Global.Parameters` (Adventure).
- `CreateCharacterStateAction` — создание нового `CharacterStateData` из `CreateCharacterRequestData`, запись в `Characters[NextCharacterId]`, опциональное добавление id в `ActivePartyCharacterIds`, инкремент `NextCharacterId` (Adventure).
- `UpdateCharacterStateAction` — обновление существующего персонажа по `CharacterId` из `UpdateCharacterRequestData` (контракт: **только прокачка**). `Parameters` собираются через `CharacterParametersOperator` на слепке; экшен персистит готовый `CharacterData` wholesale (Adventure).
- `EquipItemFromInventoryStateAction` — из `Inventory.Items` в слот; при занятом слоте старый в инвентарь; Apply/Unapply `ItemDef.Features` через `CharacterItemFeaturesOperator` (конструктор: request + `DefinitionsManager`) (Adventure).
- `UnequipItemToInventoryStateAction` — из слота в `Inventory.Items` + Unapply Features если слот носимый (Adventure).
- `MoveEquippedItemBetweenSlotsStateAction` — перенос/swap между слотами; Features по правилу Bag (носимый↔Bag) (Adventure).
- `AddInventoryItemsStateAction` — добавить `Count` предметов `ItemId` в `Inventory.Items` (Adventure).
- `RemoveInventoryItemsStateAction` — удалить до `Count` предметов `ItemId` из `Inventory.Items` с clamp до 0; валидация требует наличие хотя бы 1 шт. (Adventure).
- `AddCharacterItemStateAction` — выдать 1 предмет в свободный `Bag` или общий инвентарь; Features **не** применяет (Adventure).
- `RemoveCharacterEquippedItemStateAction` — удалить из слота без возврата в инвентарь + Unapply Features если слот носимый; `SlotIndex = -1` ищет по `ItemId` (Adventure).
- `EndCharacterTurnStateAction` — конец хода персонажа: декремент `StatusEffects` для timed Condition-feats, запуск tick-обработчиков из `ConditionDef`, `UnapplyFeat` при `0` (конструктор: request + `DefinitionsManager`) (Adventure).
- `ChangeHeroPointsStateAction` — `HeroPoints += delta`; валидация: результат не отрицательный (Adventure).
- `SetCurrentActiveCharacterIdStateAction` — установка `CurrentActiveCharacterId`; персонаж должен существовать в `Characters` (Adventure).
- `AddCharacterToActivePartyStateAction` — добавить id в `ActivePartyCharacterIds` (лимит из `Profile.Parameters[Glossary.ProfileState.MAX_PARTY_SLOTS]`, без дублей) (Adventure).
- `RemoveCharacterFromActivePartyStateAction` — убрать id из `ActivePartyCharacterIds`; нельзя удалить текущего (`CurrentActiveCharacterId`) (Adventure).
- `SetProfileParameterStateAction` — `Profile.Parameters[key] = value` (`value >= 0`) (Adventure; **не** для choice-pipeline).
- `AddProfileParameterStateAction` — `Profile.Parameters[key] += delta` (Adventure; **не** для choice-pipeline).

## Как добавить новый state-action

1. Создать класс в `Implementation/<Mode>/Actions/` (или в `Implementation/Wallet/Actions/` для общих секций).
2. Унаследовать от `StateActionBase<TStateData>`.
3. Переопределить `Source` — вернуть категорию затронутой секции из `StateChangeSource` (`Profile`, `Wallet`, `Characters`, `CharactersAndInventory`, …). Новый enum-value добавлять только если появилась **новая область** стейта, а не новый экшен.
4. Принять в конструктор только данные, нужные для изменения.
5. Переопределить `Validate(state)` при необходимости.
6. Реализовать `Execute(state)` — единственное место мутации соответствующей секции состояния.
7. Вызывать через `stateLogic.ProcessAction(new YourAction(...))`.

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
- Язык локализации в Adventure хранится в `StateData.Localization` и используется `LocalizationInitTask` при выборе стартового языка.
- Для Adventure новый профиль создаётся через `IAdventureStateDataFactory` (`AdventureStateDataFactory`).
- Реализованы секции Adventure state: `Characters`, `Inventory`, `Adventures` (см. выше).
- Прямых gameplay-мутаций `State` вне state-actions сейчас нет.
- `ProcessAction` вызывается из runtime-кода `AdventureStateManager` и `RuntimeSceneData`.
- `AdventureStateLogic.StateChanged` используется `AdventuresManager` для реакции на изменения state (подписка в `Init()`, отписка в `Dispose()`).
