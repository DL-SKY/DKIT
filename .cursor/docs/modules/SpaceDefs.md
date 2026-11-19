# Дефы Space

**Последнее обновление:** 2026-11-19

Справочник каталога подпроекта Space. Это не каталог Приключений и не Match-3: `DefinitionsManager` из [Definitions.md](Definitions.md) эти записи не читает.

Связанные документы:

- [Space.md](Space.md) — куда класть код модуля и что уже собрано
- [план прототипа](../../plans/SPACE/SPACE_рабочий_прототип_план_2026-11-19.md) — очереди и формула времени

## Где лежат записи

Код: `Assets/Modules/Space/Scripts/Implementation/Definitions`.  
JSON: `Assets/Modules/Space/Resources/Definitions/Space`.

| Def | Базовый класс | JSON-папка | Словарь |
|---|---|---|---|
| `SpaceHullDef` | `AbstractDefinition` | `Definitions/Space/Hulls` | `SpaceDefinitionsManager.Hulls` |
| `SpaceModuleDef` | `AbstractDefinition` | `Definitions/Space/Modules` | `SpaceDefinitionsManager.Modules` |
| `SpaceCommandDef` | `AbstractDefinition` | `Definitions/Space/Commands` | `SpaceDefinitionsManager.Commands` |
| `SpaceTimedTargetDef` | `AbstractDefinition` | `Definitions/Space/TimedTargets` | `SpaceDefinitionsManager.TimedTargets` |

`Id` в JSON не пишется. `SpaceDefinitionLoader` ставит его из имени файла (`approach.json` → `approach`). Файл из одних пробелов пропускается, поэтому у команды без полей в файле лежит `{}`. Повторный id пропускается с предупреждением, в словарь остаётся первая запись. Битый JSON из загрузчика не глотается: исключение выходит из `Init` до `IsReady`.

Перечисления пишутся строковым именем (`"Skill"`, `"Production"`). Загрузчик использует `StringEnumConverter`.

## Как каталог попадает в игру

`ProjectInstaller` регистрирует `SpaceDefinitionsManager`. На старте сцены `SpaceDefinitionsInitTask` вызывает `Init()`. Каталог готов, только если сразу выполнены все условия:

- хотя бы один корпус, один модуль и одна команда;
- у каждой цели `BaseTimeMs` не меньше 1;
- среди целей есть все пять значений `SpaceQueueKind`;
- среди целей с `Kind = Skill` есть хотя бы три разных `BaseTimeMs`.

Иначе задача старта падает с кодом `SpaceInitErrorCodes.DEFINITIONS` (2). В Play окно `Tools/Space`, секция Definitions, показывает те же четыре словаря. Вне Play секция пишет, что менеджера нет.

Полёт эти записи пока не читает. Очередь умеет взять цель по id: `TryEnqueue` сверяет, что `SpaceTimedTargetDef.Kind` совпадает с очередью.

## Поля

`Id` у всех четырёх — `string`, `[JsonIgnore]`, в JSON нет.

| Def | Поле | Тип | В JSON | Смысл |
|---|---|---|---|---|
| `SpaceHullDef` | `ModuleSlotIds` | `List<string>` | `ModuleSlotIds` | id слотов, куда садится модуль. Это не id `SpaceModuleDef` |
| `SpaceHullDef` | `CommandIds` | `List<string>` | `CommandIds` | id `SpaceCommandDef`, которые корпус умеет отдать |
| `SpaceModuleDef` | `SlotId` | `string` | `SlotId` | слот корпуса, в который модуль ставится |
| `SpaceModuleDef` | `WeaponGroup` | `string` | `WeaponGroup` | группа оружия. Сейчас в контенте `"A"` |
| `SpaceCommandDef` | — | — | — | своих полей нет. Id файла и есть команда. Исполнения в дефе нет |
| `SpaceTimedTargetDef` | `Kind` | `SpaceQueueKind` | `Kind` | в какую из пяти очередей цель ставится |
| `SpaceTimedTargetDef` | `BaseTimeMs` | `long` | `BaseTimeMs` | базовая длительность в миллисекундах, без модификаторов |

Слоты модулей и команды на корпусе — разные списки. Модуль ссылается на слот через `SlotId`. Команда — это id `SpaceCommandDef`.

`BaseTimeMs` в деф не запекается с бонусами. Длительность строки очереди считает только `SpaceQueueTime`: база умножается на произведение факторов, результат не короче 1 мс. Процент `−25` даёт множитель `0.75`, явный множитель `2` удваивает срок.

`SpaceQueueKind`:

| Значение | Очередь |
|---|---|
| `Skill` | `skill` |
| `BlueprintImprovement` | `blueprint_improvement` |
| `Disassembly` | `disassembly` |
| `BlueprintAttempt` | `blueprint_attempt` |
| `Production` | `production` |

## Текущий контент

| Файл | Id | Поля |
|---|---|---|
| `Hulls/Fighter.json` | `Fighter` | слот `weapon_a`, команда `approach` |
| `Modules/laser_a.json` | `laser_a` | слот `weapon_a`, группа `A` |
| `Commands/approach.json` | `approach` | полей нет, `{}` |
| `TimedTargets/skill_survey.json` | `skill_survey` | `Skill`, 45000 мс |
| `TimedTargets/skill_mining.json` | `skill_mining` | `Skill`, 90000 мс |
| `TimedTargets/skill_repair.json` | `skill_repair` | `Skill`, 180000 мс |
| `TimedTargets/improve_laser.json` | `improve_laser` | `BlueprintImprovement`, 600000 мс |
| `TimedTargets/disassemble_salvage.json` | `disassemble_salvage` | `Disassembly`, 120000 мс |
| `TimedTargets/attempt_laser_blueprint.json` | `attempt_laser_blueprint` | `BlueprintAttempt`, 300000 мс |
| `TimedTargets/craft_laser.json` | `craft_laser` | `Production`, 240000 мс |

## Чего в каталоге ещё нет

Отдельных дефов сектора, контакта, чертежа, склада и сертификата корпуса нет. Цель очереди — общая запись `SpaceTimedTargetDef`: у навыка, улучшения чертежа, разбора, попытки создать чертёж и производства одни и те же поля `Kind` и `BaseTimeMs`.
