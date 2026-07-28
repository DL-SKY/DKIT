# Модуль Definitions

**Последнее обновление:** 2026-07-27 18:12:08 (+03:00)

## Назначение

`Definitions` отвечает за загрузку и хранение конфигурационных данных проекта (JSON-дефов) из `Resources/Definitions` и предоставляет их в рантайме через единый менеджер.

## Краткая логика работы

1. На старте вызывается `DefinitionsManager.InitAsync()`, который запускает корутину `LoadAll()`.
2. `LoadAll()` последовательно вызывает набор методов загрузки (`LoadGlobalSettings`, `LoadAdventures`, `LoadGameZones`, `LoadRounds` и т.д.).
3. Каждый метод делегирует чтение в `Loader`:
   - `LoadSingle<T>()` для одиночного JSON;
   - `LoadCollection<T>()` для папки JSON-файлов.
4. `Loader` загружает `TextAsset` через `Resources.Load*`, десериализует JSON через `JsonConvert.DeserializeObject<T>()` и присваивает `definition.Id = asset.name`.  
   `LoadCollection()` обходит папку рекурсивно (`Resources.LoadAll`), пропускает битые JSON и дубликаты id (в словарь попадает первый деф, остальные — с `LogWarning`).
5. Загруженные данные кэшируются в полях `DefinitionsManager` (single-def поля и словари коллекций), после чего выставляется флаг завершения `SimpleAsyncOperation`.

## Основные классы

- `AbstractDefinition`  
  Базовый тип для всех дефов; содержит `Id` (не сериализуется в JSON).

- `Loader`  
  Универсальный загрузчик: чтение JSON из `Resources`, десериализация и возврат typed-объектов.

- `DefinitionsManager` (Match3)  
  Фасад доступа к дефам Match3. Хранит:
  - single-def: `GlobalSettings`, `LocalizationSettings`, `Match3GlobalSettings`, `CellsMap`, `PresetsMap`;
  - коллекции: `GameZones`, `Cells`, `Presets`, `Gems`, `GameZoneGems`, `Objectives`, `Rounds`.

- `DefinitionsManager` (Adventures)  
  Фасад доступа к дефам adventure-проекта (`Modules.Definitions.Scripts.Implementation.Adventures`). Хранит:
  - single-def: `GlobalSettings` (`ProjectGlobalSettingsDef`), `LocalizationSettings` (`LocalizationSettingsDef`), `RuleSettings` (`RuleSettingsDef`), `Avatars` (`AvatarsDef`);
  - коллекции: `Adventures`, `Classes`, `Ancestries`, `Backgrounds`, `PregeneratedCharacters`, `Feats`, `Items`, `Spells`, `BattleActions`, `Rules`, `BattleRules`.  
  Используется и `CharacterParametersProxy` (модуль `State`) для резолва `ANCESTRY_HP` / `CLASS_HP` из `Ancestries` / `Classes`.

- `Glossary` (`Implementation/Adventures/Constants/Glossary.cs`)  
  Константы id/ключей adventure-контента. Для параметров персонажа — `Glossary.Characters`:
  - abilities: `STR`, `DEX`, `CON`, `INT`, `WIS`, `CHA`;
  - прогресс: `LEVEL` (`"Level"`), `EXPERIENCE`;
  - HP: `MAX_HIT_POINTS` (`"MaxHitPoints"`, **вычисляемый**), `HIT_POINTS` (`"HitPoints"`, текущие);
  - навыки / `Perception` — имена как в `SkillDependencies`;
  - суффиксы сырых составляющих: `PROFICIENCY_SUFFIX` (`.ProfRank`), `ITEMS_SUFFIX` (`.ItemsBonus`), `PER_LEVEL_SUFFIX` (`.PerLevel`), `BONUS_SUFFIX` (`.Bonus`).

  Для оружия — `Glossary.Weapons`:
  - типы (`ItemDef.Type`): `SIMPLE` = `"Weapon.Simple"`, `MARTIAL` = `"Weapon.Martial"`, `ADVANCED` = `"Weapon.Advanced"`;
  - группы (`ItemDef.Group`): `AXE`, `BRAWLING`, `CLUB`, `FLAIL`, `HAMMER`, `KNIFE`, `PICK`, `POLEARM`, `SHIELD`, `SPEAR`, `SWORD`, `BOW`, `CROSSBOW`, `DART`, `SLING`, `BOMB`, `FIREARM` (значения CamelCase: `"Axe"`, `"Bow"`, …).

  Для слотов экипировки — `Glossary.Items` (значения пишутся в `EquippedItemStateData.Slot` / `ItemDef.AvailableSlots` / `FeatDef.AdditionalSlots` / `ClassDef.EquippedItems` / `AncestryDef.EquippedItems`):
  - `SLOT_TYPE_HAND` = `"Hand"`;
  - `SLOT_TYPE_LEGS` = `"Legs"`;
  - `SLOT_TYPE_HEAD` = `"Head"`;
  - `SLOT_TYPE_BODY` = `"Body"`;
  - `SLOT_TYPE_BAG` = `"Bag"` (хранилище на персонаже; **не** даёт `ItemDef.Features`);
  - `SLOT_TYPE_FINGER` = `"Finger"` (кольца и т.п.);
  - `SLOT_TYPE_NECK` = `"Neck"` (ожерелья и т.п.);
  - `SLOT_TYPE_TAIL` = `"Tail"` (украшения/снаряжение на хвост).  
  Хелпер: `Glossary.Items.GrantsItemFeatures(slotType)` — `false` только для `Bag` (и пустого типа).  
  Контракт — **повторяемый список типов**, без индексов: несколько рук/сумок/пальцев задаются несколькими записями с одним и тем же типом.

### Соглашение по наследованию adventure-дефов

- **По умолчанию** класс дефа наследуется напрямую от `AbstractDefinition` и содержит все сериализуемые поля (пример: `GemDef`, `GameZoneDef`, `ClassDef`, `ItemDef`).
- **Исключение — `AdventureDef`:** наследует `AdventureData` из модуля `RPG`, потому что большой контракт приключения (сцены, выборы, ограничения) живёт в `RPG` и используется в runtime-слое (`AdventuresManager`, `RuntimeSceneData`). Legacy `IAdventureFlowController` и `ObsoleteGoToSceneChoiceActionExecutor` помечены `[Obsolete]`; актуальный переход по сцене — `GoToSceneChoiceActionExecutor` через state-actions. Промежуточные `*Data`-прослойки для остальных типов не используются.

| Def | Базовый класс | JSON-папка |
|---|---|---|
| `AdventureDef` | `AdventureData` (модуль `RPG`) | `Definitions/_ADVENTURES_/Adventures` |
| `ClassDef` | `AbstractDefinition` | `Definitions/_ADVENTURES_/Classes` |
| `AncestryDef` | `AbstractDefinition` | `Definitions/_ADVENTURES_/Ancestries` |
| `BackgroundDef` | `AbstractDefinition` | `Definitions/_ADVENTURES_/Backgrounds` |
| `PregeneratedCharacterDef` | `AbstractDefinition` | `Definitions/_ADVENTURES_/PregeneratedCharacters` |
| `CharacterParamsPatchData` | POCO (не def) | вложен в JSON `FeatDef.Apply` |
| `FeatDef` | `AbstractDefinition` | `Definitions/_ADVENTURES_/Feats` |
| `ItemDef` | `AbstractDefinition` | `Definitions/_ADVENTURES_/Items` |
| `SpellDef` | `AbstractDefinition` | `Definitions/_ADVENTURES_/Spells` |
| `BattleActionDef` | `AbstractDefinition` | `Definitions/_ADVENTURES_/BattleActions` |
| `RuleDef` | `AbstractDefinition` | `Definitions/_ADVENTURES_/Rules` |
| `BattleRuleDef` | `AbstractDefinition` | `Definitions/_ADVENTURES_/BattleRules` |
| `RuleSettingsDef` | `AbstractDefinition` | `Definitions/_ADVENTURES_/RuleSettings/RuleSettings` (single) |
| `AvatarsDef` | `AbstractDefinition` | `Definitions/_ADVENTURES_/Avatars/Avatars` (single) |
| `ProjectGlobalSettingsDef` | `AbstractDefinition` | `Definitions/_ADVENTURES_/GlobalSettings/GlobalSettings` (single) |
| `LocalizationSettingsDef` | `AbstractDefinition` | `Definitions/_ADVENTURES_/LocalizationSettings/LocalizationSettings` (single) |

### Справочник полей Adventure-дефов (сверка C# ↔ JSON)

`Id` задаётся загрузчиком из имени JSON-файла (`[JsonIgnore]` в `AbstractDefinition`), в JSON не пишется.

Перечисления в JSON сериализуются **строковыми именами** enum (`"Medium"`, `"Weapon"`, `"GeneralFeat"`, `"Location"` и т.д.). `Loader` использует `JsonConvert.DeserializeObject` без кастомных настроек.

| Def | Поле (C#) | Тип | В JSON | В текущем контенте |
|---|---|---|---|---|
| `ProjectGlobalSettingsDef` | `SaveName` | `string` | `SaveName` | да |
| | `SaveFolder` | `string` | `SaveFolder` | да |
| | `FileExtension` | `string` | `FileExtension` | да |
| | `EnabledEncryption` | `bool` | `EnabledEncryption` | да |
| | `EncryptionKey` | `string` | `EncryptionKey` | да |
| `LocalizationSettingsDef` | `DefaultLanguage` | `SystemLanguage` | `DefaultLanguage` | да |
| | `LanguageFolders` | `Dictionary<SystemLanguage, string>` | `LanguageFolders` | да |
| `RuleSettingsDef` | `Tags` | `List<string>` | `Tags` | да |
| | `Rule` | `string` | `Rule` | да, id `RuleDef` (`"GeneralRule"`) |
| | `BattleRule` | `string` | `BattleRule` | да, id `BattleRuleDef` (`"GeneralBattleRule"`) |
| | `StartAdventure` | `string` | `StartAdventure` | да, id `AdventureDef` (`"AdventureTavernByMartha"`) |
| `AvatarsDef` | `Free` | `Dictionary<string, List<string>>` | `Free` | да; ключ — id `AncestryDef`, значение — список avatar id |
| | `Packs` | `Dictionary<string, AvatarPackData>` | `Packs` | да; ключ — pack id |
| `AvatarPackData` | `ProductId` | `string` | `ProductId` | да (IAP / store key; может быть `""`) |
| | `Price` | `int` | `Price` | да (soft-currency; `0` если не используется) |
| | `Avatars` | `Dictionary<string, List<string>>` | `Avatars` | да; ключ — id `AncestryDef`, значение — список avatar id |
| `RuleDef` | `Tags` | `List<string>` | `Tags` | да |
| | `AbilityBoostPointCost` | `Dictionary<int, int>` | `AbilityBoostPointCost` | да; ключи в JSON — строки (`"4"`, `"5"`) |
| | `SkillDependencies` | `Dictionary<string, string>` | `SkillDependencies` | да, 18 навыков (включая `Perception`) |
| | `ParameterFormulas` | `Dictionary<string, string>` | `ParameterFormulas` | да; навыки (`ABILITY+PROFICIENCY+ITEMS`) + `MaxHitPoints` |
| `BattleRuleDef` | `Tags` | `List<string>` | `Tags` | да |
| | `UseMultipleAttackPenalty` | `bool` | `UseMultipleAttackPenalty` | да (`true` в `GeneralBattleRule`) |
| | `AttackTags` | `List<string>` | `AttackTags` | да; теги действия/способности для учёта MAP (`"ATTACK"`) |
| | `MultipleAttackPenaltyTable` | `List<int>` | `MultipleAttackPenaltyTable` | да; таблица с 1-й атаки (`[0, -5, -10, -10, -10, -10]`) |
| | `MultipleAttackPenaltyTableByWeaponTag` | `Dictionary<string, List<int>>` | `MultipleAttackPenaltyTableByWeaponTag` | да; override по тегу оружия (`AGILE` → `[0, -4, -8, -8, -8, -8]`) |
| `BattleActionDef` | `Disabled` | `bool` | `Disabled` | да |
| | `Tags` | `List<string>` | `Tags` | да (`ATTACK`, `SPELL`, …) |
| | `Icon` | `string` | `Icon` | да (часто `""`) |
| | `Title` | `string` | `Title` | да |
| | `Description` | `string` | `Description` | да |
| | `ActionCost` | `int` | `ActionCost` | да |
| | `TargetType` | `BattleActionTargetType` | `TargetType` | да (`Self`, `EnemySingle`, `EnemyAll`, …) |
| | `AvailabilityRestrictions` | `List<Restriction>` | `AvailabilityRestrictions` | да (`[]` или `CharacterParams`) |
| | `RequiredWeaponTagsAny` | `List<string>` | `RequiredWeaponTagsAny` | да (часто `[]`) |
| | `RequiredSpellId` | `string` | `RequiredSpellId` | да (`""` = spell выбирается в runtime) |
| | `Effects` | `List<BattleActionEffectData>` | `Effects` | да |
| `BattleActionEffectData` | `Type` | `BattleActionEffectType` | `Type` | да (`Damage`, `Heal`, `AddStatus`, …) |
| | `Value` | `int` | `Value` | да |
| | `ValueFormula` | `string` | `ValueFormula` | да (`""` = Value / урон оружия) |
| | `StatusId` | `string` | `StatusId` | да |
| | `StatusValue` | `int` | `StatusValue` | да |
| | `DurationRounds` | `int` | `DurationRounds` | да |
| | `OnHitOnly` | `bool` | `OnHitOnly` | да |
| `ClassDef` | `Disabled` | `bool` | `Disabled` | да |
| | `Restrictions` | `List<Restriction>` | `Restrictions` | нет (опционально) |
| | `Tags` | `List<string>` | `Tags` | да |
| | `Icon` | `string` | `Icon` | нет (опционально) |
| | `Title` | `string` | `Title` | да |
| | `Description` | `string` | `Description` | да |
| | `HitPointsPerLevel` | `int` | `HitPointsPerLevel` | да (HP класса за уровень) |
| | `Features` | `Dictionary<int, List<string>>` | `Features` | нет (опционально; ключи в JSON — строки) |
| | `EquippedItems` | `List<EquippedItemStateData>` | `EquippedItems` | нет (опционально; базовые слоты класса + стартовая экипировка; `Slot` из `Glossary.Items`, пустой `ItemId` = свободный слот) |
| `AncestryDef` | `Disabled` | `bool` | `Disabled` | да |
| | `Restrictions` | `List<Restriction>` | `Restrictions` | нет (опционально) |
| | `Tags` | `List<string>` | `Tags` | да |
| | `Icon` | `string` | `Icon` | нет (опционально) |
| | `Title` | `string` | `Title` | да |
| | `Description` | `string` | `Description` | да |
| | `Size` | `AncestrySize` | `Size` | да (`Small` / `Medium` / `Large`) |
| | `Speed` | `int` | `Speed` | да |
| | `HitPoints` | `int` | `HitPoints` | да (HP происхождения, один раз) |
| | `Features` | `Dictionary<int, List<string>>` | `Features` | нет (опционально; ключи в JSON — строки) |
| | `EquippedItems` | `List<EquippedItemStateData>` | `EquippedItems` | нет (опционально; слоты происхождения: Finger/Neck/Tail и т.п.; пустой `ItemId` = свободный слот) |
| | `Names` | `Dictionary<CharacterGender, List<string>>` | `Names` | нет (контент ещё на legacy `MaleNames`/`FemaleNames`) |
| `BackgroundDef` | `Disabled` | `bool` | `Disabled` | да |
| | `Restrictions` | `List<Restriction>` | `Restrictions` | да, `[]` |
| | `Tags` | `List<string>` | `Tags` | да |
| | `Icon` | `string` | `Icon` | да (часто `""`) |
| | `Title` | `string` | `Title` | да |
| | `Description` | `string` | `Description` | да |
| | `Features` | `List<string>` | `Features` | да (id feat/feature) |
| `PregeneratedCharacterDef` | `Avatar` | `string` | `Avatar` | да (часто `""`; id/ключ аватара) |
| | `Name` | `string` | `Name` | нет (опционально) |
| | `Gender` | `CharacterGender` | `Gender` | нет (опционально) |
| | `Ancestry` | `string` | `Ancestry` | нет (опционально; id `AncestryDef`) |
| | `Class` | `string` | `Class` | да, id `ClassDef` |
| | `Background` | `string` | `Background` | нет (опционально; id `BackgroundDef`) |
| | `Parameters` | `Dictionary<string, int>` | `Parameters` | нет (опционально) |
| | `EquippedItems` | `List<EquippedItemStateData>` | `EquippedItems` | нет (опционально; слоты/стартовая экипировка прегена) |
| | `Spells` | `Dictionary<string, int>` | `Spells` | нет (опционально) |
| `CharacterParamsPatchData` | `Add` | `Dictionary<string, int>` | `Add` | нет (опционально) |
| | `Set` | `Dictionary<string, int>` | `Set` | нет (опционально) |
| | `AlsoApplyFeatIds` | `List<string>` | `AlsoApplyFeatIds` | нет (опционально; id `FeatDef`) |
| | `ConditionDuration` | `int` | `ConditionDuration` | нет (опционально; ходы для `FeatType.Condition`) |
| `FeatDef` | `Disabled` | `bool` | `Disabled` | да |
| | `Restrictions` | `List<Restriction>` | `Restrictions` | да, но везде `[]` |
| | `Tags` | `List<string>` | `Tags` | да (часто пустой) |
| | `Icon` | `string` | `Icon` | нет (опционально) |
| | `Title` | `string` | `Title` | да |
| | `Description` | `string` | `Description` | да |
| | `Type` | `FeatType` | `Type` | да |
| | `Level` | `int` | `Level` | да |
| | `Apply` | `CharacterParamsPatchData` | `Apply` | нет (опционально) |
| | `Options` | `List<string>` | `Options` | нет (опционально; id `FeatDef`) |
| | `AdditionalSlots` | `List<string>` | `AdditionalSlots` | нет (опционально; типы слотов из `Glossary.Items`, например дополнительные `Hand`) |
| `ItemDef` | `Disabled` | `bool` | `Disabled` | да |
| | `IsQuestItem` | `bool` | `IsQuestItem` | да, везде `false` |
| | `Category` | `ItemCategory` | `Category` | да |
| | `Level` | `int` | `Level` | да |
| | `Tags` | `List<string>` | `Tags` | да |
| | `Title` | `string` | `Title` | да |
| | `Description` | `string` | `Description` | да |
| | `Price` | `int` | `Price` | да |
| | `AvailableSlots` | `List<string>` | `AvailableSlots` | нет (опционально; whitelist типов слотов из `Glossary.Items`) |
| | `Features` | `List<string>` | `Features` | нет (опционально; id `FeatDef`) |
| | `Type` | `string` | `Type` | да для оружия (`Glossary.Weapons.SIMPLE` / `MARTIAL` / `ADVANCED`) |
| | `Group` | `string` | `Group` | да для оружия (`Glossary.Weapons.*` группа) |
| | `AbilityDependencies` | `List<string>` | `AbilityDependencies` | да для оружия (ключи ability: `STR`, `DEX`, …) |
| | `AttackModifierFormula` | `string` | `AttackModifierFormula` | да для оружия |
| | `DamageModifierFormula` | `string` | `DamageModifierFormula` | да для оружия |
| | `DamageDice` | `List<WeaponDamageDicePartData>` | `DamageDice` | да для оружия |
| `WeaponDamageDicePartData` | `Count` | `int` | `Count` | да (число костей, например `1` или `2`) |
| | `DiceType` | `DiceType` | `DiceType` | да (`D4`, `D6`, `D8`, … из `Modules.Dices`) |
| `SpellDef` | `Disabled` | `bool` | `Disabled` | да |
| | `Type` | `SpellType` | `Type` | да |
| | `Level` | `int` | `Level` | да (`0` для cantrip) |
| | `Tags` | `List<string>` | `Tags` | да |
| | `Title` | `string` | `Title` | да |
| | `Description` | `string` | `Description` | да |
| `AdventureDef` | см. `AdventureData` | — | — | см. ниже |

**Значения enum (C# = JSON):**

| Enum | Значения |
|---|---|
| `AncestrySize` | `Small`, `Medium`, `Large` |
| `CharacterGender` (модуль `State`) | `Male`, `Female` — ключи словаря `AncestryDef.Names` |
| `FeatType` | `AncestryFeat`, `BackgroundSkillFeat`, `SkillFeat`, `GeneralFeat`, `ClassFeat`, `ClassFeature`, `Boost`, `Condition` |
| `ItemCategory` | `Weapon`, `Armor`, `Shield`, `Consumable`, `Equipment` |
| `SpellType` | `Cantrip`, `Spell`, `Focus`, `Ritual` |
| `AdventureType` | `Adventure`, `Chapter`, `Location` |

- `AdventureDef`  
  Деф приключения; наследует `AdventureData` из модуля `RPG`. Поля контракта:

  | Поле | Тип | В текущем JSON |
  |---|---|---|
  | `Disabled` | `bool` | нет (опционально) |
  | `Tags` | `List<string>` | да |
  | `IgnoredTags` | `List<string>` | нет (опционально) |
  | `IsRepeatable` | `bool` | да (`true` только для `Location` в типовых шаблонах TEA) |
  | `Type` | `AdventureType` | да |
  | `AdventureLinks` | `List<string>` | нет (опционально) |
  | `Title` | `string` | да |
  | `Description` | `string` | да |
  | `Restrictions` | `List<Restriction>` | да (часто `[]`) |
  | `StartScenes` | `List<string>` | да |
  | `Scenes` | `Dictionary<string, SceneData>` | да |

  Вложенные типы сцены (`SceneData`, `SceneContentData`, `ChoiceData`, `ChoiceActionData`, `ChoiceDiceCheckData`) — в модуле `RPG`; подробнее в [RPG.md](RPG.md#модель-данных-adventure). Контент сцены поддерживает `RandomImage` и `Slideshow` (поле `Values`). В `ChoiceType` доступны `Default` и `DiceCheck`; для `DiceCheck` используется блок `ChoiceData.DiceCheck` с полями броска (`DifficultyClass`, `DiceType`, `DiceOptions`, `DiceCheckParam` — ключ атрибута/скилла) и outcome action-списками (`OnCriticalSuccess` / `OnSuccess` / `OnFailure` / `OnCriticalFailure`); для `Default` блок `DiceCheck` должен отсутствовать, а `Actions` — использоваться вместо outcome-списков (TEA-валидация с `Fix`). Choice-actions в runtime: `GoToScene` (`SceneId`), `GoToAdventure` (`AdventureId`), `GoToRandomAdventure`, `GoToRandomScene` (`SceneId` через `;`), `OpenWindow` (`WindowId`, stub), `SetWorldParams`, `SetAdventureParams`, `SetGlobalParams`; ключи — `Glossary.ChoiceActions.*`, ids окон хаба — `Glossary.Windows.*`. Executors пишут в `State` через соответствующие state-actions (см. [RPG.md](RPG.md)).

- `ClassDef`  
  Класс персонажа. Поля: `Disabled`, `Restrictions`, `Tags`, `Icon`, `Title`, `Description`, `HitPointsPerLevel`, `Features` (`Dictionary<int, List<string>>` — уровень → список id фич/черт), `EquippedItems` (`List<EquippedItemStateData>` — базовый набор слотов класса и стартовая экипировка: оружие/броня/`Bag` и т.п.).  
  `HitPointsPerLevel` — HP класса за каждый уровень (до модификатора `CON`). Значения также временно дублируются в `Tags` как `hp_per_level-*`.  
  В `EquippedItems` поле `Slot` — тип слота из `Glossary.Items` (`Hand`, `Legs`, `Head`, `Body`, `Bag`, `Finger`, `Neck`, `Tail`); несколько одинаковых типов допустимы. Пустой/`null` `ItemId` означает свободный доступный слот.  
  Общий каркас метаданных совпадает с `BackgroundDef` / `AncestryDef` (без size/speed/имён).

- `AncestryDef`  
  Ancestry (раса/происхождение в смысле PF2e Ancestry) персонажа. Поля: `Disabled`, `Restrictions`, `Tags`, `Icon`, `Title`, `Description`, `Size` (`AncestrySize`), `Speed`, `HitPoints`, `Features` (`Dictionary<int, List<string>>`), `EquippedItems` (`List<EquippedItemStateData>` — слоты/стартовые предметы происхождения: украшения `Finger` / `Neck` / `Tail` и т.п.), `Names` (`Dictionary<CharacterGender, List<string>>`).  
  `HitPoints` — HP происхождения (добавляются один раз, не масштабируются уровнем). Значения также временно дублируются в `Tags` как `hp-*`.  
  Разделение со слотами класса: оружие/броня/`Bag` обычно в `ClassDef.EquippedItems`; ancestry-зависимые слоты (пальцы, шея, хвост) — в `AncestryDef.EquippedItems`. При создании персонажа runtime должен объединить оба списка в `CharacterStateData.EquippedItems`.  
  `Names` — пул имён по полу (`Male` / `Female` из `CharacterGender` в модуле `State`); связь с `CharacterStateData.Gender` — в [State.md](State.md#adventure-charactersstatedata-и-characterstatedata). Legacy-ключи JSON `MaleNames` / `FemaleNames` больше не соответствуют полям C#.  
  Каталог аватаров — не в ancestry: single-def `AvatarsDef` (`DefinitionsManager.Avatars`).

- `AvatarsDef`  
  Single-каталог аватаров персонажа (`Definitions/_ADVENTURES_/Avatars/Avatars`). Поля: `Free` (`Dictionary<string, List<string>>` — ancestry id → бесплатные avatar id), `Packs` (`Dictionary<string, AvatarPackData>` — pack id → пак).  
  `AvatarPackData`: `ProductId` (store/IAP key), `Price` (soft-currency), `Avatars` (ancestry id → avatar id пака).  
  Разблокировка задаётся местом id: `Free` = всегда доступен для ancestry; id внутри `Packs[packId].Avatars` = через владение паком. Путь арта по конвенции от avatar id (например `Resources/Adventures/Avatars/{id}`).  
  Загрузка: `DefinitionsManager.Avatars` через `LoadSingle` (рядом с `RuleSettings` в `LoadAll()`).

- `BackgroundDef`  
  Предыстория персонажа (PF2e Background). Поля: `Disabled`, `Restrictions`, `Tags`, `Icon`, `Title`, `Description`, `Features` (`List<string>` — id связанных feat/feature).  
  Загрузка: `DefinitionsManager.Backgrounds` из `_ADVENTURES_/Backgrounds` (между Ancestries и Feats в `LoadAll()`). Id в state — `CharacterStateData.Background`.

- `PregeneratedCharacterDef`  
  Заготовленный персонаж (шаблон для быстрого старта / выбора готового героя). Поля: `Avatar`, `Name`, `Gender`, `Ancestry`, `Class`, `Background`, `Parameters`, `EquippedItems`, `Spells`.  
  `EquippedItems` — слоты и стартовая экипировка прегена (`Slot` из `Glossary.Items`, `ItemId` — id `ItemDef` или пусто для свободного слота).  
  Загрузка: `DefinitionsManager.PregeneratedCharacters` из `_ADVENTURES_/PregeneratedCharacters` (между Backgrounds и Feats в `LoadAll()`).

- `FeatDef`  
  Черта/способность. Поля: `Disabled`, `Restrictions`, `Tags`, `Icon`, `Title`, `Description`, `Type` (`FeatType`), `Level`, `Apply` (`CharacterParamsPatchData`), `Options` (`List<string>` — id дочерних feat при выборе), `AdditionalSlots` (`List<string>` — дополнительные типы слотов из `Glossary.Items`).  
  `Level` — минимальный уровень персонажа для взятия черты (PF2e-style). `Apply` — статический эффект при выдаче / снятии черты; runtime Apply / Unapply — через `CharacterParametersOperator` в модуле `State` (см. [State.md](State.md#adventure-write-api-параметров-apply--unapply)). Практика применения: [Feats.md](Feats.md). `AlsoApplyFeatIds` внутри патча — каскад. `AdditionalSlots` расширяет набор слотов персонажа (например две дополнительные `Hand` для четырёхрукого существа); при Unapply оператор удаляет эти слоты и, если слот занят, переносит предмет в свободный `Bag`/общий инвентарь. `Restrictions` — структурированные требования (prerequisites); формат `Restriction` — см. [Restrictions.md](Restrictions.md).  
  `Type = Condition` — статусный / временный эффект (не брать в level-up). При `Apply.ConditionDuration > 0` оператор пишет таймер в `CharacterStateData.StatusEffects[featId]`; декремент — `EndCharacterTurnStateAction` (см. [Feats.md](Feats.md)). Сложная tick-логика (урон, per-tick patch и т.д.) задаётся опциональным `ConditionDef` с тем же id. JSON с `Apply`/`Options` уже есть в стартовом контенте.

- `CharacterParamsPatchData`  
  Общий POCO патча параметров персонажа (`Assets/Modules/Definitions/Scripts/Implementation/Adventures/Defs/CharacterParamsPatchData.cs`). Не наследует `AbstractDefinition`, не загружается отдельно. Сейчас используется в `FeatDef.Apply`; позже может переиспользоваться в других adventure-дефах (ancestry, background, предметы / `ItemDef.Features`). Ключи — `Glossary.Characters` и согласованные id (в т.ч. id feat для флага «черта взята»).

  **Семантика Apply / Unapply** (владелец — `CharacterParametersOperator` в `State`, не JSON):

  | Поле | Apply | Unapply |
  |------|-------|---------|
  | `Add` | `current += value` | `current += -value` |
  | `Set` | `current = value` | патч ставил **≠ 0** → `0`; патч ставил **0** → `1` (инверсия флага) |
  | `AlsoApplyFeatIds` | каскадный Apply | каскадный Unapply (обычно обратный порядок) |
  | `ConditionDuration` | если feat `Type=Condition` и `> 0` → `StatusEffects[featId] = Duration` (повторный Apply только refresh) | удалить `StatusEffects[featId]` |

  Пример JSON (`Apply` внутри feat):

  ```json
  "Apply": {
    "Add": { "MaxHitPoints.PerLevel": 1, "Speed": 5 },
    "Set": { "_Toughness": 1 },
    "AlsoApplyFeatIds": ["_SomeLinkedFeat"]
  },
  "Options": ["_FightingStyleArchery", "_FightingStyleSword"]
  ```

  Пример timed Condition:

  ```json
  "Type": "Condition",
  "Apply": {
    "Add": { "Perception.Bonus": -1 },
    "Set": { "_ConditionFrightened": 1 },
    "ConditionDuration": 3
  }
  ```

  Для Max HP не пишите итог в `MaxHitPoints`: бонусы за уровень кладите в `MaxHitPoints.PerLevel`, flat — в `MaxHitPoints.Bonus` (чтение итога — `CharacterParametersProxy.GetTotalValue`).
- `ItemDef`  
  Предмет. Базовые поля: `Disabled`, `IsQuestItem`, `Category` (`ItemCategory`), `Level`, `Tags`, `Title`, `Description`, `Price`, `AvailableSlots` (`List<string>` — whitelist типов слотов из `Glossary.Items`, куда предмет можно надеть/положить), `Features` (`List<string>` — id связанных `FeatDef`; Apply/Unapply при экипировке через `CharacterItemFeaturesOperator` в inventory state-actions, правило Bag — см. [Feats.md](Feats.md) / [State.md](State.md)).  
  Для оружия (`Category = Weapon`) дополнительно:
  - `Type` — тип владения (`Glossary.Weapons.SIMPLE` / `MARTIAL` / `ADVANCED`, значения `"Weapon.Simple"` и т.п.);
  - `Group` — группа оружия (`Glossary.Weapons.SWORD`, `BOW`, …);
  - `AbilityDependencies` — кандидаты ability для формул; если больше одного, ключевое слово `ATTACK_ABILITY_BEST` берёт максимум;
  - `AttackModifierFormula` — формула модификатора атаки (литералы, `+`/`*`/`()`, ключевые слова `WeaponProxy`);
  - `DamageModifierFormula` — формула модификатора урона **без** броска костей;
  - `DamageDice` — список `WeaponDamageDicePartData` (`Count` + `DiceType`) для броска урона (прокси кости не бросает).

  Пример JSON оружия:

  ```json
  "Type": "Weapon.Martial",
  "Group": "Sword",
  "AbilityDependencies": ["STR", "DEX"],
  "AttackModifierFormula": "ATTACK_ABILITY_BEST+PROFICIENCY+ITEMS+GROUP_ATTACK_BONUS",
  "DamageModifierFormula": "STR+ITEMS+GROUP_DAMAGE_BONUS",
  "DamageDice": [{ "Count": 1, "DiceType": "D8" }]
  ```

  Чтение модификаторов — `WeaponProxy` в модуле `State` (см. [State.md](State.md#adventure-weaponproxy)). Сырые бонусы в `Parameters`:
  - профа по типу: `Weapon.Martial.ProfRank`;
  - item bonus: `<ItemId>.Attack.ItemsBonus`, `<ItemId>.Damage.ItemsBonus`;
  - group bonus: `<Group>.Attack.Group.Bonus`, `<Group>.Damage.Group.Bonus` (например `Bow.Attack.Group.Bonus`).

  Стартовый контент: 6 weapon JSON (`_BattleAxe`, `_Dagger`, `_Longsword`, `_Rapier`, `_Shortbow`, `_Shortsword`) заполнены этими полями; у не-оружия поля опциональны / отсутствуют.
- `SpellDef`  
  Заклинание. Поля: `Disabled`, `Type` (`SpellType`), `Level`, `Tags`, `Title`, `Description`.

- `RuleDef`  
  Общее правило приключения (вне боя): механика создания/прокачки персонажа и связанные настройки. Поля:
  - `Tags` — метки набора правил (`core`, `exploration`, `social` и т.п.);
  - `AbilityBoostPointCost` (`Dictionary<int, int>`) — пороговая таблица стоимости ability boost в очках: **ключ** — пороговое значение (номер буста / уровень шкалы), **значение** — цена в очках для этого порога. В рантайме для текущего номера буста выбирается **ближайший ключ** из словаря — по нему определяется стоимость (например, `4 → 1`, `5 → 2`: до порога 5 стоимость 1, начиная с порога 5 — 2);
  - `SkillDependencies` (`Dictionary<string, string>`) — привязка навыка к характеристике: **ключ** — id навыка (имя skill, см. `Glossary.Characters`), **значение** — ключ ability (`STR`, `DEX`, `CON`, `INT`, `WIS`, `CHA` из `Glossary.Characters`);
  - `ParameterFormulas` (`Dictionary<string, string>`) — формулы вычисляемых параметров персонажа: **ключ** — id параметра, **значение** — выражение с `+`, `*`, скобками. Обычные токены (`STR`, `DEX`, `CON`, `Level`, …) читаются сырыми из `CharacterStateData.Parameters`. Ключевые слова:
    - `PROFICIENCY` / `ITEMS` — через суффиксы `PROFICIENCY_SUFFIX` / `ITEMS_SUFFIX` к requested key;
    - `ANCESTRY_HP` — `AncestryDef.HitPoints` по `CharacterStateData.Ancestry`;
    - `CLASS_HP` — `ClassDef.HitPointsPerLevel` по `CharacterStateData.Class`;
    - `PER_LEVEL` / `BONUS` — через суффиксы `PER_LEVEL_SUFFIX` / `BONUS_SUFFIX` (например `MaxHitPoints.PerLevel`, `MaxHitPoints.Bonus`).  
    Чтение итога — через `CharacterParametersProxy.GetTotalValue` (нужны `CharacterStateData`, `RuleDef`, `DefinitionsManager`; см. [State.md](State.md#adventure-characterparametersproxy)).  
  Стартовый контент: `GeneralRule` — `Tags`: `core`, `exploration`, `social`; `AbilityBoostPointCost`: пороги `4 → 1`, `5 → 2`; `SkillDependencies` и skill-формулы — 18 навыков PF2e Remaster (включая `Perception`); плюс `MaxHitPoints`: `ANCESTRY_HP+(CLASS_HP+CON+PER_LEVEL)*Level+BONUS`.

- `BattleRuleDef`  
  Боевое правило приключения. Поля: `Tags`; MAP — `UseMultipleAttackPenalty`, `AttackTags`, `MultipleAttackPenaltyTable`, `MultipleAttackPenaltyTableByWeaponTag`.  
  Таблица MAP: индекс `i` → штраф для атаки номер `i + 1` в раунде (индекс 0 — первая атака); за пределами списка повторяется последнее значение.  
  Стартовый контент: `GeneralBattleRule` (`core`, `combat`, `encounter`; MAP как в PF2e: `[0, -5, -10, …]`, Agile `[0, -4, -8, …]`).  
  Расширение economy боя — в раннем прототипе [Battle.md](Battle.md). Каталог действий: `BattleActionDef`.

- `BattleActionDef`  
  Боевое действие (опция в бою). Поля: `Disabled`, `Tags`, `Icon`, `Title`, `Description`, `ActionCost`, `TargetType`, `AvailabilityRestrictions`, `RequiredWeaponTagsAny`, `RequiredSpellId`, `Effects`.  
  Стартовый контент (тестовый, id с префиксом `_`): `_Strike`, `_CastSpell`, `_RaiseShield`, `_Cleave`, `_PaladinSuperPuperAttack` (с `CharacterParams`). Runtime resolver ещё не реализован — см. [Battle.md](Battle.md).

- `RuleSettingsDef`  
  Single-def настроек правил и стартовой точки приключения. Поля: `Tags`, `Rule` (id `RuleDef`), `BattleRule` (id `BattleRuleDef`), `StartAdventure` (id `AdventureDef`). Стартовые значения: `Rule = "GeneralRule"`, `BattleRule = "GeneralBattleRule"`, `StartAdventure = "AdventureTavernByMartha"`.

- `ProjectGlobalSettingsDef`  
  Глобальные настройки adventure-проекта (сейв профиля). Поля: `SaveName`, `SaveFolder`, `FileExtension`, `EnabledEncryption`, `EncryptionKey`. Используется `AdventureStateInitTask` для инициализации `AdventureStateManager`.

- `LocalizationSettingsDef`  
  Single-def настроек локализации. Поля: `DefaultLanguage`, `LanguageFolders` (`SystemLanguage -> folder`).  
  Используется `LocalizationManager` для выбора fallback-языка и сопоставления `SystemLanguage` с папкой в `Assets/Modules/Localization/Resources/Langs`.

- `RoundDef`  
  Деф раунда Match3; связывает `GameZone`, `Gems`, `Objectives` по id.

- `GameZoneDef` и `CellsMapDef`  
  Описывают форму поля (`Mask`/`Presets`) и маппинг чисел в матрице на `CellDef`.

- `GemDef` и `GameZoneGemsDef`  
  Описывают типы фишек, префабы, теги, match-действия и весовую конфигурацию генерации.

- `ObjectivesDef`  
  Стартовые счётчики и условия победы/поражения (`Restriction`):
  - `StartScores` — начальные значения счётчиков;
  - `VictoryConditions` — список `Restriction` для победы;
  - `DefeatConditions` — список `Restriction` для поражения.

- `GameZonesEditorWindow` (Editor-only)  
  Визуальный редактор `GameZone` JSON в `Tools/Definitions/Match3/GameZonesEditor`.

- **TEA** (*Tool Edit Adventures*, Editor-only)  
  Редактор JSON-приключений (`AdventureData` и вложенные сущности). Меню: `Tools/Definitions/Adventures/Adventure Editor`.  
  Код: `Assets/Modules/Definitions/Scripts/Editor/Adventures/`. Документация: `Assets/Modules/Definitions/Scripts/Editor/Adventures/README.md`.  
  Состав: `AdventureEditorWindow` (главное окно с адаптивной раскладкой `Scenes`/`Content`/`Choices`), `AdventureGraphPreviewWindow`, `AdventureValidationWindow`, `AdventureLocalizationExportWindow`, `IdentifierPromptWindow`, `CreateOptionPickerWindow`, `AdventureEditorFileRepository`, `AdventureEditorServices` (в т.ч. `AdventureLocalizationGenerationService`), create-option реестры в `CreateOptions/`.  
  Локализация: кнопка `Localization` в toolbar → генерация ключей в `Title`, `Description`, `SceneContentData.Value` (только `Text`), `ChoiceData.Text`/`Description` + экспорт tab-separated `.txt` для Google Sheets.

## Adventure-дефы правил: текущий контракт

На текущем этапе правила приключения вынесены в отдельные типы:

| Слой | Назначение |
|---|---|
| `RuleDef` | Статические правила вне боя: теги набора + механические таблицы (стоимость ability boost, skill → ability, формулы итоговых параметров) |
| `BattleRuleDef` | Статические боевые правила, включая таблицы MAP (`MultipleAttackPenaltyTable` / `…ByWeaponTag`) |
| `BattleActionDef` | Каталог боевых действий (`_Strike`, `_CastSpell`, …); ранний прототип — см. [Battle.md](Battle.md) |
| `RuleSettingsDef` | Single-def, указывающий, какие правила из коллекций считаются активными по умолчанию |

Активный `RuleDef` выбирается через `RuleSettingsDef.Rule` (id JSON-файла, например `GeneralRule`). Потребитель читает поля правила как конфигурацию: для ability boost — ближайший порог в `AbilityBoostPointCost`; для связанной ability навыка — `SkillDependencies[skillId]`; для итогового значения навыка/perception/MaxHitPoints — `ParameterFormulas[...]` через `CharacterParametersProxy.GetTotalValue`. Ключи навыков и ability согласованы с `Glossary.Characters`; ключи параметров choice-actions — с `Glossary.ChoiceActions` (например, `SCENE_ID` = `"SceneId"`) в `Modules.Definitions.Scripts.Implementation.Adventures.Constants`.

Ссылки между дефами — строковые id (имя JSON-файла), по тому же принципу, что `RoundDef -> GameZone/Gems/Objectives`.

Порядок загрузки в adventures `DefinitionsManager`: `LoadRules` и `LoadBattleRules` выполняются **до** `LoadRuleSettings`, чтобы к моменту чтения настроек коллекции правил уже были в памяти (валидация ссылок по-прежнему на стороне потребителя).

## Adventure-дефы персонажа: текущий контракт и эволюция

### Минимальный контракт (сейчас)

На текущем этапе adventure-дефы персонажа содержат **необходимый минимум** для загрузки, отображения в UI, ссылок из `Modules.State` и вычисления ключевых производных параметров:

- идентификация через `Id` (имя JSON-файла);
- метаданные (`Title`, `Description`, `Disabled`, `Icon`);
- классификация (`Type`, `Level`, `Category`, `Size`, `Speed`, `IsQuestItem` — где применимо);
- `AncestryDef.HitPoints` / `ClassDef.HitPointsPerLevel` — константы для формулы `MaxHitPoints`;
- `Tags` — произвольные строковые метки (источник книги, роль; legacy-подсказки вроде `hp-*` / `hp_per_level-*` пока дублируют поля HP);
- `Restrictions` на `ClassDef` / `AncestryDef` / `BackgroundDef` / `FeatDef` — структурированные требования (см. [Restrictions.md](Restrictions.md));
- `Features` — ссылки на связанные feat/feature id (`Dictionary<int, List<string>>` у класса/ancestry по уровню; `List<string>` у background и `ItemDef`);
- `Names` на `AncestryDef` — пул имён по `CharacterGender` для создания персонажа;
- `AvatarsDef` — каталог аватаров: `Free` / `Packs` по ancestry id (не на `AncestryDef`);
- `FeatDef.Apply` / `Options` — контракт эффектов черты; runtime Apply / Unapply — `CharacterParametersOperator`;
- `RuleDef.ParameterFormulas` — декларативные формулы итоговых параметров (навыки, `MaxHitPoints`), читаются через `CharacterParametersProxy`.

`Tags` **не являются** финальным механическим слоем: это временный способ группировки и заметок; механические числа (HP ancestry/class) уже вынесены в отдельные поля.

### Разделение Defs и State

| Слой | Что хранит |
|---|---|
| **Defs** | Статический контент: что даёт класс, ancestry, предыстория, черта, предмет или заклинание; формулы в `RuleDef.ParameterFormulas` и оружейные формулы в `ItemDef`; патчи в `FeatDef.Apply` |
| **State** | Сырой прогресс персонажа: `CharacterStateData.Parameters` (abilities, ranks, bonuses, текущие HP…), `Spells`, `StatusEffects` (таймеры Condition-feats: feat id → оставшиеся ходы), экипировка |
| **Read API** | `CharacterParametersProxy` — raw/total; конвенция закреплена в доках/комментариях |
| **Write API** | `CharacterParametersOperator` — Apply / Unapply feats/патчей (+ таймер Condition в `StatusEffects`); `CharacterItemFeaturesOperator` — `ItemDef.Features` в equip-экшенах (правило Bag); `EndCharacterTurnStateAction` — декремент таймеров; слепок для прокачки |
| **Weapon proxy** | Модификаторы оружия через `WeaponProxy` |

### Планируемое расширение (механики)

Уже есть:
- формулы навыков и `MaxHitPoints` в `RuleDef.ParameterFormulas`;
- HP ancestry/class в полях дефов;
- контракт `FeatDef.Apply` и runtime `CharacterParametersOperator`;
- `ItemDef.Features` + `CharacterItemFeaturesOperator` в Equip/Unequip/Move/Remove;
- контракт прокачки: `UpdateCharacter` = слепок + последовательный Apply (см. [State.md](State.md) / [Feats.md](Feats.md));
- `FeatType.Condition` + `Apply.ConditionDuration` + `StatusEffects` таймеры + `EndCharacterTurnStateAction` (см. [Feats.md](Feats.md)).

Следующий этап — UI прокачки / создания персонажа и turn-loop боя.

По мере разработки тот же формат патча может появиться в других дефах (ancestry, background, предметы):

- бонусы и штрафы к характеристикам, навыкам, спасброскам (через сырые ключи / суффиксы);
- особенности (сопротивления, чувства, ограничения);
- эффекты заклинаний с явными значениями для записи в state.

Adventure-choice params (`ChoiceActionData.Params`) остаются отдельным контрактом для world/adventure/global; у персонажа — своя семантика (`Add` / `Set` вместо assign-only merge).

### Порядок полей в JSON

Порядок ключей в JSON следует порядку полей в C#-классе дефа (см. таблицу выше). `Id` в JSON не задаётся — присваивается загрузчиком из имени файла.

### Контент `_ADVENTURES_` (стартовый набор)

Стартовый контент ориентирован на PF2e Player Core (и смежные книги — в `Tags`). Подпапки используются для удобства редактирования; `LoadCollection()` загружает их рекурсивно, `Id` — только имя файла.

| Коллекция | Папка | Примеры подпапок | Фактический объём (2026-07-23) |
|---|---|---|---|
| `Adventures` | `_ADVENTURES_/Adventures` | `Locations`, `Tutorials`, `Debug` | 4 приключения |
| `Classes` | `_ADVENTURES_/Classes` | — | 24 класса |
| `Ancestries` | `_ADVENTURES_/Ancestries` | — | 8 ancestries |
| `Backgrounds` | `_ADVENTURES_/Backgrounds` | — | 2 тестовые (`_Farmhand`, `_Scholar`) |
| `PregeneratedCharacters` | `_ADVENTURES_/PregeneratedCharacters` | — | 1 тестовый (`_TestFighter`) |
| `Feats` | `_ADVENTURES_/Feats` | `General`, `Ancestry`, `Class`, `ClassFeature`, `Skill` | 15 черт |
| `Spells` | `_ADVENTURES_/Spells` | `Cantrips`, `Arcane`, `Divine` | 13 заклинаний |
| `Items` | `_ADVENTURES_/Items` | `Weapons`, `Armor`, `Shields`, `Consumables`, `Equipment` | 18 предметов |
| `BattleActions` | `_ADVENTURES_/BattleActions` | — | 5 тестовых действий (`_Strike`, `_CastSpell`, `_RaiseShield`, `_Cleave`, `_PaladinSuperPuperAttack`) |
| `Rules` | `_ADVENTURES_/Rules` | — | 1 правило (`GeneralRule`) |
| `BattleRules` | `_ADVENTURES_/BattleRules` | — | 1 боевое правило (`GeneralBattleRule`) |

Single-def:
- `RuleSettings` — `_ADVENTURES_/RuleSettings/RuleSettings.json`
- `GlobalSettings` — `_ADVENTURES_/GlobalSettings/GlobalSettings.json`
- `LocalizationSettings` — `_ADVENTURES_/LocalizationSettings/LocalizationSettings.json`

## Добавление нового def (явный чек-лист)

1. Создать C#-класс def:
   - Match3: `Assets/Modules/Definitions/Scripts/Implementation/Defs/...`;
   - Adventures: `Assets/Modules/Definitions/Scripts/Implementation/Adventures/Defs/...`;
   - унаследовать от `AbstractDefinition` (или от проектного исключения вроде `AdventureData` для `AdventureDef`).
2. Разместить JSON-файл(ы) в `Assets/Modules/Definitions/Resources/Definitions/<Folder>`.
3. Добавить поле в `DefinitionsManager`:
   - одиночный def: `MyDef MySettings;`
   - коллекция: `Dictionary<string, MyDef> MyDefs;`
4. Добавить метод загрузки в `DefinitionsManager`:
   - `LoadSingle<MyDef>("Definitions/<Folder>/<FileName>")`, или
   - `LoadCollection<MyDef>("Definitions/<Folder>")`.
5. Включить метод в список `loadMethods` внутри `LoadAll()` в корректном порядке (особенно если есть зависимости между дефами).
6. Для adventures-коллекций/single: актуализировать блок читов Definitions (`DefinitionsCheatSection` в `Tools/Cheats`) — вывод нового дефа в том же порядке, что в `LoadAll()`.
7. Проверить в рантайме:
   - JSON корректно десериализуется;
   - `Id` соответствует имени файла;
   - доступ к дефу из потребляющего модуля не возвращает `null` и не падает на `TryGetValue`.

## Практические заметки

- В проекте два менеджера дефов: Match3 (`Implementation.Defs`) и Adventures (`Implementation.Adventures`). Каждый загружает свой набор JSON из `Resources/Definitions`.
- Оба менеджера сейчас загружают `LocalizationSettingsDef` (пути: `Definitions/LocalizationSettings/LocalizationSettings` и `Definitions/_ADVENTURES_/LocalizationSettings/LocalizationSettings`).
- Adventure-контент лежит в `Definitions/_ADVENTURES_/...` (`GlobalSettings`, `RuleSettings`, `Avatars`, `Adventures`, `Classes`, `Ancestries`, `Backgrounds`, `PregeneratedCharacters`, `Feats`, `Items`, `Spells`, `BattleActions`, `Rules`, `BattleRules`). Коллекции могут иметь вложенные подпапки — на загрузку это не влияет.
- `RuleSettingsDef` ссылается на `RuleDef`, `BattleRuleDef` и стартовое приключение по id (`Rule`, `BattleRule`, `StartAdventure`); при добавлении новых правил или смене стартовой точки обновляйте `RuleSettings.json` или потребляющий код. Поля `RuleDef.AbilityBoostPointCost`, `RuleDef.SkillDependencies` и `RuleDef.ParameterFormulas` — опциональны в JSON; отсутствующий ключ словаря трактуется потребителем (дефолт / запрет / сырое чтение параметра) на стороне runtime. Для навыков в `GeneralRule` набор ключей skill-формул совпадает с `SkillDependencies`; отдельно задана формула `MaxHitPoints`.
- Ссылки из `Modules.State` на контент персонажа (`CharacterStateData.Ancestry`, `Class`, `Background`, `Gender`, `EquippedItems.ItemId`, стаки в `InventoryStateData`) — это `Id` соответствующих adventure-дефов (имя JSON-файла) или enum/state-поля (`Gender` — см. [State.md](State.md)). `ANCESTRY_HP` / `CLASS_HP` в формулах резолвятся через эти id в `AncestryDef.HitPoints` / `ClassDef.HitPointsPerLevel`.
- `AncestryDef.Names` индексируется по `CharacterGender`; id ancestry — в `CharacterStateData.Ancestry`. Текущие JSON ancestries ещё могут содержать legacy `MaleNames`/`FemaleNames` — их нужно мигрировать на `Names`.
- Аватары персонажа описываются в single-def `AvatarsDef` (`Free` / `Packs` по id `AncestryDef`); выбранный avatar id хранится в `CharacterStateData.Avatar` / `PregeneratedCharacterDef.Avatar`. Ownership паков в state — следующий этап.
- `AncestryDef.HitPoints` и `ClassDef.HitPointsPerLevel` заполнены в стартовом контенте; теги `hp-*` / `hp_per_level-*` пока оставлены как дубликаты для удобства чтения JSON.
- `BackgroundDef.Features` / `ItemDef.Features` — плоский список id feat; у `ClassDef` / `AncestryDef` поле `Features` — словарь уровень → список id.
- `AncestryDef.EquippedItems` — слоты происхождения (украшения `Finger` / `Neck` / `Tail`); объединяется с `ClassDef.EquippedItems` при создании персонажа.
- Слоты `Finger` / `Neck` / `Tail` в `Glossary.Items`; `Bag` не даёт `ItemDef.Features` (`GrantsItemFeatures`).
- Итоговые `MaxHitPoints` / навыки **не** пишутся в `CharacterStateData.Parameters`: в state — сырые ключи (`CON`, `Level`, `MaxHitPoints.PerLevel`, `MaxHitPoints.Bonus`, `*.ProfRank`, …), итог — `CharacterParametersProxy.GetTotalValue`.
- Механические эффекты дефов: Apply / Unapply через `CharacterParametersOperator` (`FeatDef.Apply`); экипировка — `CharacterItemFeaturesOperator` по `ItemDef.Features` в Equip/Unequip/Move/Remove (см. [State.md](State.md), [Feats.md](Feats.md)). UI создания/прокачки — следующий этап.
- `ItemDef.IsQuestItem` — признак квестового предмета; в стартовом контенте у всех предметов `false`.
- Оружейные `ItemDef` в `_ADVENTURES_/Items/Weapons` содержат `Type` / `Group` / формулы / `DamageDice`; модификаторы атаки и урона считает `WeaponProxy` (модуль `State`), кости урона — только метаданные для будущего броска.
- `Restrictions` на class/ancestry/background/feat может быть пустым или отсутствовать в JSON; при добавлении ограничений JSON-ключи совпадают с полями `Restriction` (см. [Restrictions.md](Restrictions.md)).
- В `Tools/Cheats` секция `Definitions` показывает загруженные коллекции adventures `DefinitionsManager`, включая `Backgrounds` и `PregeneratedCharacters` (между Ancestries и Feats), а также id single-def `Avatars`.
- Все id дефов фактически задаются именем JSON-файла, поэтому переименование файла меняет id.
- `LoadCollection()` загружает JSON из указанной папки и всех вложенных подпапок; `Id` — только имя файла, без пути.
- Для коллекций id должен быть уникален в рамках всего дерева папки; при совпадении имён побеждает первый загруженный деф, дубликат пишется в `LogWarning`.
- При ссылках между дефами (`RoundDef -> GameZone/Gems/Objectives`) валидность обеспечивается только на этапе использования (`TryGetValue`), поэтому полезно держать ручную/авто-проверку ссылок.
- JSON-ключи должны точно совпадать с именами публичных полей C#-классов (латиница; см. также [Restrictions.md](Restrictions.md#соглашения-по-именованию)).
- Отсутствующие в JSON опциональные поля получают значения по умолчанию CLR (`false`, `0`, `null`). Для `AdventureDef` в текущем контенте не используются `Disabled`, `IgnoredTags`, `AdventureLinks`.
- Enum-поля в JSON задаются строковыми именами членов enum, не числовыми кодами.
