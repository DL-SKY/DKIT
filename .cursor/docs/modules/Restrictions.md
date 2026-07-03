# Модуль Restrictions

**Последнее обновление:** 2026-07-04 00:45:00 (+03:00)

## Назначение

`Restrictions` выполняет проверку условий доступа/доступности через список ограничений (`Restriction`) и набор специализированных checker-классов.

## Краткая логика работы

1. Входной код передает в `RestrictionsChecker.Check(...)` список `Restriction`.
2. Для каждого `Restriction.Type` выбирается соответствующий `IChecker`.
3. Чекер создается через `RestrictionFactory` и кэшируется по `RestrictionType`.
4. Каждый чекер сравнивает входные данные с требуемыми (через `CompareRestrictionStaticChecker` или собственную логику).
5. Если хотя бы один restriction не проходит — общий результат `false`; если все прошли — `true`.

## Основные классы

- `RestrictionsChecker`  
  Главный фасад проверки списка ограничений и маршрутизации к нужным checker-ам.

- `Restriction`  
  Модель ограничения: `Type`, `StringValues`, `IntValues`, `LongValues`, `BoolValues`, `CompareOptions`.

- `RestrictionType`  
  Перечень типов ограничений: `TimeNow`, `WorldParams`, `AdventureParams`.

- `IChecker`  
  Контракт конкретной проверки: `bool Check(Restriction restriction)`.

- `TimeNowRestrictionChecker`  
  Проверка текущего времени (`UTC ms`) относительно порога из `LongValues[0]`.

- `WorldParamsRestrictionChecker`  
  Проверка параметров `AdventuresStateData.World.Parameters`.

- `AdventureParamsRestrictionChecker`  
  Проверка параметров `AdventuresStateData.Adventures[currentAdventureId].Parameters`.

- `CompareRestrictionStaticChecker` + `CompareType`  
  Универсальный слой сравнения типов `string/int/long` с операциями `Equal`, `More`, `Less` и т.д.

## Как добавить новый тип ограничения

1. Добавить новый элемент в `RestrictionType`.
2. Создать новый checker-класс, реализующий `IChecker` (например, `PlayerLevelRestrictionChecker`).
3. Реализовать извлечение нужных значений из `Restriction` и бизнес-правило проверки.
4. Зарегистрировать checker в `RestrictionsChecker.CreateChecker(...)`:
   - добавить `case` в `switch` для нового `RestrictionType`;
   - создавать экземпляр через `_restrictionFactory.Create<NewChecker>()`.
5. Если нужен новый тип сравнения, расширить `CompareType` и соответствующий comparer в `CompareRestrictionStaticChecker`.
6. Проверить:
   - положительный и отрицательный кейс;
   - отсутствие падений при пустых/некорректных данных в `Restriction`.

### Формат `Restriction` для `WorldParams` / `AdventureParams`

Общая логика в `AdventureStateParamsRestrictionCheckHelper`; checker-ы отличаются только источником данных:

| `RestrictionType` | Источник state |
|---|---|
| `WorldParams` | `AdventuresStateData.World.Parameters` |
| `AdventureParams` | `AdventuresStateData.Adventures[CurrentAdventureId].Parameters` |

Поля `Restriction`:

- `StringValues[0]` — ключ параметра в `AdventureStateParamsData`.
- Если задан `IntValues[0]` — сравнивается `Parameters.Ints[key]` (поддерживаются все `CompareType` для `int`).
- Иначе если задан `BoolValues[0]` — сравнивается `Parameters.Bools[key]` (поддерживаются `Equal` / `NotEqual`).
- Иначе строковый режим: `StringValues[1]` сравнивается с `Parameters.Strings[key]` (поддерживаются `Equal` / `NotEqual`).

Примеры JSON:

```json
{
  "Type": "WorldParams",
  "CompareOptions": "Equal",
  "StringValues": ["world.tavern_unlocked"],
  "IntValues": [],
  "LongValues": [],
  "BoolValues": [true]
}
```

```json
{
  "Type": "AdventureParams",
  "CompareOptions": "MoreEqual",
  "StringValues": ["adventure.quest_stage"],
  "IntValues": [2],
  "LongValues": [],
  "BoolValues": []
}
```

```json
{
  "Type": "WorldParams",
  "CompareOptions": "Equal",
  "StringValues": ["world.last_location", "tavern"],
  "IntValues": [],
  "LongValues": [],
  "BoolValues": []
}
```

## Где используется `Restriction` в проекте

| Модуль | Класс / поле | Назначение |
|---|---|---|
| `RPG` | `AdventureData.Restrictions` | Ограничения доступа к приключению |
| `RPG` | `ChoiceData.Restrictions` | Ограничения доступности выбора |
| `RPG` | `SceneContentData.Restrictions` | Ограничения видимости элемента контента сцены |
| `Definitions` | `ObjectivesDef.VictoryConditions` | Условия победы раунда Match3 |
| `Definitions` | `ObjectivesDef.DefeatConditions` | Условия поражения раунда Match3 |
| `Definitions` | `FeatDef.Restrictions` | Требования/ограничения для взятия или использования черты (prerequisites; checker’ы для персонажа — в планах) |
| `Match3` | `IObjectivesData.GetVictoryConditions()` / `GetDefeatConditions()` | Доступ к условиям из data-слоя |
| `ECS` | `RoundEndConditionsData.Victory` / `.Defeat` | Копии условий в ECS для проверки в рантайме |

JSON-ключи в дефах и adventure-контенте должны совпадать с именами полей C#-классов (`Restrictions`, `VictoryConditions`, `DefeatConditions`).

## Соглашения по именованию

- Имена полей, методов и JSON-ключей пишутся **латиницей**; кириллические homoglyph-символы (например, `С` вместо `C`) недопустимы в идентификаторах.
- Исторические опечатки исправлены: `Restictions` → `Restrictions`, `VictoryСonditions` / `DefeatСonditions` → `VictoryConditions` / `DefeatConditions` (вторая пара содержала кириллическую `С`).
