---
name: adventure-json-scenario-from-template
description: Формирует JSON-сценарий приключения для TEA по текстовому шаблону (из чата или файла), с опциональным целевым файлом и строгими правилами по тегам, локализуемым текстам, переносам строк, CamelCase имени сценария и консистентности ids; предупреждает о Image без путей и предлагает/заполняет смысловые теги (adventure ≥ 3, scene/choice). Использовать при запросах создать/собрать adventure JSON по описанию сцены.
disable-model-invocation: true
---

# Adventure JSON Scenario From Template

## Назначение

Skill для создания корректного JSON-сценария приключения по шаблону:
- шаблон может быть передан текстом в чате;
- или как отдельный файл (например, `.txt`);
- итог сохраняется в указанный целевой JSON-файл или в новый JSON в `_ADVENTURES_/Adventures`.

Точка данных:
- База: `Assets/Modules/Definitions/Resources/Definitions/_ADVENTURES_/Adventures`

## Аргументы вызова

### Обязательный аргумент

- `template`: шаблон сценария текстом **или** путь к файлу шаблона.

### Необязательные аргументы

- `targetFile`: путь к целевому JSON-файлу сценария (пример: `Assets/.../AdventureTavernByMartha.json`).
- `extraRequirements`: дополнительные условия в свободной форме.

## Ключевые правила (обязательные)

1. Все теги всегда заглавными буквами с нижним подчеркиванием между словами (`TEST_TAG_NAME`).
2. Все тексты/дескрипшены и пр. (где по проекту подразумевается локализация) в сценарий вносить в виде текста, а не ключей локализации. Это правило действует, если явно не сказано иное.
3. Один фрагмент текста (то, что задается полям) если предполагается новая строка/абзацы и т.д. не содержит явный enter; перенос строк делается через `\n` или RTF-теги.
4. **Дословно сохраняй маркеры прямой речи и переносы из шаблона** (критично для UI и дальнейшей локализации):
   - все явные `\n` из шаблона должны попасть в итоговый `Value`/`Text`/`Description` как `\n` (не «склеивать» строки и не заменять на пробел);
   - ведущие `-`, `—`, `–` и схожие символы в начале реплики **нельзя удалять** — это оформление прямой речи персонажа (пример: `- Дорогуша!` / `\n— А, классика!`);
   - сохраняй тот же символ тире/дефиса, что в шаблоне (`-` vs `—`), без «нормализации» и без «улучшения» пунктуации;
   - не «причёсывай» текст шаблона: кавычки, многоточия, регистр и пробелы после тире копируй как есть.
5. Название/идентификатор сценария всегда CamelCase.
6. Для id сцен/выборов/шагов жестких требований нет, но стиль должен быть единым по всему сценарию.

## Дополнительные ограничения проекта

- Не создавать/редактировать/удалять `MpGenerated` и MessagePack generated файлы.
- Не изменять файлы в `Assets/Scripts/Meta/SharedLogic/MergeGame/Models/Implementation/`.
- Не создавать и не редактировать Unity `.meta` файлы.

## Алгоритм работы

1. Получи шаблон:
   - если `template` похож на путь к существующему файлу — прочитай файл;
   - иначе используй `template` как raw-текст.
2. Определи целевой JSON:
   - если передан `targetFile`, используй его;
   - иначе создай новый файл в `Assets/Modules/Definitions/Resources/Definitions/_ADVENTURES_/Adventures` (или тематической подпапке), имя файла в CamelCase, например `AdventureMarthaTavernHub.json`.
3. Определи базовые метаданные сценария:
   - `Type`: `Adventure` (по умолчанию), `Chapter` или `Location` по явному смыслу шаблона;
   - `Title`, `Description`, `Tags`, `StartScenes`;
   - `IsRepeatable` обычно `true` для `Location`, иначе по смыслу.
4. Преобразуй шаблон в `Scenes`:
   - блоки вида `Сцена N` -> отдельные элементы словаря `Scenes`;
   - `Точка входа` -> `StartScenes`;
   - строки `Контент` -> массив `Content` в порядке следования;
   - строки `Варианты` -> массив `Choices` в порядке следования.
5. Разбери служебные пометки:
   - `(переход к Сцена X)` -> action `GoToScene` с `Params.Strings.SceneId`;
   - `(переход к другому сценарию ...)` -> action `GoToAdventure` с `Params.Strings.AdventureId`;
   - `(сохранение в ... World.Parameters PARAM_NAME)` -> `SetWorldParams` (чаще `Bools.PARAM_NAME = true`, если не указано иное);
   - `(сохранение в ... Adventures[XYZ].Parameters PARAM_NAME)` -> `SetAdventureParams`;
   - `(сохранение в ... Global.Parameters PARAM_NAME)` -> `SetGlobalParams`;
   - `(открыть окно ...)` -> `OpenWindow` c `Params.Strings.WindowId`.
6. Для условных строк `(условие: ...)` создавай `Restrictions` на уровне контента и/или выбора:
   - `World.Parameters` -> `RestrictionType.WorldParams`;
   - `Adventures[...].Parameters` -> `RestrictionType.AdventureParams`;
   - `Global.Parameters` -> `RestrictionType.GlobalParams`;
   - `ActivePartyCharacterIds больше N` -> `RestrictionType.ActivePartyCount` с `CompareOptions = More` и `IntValues = [N]`;
   - `Parameters персонажа KEY >= N` / feat-флаг -> `RestrictionType.CharacterParams` (`StringValues[0]=KEY`, `IntValues=[N]`, обычно `MoreEqual`);
   - “НЕТ X” обычно -> bool-проверка `Equal false`, “ЕСТЬ X” -> bool-проверка `Equal true`.
7. Приведи финальный JSON к рабочему контракту TEA и runtime.
8. Проверь изображения без путей (см. «Предупреждения об изображениях»).
9. Проведи анализ и заполнение/предложение тегов (см. «Анализ и теги»).
10. Перед записью файла сделай сверку текстов с шаблоном (см. «Сверка текстов с шаблоном»).

## Сверка текстов с шаблоном

Обязательная проверка перед сохранением JSON:

1. **`\n`**: посчитай/найди все явные `\n` в текстовых фрагментах шаблона и убедись, что каждый присутствует в соответствующем поле итогового JSON.
2. **Прямая речь**: найди строки/фрагменты, начинающиеся с `-` / `—` / `–` (в т.ч. после `\n`), и убедись, что тире **не потеряно** в JSON.
3. Если шаблон содержит реплику как отдельный content-пункт вида `- Дорогуша! ...`, в JSON `Value` тоже должен начинаться с того же тире/дефиса.
4. При расхождении — исправь JSON по шаблону, а не «упрощай» шаблон.

Типичная ошибка: убрать ведущее `-` у реплики Марты (`Дорогуша!` вместо `- Дорогуша!`) — так делать нельзя.

## Предупреждения об изображениях

Если в шаблоне или в итоговом JSON есть контент типа `Image` / `RandomImage` / `Slideshow`, но путь(и) к картинкам не указаны (пустой `Value`, пустой/null `Values`, или в шаблоне только пометка вроде «(картинка …)» без пути):

- всё равно создавай соответствующий content-блок (заглушка допустима);
- **обязательно предупреди** пользователя в итоговом отчёте: перечисли сцены и кратко, какая картинка ожидалась;
- не выдумывай реальные asset-пути, если их нет в шаблоне/`extraRequirements`.

## Анализ и теги

После разбора шаблона (и перед/сразу после записи JSON) проведи краткий смысловой анализ шаблона и всех полей `Tags`:

### Adventure-level `Tags` (топ-уровень сценария)

- Если теги **не указаны** или их **меньше 3** — предложи и (если пользователь не запретил) **добавь** связанные по смыслу теги в `UPPER_SNAKE_CASE`, чтобы стало ≥ 3.
- Опирайся на `Type`, Title/Description, роль хаба/квеста/обучения, локацию и явные теги из шаблона.
- Примеры: `LOCATION`, `HUB`, `TAVERN`, `SAFE`, `SOCIAL`, `TRADE`, `TUTORIAL`, `QUEST`, `START`.

### Scene / Choice `Tags`

- Пройдись по всем сценам и выборам (везде, где есть поле `Tags`).
- Если теги пустые или явно беднее смысла сцены/выбора — предложи и заполни релевантные служебные/смысловые теги.
- Рекомендуемые ориентиры (не исчерпывающий список):
  - стартовые / точка входа: `START`, `ENTRY`;
  - выход из сценария (`GoToAdventure` / явный «покинуть»): `EXIT`;
  - бой / combat: `BATTLE`;
  - проверка кубиком (`ChoiceType.DiceCheck`): `DICE_CHECK`;
  - явное обучение / tutorial / путеводитель: `TUTORIAL`;
  - хаб / меню действий: `HUB`;
  - доска объявлений / список приключений: `BOARD`;
  - торговля / отряд / персонаж: `TRADE`, `PARTY`, `CHARACTER`.
- Не перегружай: 1–4 точных тега на сцену/выбор лучше, чем длинный нерелевантный список.
- В отчёте кратко перечисли предложенные/добавленные теги (adventure + ключевые сцены).

## JSON-контракт (минимально необходимое)

Топ-уровень:
- `Disabled: bool`
- `Tags: string[]`
- `IgnoredTags: string[]`
- `IsRepeatable: bool`
- `Type: "Adventure" | "Chapter" | "Location"`
- `AdventureLinks: string[]`
- `Title: string`
- `Description: string`
- `Restrictions: Restriction[]`
- `StartScenes: string[]`
- `Scenes: { [sceneId: string]: SceneData }`

`SceneData`:
- `Id: string` (должен совпадать с ключом в словаре `Scenes`)
- `Tags: string[]`
- `Content: SceneContentData[]`
- `NotClearScene: bool`
- `Choices: ChoiceData[]`

`SceneContentData`:
- `Type: "Text" | "Image" | "RandomImage" | "Slideshow" | "Splitter" | "Item"`
- `Restrictions: Restriction[]`
- `Value: string` (для одиночного значения) или `Values: string[]` (для множественных)

`ChoiceData`:
- `Id: string`
- `Tags: string[]`
- `Type: "Default" | "DiceCheck"`
- `Text: string`
- `Description: string`
- `AlwaysShow: bool`
- `Restrictions: Restriction[]`
- `DiceCheck: null | ChoiceDiceCheckData`
- `Actions: ChoiceActionData[]`

`ChoiceActionData`:
- `Type: "GoToScene" | "SetWorldParams" | "SetAdventureParams" | "SetGlobalParams" | "GoToAdventure" | "OpenWindow" | "GoToRandomAdventure" | "GoToRandomScene"`
- `Params.Strings / Params.Ints / Params.Bools`

`Restriction`:
- `Type: "TimeNow" | "WorldParams" | "AdventureParams" | "GlobalParams" | "ActivePartyCount" | "CharacterParams"`
- `StringValues: string[]`
- `IntValues: int[]`
- `LongValues: long[]`
- `BoolValues: bool[]`
- `CompareOptions: "Equal" | "NotEqual" | "More" | "MoreEqual" | "Less" | "LessEqual"`

## Валидация перед сохранением

Проверь обязательно:
- `StartScenes` не пустой и все id существуют в `Scenes`.
- В каждой сцене `scene.Id` совпадает с ключом словаря.
- Все `choice.Id` в рамках сцены уникальны.
- Для `Choice.Type == "Default"` поле `DiceCheck` равно `null`.
- Для `Choice.Type == "DiceCheck"`:
  - `DiceCheck` заполнен;
  - `Actions` пустой (или `[]`);
  - заполнены outcome-списки (`OnCriticalSuccess`, `OnSuccess`, `OnFailure`, `OnCriticalFailure`).
- Для action-параметров:
  - `GoToScene` -> `Params.Strings.SceneId`;
  - `GoToAdventure` -> `Params.Strings.AdventureId`;
  - `GoToRandomScene` -> `Params.Strings.SceneId` (множественные id через `;`);
  - `OpenWindow` -> `Params.Strings.WindowId`.
- Все теги в формате `UPPER_SNAKE_CASE`.
- Все многострочные тексты содержат `\n`, а не literal line break внутри одной строковой value.
- Все явные `\n` из шаблона сохранены в итоговых текстах.
- Все маркеры прямой речи из шаблона (`-` / `—` / `–`, в т.ч. после `\n`) сохранены без удаления и без замены символа.
- Image/RandomImage/Slideshow без путей: не блокирует сохранение, но требует предупреждения в отчёте.
- Adventure `Tags`: если пусто или меньше 3 — дополнены/предложены смысловые теги.

## Маппинг распространенных фраз шаблона

- `Выбрать персонажа` -> `OpenWindow` + `WindowId = "SelectCharacter"`.
- `Создать персонажа` -> `OpenWindow` + `WindowId = "CreateCharacter"`.
- `Торговать` -> `OpenWindow` + `WindowId = "Trade"`.
- `Сменить состав отряда` -> `OpenWindow` + `WindowId = "Party"`.
- `Посмотреть объявления` -> `OpenWindow` + `WindowId = "AdventureList"`.

Если фраза не покрыта явным маппингом:
- не выдумывай новые `ChoiceActionType`;
- выбери ближайший поддерживаемый тип, либо задай уточнение пользователю.

## Когда задавать уточнение пользователю

Задай уточняющий вопрос **до записи файла**, если:
- шаблон не содержит однозначной точки входа (`StartScenes`);
- не удается однозначно определить `Type` (`Adventure/Chapter/Location`);
- есть ссылки на неизвестные сцены/приключения;
- неясно, где хранить параметр (`World/Adventure/Global`);
- target filename не в CamelCase и пользователь не указал, можно ли переименовать.

## Формат результата

После выполнения сообщи:
- путь к итоговому JSON;
- был ли использован `targetFile` или создан новый файл;
- какие допущения были сделаны при разборе шаблона;
- список сцен и стартовые сцены;
- какие `OpenWindow`/параметры/restrictions были сгенерированы;
- **предупреждения об изображениях без путей** (если есть);
- **анализ тегов**: что было в шаблоне, что предложено/добавлено (adventure ≥ 3; ключевые scene/choice tags);
- **сверка текстов**: кратко подтверди, что `\n` и маркеры прямой речи (`-`/`—`) из шаблона сохранены (или перечисли найденные и исправленные потери).
