# Дефы Space

**Последнее обновление:** 2026-11-20

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
| `SpaceSectorDef` | `AbstractDefinition` | `Definitions/Space/Sectors` | `SpaceDefinitionsManager.Sectors` |
| `SpaceObjectDef` | `AbstractDefinition` | `Definitions/Space/Objects` | `SpaceDefinitionsManager.Objects` |
| `SpaceCameraSettingsDef` | `AbstractDefinition` | `Definitions/Space/CameraSettings/CameraSettings` | `SpaceDefinitionsManager.CameraSettings`. Одна запись, не словарь |
| `SpaceMarkerIconsDef` | `AbstractDefinition` | `Definitions/Space/MarkerIcons/MarkerIcons` | `SpaceDefinitionsManager.MarkerIcons`. Одна запись, не словарь. Картинки типов `SpaceObjectKind`. Корабль её не читает |

`Id` в JSON не пишется. `SpaceDefinitionLoader` ставит его из имени файла (`approach.json` → `approach`). Файл из одних пробелов пропускается, поэтому у команды без полей в файле лежит `{}`. Повторный id пропускается с предупреждением, в словарь остаётся первая запись. Битый JSON из загрузчика не глотается: исключение выходит из `Init` до `IsReady`.

Перечисления пишутся строковым именем (`"Skill"`, `"Production"`). Загрузчик использует `StringEnumConverter`.

## Как каталог попадает в игру

`ProjectInstaller` регистрирует `SpaceDefinitionsManager`. На старте сцены `SpaceDefinitionsInitTask` вызывает `Init()`. Каталог готов, только если сразу выполнены все условия:

- хотя бы один корпус, один модуль и одна команда;
- у каждой цели `BaseTimeMs` не меньше 1;
- среди целей есть все пять значений `SpaceQueueKind`;
- среди целей с `Kind = Skill` есть хотя бы три разных `BaseTimeMs`;
- `CameraSettings` загружен и пригоден: минимум дистанции больше нуля и меньше максимума, старт наклона внутри диапазона, чувствительность, шаг зума и порог перетаскивания больше нуля;
- у каждого корпуса `CameraDistance` лежит внутри минимума и максимума камеры, `Radius` не меньше нуля, манёвренность больше нуля, `IconResourcesPath` не пустой, `SizeType` задан;
- `MarkerIcons` загружен и покрывает каждое значение `SpaceObjectKind` ровно одной непустой картинкой;
- среди модулей есть пригодный двигатель: в `SizeParameters` хотя бы одна строка, размеры не повторяются, в каждой скорость и инерция больше нуля. Строка на каждый `SpaceSizeType` не обязательна;
- хотя бы один объект, и у каждого масштаб больше нуля, а радиус не меньше нуля;
- хотя бы один сектор, и у каждого есть имя сцены, расстановка из известных объектов и хотя бы одна точка спавна.

Иначе задача старта падает с кодом `SpaceInitErrorCodes.DEFINITIONS` (2). В Play окно `Tools/Space`, секция Definitions, показывает словари каталога и одиночную запись камеры. Вне Play секция пишет, что менеджера нет.

Полёт по приказу эти записи пока не читает: нет компонента намерения, и корпус стоит. Загрузка сектора и точка корабля читают `SpaceSectorDef`: старт передаёт id сектора в `SpaceSectorLoad`, сцена берётся из `SceneName`. Одна точка спавна используется как есть, несколько выбираются случайно. `SpaceSectorBodies` создаёт сущности расстановки в уже поднятом мире. Камера читает `CameraSettings` и `CameraDistance` корпуса. В секторе стартовая дистанция берётся с корпуса, поворот и наклон — с камеры. На станции дистанция корпуса не применяется, остаются поворот и наклон камеры. Очередь умеет взять цель по id: `TryEnqueue` сверяет, что `SpaceTimedTargetDef.Kind` совпадает с очередью. Эта сверка временная: см. раздел ниже.

## Поля

`Id` у коллекций и у одиночной записи камеры — `string`, `[JsonIgnore]`, в JSON нет.

| Def | Поле | Тип | В JSON | Смысл |
|---|---|---|---|---|
| `SpaceSectorDef` | `SceneName` | `string` | `SceneName` | имя сцены в Build Settings, которую грузить для этого сектора |
| `SpaceSectorDef` | `Placements` | `List<SpaceSectorPlacement>` | `Placements` | расстановка тел. Корабль игрока в список не входит |
| `SpaceSectorPlacement` | `ObjectId` | `string` | `ObjectId` | id `SpaceObjectDef` |
| `SpaceSectorPlacement` | `X` | `float` | `X` | метры в сцене сектора, ось вправо |
| `SpaceSectorPlacement` | `Y` | `float` | `Y` | метры в сцене сектора, ось вверх |
| `SpaceSectorPlacement` | `Z` | `float` | `Z` | метры в сцене сектора, ось вперёд |
| `SpaceSectorPlacement` | `Yaw` | `float` | `Yaw` | градусы вокруг вертикали. `0` — нос по +Z. Положительный угол поворачивает нос к +X |
| `SpaceObjectDef` | `Kind` | `SpaceObjectKind` | `Kind` | `PointOfInterest`, `Station`, `Asteroid`, `Spawn`. Дальнейшие типы добавляются вместе с объектами |
| `SpaceObjectDef` | `VisualResourcesPath` | `string` | `VisualResourcesPath` | путь префаба в `Resources`. Пустая строка — сущность без объекта |
| `SpaceObjectDef` | `UniformScale` | `float` | `UniformScale` | единый масштаб префаба. На расстановке не переопределяется |
| `SpaceObjectDef` | `Radius` | `float` | `Radius` | радиус сферы в метрах. Ноль не отталкивает другие тела |
| `SpaceObjectDef` | `CommandIds` | `List<string>` | `CommandIds` | id команд, которые касание метки раскладывает по кругу. Пустой список круг не строит |
| `SpaceHullDef` | `Slots` | `List<SpaceHullSlotDef>` | `Slots` | гнёзда корпуса. У гнезда свой тип и свой типоразмер, не id модуля |
| `SpaceHullSlotDef` | `SlotType` | `SpaceSlotType` | `SlotType` | что в гнездо садится: `Weapon` или `Engine` |
| `SpaceHullSlotDef` | `SizeType` | `SpaceSizeType` | `SizeType` | типоразмер гнезда. С ним сравниваются `SizeParameters` модуля |
| `SpaceHullDef` | `CommandIds` | `List<string>` | `CommandIds` | id `SpaceCommandDef`, которые корпус умеет отдать |
| `SpaceHullDef` | `VisualResourcesPath` | `string` | `VisualResourcesPath` | путь в `Resources` на префаб корпуса. Пустой путь корпус в сцену не ставит |
| `SpaceHullDef` | `IconResourcesPath` | `string` | `IconResourcesPath` | картинка метки этого корпуса. Путь в `Resources` на спрайт |
| `SpaceHullDef` | `CameraDistance` | `float` | `CameraDistance` | стартовая дистанция камеры в секторе. На станции не применяется |
| `SpaceHullDef` | `Radius` | `float` | `Radius` | радиус сферы корпуса в метрах |
| `SpaceHullDef` | `ManeuverabilityDegreesPerSecond` | `float` | `ManeuverabilityDegreesPerSecond` | насколько быстро нос поворачивается к желаемому рысканию. Форма корпуса, не двигатель |
| `SpaceHullDef` | `SizeType` | `SpaceSizeType` | `SizeType` | типоразмер корпуса: `XXS` люди и боеприпасы, `XS` дроны, `S` лёгкий истребитель, затем `M`, `L`, `XL`, `XXL` |
| `SpaceModuleDef` | `SlotType` | `SpaceSlotType` | `SlotType` | в гнездо какого типа модуль садится: `Weapon` или `Engine` |
| `SpaceModuleDef` | `WeaponGroup` | `string` | `WeaponGroup` | группа оружия. У двигателя пусто |
| `SpaceModuleDef` | `SizeType` | `SpaceSizeType` | `SizeType` | типоразмер самой единицы оборудования |
| `SpaceModuleDef` | `SizeParameters` | `List<SpaceModuleSizeParameters>` | `SizeParameters` | параметры по типоразмерам гнезда. Нет строки типоразмера гнезда — модуль в это гнездо не ставится. Пустой список значит, что допустим только гнездо собственного `SizeType` модуля |
| `SpaceModuleSizeParameters` | `SizeType` | `SpaceSizeType` | `SizeType` | типоразмер гнезда, к которому относится строка. При спавне выбирается строка типоразмера гнезда двигателя |
| `SpaceModuleSizeParameters` | `MaxSpeedMetersPerSecond` | `float` | `MaxSpeedMetersPerSecond` | потолок скорости этой строки |
| `SpaceModuleSizeParameters` | `InertiaSeconds` | `float` | `InertiaSeconds` | постоянная времени разгона и торможения этой строки, в секундах. Больше — медленнее набор и сброс |
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
| `SpaceMarkerIconsDef` | `Icons` | `List<SpaceMarkerKindIcon>` | `Icons` | по одной записи на каждое значение `SpaceObjectKind` |
| `SpaceMarkerKindIcon` | `Kind` | `SpaceObjectKind` | `Kind` | тип тела, которому принадлежит картинка |
| `SpaceMarkerKindIcon` | `IconResourcesPath` | `string` | `IconResourcesPath` | путь спрайта в `Resources` |

Точка появления — место корпуса в уже загруженной сцене сектора. `X`, `Y`, `Z` задают это место в метрах: вправо, вверх и вперёд. `Yaw` только крутит корпус вокруг вертикали, крен и наклон точки не задают. Ноль градусов смотрит носом по +Z. Код ставит поворот как `Quaternion.Euler(0, Yaw, 0)`. Нуля в коде нет: числа берутся из выбранной точки.

Гнёзда и команды на корпусе — разные списки. Гнездо задаёт `SlotType` и `SizeType`. Модуль называет свой `SlotType` и не хранит id гнезда. Команда — это id `SpaceCommandDef`.

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
| `Sectors/SpaceDebug.json` | `SpaceDebug` | сцена `SpaceDebug`. Расстановка: `player_spawn` в `(0, 0, 0)`, `asteroid_near` в `(120, 0, 70)`, `asteroid_far` в `(−140, 10, 160)` с рысканием `45` |
| `Objects/player_spawn.json` | `player_spawn` | `Spawn`, без префаба, масштаб `1`, радиус `0` |
| `Objects/asteroid_near.json` | `asteroid_near` | `Asteroid`, `Prefabs/Asteroids/AsteroidNear`, масштаб `0.08`, радиус `9.967` |
| `Objects/asteroid_far.json` | `asteroid_far` | `Asteroid`, `Prefabs/Asteroids/AsteroidFar`, масштаб `0.12`, радиус `15.812` |
| `Hulls/Fighter.json` | `Fighter` | гнёзда `Weapon S` и `Engine S`, команда `approach`, вид `Prefabs/Spaceships/Fighter`, иконка метки `Sprites/Space/Markers/Fighter`, дистанция камеры `32.5`, радиус `7.249`, манёвренность `90` °/с, типоразмер корпуса `S` |
| `MarkerIcons/MarkerIcons.json` | `MarkerIcons` | `PointOfInterest` → `Sprites/Space/Markers/Point`, `Station` → `Sprites/Space/Markers/Station`, `Asteroid` → `Sprites/Space/Markers/Asteroid`, `Spawn` → `Sprites/Space/Markers/Spawn` |
| `Modules/engine_default.json` | `engine_default` | `Engine`, типоразмер модуля `S`. Строки гнёзд: `XXS` 70 м/с и 0.6 с, `XS` 55 м/с и 1 с, `S` 40 м/с и 1.5 с, `M` 32 м/с и 2.2 с, `L` 24 м/с и 3.2 с, `XL` 18 м/с и 4.5 с, `XXL` 12 м/с и 6.5 с |
| `Modules/laser_a.json` | `laser_a` | `Weapon`, группа `A`, типоразмер `S`, списка `SizeParameters` нет |
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

Отдельных дефов навыка, чертежа, разбора, склада и сертификата корпуса нет. Контакт сектора — это `SpaceObjectDef`, не отдельный класс.

`SpaceTimedTargetDef` и папка `TimedTargets` — временная заглушка. Запись будет удалена. Строка очереди станет хранить id настоящего дефа цели: навыка, чертежа и остальных работ. Длительность в миллисекундах будет полем этого дефа, а не отдельной общей записи. `Kind` на заглушке нужен только пока один каталог обслуживает все пять очередей.
