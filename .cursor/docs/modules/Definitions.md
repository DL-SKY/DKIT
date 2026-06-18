# Модуль Definitions

**Последнее обновление:** 2026-06-18 18:00:00 (+03:00)

## Назначение

`Definitions` отвечает за загрузку и хранение конфигурационных данных проекта (JSON-дефов) из `Resources/Definitions` и предоставляет их в рантайме через единый менеджер.

## Краткая логика работы

1. На старте вызывается `DefinitionsManager.InitAsync()`, который запускает корутину `LoadAll()`.
2. `LoadAll()` последовательно вызывает набор методов загрузки (`LoadGlobalSettings`, `LoadGameZones`, `LoadRounds` и т.д.).
3. Каждый метод делегирует чтение в `Loader`:
   - `LoadSingle<T>()` для одиночного JSON;
   - `LoadCollection<T>()` для папки JSON-файлов.
4. `Loader` загружает `TextAsset` через `Resources.Load*`, десериализует JSON через `JsonConvert.DeserializeObject<T>()` и присваивает `definition.Id = asset.name`.
5. Загруженные данные кэшируются в полях `DefinitionsManager` (single-def поля и словари коллекций), после чего выставляется флаг завершения `SimpleAsyncOperation`.

## Основные классы

- `AbstractDefinition`  
  Базовый тип для всех дефов; содержит `Id` (не сериализуется в JSON).

- `Loader`  
  Универсальный загрузчик: чтение JSON из `Resources`, десериализация и возврат typed-объектов.

- `DefinitionsManager`  
  Фасад доступа к дефам проекта. Хранит:
  - single-def: `GlobalSettings`, `Match3GlobalSettings`, `CellsMap`, `PresetsMap`;
  - коллекции: `GameZones`, `Cells`, `Presets`, `Gems`, `GameZoneGems`, `Objectives`, `Rounds`.

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

## Добавление нового def (явный чек-лист)

1. Создать C#-класс def в `Assets/Modules/Definitions/Scripts/Implementation/Defs/...` и унаследовать его от `AbstractDefinition`.
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

- Все id дефов фактически задаются именем JSON-файла, поэтому переименование файла меняет id.
- Для коллекций id должен быть уникален в рамках папки.
- При ссылках между дефами (`RoundDef -> GameZone/Gems/Objectives`) валидность обеспечивается только на этапе использования (`TryGetValue`), поэтому полезно держать ручную/авто-проверку ссылок.
- JSON-ключи должны точно совпадать с именами публичных полей C#-классов (латиница; см. также [Restrictions.md](Restrictions.md#соглашения-по-именованию)).
