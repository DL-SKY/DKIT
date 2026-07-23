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
- выборы (`ChoiceData`): `ChoiceType.Default` и `ChoiceType.DiceCheck`,
- action-ы выборов (`ChoiceActionData`): `ChoiceActionType.GoToScene` (`Params.Strings["SceneId"]`), `ChoiceActionType.GoToAdventure` (`Params.Strings["AdventureId"]`), `ChoiceActionType.GoToRandomAdventure`, `ChoiceActionType.GoToRandomScene` (`Params.Strings["SceneId"]` через `;`), `ChoiceActionType.OpenWindow` (`Params.Strings["WindowId"]`), `ChoiceActionType.SetWorldParams`, `ChoiceActionType.SetAdventureParams`, `ChoiceActionType.SetGlobalParams` (см. `.cursor/docs/modules/RPG.md`),
- для `ChoiceType.DiceCheck` — блок `ChoiceData.DiceCheck` с параметрами броска (`DifficultyClass`, `DiceType`, `DiceOptions`, `DiceCheckParam`) и action-списками исходов (`OnCriticalSuccess`, `OnSuccess`, `OnFailure`, `OnCriticalFailure`).

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

### 0) Опции создания adventure

Класс:
- `AdventureCreateOptionsRegistry`

Доступные шаблоны в блоке `Create Adventure`:

| Id | Кнопка | `AdventureType` | Иконка | `IsRepeatable` |
|---|---|---|---|---|
| `adventure.default` | Adventure | `Adventure` | `path-distance` | `false` |
| `adventure.chapter` | Chapter | `Chapter` | `TextAsset Icon` (как у Text Scene) | `false` |
| `adventure.location` | Location | `Location` | `wireframe-globe` | `true` |

Все три шаблона создают приключение со стартовой сценой `start` и одним текстовым блоком контента. После нажатия кнопки открывается `IdentifierPromptWindow` для ввода id файла.

Добавить новый шаблон — новый `CreateOptionDescriptor<AdventureData>` в `_options` и при необходимости расширить `BuildAdventureTemplate(AdventureType type)`.

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

В реестре `SceneContentCreateOptionsRegistry` есть кнопки для всех значений `SceneContentType`: `Text`, `Image`, `RandomImage`, `Slideshow`, `Splitter`, `Item`.

### 3) Новая опция создания choice

Класс:
- `ChoiceCreateOptionsRegistry`

Доступные значения `ChoiceType` в модели данных:

| Тип | Назначение |
|---|---|
| `Default` | Стандартный выбор |
| `DiceCheck` | Выбор-проверка кубика с блоком `DiceCheck` |

> Ранее в enum была опечатка `Dafault`; в JSON сериализуется как `"Default"`.
>
> Для `ChoiceType.DiceCheck` в `ChoiceData` используется опциональный блок:
> - `DiceCheck.DifficultyClass` — СЛ;
> - `DiceCheck.DiceType` — тип кубика (`DiceType`, обычно `D20`);
> - `DiceCheck.DiceOptions` — флаги броска (`DiceOptions`).
> - `DiceCheck.DiceCheckParam` — строковый ключ проверяемого атрибута/скилла.
> - `DiceCheck.OnCriticalSuccess` / `OnSuccess` / `OnFailure` / `OnCriticalFailure` — списки `ChoiceActionData` для исходов броска.

Шаблон `choice.default` создаёт choice с пустым списком `Actions` (без автоматического `GoToScene`).
Шаблон `choice.dice_check` создаёт choice типа `DiceCheck` с иконкой `dice-twenty-faces-twenty` и предзаполненным блоком `DiceCheck` (включая пустые outcome action-списки).

Пример JSON для `ChoiceType.DiceCheck`:

```json
{
  "Id": "pick_lock",
  "Type": "DiceCheck",
  "Text": "Взломать замок",
  "AlwaysShow": true,
  "Restrictions": [],
  "DiceCheck": {
    "DifficultyClass": 18,
    "DiceType": "D20",
    "DiceOptions": "None",
    "DiceCheckParam": "Thievery",
    "OnCriticalSuccess": [
      {
        "Type": "GoToScene",
        "Params": { "Strings": { "SceneId": "lock_open_fast" } }
      }
    ],
    "OnSuccess": [
      {
        "Type": "GoToScene",
        "Params": { "Strings": { "SceneId": "lock_open" } }
      }
    ],
    "OnFailure": [
      {
        "Type": "SetAdventureParams",
        "Params": { "Ints": { "adventure.lock_attempts": 1 } }
      }
    ],
    "OnCriticalFailure": [
      {
        "Type": "GoToScene",
        "Params": { "Strings": { "SceneId": "trap_triggered" } }
      }
    ]
  },
  "Actions": []
}
```

### 4) Новая опция для action в choice

Класс:
- `ChoiceActionCreateOptionsRegistry`

Сейчас в реестре добавлены шаблоны:
- `Go To Scene` (`Type = GoToScene`, `Params.Strings["SceneId"]` — константа `Glossary.ChoiceActions.SCENE_ID`);
- `Go To Adventure` (`Type = GoToAdventure`, `Params.Strings["AdventureId"]` — `Glossary.ChoiceActions.ADVENTURE_ID`);
- `Open Window` (`Type = OpenWindow`, `Params.Strings["WindowId"]` — `Glossary.ChoiceActions.WINDOW_ID`; шаблонные ids в `Glossary.Windows`);
- `Set World Params` (`Type = SetWorldParams`, редактируемые `Params.Strings/Ints/Bools`);
- `Set Adventure Params` (`Type = SetAdventureParams`, редактируемые `Params.Strings/Ints/Bools`);
- `Set Global Params` (`Type = SetGlobalParams`, редактируемые `Params.Strings/Ints/Bools`, иконка `save.png` из `ButtonIcons/`).

Legacy-формат (`Type = 100` / `sceneId`, а также `Type = None`) не мигрируется автоматически при загрузке: он ловится в `Validation` и исправляется через кнопку `Fix`.

### Отображение в `Selected Choice`

- Для `ChoiceType.Default`:
  - показывается блок `Actions` (список, add/remove/reorder, `Selected Action`).
- Для `ChoiceType.DiceCheck`:
  - показывается блок `Dice Check` с полями `Difficulty Class`, `Dice Type`, `Dice Options`, `Dice Check Param`;
  - общий блок `Actions` не отображается;
  - доступны отдельные редакторы action-списков для исходов (add/remove/reorder + `Selected Action`):
    - `On Critical Success Actions`,
    - `On Success Actions`,
    - `On Failure Actions`,
    - `On Critical Failure Actions`.
  - UI action-списков реализован через общий helper `DrawChoiceActionListEditor(...)` в `AdventureEditorWindow`.

### Ограничения `DiceCheck` в TEA (текущая версия)

- `Scene Graph` пока строит рёбра только из `choice.Actions`, переходы внутри `DiceCheck.*` outcome-списков на графе не отображаются.
- При rename сцены обновляются `SceneId` только в `choice.Actions`; ссылки в `DiceCheck.OnCriticalSuccess/OnSuccess/OnFailure/OnCriticalFailure` пока не переписываются автоматически.
- `Validation` проверяет взаимную исключительность блоков по `ChoiceType`:
  - для `Default` — `DiceCheck` должен быть `null` (с `Fix`: очистка блока);
  - для `DiceCheck` — обязательный блок `DiceCheck`, `DifficultyClass >= 0`, пустой (или `null`) `Actions` (с `Fix`: создание блока / сброс DC / очистка `Actions`).
- Контракты `ChoiceActionData` в outcome-списках `DiceCheck` пока не валидируются (в отличие от `choice.Actions` у `Default`).

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
- Шаблоны `Create Adventure`: `Adventure`, `Chapter`, `Location` (`AdventureCreateOptionsRegistry`).
- CRUD сцен, контента, выборов.
- CRUD actions выбора:
  - `ChoiceActionType.GoToScene` + `SceneId`;
  - `ChoiceActionType.GoToAdventure` + `AdventureId`;
  - `ChoiceActionType.OpenWindow` + `WindowId`;
  - `ChoiceActionType.SetWorldParams` + словари `Params.Strings/Ints/Bools`;
  - `ChoiceActionType.SetAdventureParams` + словари `Params.Strings/Ints/Bools`;
  - `ChoiceActionType.SetGlobalParams` + словари `Params.Strings/Ints/Bools`.
- Редактор `Selected Choice`:
  - для `ChoiceType.Default` — блок `Actions`;
  - для `ChoiceType.DiceCheck` — блок `Dice Check` + action-редакторы исходов (`OnCriticalSuccess/OnSuccess/OnFailure/OnCriticalFailure`) без общего блока `Actions`.
- Редактор `Selected Content`: `Value` или список `Values` (для `RandomImage` / `Slideshow`).
- Адаптивная раскладка главного окна: секции `Scenes` / `Content` / `Choices` подстраиваются под высоту окна, с прокруткой при переполнении; кнопки `Delete Scene` / `Duplicate Scene` закреплены внизу колонки `Scenes`.
- Быстрое управление списками через маленькие кнопки в строках:
  - `R` — rename (для файлов приключений и сцен),
  - `↑` / `↓` — смена порядка элемента в списке (для content, choices, actions),
  - `X` — delete (для файлов, сцен, контента, выборов, actions).
- Обновление ссылок при переименовании сцены:
  - обновление `StartScenes`,
  - обновление `SceneId` в scene transition actions.
- Граф связей сцен.
- Базовая валидация.
- Валидация `ChoiceType.DiceCheck`:
  - обязательный блок `DiceCheck` (с `Fix`: создаёт шаблон с `DC=15`, `D20`, пустым `DiceCheckParam` и пустыми outcome-списками);
  - `DifficultyClass >= 0` (с `Fix` для отрицательных значений);
  - `Actions` должен быть `null` или пустым (`Count == 0`) — для `DiceCheck` action-ы должны жить в outcome-списках (с `Fix`: `Actions.Clear()`).
- Валидация `ChoiceType.Default`:
  - `DiceCheck` должен быть `null` (с `Fix`: `DiceCheck = null`).
- Валидация `ChoiceActionData` по контрактам `ChoiceActionType`:
  - `GoToScene` — обязательный `Params.Strings["SceneId"]` (`Glossary.ChoiceActions.SCENE_ID`);
  - `GoToAdventure` — обязательный `Params.Strings["AdventureId"]` (`Glossary.ChoiceActions.ADVENTURE_ID`);
  - `OpenWindow` — обязательный `Params.Strings["WindowId"]` (`Glossary.ChoiceActions.WINDOW_ID`);
  - `SetWorldParams` / `SetAdventureParams` / `SetGlobalParams` — хотя бы один ключ в `Params.Strings/Ints/Bools`.
- Валидация и автокоррекция тегов:
  - `Adventure.Tags`, `Adventure.IgnoredTags`, `Scene.Tags`, `Choice.Tags` проверяются на `UPPER_SNAKE_CASE`;
  - для некорректных тегов доступен `Fix`, который нормализует значение в `UPPER_SNAKE_CASE`.
- Валидация локализуемых текстовых полей (`Title`, `Description`, `Choice.Text`, `Choice.Description`, `SceneContentData.Value`):
  - предупреждение, если поле похоже на ключ локализации (например, `loc:SOME_KEY` или `SOME_KEY_NAME`);
  - переносы строк (`\n`) в тексте допустимы (в JSON это escaped `\n`, в памяти — обычный line break);
  - при переносе текстов из авторского шаблона в JSON **нельзя** выкидывать `\n` и маркеры прямой речи (`-` / `—` / `–` в начале реплики или после `\n`) — это часть отображаемого текста и последующей локализации (пример: `- Дорогуша!`).
- Валидация стиля id внутри сценария:
  - предупреждение, если id сцены/выбора не в `lower_snake_case` и не в `CamelCase/PascalCase`;
  - предупреждение, если стили id смешаны внутри одного adventure.
- Редактор `Selected Action` для `SetWorldParams` / `SetAdventureParams` / `SetGlobalParams`: inline-редактирование словарей `Params.Strings`, `Params.Ints`, `Params.Bools`; для `GoToAdventure` — поле Target Adventure; для `GoToRandomAdventure` — без params; для `GoToRandomScene` — поле списка Scene Ids через `;`; для `OpenWindow` — popup `Window Id` по `Glossary.Windows` (кастомный id тоже сохраняется в списке).
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
  - Введенный id используется как имя JSON-файла и автоматически нормализуется в CamelCase (например, `adventure tavern by martha` -> `AdventureTavernByMartha`).
- **Создание сцены**
  - Нажатие кнопки в `Add Scene` открывает окно ввода id сцены.
  - Если id занят, автоматически подбирается уникальный (через суффикс).
- **Переименование приключения**
  - Кнопка `R` в списке `Files` открывает окно с текущим id.
  - Переименовывается сам файл (это и есть id приключения в текущем пайплайне) с той же CamelCase-нормализацией имени.
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
| `GlobalParams` | Проверка `AdventuresStateData.Global.Parameters` (данные игрока, не сбрасываются при перезапуске приключений) |
| `ActivePartyCount` | Сравнение `Characters.ActivePartyCharacterIds.Count` с `IntValues[0]` через `CompareOptions` |

Формат `WorldParams` / `AdventureParams` / `GlobalParams`:

- `StringValues[0]` — ключ параметра;
- `IntValues[0]` — сравнение с `Parameters.Ints[key]`;
- `LongValues[0]` — сравнение long/int (например, `TimeNow`);
- `BoolValues[0]` — сравнение с `Parameters.Bools[key]`;
- иначе `StringValues[1]` — сравнение с `Parameters.Strings[key]`.

### Профили полей редактора

По аналогии с `Selected Content` (`Value` vs `Values`) TEA показывает не все поля `Restriction` для каждого типа, а только нужные.

Реестр профилей:
- `Assets/Modules/Definitions/Scripts/Editor/Adventures/Restrictions/RestrictionEditorFieldProfiles.cs`
- `RestrictionEditorFieldProfilesRegistry` — словарь `RestrictionType` → набор видимых полей (`RestrictionEditorField` flags).

Правило по умолчанию: если тип не указан в реестре, показываются все поля (`Compare` + `Strings` / `Ints` / `Longs` / `Bools` CSV).

| `RestrictionType` | Видимые поля в TEA |
|---|---|
| `TimeNow` | `Compare`, `Longs (csv)` |
| `ActivePartyCount` | `Compare`, `Ints (csv)` |
| `WorldParams` | все поля (default) |
| `AdventureParams` | все поля (default) |
| `GlobalParams` | все поля (default) |

Чтобы добавить узкий профиль для нового типа — добавь запись в `_profilesByType` в `RestrictionEditorFieldProfilesRegistry`. Поле `Type` отображается всегда; скрытые поля не удаляются из JSON и сохраняются при save.

---

## Куда расширять дальше

- Добавить новые create-option реестры/опции без изменения общей архитектуры.
- Расширить стратегию `IAdventureLocalizationKeyCollector` (например, поддержку альтернативных форматов ключей).
- Добавить визуальный canvas-граф (zoom/pan/drag) поверх текущего списка связей.
- Расширить `RestrictionEditorFieldProfilesRegistry` подсказками/шаблонами для `WorldParams` / `AdventureParams` / `GlobalParams`.

