# Adventure Editor (Unity Editor Tool)

## Назначение

`Adventure Editor` — это редактор JSON-приключений в Unity для работы с данными:
- приключение (`AdventureData`),
- сцены (`SceneData`),
- контент сцены (`SceneContentData`),
- выборы (`ChoiceData`),
- action-ы выборов (`ChoiceActionData`, сейчас только `GoToScene` в UI как основной поддерживаемый сценарий).

Инструмент доступен через меню:
- `Tools/Definitions/Adventures/Adventure Editor`

Основной класс окна:
- `Assets/Modules/Definitions/Scripts/Editor/Adventures/AdventureEditorWindow.cs`

---

## Где хранятся данные

Редактор работает с JSON-файлами в директории:
- `Assets/Modules/Definitions/Resources/Definitions/_ADVENTURES_/Adventures`

Файловый слой:
- `Assets/Modules/Definitions/Scripts/Editor/Adventures/AdventureEditorFileRepository.cs`

Важно:
- Id приключения в runtime определяется именем файла (как и в существующем loader-пайплайне проекта).

---

## Архитектура (кратко)

### UI-слой
- `AdventureEditorWindow.cs`
- Панели:
  - список файлов приключений,
  - редактор Adventure/Scenes/Content/Choices/Actions,
  - граф связей сцен,
  - валидация,
  - список localization keys.
- Дополнительное окно ввода идентификатора:
  - `IdentifierPromptWindow.cs`
  - используется для create/rename приключений и сцен.

### Сервисы
- `AdventureEditorServices.cs`
  - `AdventureGraphBuilder` — строит граф переходов между сценами.
  - `AdventureValidationService` — проверяет консистентность данных.
  - `IAdventureLocalizationKeyCollector` + `DefaultAdventureLocalizationKeyCollector` — собирает ключи локализации (по стратегии `loc:`).

### Реестры create-option (расширяемость)
- `CreateOptionDescriptor.cs`
- `AdventureCreateOptions.cs`

Именно здесь задаются “кнопки шаблонов” для создания сущностей.

---

## ВАЖНО: где настраиваются кнопки опций CRUD

Главная точка настройки:
- `Assets/Modules/Definitions/Scripts/Editor/Adventures/CreateOptions/AdventureCreateOptions.cs`

В файле определены реестры:
- `AdventureCreateOptionsRegistry`
- `SceneCreateOptionsRegistry`
- `SceneContentCreateOptionsRegistry`
- `ChoiceCreateOptionsRegistry`
- `ChoiceActionCreateOptionsRegistry`

Каждый реестр реализует:
- `ICreateOptionsRegistry<T>`

и возвращает список:
- `IReadOnlyList<CreateOptionDescriptor<T>>`

### Поля, которые управляют внешним видом и поведением кнопки

`CreateOptionDescriptor<T>` (файл `CreateOptionDescriptor.cs`) содержит:
- `Id` — внутренний идентификатор опции.
- `ButtonText` — текст на кнопке.
- `Tooltip` — текст подсказки (tooltip).
- `IconName` — имя Unity-иконки (`EditorGUIUtility.IconContent`).
- `Create` — фабрика, создающая шаблон объекта.

То есть для любой новой CRUD-опции нужно добавить новый `CreateOptionDescriptor` в соответствующий реестр.

### Где задается ширина и отрисовка create-кнопок

- Файл: `AdventureEditorWindow.cs`
- Метод: `DrawCreateOptionButtons<T>(...)`
- Для конкретной секции можно передать `buttonWidth` (например, для `Create Adventure`).

---

## Примеры: как добавить новую кнопку-шаблон

### 1) Новая опция создания сцены

Файл:
- `AdventureCreateOptions.cs`

Класс:
- `SceneCreateOptionsRegistry`

Добавить новый descriptor в `_options`, например:
- `Id = "scene.combat_start"`
- `ButtonText = "Combat Start"`
- `Tooltip = "Create a scene preconfigured for combat intro."`
- `IconName = "d_Rigidbody Icon"`
- `Create = () => new SceneData { ... }`

### 2) Новая опция создания контента

Класс:
- `SceneContentCreateOptionsRegistry`

Добавить descriptor с нужным `SceneContentType`, подписью и фабрикой.

### 3) Новая опция для action в choice

Класс:
- `ChoiceActionCreateOptionsRegistry`

Сейчас в реестре базово добавлен `GoToScene`. При расширении можно добавить дополнительные шаблоны, не меняя саму структуру окна.

---

## Как UI использует реестры

В `AdventureEditorWindow.cs` кнопки создаются универсальным методом:
- `DrawCreateOptionButtons<T>(...)`

Он:
- берет список опций из реестра,
- строит `GUIContent` (текст + иконка + tooltip),
- вызывает `Create` выбранной опции.
- при create приключения/сцены открывает окно `IdentifierPromptWindow` для ввода id.

Именно поэтому добавление новых кнопок делается централизованно в реестрах, а не в коде UI-панелей.

---

## Что поддерживается сейчас

- CRUD приключений через JSON-файлы.
- CRUD сцен, контента, выборов.
- CRUD actions выбора с фокусом на `GoToScene`.
- Быстрое управление списками через маленькие кнопки в строках:
  - `R` — rename (для файлов приключений и сцен),
  - `X` — delete (для файлов, сцен, контента, выборов).
- Обновление ссылок при переименовании сцены:
  - обновление `StartScenes`,
  - обновление `sceneId` в `GoToScene`.
- Граф связей сцен.
- Базовая валидация.
- Базовый сбор localization keys.
- Цветовая индикация состояния:
  - статус в toolbar: `Saved` (зеленый), `Modified` (желтый),
  - выбранный файл в блоке `Files` окрашивается в тот же цвет состояния.

---

## Поведение create/rename id

- **Создание приключения**
  - Нажатие кнопки в `Create Adventure` открывает окно ввода id.
  - Введенный id используется как имя JSON-файла (с нормализацией имени).
- **Создание сцены**
  - Нажатие кнопки в `Add Scene` открывает окно ввода id сцены.
  - Если id занят, автоматически подбирается уникальный (через суффикс).
- **Переименование приключения**
  - Кнопка `R` в списке `Files` открывает окно с текущим id.
  - Переименовывается сам файл (это и есть id приключения в текущем пайплайне).
- **Переименование сцены**
  - Кнопка `R` в списке `Scenes` открывает окно с текущим id.
  - Переименование обновляет `StartScenes` и все `GoToScene.sceneId` ссылки.

---

## Куда расширять дальше

- Добавить новые create-option реестры/опции без изменения общей архитектуры.
- Расширить стратегию `IAdventureLocalizationKeyCollector` (например, поддержку альтернативных форматов ключей).
- Добавить визуальный canvas-граф (zoom/pan/drag) поверх текущего списка связей.
- Добавить более детальный editor для `Restrictions`.

