# TEA — Tool Edit Adventures (Unity Editor Tool)

## Сокращение TEA

**TEA** (*Tool Edit Adventures*) — принятое в проекте краткое имя для редактора приключений и всех связанных с ним Editor-окон, сервисов и create-option реестров.

В чате с агентом и в правилах Cursor можно писать просто «TEA» вместо длинного «редактор приключений / Adventure Editor / AdventureEditorWindow».

| Синоним | Что имеется в виду |
|---|---|
| TEA | весь инструмент целиком |
| Adventure Editor | то же, официальное имя в меню Unity |
| `AdventureEditorWindow` | главное окно TEA |

**Не путать с runtime:** `AdventuresManager`, adventure UI (`AdventureMainView`) и прочий игровой слой — это не TEA.

Подробное правило для агента: `.cursor/rules/tea-tool-edit-adventures.mdc`

---

## Назначение

`Adventure Editor` (TEA) — это редактор JSON-приключений в Unity для работы с данными:
- приключение (`AdventureData`),
- сцены (`SceneData`),
- контент сцены (`SceneContentData`),
- выборы (`ChoiceData`),
- action-ы выборов (`ChoiceActionData`): `ChoiceActionType.GoToScene` (`Params.Strings["SceneId"]`), `ChoiceActionType.SetWorldParams`, `ChoiceActionType.SetAdventureParams` (см. `.cursor/docs/modules/RPG.md`).

Инструмент доступен через меню:
- `Tools/Definitions/Adventures/Adventure Editor`

Основной класс окна:
- `Assets/Modules/Definitions/Scripts/Editor/Adventures/AdventureEditorWindow.cs`

Код всего инструмента:
- `Assets/Modules/Definitions/Scripts/Editor/Adventures/`
- namespace: `Modules.Definitions.Scripts.Editor.Adventures`

### Окна TEA

| Окно | Класс | Роль |
|---|---|---|
| Adventure Editor | `AdventureEditorWindow` | Главное окно: файлы, сцены, контент, выборы, actions, toolbar |
| Adventure Graph | `AdventureGraphPreviewWindow` | Canvas-граф переходов между сценами (zoom/pan/drag) |
| Adventure Validation | `AdventureValidationWindow` | Детальный список ошибок валидации |
| TEA Localization | `AdventureLocalizationExportWindow` | Генерация ключей локализации и экспорт `.txt` для Google Sheets |
| Identifier Prompt | `IdentifierPromptWindow` | Модальный ввод id при create/rename |
| Create Option Picker | `CreateOptionPickerWindow` | Выбор шаблона, когда опций больше одной |

---

## Где хранятся данные

Редактор работает с JSON-файлами в директории:
- `Assets/Modules/Definitions/Resources/Definitions/_ADVENTURES_/Adventures`

Файловый слой:
- `Assets/Modules/Definitions/Scripts/Editor/Adventures/AdventureEditorFileRepository.cs`

Важно:
- Id приключения в runtime определяется именем файла (как и в существующем loader-пайплайне проекта).
- При сохранении через TEA enum-поля adventure JSON сериализуются строками (например, `Type: "Adventure"`), при загрузке поддерживаются и старые числовые значения.
- Это распространяется в том числе на:
  - `AdventureData.Type`,
  - `SceneContentData.Type`,
  - `ChoiceData.Type`,
  - `ChoiceActionData.Type`,
  - `Restriction.Type` (`RestrictionType`),
  - `Restriction.CompareOptions` (`CompareType`).

---

## Архитектура (кратко)

### UI-слой
- `AdventureEditorWindow.cs`
- Панели:
  - список файлов приключений,
  - редактор Adventure/Scenes/Content/Choices/Actions,
  - граф связей сцен,
  - валидация,
  - локализация (генерация ключей и экспорт).
- Дополнительное окно ввода идентификатора:
  - `IdentifierPromptWindow.cs`
  - используется для create/rename приключений и сцен.

### UI главного окна (`AdventureEditorWindow`)

Нижняя часть окна (после блока `Adventure` meta) растягивается по доступной высоте окна (`GUILayout.ExpandHeight`).

| Колонка | Метод | Поведение |
|---|---|---|
| `Scenes` | `DrawScenesPanel` | Список сцен в scroll-view; кнопки `Delete Scene` / `Duplicate Scene` закреплены внизу колонки |
| `Content` | `DrawContentSection` | Scroll-view на весь раздел: список content-блоков + `Selected Content` |
| `Choices` | `DrawChoicesSection` | Scroll-view на весь раздел: список choices + `Selected Choice` + `Selected Action` |

Принципы раскладки:
- фиксированные высоты scroll-view (`340f`, `140f`) убраны — высота секций подстраивается под размер окна;
- в колонке `Scenes` между списком и action-кнопками стоит `GUILayout.FlexibleSpace()`, чтобы `Delete` / `Duplicate` всегда были внизу;
- если содержимое секции не помещается (длинный список сцен, выбранный choice с actions и restrictions), появляется вертикальная прокрутка **внутри соответствующей колонки**.

Редактор выбранного content (`DrawSelectedContentEditor`):
- для `Text`, `Image`, `Splitter`, `Item` — поле `Value`;
- для `RandomImage`, `Slideshow` — редактируемый список `Values` (строки + `Add Value` / `X`).

Ключевые методы отрисовки: `DrawScenesPanel`, `DrawSceneDetailsPanel`, `DrawContentSection`, `DrawChoicesSection`, `DrawSelectedContentEditor`, `DrawSceneContentValuesEditor`.

### Сервисы
- `AdventureEditorServices.cs`
  - `AdventureGraphBuilder` — строит граф переходов между сценами.
  - `AdventureValidationService` — проверяет консистентность данных.
  - `IAdventureLocalizationKeyCollector` + `DefaultAdventureLocalizationKeyCollector` — собирает уже существующие ключи (по префиксу `loc:`).
  - `AdventureLocalizationGenerationService` — генерирует ключи из текстовых полей и экспортирует tab-separated `.txt`.

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

Доступные значения `SceneContentType` в модели данных:

| Тип | Поле | Назначение |
|---|---|---|
| `Text` | `Value` | Текстовый блок |
| `Image` | `Value` | Путь или URL одного изображения |
| `RandomImage` | `Values` | Список путей/URL; runtime выбирает случайное |
| `Slideshow` | `Values` | Список путей/URL для слайдшоу |
| `Splitter` | — | Визуальный разделитель |
| `Item` | `Value` | Элемент предмета / иконки |

В реестре `SceneContentCreateOptionsRegistry` пока есть кнопки только для `Text`, `Image`, `Splitter`, `Item`. Для `RandomImage` и `Slideshow` нужно добавить новые descriptor-ы.

### 3) Новая опция для action в choice

Класс:
- `ChoiceActionCreateOptionsRegistry`

Сейчас в реестре добавлены шаблоны:
- `Go To Scene` (`Type = GoToScene`, `Params.Strings["SceneId"]` — константа `Glossary.ChoiceActions.SCENE_ID`);
- `Set World Params` (`Type = SetWorldParams`, редактируемые `Params.Strings/Ints/Bools`);
- `Set Adventure Params` (`Type = SetAdventureParams`, редактируемые `Params.Strings/Ints/Bools`).

Legacy-формат (`Type = 100` / `sceneId`, а также `Type = None`) не мигрируется автоматически при загрузке: он ловится в `Validation` и исправляется через кнопку `Fix`.

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
- CRUD actions выбора:
  - `ChoiceActionType.GoToScene` + `SceneId`;
  - `ChoiceActionType.SetWorldParams` + словари `Params.Strings/Ints/Bools`;
  - `ChoiceActionType.SetAdventureParams` + словари `Params.Strings/Ints/Bools`.
- Редактор `Selected Content`: `Value` или список `Values` (для `RandomImage` / `Slideshow`).
- Адаптивная раскладка главного окна: секции `Scenes` / `Content` / `Choices` подстраиваются под высоту окна, с прокруткой при переполнении; кнопки `Delete Scene` / `Duplicate Scene` закреплены внизу колонки `Scenes`.
- Быстрое управление списками через маленькие кнопки в строках:
  - `R` — rename (для файлов приключений и сцен),
  - `X` — delete (для файлов, сцен, контента, выборов).
- Обновление ссылок при переименовании сцены:
  - обновление `StartScenes`,
  - обновление `SceneId` в scene transition actions.
- Граф связей сцен.
- Базовая валидация.
- Валидация `ChoiceActionData` по контрактам `ChoiceActionType`:
  - `GoToScene` — обязательный `Params.Strings["SceneId"]` (`Glossary.ChoiceActions.SCENE_ID`);
  - `SetWorldParams` / `SetAdventureParams` — хотя бы один ключ в `Params.Strings/Ints/Bools`.
- Редактор `Selected Action` для `SetWorldParams` / `SetAdventureParams`: inline-редактирование словарей `Params.Strings`, `Params.Ints`, `Params.Bools`.
- В `Validation` для исправляемых кейсов доступна кнопка `Fix` (например, `sceneId` → `SceneId`).
- Окно `Localization` для генерации ключей и экспорта в `.txt` (tab-separated) для Google Sheets.
- Цветовая индикация состояния:
  - статус в toolbar: `Saved` (зеленый), `Modified` (желтый),
  - выбранный файл в блоке `Files` окрашивается в тот же цвет состояния.

---

## Локализация в TEA

### Точка входа

- Кнопка `Localization` в toolbar главного окна (`AdventureEditorWindow`).
- Открывает модальное окно `AdventureLocalizationExportWindow` (заголовок: `TEA Localization`).

### UI окна

- **Export Folder** — выбор папки, куда будет записан `.txt` файл (по умолчанию — Desktop).
- **Generate Localization Keys** — запуск генерации ключей и экспорта.
- **Reveal Export File** — открыть папку с последним созданным файлом.
- Статусная строка: сколько полей обновлено, ключей сгенерировано, ключей переиспользовано, строк в экспорте.

После генерации adventure помечается как `Modified` — не забудь нажать `Save` в главном окне TEA.

### Какие поля обрабатываются

| Поле | Условие |
|---|---|
| `AdventureData.Title` | всегда |
| `AdventureData.Description` | всегда |
| `SceneContentData.Value` | только если `Type == Text` |
| `ChoiceData.Text` | всегда |
| `ChoiceData.Description` | всегда |

### Ключ vs текст

**Ключ** — одно «слово» латиницей: `camelCase` или `snake_case` (`MY_KEY`, `myKey`, `ADV_TITLE`).  
**Текст** — всё остальное: пробелы, знаки препинания, кириллица, placeholders `{0}` / `{1}` и т.д.

Поле считается уже ключом, если значение:
- совпадает с шаблоном ключа (`^[A-Za-z][A-Za-z0-9]*(?:_[A-Za-z0-9]+)*$`), или
- начинается с `loc:` (префикс снимается перед проверкой).

Такие поля **не изменяются**.

### Алгоритм генерации

1. Обход полей в порядке: adventure meta → сцены (по id) → content → choices.
2. Если поле пустое — пропуск.
3. Если поле уже ключ — пропуск.
4. Если текст уже встречался — подставляется ранее сгенерированный ключ (дедупликация).
5. Иначе генерируется новый ключ, подставляется в поле и добавляется в экспорт.

### Именование ключей

Префикс приключения берётся из имени JSON-файла (аббревиатура/сокращение, uppercase, до 20 символов).

| Поле | Шаблон ключа |
|---|---|
| Title | `{ADV}_ADV_TITLE` |
| Description | `{ADV}_ADV_DESCR` |
| Content (Text) | `{ADV}_{SCENE}_CNT_{N}_TEXT` |
| Choice Text | `{ADV}_{SCENE}_CH_{N}_TEXT` |
| Choice Description | `{ADV}_{SCENE}_CH_{N}_DESCR` |

- `{ADV}` — префикс приключения (например, `ADVENTURETAVERNBYMARTHA` → `ADVENTURETAVERNBYMART`).
- `{SCENE}` — токен сцены (из id сцены).
- `{N}` — 1-based индекс content/choice в сцене.

Если в тексте есть placeholders (`{0}`, `{1}`, ...), к базовому ключу добавляется суффикс `_ARG_N`, где `N` — число **уникальных** индексов аргументов.

При коллизии к имени добавляется суффикс `_2`, `_3`, ...

Примеры:
- `ADVENTURETAVERNBYMART_ADV_TITLE`
- `ADVENTURETAVERNBYMART_TAVERN_INTRO_CNT_1_TEXT`
- `ADVENTURETAVERNBYMART_TAVERN_INTRO_CH_2_TEXT_ARG_1`

### Экспортный файл

- Формат: UTF-8 без BOM, одна пара на строку: `KEY<TAB>TEXT`.
- Имя файла: `{ADV}_localization_{yyyyMMdd_HHmmss}.txt`.
- В экспорт попадают **только новые** ключи (переиспользованные дубликаты не дублируются в файле).

Пример содержимого:

```
TEST_KEY_1	Перевод номер 1
TEST_KEY_2	Перевод номер 2
```

### Вставка в Google Sheets

Таблица локализации обычно имеет колонки `Key | rus | eng | ...`.  
Скопируй столбцы `KEY` и `TEXT` из `.txt` и вставь в пустые строки — tab-разделитель автоматически разложит значения по ячейкам.

Пример таблицы:

| Key | rus | eng |
|---|---|---|
| TEST_KEY_1 | Перевод номер 1 | Translate N 1 |
| TEST_KEY_2 | Перевод номер 2 | Translate N 2 |

### Связанные классы

- UI: `AdventureLocalizationExportWindow.cs`
- Логика: `AdventureLocalizationGenerationService` в `AdventureEditorServices.cs`
- Модели результата: `AdventureLocalizationGenerationResult`, `LocalizationExportEntry`

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
  - Переименование обновляет `StartScenes` и все `SceneId` ссылки в choice actions.

---

## Restrictions в TEA

В adventure JSON (`AdventureData`, `SceneContentData`, `ChoiceData`) поле `Restrictions` редактируется через `DrawRestrictionsSection` в `AdventureEditorWindow`.

Поддерживаемые `RestrictionType` в runtime (см. `.cursor/docs/modules/Restrictions.md`):

| Type | Назначение |
|---|---|
| `TimeNow` | Сравнение текущего UTC-времени с `LongValues[0]` |
| `WorldParams` | Проверка `AdventuresStateData.World.Parameters` |
| `AdventureParams` | Проверка `AdventuresStateData.Adventures[currentAdventureId].Parameters` |

Формат `WorldParams` / `AdventureParams`:

- `StringValues[0]` — ключ параметра;
- `IntValues[0]` — сравнение с `Parameters.Ints[key]`;
- `LongValues[0]` — сравнение long/int (например, `TimeNow`);
- `BoolValues[0]` — сравнение с `Parameters.Bools[key]`;
- иначе `StringValues[1]` — сравнение с `Parameters.Strings[key]`.

TEA пока использует универсальный CSV-редактор для `StringValues` / `IntValues` / `LongValues` / `BoolValues`; подсказки по формату — в документации модуля `Restrictions`.

---

## Куда расширять дальше

- Добавить новые create-option реестры/опции без изменения общей архитектуры.
- Расширить стратегию `IAdventureLocalizationKeyCollector` (например, поддержку альтернативных форматов ключей).
- Добавить визуальный canvas-граф (zoom/pan/drag) поверх текущего списка связей.
- Добавить более детальный editor для `Restrictions` (подсказки/шаблоны по `RestrictionType`, в т.ч. `WorldParams` / `AdventureParams`).

