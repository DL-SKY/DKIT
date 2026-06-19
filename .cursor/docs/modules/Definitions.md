# Модуль Definitions

**Последнее обновление:** 2026-06-19 20:00:00 (+03:00)

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
  - single-def: `GlobalSettings`, `Match3GlobalSettings`, `CellsMap`, `PresetsMap`;
  - коллекции: `GameZones`, `Cells`, `Presets`, `Gems`, `GameZoneGems`, `Objectives`, `Rounds`.

- `DefinitionsManager` (Adventures)  
  Фасад доступа к дефам adventure-проекта (`Modules.Definitions.Scripts.Implementation.Adventures`). Хранит:
  - single-def: `GlobalSettings`, `RuleSettings`;
  - коллекции: `Adventures`, `Classes`, `Ancestries`, `Feats`, `Items`, `Spells`, `Rules`, `BattleRules`.

### Соглашение по наследованию adventure-дефов

- **По умолчанию** класс дефа наследуется напрямую от `AbstractDefinition` и содержит все сериализуемые поля (пример: `GemDef`, `GameZoneDef`, `ClassDef`, `ItemDef`).
- **Исключение — `AdventureDef`:** наследует `AdventureData` из модуля `RPG`, потому что большой контракт приключения (сцены, выборы, ограничения) живёт в `RPG` и будет использоваться `AdventuresManager` / `IAdventureFlowController`. Промежуточные `*Data`-прослойки для остальных типов не используются.

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

- `AdventureDef`  
  Деф приключения; наследует `AdventureData` из модуля `RPG` (сцены, выборы, ограничения, метаданные). Подробнее о полях — в [RPG.md](RPG.md#модель-данных-adventure).

- `ClassDef`  
  Класс персонажа. Поля: `Disabled`, `Tags`, `Title`, `Description`.

- `AncestryDef`  
  Происхождение персонажа. Поля: `Disabled`, `Size` (`AncestrySize`), `Speed`, `Tags`, `Title`, `Description`, `MaleNames`, `FemaleNames`.  
  Списки имён — пул для генерации/выбора имени при создании персонажа (связь с `CharacterStateData.Gender` в [State.md](State.md#adventure-charactersstatedata-и-characterstatedata)).

- `FeatDef`  
  Черта/способность. Поля: `Disabled`, `Type` (`FeatType`), `Level`, `Tags`, `Title`, `Description`, `Restrictions` (`List<Restriction>`).  
  `Level` — минимальный уровень персонажа для взятия черты (PF2e-style). `Restrictions` — структурированные требования/ограничения (prerequisites, class, ancestry, proficiency и т.д.); формат `Restriction` — см. [Restrictions.md](Restrictions.md). Проверка в runtime пока не реализована; часть условий временно дублируется в `Tags`.

- `ItemDef`  
  Предмет. Поля: `Disabled`, `IsQuestItem`, `Category` (`ItemCategory`), `Level`, `Tags`, `Title`, `Description`, `Price`.

- `SpellDef`  
  Заклинание. Поля: `Disabled`, `Type` (`SpellType`), `Level`, `Tags`, `Title`, `Description`.

- `RuleDef`  
  Общее правило приключения (вне боя). Поля: `Tags`. Стартовый контент: `GeneralRule` (`core`, `exploration`, `social`).

- `BattleRuleDef`  
  Боевое правило приключения. Поля: `Tags`. Стартовый контент: `GeneralBattleRule` (`core`, `combat`, `encounter`).

- `RuleSettingsDef`  
  Single-def настроек правил; связывает активные правила по id. Поля: `Tags`, `Rule` (`RuleDef`), `BattleRule` (`BattleRuleDef`). Стартовое значение: `Rule = "GeneralRule"`, `BattleRule = "GeneralBattleRule"`.

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
| `RuleDef` | Статические правила вне боя (исследование, социальное взаимодействие и т.п.) |
| `BattleRuleDef` | Статические боевые правила (столкновения, тактика и т.п.) |
| `RuleSettingsDef` | Single-def, указывающий, какие правила из коллекций считаются активными по умолчанию |

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

| Коллекция | Папка | Примеры подпапок | Ориентир по объёму |
|---|---|---|---|
| `Classes` | `_ADVENTURES_/Classes` | — | ~24 класса |
| `Ancestries` | `_ADVENTURES_/Ancestries` | — | 8 ancestries Player Core |
| `Feats` | `_ADVENTURES_/Feats` | `General`, `Ancestry`, `Class`, `ClassFeature`, `Skill` | ~15+ черт |
| `Spells` | `_ADVENTURES_/Spells` | `Cantrips`, `Arcane`, `Divine`, `Primal` | ~13+ заклинаний |
| `Items` | `_ADVENTURES_/Items` | `Weapons`, `Armor`, `Shields`, `Consumables`, `Equipment` | ~18+ предметов |
| `Rules` | `_ADVENTURES_/Rules` | — | 1+ правило (`GeneralRule`) |
| `BattleRules` | `_ADVENTURES_/BattleRules` | — | 1+ боевое правило (`GeneralBattleRule`) |

Single-def `RuleSettings` лежит в `_ADVENTURES_/RuleSettings/RuleSettings.json`.

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
- Adventure-контент лежит в `Definitions/_ADVENTURES_/...` (`GlobalSettings`, `RuleSettings`, `Adventures`, `Classes`, `Ancestries`, `Feats`, `Items`, `Spells`, `Rules`, `BattleRules`). Коллекции могут иметь вложенные подпапки — на загрузку это не влияет.
- `RuleSettingsDef` ссылается на `RuleDef` и `BattleRuleDef` по id; при добавлении новых правил обновляйте JSON настроек или потребляющий код, если нужно переключить активный набор.
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
