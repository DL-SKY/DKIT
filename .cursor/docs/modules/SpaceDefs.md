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
| `SpaceTimedTargetDef` | `AbstractDefinition` | `Definitions/Space/TimedTargets` | `SpaceDefinitionsManager.TimedTargets`. Временная запись, будет удалена |
| `SpaceCameraSettingsDef` | `AbstractDefinition` | `Definitions/Space/CameraSettings/CameraSettings` | `SpaceDefinitionsManager.CameraSettings`. Одна запись, не словарь |

`Id` в JSON не пишется. `SpaceDefinitionLoader` ставит его из имени файла (`approach.json` → `approach`). Файл из одних пробелов пропускается, поэтому у команды без полей в файле лежит `{}`. Повторный id пропускается с предупреждением, в словарь остаётся первая запись. Битый JSON из загрузчика не глотается: исключение выходит из `Init` до `IsReady`.

Перечисления пишутся строковым именем (`"Skill"`, `"Production"`). Загрузчик использует `StringEnumConverter`.

## Как каталог попадает в игру

`ProjectInstaller` регистрирует `SpaceDefinitionsManager`. На старте сцены `SpaceDefinitionsInitTask` вызывает `Init()`. Каталог готов, только если сразу выполнены все условия:

- хотя бы один корпус, один модуль и одна команда;
- у каждой цели `BaseTimeMs` не меньше 1;
- среди целей есть все пять значений `SpaceQueueKind`;
- среди целей с `Kind = Skill` есть хотя бы три разных `BaseTimeMs`;
- `CameraSettings` загружен и пригоден: минимум дистанции больше нуля и меньше максимума, старт наклона внутри диапазона, чувствительность, шаг зума и порог перетаскивания больше нуля;
- у каждого корпуса `CameraDistance` лежит внутри минимума и максимума камеры.

Иначе задача старта падает с кодом `SpaceInitErrorCodes.DEFINITIONS` (2). В Play окно `Tools/Space`, секция Definitions, показывает четыре словаря и одиночную запись камеры. Вне Play секция пишет, что менеджера нет.

Полёт корпуса эти записи пока не читает. Камера читает `CameraSettings` и `CameraDistance` корпуса. В секторе стартовая дистанция берётся с корпуса, поворот и наклон — с камеры. На станции дистанция корпуса не применяется, остаются поворот и наклон камеры. Очередь умеет взять цель по id: `TryEnqueue` сверяет, что `SpaceTimedTargetDef.Kind` совпадает с очередью. Эта сверка временная: см. раздел ниже.

## Поля

`Id` у коллекций и у одиночной записи камеры — `string`, `[JsonIgnore]`, в JSON нет.

| Def | Поле | Тип | В JSON | Смысл |
|---|---|---|---|---|
| `SpaceHullDef` | `ModuleSlotIds` | `List<string>` | `ModuleSlotIds` | id слотов, куда садится модуль. Это не id `SpaceModuleDef` |
| `SpaceHullDef` | `CommandIds` | `List<string>` | `CommandIds` | id `SpaceCommandDef`, которые корпус умеет отдать |
| `SpaceHullDef` | `VisualResourcesPath` | `string` | `VisualResourcesPath` | путь в `Resources` на префаб корпуса. Пустой путь корпус в сцену не ставит |
| `SpaceHullDef` | `CameraDistance` | `float` | `CameraDistance` | стартовая дистанция камеры в секторе. На станции не применяется |
| `SpaceModuleDef` | `SlotId` | `string` | `SlotId` | слот корпуса, в который модуль ставится |
| `SpaceModuleDef` | `WeaponGroup` | `string` | `WeaponGroup` | группа оружия. Сейчас в контенте `"A"` |
| `SpaceCommandDef` | — | — | — | своих полей нет. Id файла и есть команда. Исполнения в дефе нет |
| `SpaceTimedTargetDef` | `Kind` | `SpaceQueueKind` | `Kind` | в какую из пяти очередей цель ставится. Поле уйдёт вместе с этой записью |
| `SpaceTimedTargetDef` | `BaseTimeMs` | `long` | `BaseTimeMs` | базовая длительность в миллисекундах, без модификаторов. Потом это поле будет у дефа самой цели |
| `SpaceCameraSettingsDef` | `MinDistance` | `float` | `MinDistance` | ближайший зум |
| `SpaceCameraSettingsDef` | `MaxDistance` | `float` | `MaxDistance` | дальний зум |
| `SpaceCameraSettingsDef` | `DefaultYaw` | `float` | `DefaultYaw` | стартовый поворот в градусах. Ноль — камера сзади корпуса |
| `SpaceCameraSettingsDef` | `DefaultPitch` | `float` | `DefaultPitch` | стартовый наклон в градусах. Положительный — сверху |
| `SpaceCameraSettingsDef` | `MinPitch` | `float` | `MinPitch` | нижний предел наклона. Отрицательный заглядывает под корпус |
| `SpaceCameraSettingsDef` | `MaxPitch` | `float` | `MaxPitch` | верхний предел наклона |
| `SpaceCameraSettingsDef` | `DegreesPerPixel` | `float` | `DegreesPerPixel` | сколько градусов даёт один пиксель перетаскивания |
| `SpaceCameraSettingsDef` | `ScrollStep` | `float` | `ScrollStep` | шаг дистанции на один щелчок колеса |
| `SpaceCameraSettingsDef` | `DragThresholdPixels` | `float` | `DragThresholdPixels` | сдвиг, после которого касание считается поворотом, а не кликом |

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
| `Hulls/Fighter.json` | `Fighter` | слот `weapon_a`, команда `approach`, вид `Prefabs/Spaceships/Fighter`, дистанция камеры 32.5 |
| `Modules/laser_a.json` | `laser_a` | слот `weapon_a`, группа `A` |
| `Commands/approach.json` | `approach` | полей нет, `{}` |
| `CameraSettings/CameraSettings.json` | `CameraSettings` | дистанция 4..160, поворот 141.474, наклон 28.779 (−75..75), 0.2°/пиксель, шаг зума 2, порог 6 |
| `TimedTargets/skill_survey.json` | `skill_survey` | `Skill`, 45000 мс |
| `TimedTargets/skill_mining.json` | `skill_mining` | `Skill`, 90000 мс |
| `TimedTargets/skill_repair.json` | `skill_repair` | `Skill`, 180000 мс |
| `TimedTargets/improve_laser.json` | `improve_laser` | `BlueprintImprovement`, 600000 мс |
| `TimedTargets/disassemble_salvage.json` | `disassemble_salvage` | `Disassembly`, 120000 мс |
| `TimedTargets/attempt_laser_blueprint.json` | `attempt_laser_blueprint` | `BlueprintAttempt`, 300000 мс |
| `TimedTargets/craft_laser.json` | `craft_laser` | `Production`, 240000 мс |

## Чего в каталоге ещё нет

Отдельных дефов сектора, контакта, навыка, чертежа, разбора, склада и сертификата корпуса нет.

`SpaceTimedTargetDef` и папка `TimedTargets` — временная заглушка. Запись будет удалена. Строка очереди станет хранить id настоящего дефа цели: навыка, чертежа и остальных работ. Длительность в миллисекундах будет полем этого дефа, а не отдельной общей записи. `Kind` на заглушке нужен только пока один каталог обслуживает все пять очередей.
