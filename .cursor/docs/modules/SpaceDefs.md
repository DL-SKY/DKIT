# Дефы Space

**Последнее обновление:** 2026-11-25

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
| `SpaceMarkerSettingsDef` | `AbstractDefinition` | `Definitions/Space/MarkerSettings/MarkerSettings` | `SpaceDefinitionsManager.MarkerSettings`. Одна запись, не словарь. Картинки типов тел, кроме `Ship`, и радиус круга. Корабль картинку отсюда не читает |

`Id` в JSON не пишется. `SpaceDefinitionLoader` ставит его из имени файла (`approach.json` → `approach`). Файл из одних пробелов пропускается. Повторный id пропускается с предупреждением, в словарь остаётся первая запись. Битый JSON из загрузчика не глотается: исключение выходит из `Init` до `IsReady`.

Перечисления пишутся строковым именем (`"Skill"`, `"Production"`). Загрузчик использует `StringEnumConverter`.

## Как каталог попадает в игру

`ProjectInstaller` регистрирует `SpaceDefinitionsManager`. На старте сцены `SpaceDefinitionsInitTask` вызывает `Init()`. Каталог готов, только если сразу выполнены все условия:

- хотя бы один корпус, один модуль и одна команда;
- у каждой цели `BaseTimeMs` не меньше 1;
- среди целей есть все пять значений `SpaceQueueKind`;
- среди целей с `Kind = Skill` есть хотя бы три разных `BaseTimeMs`;
- `CameraSettings` загружен и пригоден: минимум дистанции больше нуля и меньше максимума, старт наклона внутри диапазона, чувствительность, шаг зума и порог перетаскивания больше нуля;
- у каждого корпуса `CameraDistance` лежит внутри минимума и максимума камеры, `Radius` не меньше нуля, манёвренность больше нуля, `Name`, `Description` и `IconResourcesPath` не пустые, `SizeType` задан, каждый `CommandId` указывает на пригодную команду и не повторяется;
- у каждой команды непустые `Name`, `Description` и `IconResourcesPath`, а в `ObjectTypes` и `Relations` хотя бы по одному неповторяющемуся значению;
- у каждого модуля непустые `Name`, `Description` и `IconResourcesPath`, и те же правила для `CommandIds`;
- `MarkerSettings` загружен, `MenuRadius` больше нуля и покрывает каждое значение `SpaceObjectType`, кроме `Ship`, ровно одной непустой картинкой;
- среди модулей есть пригодный двигатель: в `SizeParameters` хотя бы одна строка, размеры не повторяются, в каждой скорость и инерция больше нуля. Строка на каждый `SpaceSizeType` не обязательна;
- хотя бы один объект, и у каждого масштаб больше нуля, а радиус не меньше нуля;
- хотя бы один сектор, и у каждого есть имя сцены, расстановка из известных объектов и хотя бы одна точка спавна.

Иначе задача старта падает с кодом `SpaceInitErrorCodes.DEFINITIONS` (2). В Play окно `Tools/Space`, секция Definitions, показывает словари каталога и одиночную запись камеры. Вне Play эта часть секции пишет, что менеджера нет.

## Проверка дефов кнопкой

Это отчёт, не запрет запуска. Кнопка **Test definitions** стоит вверху секции Definitions окна `Tools/Space` и работает без Play.

Кнопка создаёт новый `SpaceDefinitionsManager` и вызывает его `Init()`. В отчёт попадают только те записи, которые этот менеджер загружает для сборки: корпуса, модули, команды, объекты, сектора, временные цели, камера и настройки меток. Список проблем собирает `Assets/Modules/Space/Scripts/Editor/Cheats/SpaceDefinitionsValidation.cs`, метод `Test()`. Условий в JSON нет: каждое требование — отдельная строка в методе `Check*` этого файла. Повторное нажатие читает каталог заново.

Кнопка строже старта Play там, где проверяет файлы и связи, которых `IsReady` не открывает:

- спрайт по непустому `IconResourcesPath` реально грузится из `Resources`;
- префаб корпуса обязателен и грузится; у объекта префаб проверяется, только если путь указан;
- сцена сектора с именем `SceneName` есть в проекте;
- поза расстановки конечная;
- у **каждого** модуля есть непустой `SizeParameters`. Пустой список и отсутствующее поле — одна ошибка. У `laser_a` списка нет, поэтому кнопка его покажет. Старт Play из-за этого не падает: там строки `SizeParameters` обязательны только у двигателя.

Остальные условия кнопки совпадают с готовностью каталога: пустые имя, описание и иконка, повтор или битая ссылка `CommandIds`, пустые или повторяющиеся `ObjectTypes` и `Relations`, радиус и манёвренность корпуса, дистанция камеры внутри пределов, масштаб и радиус объекта, годность камеры, одна картинка метки на каждый `SpaceObjectType` кроме `Ship`, `MenuRadius` больше нуля, `BaseTimeMs` не короче 1 мс, все пять видов очередей и три разные длительности навыков, у двигателя в каждой строке `SizeParameters` скорость и инерция больше нуля и размеры не повторяются, у сектора есть хотя бы одна точка спавна и каждая запись расстановки указывает на существующий пригодный объект.

Установка модуля в ангар эту кнопку не вызывает. Она смотрит `AllowsSlotSize` и тип гнезда. Двигатель без строк `SizeParameters` кнопка уже бракует, а `AllowsSlotSize` при совпадении `SizeType` его ещё пропускает.

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
| `SpaceObjectDef` | `ObjectType` | `SpaceObjectType` | `ObjectType` | `PointOfInterest`, `Station`, `Asteroid`, `Spawn`. `Ship` в расстановке не пишется: это тип метки корпуса |
| `SpaceObjectDef` | `VisualResourcesPath` | `string` | `VisualResourcesPath` | путь префаба в `Resources`. Пустая строка — сущность без объекта |
| `SpaceObjectDef` | `UniformScale` | `float` | `UniformScale` | единый масштаб префаба. На расстановке не переопределяется |
| `SpaceObjectDef` | `Radius` | `float` | `Radius` | радиус сферы в метрах. Ноль не отталкивает другие тела |
| `SpaceObjectDef` | `CommandIds` | `List<string>` | `CommandIds` | id команд, которые этот объект мог бы принять. Круг метки их не читает |
| `SpaceHullDef` | `Name` | `string` | `Name` | ключ локализации названия. Круг его не читает |
| `SpaceHullDef` | `Description` | `string` | `Description` | ключ локализации описания. Круг его не читает |
| `SpaceHullDef` | `Slots` | `List<SpaceHullSlotDef>` | `Slots` | гнёзда корпуса. У гнезда свой тип и свой типоразмер, не id модуля |
| `SpaceHullSlotDef` | `SlotType` | `SpaceSlotType` | `SlotType` | что в гнездо садится: `Weapon` или `Engine` |
| `SpaceHullSlotDef` | `SizeType` | `SpaceSizeType` | `SizeType` | типоразмер гнезда. С ним сравниваются `SizeParameters` модуля |
| `SpaceHullDef` | `CommandIds` | `List<string>` | `CommandIds` | id `SpaceCommandDef`, которые этот корпус игрока умеет отдать. Чужой корпус в круг не подмешивается |
| `SpaceHullDef` | `VisualResourcesPath` | `string` | `VisualResourcesPath` | путь в `Resources` на префаб корпуса. Пустой путь корпус в сцену не ставит |
| `SpaceHullDef` | `IconResourcesPath` | `string` | `IconResourcesPath` | картинка метки этого корпуса. Путь в `Resources` на спрайт |
| `SpaceHullDef` | `CameraDistance` | `float` | `CameraDistance` | стартовая дистанция камеры в секторе. На станции не применяется |
| `SpaceHullDef` | `Radius` | `float` | `Radius` | радиус сферы корпуса в метрах |
| `SpaceHullDef` | `ManeuverabilityDegreesPerSecond` | `float` | `ManeuverabilityDegreesPerSecond` | насколько быстро нос поворачивается к желаемому рысканию. Форма корпуса, не двигатель |
| `SpaceHullDef` | `SizeType` | `SpaceSizeType` | `SizeType` | типоразмер корпуса: `XXS` люди и боеприпасы, `XS` дроны, `S` лёгкий истребитель, затем `M`, `L`, `XL`, `XXL` |
| `SpaceModuleDef` | `Name` | `string` | `Name` | ключ локализации названия. Круг его не читает |
| `SpaceModuleDef` | `Description` | `string` | `Description` | ключ локализации описания. Круг его не читает |
| `SpaceModuleDef` | `IconResourcesPath` | `string` | `IconResourcesPath` | картинка модуля. Путь в `Resources` на спрайт. Круг её не читает |
| `SpaceModuleDef` | `CommandIds` | `List<string>` | `CommandIds` | id `SpaceCommandDef`, которые модуль добавляет кругу, когда стоит в гнезде активного корабля |
| `SpaceModuleDef` | `SlotType` | `SpaceSlotType` | `SlotType` | в гнездо какого типа модуль садится: `Weapon` или `Engine` |
| `SpaceModuleDef` | `WeaponGroup` | `string` | `WeaponGroup` | группа оружия. У двигателя пусто |
| `SpaceModuleDef` | `SizeType` | `SpaceSizeType` | `SizeType` | типоразмер самой единицы оборудования |
| `SpaceModuleDef` | `SizeParameters` | `List<SpaceModuleSizeParameters>` | `SizeParameters` | параметры по типоразмерам гнезда. Нет строки типоразмера гнезда — модуль в это гнездо не ставится. Пустой список значит, что допустим только гнездо собственного `SizeType` модуля |
| `SpaceModuleSizeParameters` | `SizeType` | `SpaceSizeType` | `SizeType` | типоразмер гнезда, к которому относится строка. При спавне выбирается строка типоразмера гнезда двигателя |
| `SpaceModuleSizeParameters` | `MaxSpeedMetersPerSecond` | `float` | `MaxSpeedMetersPerSecond` | потолок скорости этой строки |
| `SpaceModuleSizeParameters` | `InertiaSeconds` | `float` | `InertiaSeconds` | постоянная времени разгона и торможения этой строки, в секундах. Больше — медленнее набор и сброс |
| `SpaceCommandDef` | `Name` | `string` | `Name` | ключ локализации подписи кнопки круга |
| `SpaceCommandDef` | `Description` | `string` | `Description` | ключ локализации описания. Круг его не читает |
| `SpaceCommandDef` | `IconResourcesPath` | `string` | `IconResourcesPath` | картинка кнопки круга. Путь в `Resources` на спрайт |
| `SpaceCommandDef` | `PriorityOrder` | `int` | `PriorityOrder` | порядок на круге. Меньше — раньше. При равенстве порядок по id |
| `SpaceCommandDef` | `CommandType` | `SpaceCommandType` | `CommandType` | что делает нажатие: `None`, `Approach`, `Stop`, `HoldPosition`, `HoldOrbit`. Окно по строке id не ветвится |
| `SpaceCommandDef` | `ObjectTypes` | `List<SpaceObjectType>` | `ObjectTypes` | на метках каких типов кнопка видна. Пустой список — нигде |
| `SpaceCommandDef` | `Relations` | `List<SpaceRelation>` | `Relations` | при каком отношении к игроку кнопка видна. Пустой список — нигде |
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
| `SpaceMarkerSettingsDef` | `MenuRadius` | `float` | `MenuRadius` | расстояние кнопок круга от центра метки, в пикселях. Больше нуля |
| `SpaceMarkerSettingsDef` | `Icons` | `List<SpaceMarkerKindIcon>` | `Icons` | по одной записи на каждый `SpaceObjectType`, кроме `Ship` |
| `SpaceMarkerKindIcon` | `ObjectType` | `SpaceObjectType` | `ObjectType` | тип тела, которому принадлежит картинка |
| `SpaceMarkerKindIcon` | `IconResourcesPath` | `string` | `IconResourcesPath` | путь спрайта в `Resources` |

Точка появления — место корпуса в уже загруженной сцене сектора. `X`, `Y`, `Z` задают это место в метрах: вправо, вверх и вперёд. `Yaw` только крутит корпус вокруг вертикали, крен и наклон точки не задают. Ноль градусов смотрит носом по +Z. Код ставит поворот как `Quaternion.Euler(0, Yaw, 0)`. Нуля в коде нет: числа берутся из выбранной точки.

Гнёзда и команды на корпусе — разные списки. Гнездо задаёт `SlotType` и `SizeType`. Модуль называет свой `SlotType` и не хранит id гнезда. Команда — это id `SpaceCommandDef`. Круг берёт команды только активного корабля игрока: с его корпуса и с надетых модулей. Тип метки и отношение к игроку — разные оси. `SpaceObjectType`: `PointOfInterest`, `Station`, `Asteroid`, `Spawn`, `Ship`. `SpaceRelation`: `Player`, `Ally`, `Enemy`, `Neutral`. Свой корпус пишет `Ship` и `Player`. Чужой корпус пишет `Ship` и `Neutral`. Тело сектора пишет свой `ObjectType` и `Neutral`.

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
| `Hulls/Fighter.json` | `Fighter` | гнёзда `Weapon S` и `Engine S`, тестовые команды `test_1`…`test_5`, вид `Prefabs/Spaceships/Fighter`, иконка метки `Sprites/Space/Markers/Fighter`, дистанция камеры `32.5`, радиус `7.249`, манёвренность `90` °/с, типоразмер корпуса `S` |
| `MarkerSettings/MarkerSettings.json` | `MarkerSettings` | `MenuRadius` `78`. `PointOfInterest` → `Sprites/Space/Markers/Point`, `Station` → `Sprites/Space/Markers/Station`, `Asteroid` → `Sprites/Space/Markers/Asteroid`, `Spawn` → `Sprites/Space/Markers/Spawn` |
| `Modules/engine_default.json` | `engine_default` | `Engine`, команды `stop`, `hold_position`, `approach`, `hold_orbit`, типоразмер модуля `S`. Строки гнёзд: `XXS` 70 м/с и 0.6 с, `XS` 55 м/с и 1 с, `S` 40 м/с и 1.5 с, `M` 32 м/с и 2.2 с, `L` 24 м/с и 3.2 с, `XL` 18 м/с и 4.5 с, `XXL` 12 м/с и 6.5 с |
| `Modules/laser_a.json` | `laser_a` | `Weapon`, группа `A`, типоразмер `S`, списка `SizeParameters` нет |
| `Commands/approach.json` | `approach` | `CommandType` `Approach`. Типы `Ship`, `Asteroid`, `Station`, `PointOfInterest`; отношения `Ally`, `Enemy`, `Neutral`. На своём корабле нет. Порядок `10` |
| `Commands/hold_orbit.json` | `hold_orbit` | `CommandType` `HoldOrbit`. Тип `Ship`; отношения `Ally`, `Enemy`, `Neutral`. Только чужой корпус. Порядок `15`. Зазор круга — радиус своего корпуса, пока у орудий нет дистанции |
| `Commands/hold_position.json` | `hold_position` | `CommandType` `HoldPosition`. Тип `Ship`, отношение `Player`. Только свой корабль. Порядок `5`. Снимает маршрут и не ведёт в точку |
| `Commands/stop.json` | `stop` | `CommandType` `Stop`. Тип `Ship`, отношение `Player`. Только свой корабль. Порядок `0` |
| `Commands/test_1.json` … `test_5.json` | `test_1`…`test_5` | `CommandType` `None`. Тестовые кнопки корпуса. Тип `Ship`, отношение `Player`. Порядок `20`, `30`, `40`, `50`, `60` |
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
