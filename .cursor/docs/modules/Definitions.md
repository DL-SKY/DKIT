# Модуль Definitions

**Последнее обновление:** 2026-06-30 10:12:00 (+03:00)

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
  - single-def: `GlobalSettings` (`ProjectGlobalSettingsDef`), `LocalizationSettings` (`LocalizationSettingsDef`), `RuleSettings` (`RuleSettingsDef`);
  - коллекции: `Adventures`, `Classes`, `Ancestries`, `Feats`, `Items`, `Spells`, `Rules`, `BattleRules`.

### Соглашение по наследованию adventure-дефов

- **По умолчанию** класс дефа наследуется напрямую от `AbstractDefinition` и содержит все сериализуемые поля (пример: `GemDef`, `GameZoneDef`, `ClassDef`, `ItemDef`).
- **Исключение — `AdventureDef`:** наследует `AdventureData` из модуля `RPG`, потому что большой контракт приключения (сцены, выборы, ограничения) живёт в `RPG` и используется в runtime-слое (`AdventuresManager`, `RuntimeSceneData`). `IAdventureFlowController` в текущем коде помечен как устаревший (`[Obsolete]`). Промежуточные `*Data`-прослойки для остальных типов не используются.

| Def | Базовый класс | JSON-папка |
|---|---|---|
| `AdventureDef` | `AdventureData` (модуль `RPG`) | `Definitions/_ADVENTURES_/Adventures` |
| `ClassDef` | `AbstractDefinition` | `Definitions/_ADVENTURES_/Classes` |
| `AncestryDef` | `AbstractDefinition` | `Definitions/_ADVENTURES_/Ancestries` |
| `FeatDef` | `AbstractDefinition` | `Definitions/_ADVENTURES_/Feats` |
| `ItemDef` | `AbstractDefinition` | `Definitions/_ADVENTURES_/Items` |
| `SpellDef` | `AbstractDefinition` | `Definitions/_ADVENTURES_/Spells` |
| `RuleDef` | `AbstractDefinition` | `Definitions/_ADVENTURES_/Rules` |
| `BattleRuleDef` | `AbstractDefinition` | `Definitions/_ADVENTURES_/BattleRules` |
| `RuleSettingsDef` | `AbstractDefinition` | `Definitions/_ADVENTURES_/RuleSettings/RuleSettings` (single) |
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
| `RuleDef` | `Tags` | `List<string>` | `Tags` | да |
| | `AbilityBoostPointCost` | `Dictionary<int, int>` | `AbilityBoostPointCost` | да; ключи в JSON — строки (`"4"`, `"5"`) |
| | `SkillDependencies` | `Dictionary<string, string>` | `SkillDependencies` | да, 18 навыков (включая `Perception`) |
| `BattleRuleDef` | `Tags` | `List<string>` | `Tags` | да |
| `ClassDef` | `Disabled` | `bool` | `Disabled` | да |
| | `Tags` | `List<string>` | `Tags` | да |
| | `Title` | `string` | `Title` | да |
| | `Description` | `string` | `Description` | да |
| `AncestryDef` | `Disabled` | `bool` | `Disabled` | да |
| | `Size` | `AncestrySize` | `Size` | да (`Small` / `Medium` / `Large`) |
| | `Speed` | `int` | `Speed` | да |
| | `Tags` | `List<string>` | `Tags` | да |
| | `Title` | `string` | `Title` | да |
| | `Description` | `string` | `Description` | да |
| | `MaleNames` | `List<string>` | `MaleNames` | да |
| | `FemaleNames` | `List<string>` | `FemaleNames` | да |
| `FeatDef` | `Disabled` | `bool` | `Disabled` | да |
| | `Type` | `FeatType` | `Type` | да |
| | `Level` | `int` | `Level` | да |
| | `Tags` | `List<string>` | `Tags` | да (часто пустой) |
| | `Title` | `string` | `Title` | да |
| | `Description` | `string` | `Description` | да |
| | `Restrictions` | `List<Restriction>` | `Restrictions` | да, но везде `[]` |
| `ItemDef` | `Disabled` | `bool` | `Disabled` | да |
| | `IsQuestItem` | `bool` | `IsQuestItem` | да, везде `false` |
| | `Category` | `ItemCategory` | `Category` | да |
| | `Level` | `int` | `Level` | да |
| | `Tags` | `List<string>` | `Tags` | да |
| | `Title` | `string` | `Title` | да |
| | `Description` | `string` | `Description` | да |
| | `Price` | `int` | `Price` | да |
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
| `FeatType` | `AncestryFeat`, `BackgroundSkillFeat`, `SkillFeat`, `GeneralFeat`, `ClassFeat`, `ClassFeature`, `Boost` |
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
  | `IsRepeatable` | `bool` | да |
  | `Type` | `AdventureType` | да |
  | `AdventureLinks` | `List<string>` | нет (опционально) |
  | `Title` | `string` | да |
  | `Description` | `string` | да |
  | `Restrictions` | `List<Restriction>` | да (часто `[]`) |
  | `StartScenes` | `List<string>` | да |
  | `Scenes` | `Dictionary<string, SceneData>` | да |

  Вложенные типы сцены (`SceneData`, `SceneContentData`, `ChoiceData`, `ChoiceActionData`) — в модуле `RPG`; подробнее в [RPG.md](RPG.md#модель-данных-adventure). В текущем контенте `ForestPath` содержит `Choices` с `Actions` типа `GoToScene`.

- `ClassDef`  
  Класс персонажа. Поля: `Disabled`, `Tags`, `Title`, `Description`.

- `AncestryDef`  
  Происхождение персонажа. Поля: `Disabled`, `Size` (`AncestrySize`), `Speed`, `Tags`, `Title`, `Description`, `MaleNames`, `FemaleNames`.  
  Списки имён — пул для генерации/выбора имени при создании персонажа (связь с `CharacterStateData.Gender` в [State.md](State.md#adventure-charactersstatedata-и-characterstatedata)).

- `FeatDef`  
  Черта/способность. Поля: `Disabled`, `Type` (`FeatType`), `Level`, `Tags`, `Title`, `Description`, `Restrictions` (`List<Restriction>`).  
  `Level` — минимальный уровень персонажа для взятия черты (PF2e-style). `Restrictions` — структурированные требования/ограничения (prerequisites, class, ancestry, proficiency и т.д.); формат `Restriction` — см. [Restrictions.md](Restrictions.md). Проверка в runtime пока не реализована; в стартовом контенте `Restrictions` везде пустые, часть условий временно дублируется в `Tags`.

- `ItemDef`  
  Предмет. Поля: `Disabled`, `IsQuestItem`, `Category` (`ItemCategory`), `Level`, `Tags`, `Title`, `Description`, `Price`.

- `SpellDef`  
  Заклинание. Поля: `Disabled`, `Type` (`SpellType`), `Level`, `Tags`, `Title`, `Description`.

- `RuleDef`  
  Общее правило приключения (вне боя): механика создания/прокачки персонажа и связанные настройки. Поля:
  - `Tags` — метки набора правил (`core`, `exploration`, `social` и т.п.);
  - `AbilityBoostPointCost` (`Dictionary<int, int>`) — пороговая таблица стоимости ability boost в очках: **ключ** — пороговое значение (номер буста / уровень шкалы), **значение** — цена в очках для этого порога. В рантайме для текущего номера буста выбирается **ближайший ключ** из словаря — по нему определяется стоимость (например, `4 → 1`, `5 → 2`: до порога 5 стоимость 1, начиная с порога 5 — 2);
  - `SkillDependencies` (`Dictionary<string, string>`) — привязка навыка к характеристике: **ключ** — id навыка (имя skill, см. `Glossary.Characters`), **значение** — ключ ability (`STR`, `DEX`, `CON`, `INT`, `WIS`, `CHA` из `Glossary.Characters`).  
  Стартовый контент: `GeneralRule` — `Tags`: `core`, `exploration`, `social`; `AbilityBoostPointCost`: пороги `4 → 1`, `5 → 2`; `SkillDependencies` — 18 навыков PF2e Remaster (включая `Perception`).

- `BattleRuleDef`  
  Боевое правило приключения. Поля: `Tags`. Стартовый контент: `GeneralBattleRule` (`core`, `combat`, `encounter`).

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

## Adventure-дефы правил: текущий контракт

На текущем этапе правила приключения вынесены в отдельные типы:

| Слой | Назначение |
|---|---|
| `RuleDef` | Статические правила вне боя: теги набора + механические таблицы (стоимость ability boost, skill → ability) |
| `BattleRuleDef` | Статические боевые правила (столкновения, тактика и т.п.) |
| `RuleSettingsDef` | Single-def, указывающий, какие правила из коллекций считаются активными по умолчанию |

Активный `RuleDef` выбирается через `RuleSettingsDef.Rule` (id JSON-файла, например `GeneralRule`). Потребитель читает поля правила как конфигурацию: для ability boost — ближайший порог в `AbilityBoostPointCost`, для модификатора навыка — `SkillDependencies[skillId]`. Ключи навыков и ability согласованы с `Glossary.Characters` в `Modules.Definitions.Scripts.Implementation.Adventures.Constants`.

Ссылки между дефами — строковые id (имя JSON-файла), по тому же принципу, что `RoundDef -> GameZone/Gems/Objectives`.

Порядок загрузки в adventures `DefinitionsManager`: `LoadRules` и `LoadBattleRules` выполняются **до** `LoadRuleSettings`, чтобы к моменту чтения настроек коллекции правил уже были в памяти (валидация ссылок по-прежнему на стороне потребителя).

## Adventure-дефы персонажа: текущий контракт и эволюция

### Минимальный контракт (сейчас)

На текущем этапе adventure-дефы персонажа содержат **необходимый минимум** для загрузки, отображения в UI и ссылок из `Modules.State`:

- идентификация через `Id` (имя JSON-файла);
- метаданные (`Title`, `Description`, `Disabled`);
- классификация (`Type`, `Level`, `Category`, `Size`, `Speed`, `IsQuestItem` — где применимо);
- `Tags` — произвольные строковые метки (источник книги, роль, черновые механические подсказки);
- `Restrictions` на `FeatDef` — структурированные требования к черте (см. [Restrictions.md](Restrictions.md));
- `MaleNames` / `FemaleNames` на `AncestryDef` — пулы имён для создания персонажа.

`Tags` **не являются** финальным механическим слоем: это временный способ группировки и заметок до появления структурированных полей (для черт — миграция в `FeatDef.Restrictions`).

### Разделение Defs и State

| Слой | Что хранит |
|---|---|
| **Defs** | Статический контент: что даёт класс, раса, черта, предмет или заклинание |
| **State** | Персистентный прогресс персонажа: `CharacterStateData.Parameters`, `SavingThrows`, `Spells`, `StatusEffects`, экипировка и т.д. |

Runtime в будущем читает деф → вычисляет или применяет эффекты → записывает результат в state (через state-actions / сервисы персонажа).

### Планируемое расширение (механики)

По мере разработки в дефы будут добавляться **структурированные данные** с конкретными величинами:

- бонусы и штрафы к характеристикам, навыкам, спасброскам;
- особенности (сопротивления, чувства, ограничения);
- эффекты черт, предметов и заклинаний с явными значениями для записи в `CharacterStateData` и связанные секции.

Формат этих блоков будет согласован с уже существующими контрактами (`ChoiceActionData.Params`, state-actions), чтобы не дублировать способы описания модификаторов.

### Порядок полей в JSON

Порядок ключей в JSON следует порядку полей в C#-классе дефа (см. таблицу выше). `Id` в JSON не задаётся — присваивается загрузчиком из имени файла.

### Контент `_ADVENTURES_` (стартовый набор)

Стартовый контент ориентирован на PF2e Player Core (и смежные книги — в `Tags`). Подпапки используются для удобства редактирования; `LoadCollection()` загружает их рекурсивно, `Id` — только имя файла.

| Коллекция | Папка | Примеры подпапок | Фактический объём (2026-06-22) |
|---|---|---|---|
| `Adventures` | `_ADVENTURES_/Adventures` | `Locations`, `Tutorials`, `Debug` | 4 приключения |
| `Classes` | `_ADVENTURES_/Classes` | — | 24 класса |
| `Ancestries` | `_ADVENTURES_/Ancestries` | — | 8 ancestries |
| `Feats` | `_ADVENTURES_/Feats` | `General`, `Ancestry`, `Class`, `ClassFeature`, `Skill` | 15 черт |
| `Spells` | `_ADVENTURES_/Spells` | `Cantrips`, `Arcane`, `Divine` | 13 заклинаний |
| `Items` | `_ADVENTURES_/Items` | `Weapons`, `Armor`, `Shields`, `Consumables`, `Equipment` | 18 предметов |
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
6. Проверить в рантайме:
   - JSON корректно десериализуется;
   - `Id` соответствует имени файла;
   - доступ к дефу из потребляющего модуля не возвращает `null` и не падает на `TryGetValue`.

## Практические заметки

- В проекте два менеджера дефов: Match3 (`Implementation.Defs`) и Adventures (`Implementation.Adventures`). Каждый загружает свой набор JSON из `Resources/Definitions`.
- Оба менеджера сейчас загружают `LocalizationSettingsDef` (пути: `Definitions/LocalizationSettings/LocalizationSettings` и `Definitions/_ADVENTURES_/LocalizationSettings/LocalizationSettings`).
- Adventure-контент лежит в `Definitions/_ADVENTURES_/...` (`GlobalSettings`, `RuleSettings`, `Adventures`, `Classes`, `Ancestries`, `Feats`, `Items`, `Spells`, `Rules`, `BattleRules`). Коллекции могут иметь вложенные подпапки — на загрузку это не влияет.
- `RuleSettingsDef` ссылается на `RuleDef`, `BattleRuleDef` и стартовое приключение по id (`Rule`, `BattleRule`, `StartAdventure`); при добавлении новых правил или смене стартовой точки обновляйте `RuleSettings.json` или потребляющий код. Поля `RuleDef.AbilityBoostPointCost` и `RuleDef.SkillDependencies` — опциональны в JSON; отсутствующий ключ словаря трактуется потребителем (дефолт / запрет) на стороне runtime.
- Ссылки из `Modules.State` на контент персонажа (`CharacterStateData.Ancestry`, `Class`, `Gender`, `EquippedItems.ItemId`, стаки в `InventoryStateData`) — это `Id` соответствующих adventure-дефов (имя JSON-файла) или enum/state-поля (`Gender` — см. [State.md](State.md)).
- `AncestryDef.MaleNames` / `FemaleNames` используются при создании персонажа вместе с `CharacterStateData.Gender`; id ancestry — в `CharacterStateData.Ancestry`.
- `ItemDef.IsQuestItem` — признак квестового предмета; в стартовом контенте у всех предметов `false`.
- `FeatDef.Restrictions` может быть пустым или отсутствовать в JSON; при добавлении ограничений JSON-ключи совпадают с полями `Restriction` (см. [Restrictions.md](Restrictions.md)).
- Механические эффекты дефов пока не применяются автоматически; `Tags` — вспомогательные метки до появления структурированных модификаторов (см. [Adventure-дефы персонажа](#adventure-дефы-персонажа-текущий-контракт-и-эволюция)).
- Все id дефов фактически задаются именем JSON-файла, поэтому переименование файла меняет id.
- `LoadCollection()` загружает JSON из указанной папки и всех вложенных подпапок; `Id` — только имя файла, без пути.
- Для коллекций id должен быть уникален в рамках всего дерева папки; при совпадении имён побеждает первый загруженный деф, дубликат пишется в `LogWarning`.
- При ссылках между дефами (`RoundDef -> GameZone/Gems/Objectives`) валидность обеспечивается только на этапе использования (`TryGetValue`), поэтому полезно держать ручную/авто-проверку ссылок.
- JSON-ключи должны точно совпадать с именами публичных полей C#-классов (латиница; см. также [Restrictions.md](Restrictions.md#соглашения-по-именованию)).
- Отсутствующие в JSON опциональные поля получают значения по умолчанию CLR (`false`, `0`, `null`). Для `AdventureDef` в текущем контенте не используются `Disabled`, `IgnoredTags`, `AdventureLinks`.
- Enum-поля в JSON задаются строковыми именами членов enum, не числовыми кодами.
