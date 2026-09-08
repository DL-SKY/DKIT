# План создания браузерного TEA-редактора (frontend-only, single-file)

## Как отмечать прогресс

- `[ ]` - не начато
- `[~]` - в работе
- `[x]` - завершено
- Сначала закрываются подзадачи, затем задача, затем этап.

**Текущий статус (2026-09-07, сверка с кодом `TEA/WEB`):** в коде и по приёмкам закрыты **0**, **15.1–15.3**, **§8 UI**, **граф MVP (§9.3 + статусы §6.3)**, **граф V2 (фильтры All/Start/Unreachable/Broken)**, **Fix (§9.2)**, **Validation UI (§9.1)**, **локализация (§10 / §6.4)**, **local static (§11.1)**, **тесты (§12)**. **CDN (§11.2) отложен** — не деплоить, пока не будет явной команды и хоста. Ручные дыры (не этапы): Firefox; Chromium same-file Save picker; клик по canvas графа. Save As download и Download TSV закрыты Playwright-ом. **Плановых этапов в очереди нет.**

**Факт по коду (не гадать):**
- Домен: `src/domain` — normalize/serialize/validate+Fix, graph (рёбра только Default `GoToScene`/`GoToRandomScene`/`None`), localization generate/TSV, templates, restriction profiles.
- IO: `src/io/adventureFile.ts` — Open/Save/Save As, File System Access + download fallback. `localStorage` нет.
- UI: `App.tsx` toolbar Open/Save/Save As/Scene Graph/Localization; панели scenes/content/choices/meta/validation; overlay Graph + Localization.
- Скрипты: `check-node.bat`, `start-web-tea.bat`, `preview-web-tea.bat`; `scripts/roundtrip.ts`, `scripts/localization-check.ts`; `npm test` = vitest (`tests/`) + Playwright (`e2e/`, Chromium).
- Сборка: `vite.config.ts` `base: "./"`; `dist/` есть (gitignore). `ParameterOverrideDescription` в generate есть, в loc-валидации **нет** (намеренно).

---

## Рекомендации по моделям Cursor

Как читать блок «Модель» у этапа:
- **Основная** — чем вести сам этап.
- **Запасная** — если основная недоступна или результат слабый.
- **Не брать** — слишком быстрая/слабая для этого типа работы, либо наоборот избыточная.

Доступный набор (имена как в Cursor):
- **Grok 4.6** — режимы **High**, **Fast**, **High Fast**. High — для разбора и решений; Fast / High Fast — для реализации UI и каркаса.
- **GPT-5.6** — режим **Medium** (при необходимости выше, если в UI есть более «думающий» пресет). Хорош для сверки правил и спорных контрактов.
- **GPT-5.3 Codex** — код с высокой точностью: порт валидации, JSON, тесты, фиксы.
- **Gemini 3.1 Pro** — большие UI-файлы, раскладка, много однотипных форм.
- **Composer 2.5 Fast** — мелкие правки «по месту», не для проектирования и не для порта домена.

Правило смены: не менять модель внутри этапа без причины. Менять на границе этапа или если текущая модель путает контракты / ломает JSON.

Сводка по этапам:

| Этап | Основная | Режим | Запасная |
|---|---|---|---|
| 0 Требования/UX | Grok 4.6 | High | GPT-5.6 Medium |
| Архитектура/стек | Grok 4.6 | High, затем High Fast | GPT-5.6 Medium |
| 1 Domain/Core | GPT-5.3 Codex | — | GPT-5.6 Medium |
| 2 File IO | Grok 4.6 | High Fast | GPT-5.3 Codex |
| 3 UI редактора | Grok 4.6 | High Fast | Gemini 3.1 Pro |
| 4 Validation/Fix/Graph | Codex (правила) + Grok (UI) | High Fast на UI | GPT-5.6 Medium |
| 5 Локализация | GPT-5.3 Codex | — | GPT-5.6 Medium |
| Сборка/публикация | Grok 4.6 | Fast / High Fast | Composer 2.5 Fast |
| Тесты | GPT-5.3 Codex | — | Grok 4.6 High |
| Старт backlog | Grok 4.6 | High Fast | Codex на приёмке |

---

## 1) Принятая модель

Целевой сценарий:
- открыть страницу web-версии TEA (локально или на сервере);
- выбрать **один** файл сценария (`.json`);
- редактировать его в браузере;
- сохранить через `Save` / `Save As`.

Архитектурные решения:
- **только frontend** (без backend, без серверного API);
- вся доменная логика (шаблоны, валидация, граф, локализация, контракты) вшита в клиентский код;
- одновременно редактируется только один загруженный adventure-файл;
- нет файлового менеджера и нет CRUD по списку adventure-файлов.

Критерии общего успеха:
- [x] страница работает локально через `npm run dev` / `start-web-tea.bat`. Static `dist/` собран, preview проверен.
- [~] страница работает как static (`npm run preview`); удалённый CDN не указан / не выкладывали;
- [~] выбранный JSON можно загрузить, править meta/сцены/content/choices/actions/DiceCheck/restrictions; граф MVP есть; Save As в приёмке §8 не гоняли;
- [x] формат JSON совместим с runtime TEA (roundtrip 15.3);
- [~] поток "открыл страницу -> выбрал файл -> работаешь" закрыт для UI §8 + граф MVP + loc + `dist/` preview; полный MVP (Save As/Firefox) не закрыт.

---

## 2) Scope

### MVP
- [x] `Open File` для одного adventure JSON;
- [x] редактирование `AdventureData` / `SceneData` / `SceneContentData` / `ChoiceData` / `ChoiceActionData` (meta + сцены + content/choices/actions);
- [x] поддержка `ChoiceType.Default` и `ChoiceType.DiceCheck`;
- [x] поддержка актуальных `ChoiceActionType`;
- [x] валидация adventure в браузере;
- [x] `Save` / `Save As` в файл;
- [x] базовый граф сцен.

### V2
- [x] `Fix`-операции по результатам валидации;
- [x] расширенный граф (unreachable/broken/start + фильтры);
- [x] генерация локализационных ключей;
- [x] экспорт TSV локализации (`download`).

### Вне scope (сейчас)
- [ ] backend / Web API;
- [ ] доступ к папке проекта `Assets/...`;
- [ ] list/create/rename/delete adventure-файлов;
- [x] создание полупустой заготовки без выбранного файла (`New File`: пустой adventure + start-сцена `start`);
- [ ] multi-file batch;
- [ ] отдельные редакторы `PregeneratedCharacterDef` / `CreatureDef` / `EncounterDef`.

Расширения (новые режимы, def-редакторы) рассматриваются **после** закрытия базовых возможностей MVP/V2.

---

## 3) Ограничения браузера (обязательно учитывать)

### 3.1 Что браузер умеет
- [x] Чтение выбранного пользователем файла через File API.
- [x] Скачивание результата через `Blob` + `download` (`Save As`).
- [x] В Chromium-based браузерах: запись в тот же файл через File System Access API (`showSaveFilePicker` / `createWritable`).

### 3.2 Чего браузер не умеет без backend
- [x] Не имеет произвольного доступа к файловой системе проекта.
- [x] Не может сам "найти" adventure в `Assets/...`.
- [x] Не может атомарно обновлять файл вне явно выбранного пользователем handle.
- [x] Не может гарантировать запись "поверх того же пути" во всех браузерах (Firefox обычно только `Save As`/download).

### 3.3 UX-правила из ограничений
- [x] Основной путь сохранения: `Save As` (скачивание обновленного JSON).
- [x] Если доступен File System Access API: `Save` пишет в исходный файл.
- [x] Если API недоступен: кнопка `Save` остаётся видимой и работает как `Save As`.
- [x] После `Open File` хранить в сессии имя файла и (если есть) file handle.
- [x] Явно показывать пользователю текущий режим сохранения (`Same file` / `Download only`).
- [x] Целевые браузеры: Chromium + Firefox. Safari — позже, не блокер MVP.
- [x] Открытие `file://` не даёт File System Access API: только input-файл + download. Надёжный `Save` в тот же файл — localhost/https + Chromium.

---

## 4) Архитектура (frontend-only)

**Модель:** Grok 4.6, **High** (основная). Запасная: GPT-5.6 **Medium**.  
Каркас репозитория после утверждения стека — Grok 4.6 **High Fast**. Не брать Composer на выбор стека.

### 4.1 Стек
- [x] Зафиксировать SPA-стек: **React + TypeScript + Vite**, каталог `TEA/WEB` (корень репозитория, не `Assets/`).
- [x] Сборка static-артефакта (`dist/`) для локального открытия и деплоя. Скрипт `npm run build`; preview `npm run preview` / `preview-web-tea.bat`.
- [x] Без серверной части: только static hosting.

### 4.2 Модули клиента
- [x] `io` - open/save/download файла.
- [~] `domain` - модели, нормализация, serialize; roundtrip на реальных JSON (15.3); локализация generate/TSV.
- [x] `validation` - список issues + Fix / Fix All.
- [x] `graph` - построение графа переходов.
- [x] `localization` - генерация ключей и экспорт TSV.
- [x] `templates` - create-options (scene/content/choice/action).
- [x] `ui` - empty state, toolbar Open/Save/Save As/Scene Graph/Localization, meta + scenes + content/choices/actions, панель Validation, canvas графа.

### 4.3 Источник правил
- [~] Портировать правила из TEA (`AdventureEditorServices`, create-options, contracts) в TypeScript-модули клиента. Валидация issues и Fix портированы; шаблоны content/choice/action в UI; локализация generate/TSV.
- [x] Зафиксировать матрицу паритета "Unity TEA -> Web TEA" (черновик этапа 0, см. §5.3).
- [x] Покрыть критичные правила unit-тестами. `npm run test:unit` (vitest): normalize/serialize, validate+Fix, graph, localization, restriction profiles, IO parse.

---

## 5) Этап 0 - Фиксация требований и UX

**Модель:** Grok 4.6, **High** (основная). Запасная: GPT-5.6 **Medium**.  
Этап без кода: чтение TEA, вопросы, UX open/save, матрица паритета. Не брать Fast/Composer — легко пропустить ограничения браузера.

### 5.0 Принятые решения (2026-09-07)

- Каталог SPA: `TEA/WEB`.
- Стек: React + TypeScript + Vite (static `dist/`, без backend).
- JSON Save: **канонический TEA-save** (вариант A). После нормализации пишется полный indented JSON, enum строками. Файл после Save может отличаться от исходного (добавятся пустые коллекции, `"DiceCheck": null` и т.п.) — это ожидаемо и совместимо с runtime.
- Неизвестные JSON-поля при Save **выкидываются** (как Unity TEA).
- Кнопка `Save` всегда видна. Нет File System Access API → `Save` = `Save As`.
- Кнопки `Revert` нет.
- Заготовка «с нуля» (`New File`) — реализована: создаётся пустой adventure с одной сценой `start`.
- Этап 15.2: валидация = прогон правил TEA + список issues в UI. Без Fix и без отдельного окна Validation.
- Граф сцен нужен в MVP (удобный обзор сцен и переходов). Не входит в 15.1/15.2.
- Браузеры приёмки: Chromium + Firefox.
- `VisualOptions.ParameterOverrideDescription` — id параметра, не локализуемый текст. Web TEA **не** гоняет по нему проверку «похоже на ключ локализации» (намеренное отличие от текущего C# TEA).

### 5.1 Пользовательский поток
- [x] Описать экран запуска (empty state + `Open File`).
- [x] Описать поток ошибок невалидного JSON.
- [x] Описать поток `Save` / `Save As`.
- [x] Описать поведение при открытии нового файла с unsaved changes.

**Empty state.** Нет загруженного файла. Центр экрана: короткий текст + `Open File` и `New File`. Toolbar: активны `Open File` и `New File`; `Save` / `Save As` / `Scene Graph` / `Localization` выключены. Панель Validation на empty state не показывается (нет workspace). Статус: `No file`, режим сохранения `—`.

**New File.** Два prompt: имя файла (`new_adventure.json` по умолчанию) и `start scene id` (`start` по умолчанию). Создаётся пустой adventure в памяти: `Type=Adventure`, пустые коллекции, `StartScenes=[<sceneId>]`, `Scenes[sceneId]` пустая сцена. `handle=null`, `dirty=true`; сохранение дальше через `Save`/`Save As` (fallback в download без File System Access API).

**Open File.** Диалог `.json` (`showOpenFilePicker` в Chromium; иначе `<input type="file" accept=".json">`). Parse → нормализация в памяти (как TEA) → сессия: `fileName`, dirty=false, handle если есть → редактор.

**Ошибки JSON.** Битый синтаксис или не-объект adventure: баннер/модалка, файл не загружается, empty state / текущий файл не трогаем. Валидный JSON с предупреждениями TEA: файл открывается, issues в списке Validation, Open не блокируется.

**Save / Save As.** `Save As` всегда доступен при открытом файле (download или picker нового пути). `Save` в тот же файл — только Chromium + handle; иначе тот же путь, что `Save As`. После успеха: dirty=false, имя/handle обновляются. В toolbar режим `Same file` / `Download only`.

**Unsaved changes.** Confirm при Open другого файла и при закрытии/reload вкладки (`beforeunload`). Текст как в TEA: «Current adventure has unsaved changes. Continue without saving?»

Текст для пользователя про файлы: редактор не видит папку проекта. Нужно самому выбрать `.json`. В Firefox и при открытии html с диска запись «поверх того же файла» недоступна — пользуйся `Save As`.

### 5.2 Состояние сессии
- [x] Dirty-state.
- [x] Имя текущего файла.
- [x] Доступность File System Access API.
- [x] Confirm перед потерей изменений.

В сессии (только в памяти вкладки, без `localStorage`): `adventure | null`, `fileName` (это runtime-id приключения), `fileHandle | null`, `dirty`, `saveMode`, selection (scene/content/choice/action). Reload страницы → empty state.

### 5.3 Артефакты
- [x] Чеклист сценариев single-file режима.
- [x] Список инвариантов adventure JSON.
- [x] Матрица паритета с TEA.

**Чеклист сценариев**

1. Страница → empty state → Open File → редактор.
2. Битый JSON → ошибка, empty state жив.
3. JSON не adventure → отказ.
4. Файл с issues TEA → открывается, validation не блокирует Open.
5. Правка → dirty / Modified.
6. Save As → файл получен, dirty сброшен.
7. Chromium + handle → Save в тот же файл.
8. Firefox → Save As работает; Save ведёт себя как Save As.
9. Open другого файла при dirty → confirm Cancel/Continue.
10. Reload вкладки при dirty → beforeunload.
11. Roundtrip open → save → reopen: runtime-поля на месте, enum строками. Исходный «худой» JSON может дополниться каноническими полями.
12. Empty state → New File → появляется введённая start-сцена (или `start` по умолчанию), статус `Modified`, Save As скачивает новый `.json`.

**Инварианты JSON**

- Id приключения = имя файла без `.json`, поле `Id` в JSON не пишется (`JsonIgnore`).
- Enum в файле — строки; на чтении допустимы старые числа.
- После нормализации коллекции не `null`: `Tags`, `IgnoredTags`, `AdventureLinks`, `Restrictions`, `StartScenes`, `Scenes`, scene `Content`/`Choices`, choice `Actions`/`Tags`/`Restrictions`.
- Ключ в `Scenes` = `SceneData.Id`.
- Хотя бы одна сцена и хотя бы один `StartScenes`; start существует в `Scenes`.
- `Default`: `DiceCheck == null`, actions в `Actions`.
- `DiceCheck`: блок обязателен, `DC >= 0`, `Actions` пустой/null; исходы в четырёх outcome-списках.
- Контракты actions: `SceneId` / `AdventureId` / `WindowId` / `ParameterKey`+`ParameterValue|ParameterDelta`; `Set*Params` — хотя бы один ключ; `GoToRandomAdventure` без params; `GoToRandomScene` — `SceneId` через `;`.
- Теги: `UPPER_SNAKE_CASE`.
- Id сцен/choices: `lower_snake_case` или CamelCase/PascalCase, без смешения стилей в одном файле.
- `VisualOptions.ParameterOverrideDescription` не считается локализуемым текстом и не проверяется правилом loc-ключа.

**Матрица паритета Unity TEA → Web TEA**

| Возможность | Unity | Web MVP | V2 | Вне scope |
|---|---|---|---|---|
| Open одного JSON | список папки | Open File | | |
| Save в тот же путь | да | Chromium + handle | | |
| Save As / download | нет | да | | |
| Revert | да | нет | | |
| Create/rename/delete файлов | да | | | да |
| Заготовка без файла | да | | | да |
| Meta + CRUD сцен/content/choice/action | да | да | | |
| DiceCheck + VisualOptions | да | да | | |
| Loc-проверка на `ParameterOverrideDescription` | да (сейчас) | нет (намеренно) | | |
| Restrictions + профили полей | да | да | | |
| Валидация (список issues) | да | да (с 15.2) | | |
| Fix | да | | да (сделано) | |
| Базовый граф сцен и переходов | да | да (после 15.2) | | |
| Расширенный граф (unreachable/broken/фильтры) | частично | | да | |
| Рёбра графа из DiceCheck outcomes | нет | нет (паритет) | можно позже | |
| Rename сцены → DiceCheck SceneId | нет | нет (паритет) | | |
| Контракты actions в DiceCheck outcomes | нет | нет (паритет) | | |
| Localization | да | | да (сделано) | |
| Def-редакторы | да | | | да |
| Backend / папка `Assets/...` | да | | | да |

### 5.4 Готовность этапа
- [x] UX open/save утвержден.
- [x] Ограничения браузера явно задокументированы для пользователей.

Этап 0 закрыт полностью. 15.1–15.3 сделаны. Этап 3 UI (§8) закрыт. Граф MVP (§9.3), граф V2 (фильтры), Fix (§9.2), Validation UI (§9.1), локализация (§10), local static `dist/` (§11.1) и тесты (§12) сделаны. **CDN (§11.2) отложен** (без хоста, не в очереди). Плановых этапов дальше нет.

### 5.5 `ParameterOverrideDescription`
- [x] Решение: не проверять как текст / loc-ключ.

Поле Visual Options — id параметра (`Thievery`, `STR`, …), не фраза для игрока. Проверку «похоже на ключ локализации» Web TEA на нём не делает. Это намеренное отличие от текущего Unity TEA.

---

## 6) Этап 1 - Domain/Core в клиенте

**Модель:** GPT-5.3 Codex (основная). Запасная: GPT-5.6 **Medium**.  
Порт нормализации, валидации, контрактов actions, тестов. Для сверки с C# TEA перед кодом можно начать на Grok 4.6 **High**, затем переключить на Codex. Не брать Fast/Composer.

### 6.1 Нормализация
- [x] Нормализация `AdventureData` и вложенных коллекций.
- [x] Нормализация `ChoiceData` / `Actions` / `DiceCheck`.
- [x] Дефолты для неполных/legacy JSON.

### 6.2 Валидация
- [x] Порт ключевых правил `AdventureValidationService`.
- [x] Контракты `ChoiceActionType`.
- [x] Проверки `Default` vs `DiceCheck`.
- [x] Подготовка fix-операций.

### 6.3 Граф
- [x] Хелперы переходов (`GoToScene` / `GoToRandomScene`) для валидации. Builder графа: `TEA/WEB/src/domain/graph.ts`.
- [x] Обработка `GoToScene`.
- [x] Обработка `GoToRandomScene`.
- [x] Статусы start/unreachable/broken.

### 6.4 Локализация
- [x] Генерация ключей.
- [x] Дедупликация.
- [x] Формирование TSV для скачивания.

### 6.5 Шаблоны
- [x] Templates для scene (empty/text), content, choice, action.
- [x] Встроенные словари ключей (`SceneId`, `AdventureId`, `WindowId`, и т.д.).

### 6.6 Тесты domain
- [x] Unit-тесты нормализации. `TEA/WEB/tests/normalize.test.ts`.
- [x] Unit-тесты валидации/fix. `TEA/WEB/tests/validate.test.ts` (фикстура 10/9/8 + реальные JSON).
- [x] Unit-тесты графа. `TEA/WEB/tests/graph.test.ts` (Crossroad 6 рёбер; ForestPath 37; DiceCheck outcomes не рёбра; фильтры start/unreachable/broken).
- [x] Unit-тесты локализации. `TEA/WEB/tests/localization.test.ts` (synthetic + реальные счётчики).
- [x] Golden/roundtrip на реальных adventure JSON: vitest + скрипт `TEA/WEB/scripts/roundtrip.ts` (5 файлов, 15.3). Скрипт сверки loc: `TEA/WEB/scripts/localization-check.ts`.

### 6.7 Готовность этапа
- [x] Domain-логика normalize/serialize/validate/fix/localization работает; UI редактирует meta, сцены, content, choices, actions; граф MVP построен.
- [x] Поведение JSON совпадает с ожидаемым TEA на 5 тестовых файлах (потери данных нет, 15.3) и покрыто vitest.

---

## 7) Этап 2 - File IO (Browser File API)

**Модель:** Grok 4.6, **High Fast** (основная). Запасная: GPT-5.3 Codex.  
Open/Save/Save As, File System Access API, fallback download. Сериализацию enum (7.3) лучше отдать Codex, если Grok расходится с TEA JSON.

### 7.1 Open
- [x] Кнопка `Open File`.
- [x] Чтение `.json` через File API.
- [x] Parse + понятные ошибки формата.
- [x] Сохранение `fileName` в сессии.
- [x] При поддержке File System Access API: сохранение file handle.

### 7.2 Save / Save As
- [x] `Save As` через download (`Blob`).
- [x] `Save` через File System Access API (если доступно).
- [x] Fallback: если `Save` недоступен -> использовать `Save As`.
- [x] Индикатор режима сохранения в toolbar.

### 7.3 Сериализация
- [x] Serialize adventure в indented JSON.
- [x] String enum serialization (как в TEA).
- [x] Совместимость с legacy numeric enum values при чтении.

### 7.4 Готовность этапа
- [x] Один файл открывается и сохраняется (код 15.2 + roundtrip 15.3).
- [x] Roundtrip `open -> serialize -> reopen` на 5 реальных JSON не теряет сцены/choices/actions/DiceCheck.
- [x] Ошибки IO понятны пользователю.

---

## 8) Этап 3 - UI редактора adventure

**Модель:** Grok 4.6, **High Fast** (основная). Запасная: Gemini 3.1 Pro.  
Много панелей и форм. Мелкие правки вёрстки — Composer 2.5 Fast. Редакторы DiceCheck/action-контрактов при расхождениях — GPT-5.3 Codex.

### 8.1 Layout
- [x] Empty state + `Open File`.
- [x] Toolbar: `Open`, `Save`, `Save As`, `Scene Graph`, `Localization`, статус, режим сохранения. Validation — панель, не отдельное окно.
- [x] Панель сцен.
- [x] Панель контента.
- [x] Панель choices.
- [x] Редактор selected action / dicecheck.

### 8.2 Редактирование структуры
- [x] Adventure meta.
- [x] `StartScenes`.
- [x] Add/Rename/Delete/Duplicate scene.
- [x] Add/Remove/Reorder content.
- [x] Add/Remove/Reorder choices.
- [x] Add/Remove/Reorder actions.

### 8.3 DiceCheck и actions
- [x] UI для `ChoiceType.DiceCheck`.
- [x] Outcome action-списки.
- [x] Редакторы параметров всех поддерживаемых `ChoiceActionType`.
- [x] Подсказки/валидация ключей параметров.

### 8.4 Restrictions и helpers
- [x] Редактор restrictions с профилями полей.
- [x] Редактор словарей `Strings/Ints/Bools`.
- [x] Списки тегов / StartScenes / links через textarea (по строке). Не CSV-хелперы TEA.

### 8.5 Состояние сессии
- [x] Dirty-state.
- [x] Confirm при открытии другого файла с несохраненными изменениями.
- [x] Восстановление selection после операций со сценами.

### 8.6 Готовность этапа
- [x] Полный цикл редактирования одного файла работает (meta/сцены/content/choices/actions + граф MVP).
- [x] Ключевые сценарии TEA для adventure закрыты в UI (граф — §9.3).
- [~] Приёмка в Chromium (2026-09-07): Crossroad + ForestPath, content/choices/DiceCheck/restrictions. Save As download закрыт Playwright-ом (§12); Firefox не гоняли.

---

## 9) Этап 4 - Validation / Fix / Graph

**Модель:** GPT-5.3 Codex (основная) для Fix и правил. UI панели и canvas графа — Grok 4.6 **High Fast** или Gemini 3.1 Pro.  
Не смешивать в одном проходе «нарисовать граф» и «портировать fix»: сначала домен, потом UI.

### 9.1 Validation UI
- [x] Панель проблем (список issues + Fix / Fix All).
- [x] Фильтры/поиск.
- [x] Навигация к месту проблемы.

### 9.2 Fix workflow
- [x] `Fix` для исправляемых проблем.
- [x] Пакетное применение fixes.
- [x] Автоповтор валидации после фиксов.

### 9.3 Graph UI
- [x] Canvas с zoom/pan.
- [x] Статусы узлов.
- [x] Переход из графа к сцене.
- [x] Фильтры All / Start / Unreachable / Broken. Статы Nodes/Broken/Unreachable остаются полными; при фильтре `Showing: N of M`. Рёбра только между видимыми узлами. DiceCheck outcomes по-прежнему не рисуются.

### 9.4 Готовность этапа
- [x] Graph, Fix и Validation UI (§9.1) применимы. Chromium: Fix (§9.2) и фильтры/навигация (§9.1). Playwright: Validation/Fix на фикстуре; граф V2 фильтры на `graph-filter.json` (3 узла → Unreachable/Broken/Start по 1). Клик по canvas графа не закрывали.

---

## 10) Этап 5 - Локализация

**Модель:** GPT-5.3 Codex (основная). Запасная: GPT-5.6 **Medium**.  
Генерация ключей должна совпасть с TEA. UI preview/download — Grok 4.6 **Fast** / **High Fast**.

### 10.1 Генерация
- [x] Генерация ключей по правилам TEA.
- [x] Preview изменений.
- [x] Применение изменений к текущей модели.

### 10.2 Экспорт
- [x] Формирование TSV.
- [x] Скачивание экспорт-файла.
- [x] Понятная обратная связь пользователю.

### 10.3 Готовность этапа
- [x] Локализация работает в single-file потоке без внешних шагов. Chromium (2026-09-07): Crossroad Generate 14/12/2 → Apply (Title `CROSSROAD_ADV_TITLE`, dirty); TutorialIntro 3/3/0 → Apply; второй Generate 0. Playwright (§12): Generate → Download TSV → Apply на `localization-demo.json`.

---

## 11) Сборка и публикация (static)

**Модель:** Grok 4.6, **Fast** или **High Fast** (основная). Запасная: Composer 2.5 Fast.  
Vite/static host, короткая README. Не нужен Codex.

### 11.1 Local run
- [x] Dev-сервер для разработки (`npm run dev`, `start-web-tea.bat`).
- [x] Сборка static `dist/`. `npm run build` (2026-09-07): `dist/index.html` + `assets/` (css 8.18 kB, js 267.81 kB), `base: ./`.
- [x] Проверка открытия через локальный static host. Chromium `npm run preview` `http://127.0.0.1:4173/`: empty state; Open `PreviewSmoke.json` → Title «Preview build», Localization/Graph enabled; ресурсы только js+css того же origin (foreign=0).
- [x] Документация: как открыть web-TEA локально (`.cursor/plans/TEA/README.md`, `TEA/WEB/README.md`).
- [x] Скрипты в `TEA/WEB`: `check-node.bat` (проверка Node 20+), `start-web-tea.bat` (install при необходимости + dev), `preview-web-tea.bat` (build при отсутствии `dist/` + preview). Node скрипты сами не ставят.

### 11.2 Hosted run
- [~] Деплой `dist/` на static hosting: **отложен** (2026-09-07). Локальный preview как static-сервер закрыт; удалённый CDN/GitHub Pages не выкладывать, пока не будет явной команды и хоста.
- [x] Проверка, что для работы не нужен backend (preview: только static js/css).
- [x] Документация: как опубликовать страницу (скопировать содержимое `dist/` на любой static-хост; https для Chromium Save).

### 11.3 Готовность этапа
- [~] Local static (`dist/` + `npm run preview`) закрыт. Этап 11 целиком не закрыт: **CDN (§11.2) отложен**, не в очереди.

---

## 12) Тестирование и качество

**Модель:** GPT-5.3 Codex (основная). Запасная: GPT-5.6 **Medium**.  
Unit/golden/roundtrip — Codex. E2E-сценарии и правка селекторов — Grok 4.6 **High Fast**. Разбор «почему расхождение с TEA» — Grok 4.6 **High**.

### 12.1 Unit
- [x] Нормализация и инварианты. `vitest` + `npm test` / `npm run test:unit`. `TEA/WEB/tests/normalize.test.ts`.
- [x] Валидация и fix. `TEA/WEB/tests/validate.test.ts`. Фикстура `tests/fixtures/validation-fix.json`: 10 issues / 9 Fix / 8 navigable; Fix All → loc `KEY_1`.
- [x] Граф и локализация. `tests/graph.test.ts`, `tests/localization.test.ts`. Ручные скрипты `scripts/roundtrip.ts` и `scripts/localization-check.ts` оставлены.

### 12.2 Integration (клиент)
- [x] Open -> serialize -> reopen: vitest roundtrip на 5 реальных JSON + E2E Save As download.
- [x] Ошибки невалидного JSON: unit (`openAdventureFromFile`) + Playwright (синтаксис / не-объект, empty state жив).
- [x] Confirm при unsaved changes: Playwright Cancel оставляет файл, Continue открывает другой.
- [x] Fallback сохранения без File System Access API: E2E удаляет picker API; `Save As` качает JSON. Firefox вручную не гоняли.

### 12.3 E2E
- [x] Открытие файла и базовое редактирование: Playwright Chromium, фикстура `minimal.json` (content Value, choice Text, dirty, Save As download).
- [x] Сложный сценарий dicecheck/action/restrictions: Playwright — Dice Check + outcome GoToScene + restriction TimeNow/ActivePartyCount. Overlay Scene Graph + фильтры V2 (`graph-filter.json`); клик по canvas узлов не гоняли.
- [x] Validation/Fix: Playwright, фикстура 10/9 → Unfixable KEY_1 → content; поиск `go` → `3 of 10` → Selected Choice; Fix + Fix All → loc `KEY_1`, Fix All disabled.
- [x] Экспорт локализации: Playwright Generate (не dirty) → Download TSV → Apply (`LOCALIZATION_DEMO_ADV_TITLE`, dirty) → второй Generate 0. Домен: vitest + `scripts/localization-check.ts`.

### 12.4 Regression pack
- [x] Набор реальных adventure JSON в vitest (5 файлов 15.3 + ForestPath для графа/loc/validate).
- [x] Сверка домена с TEA: потерь нет; канонический Save A дополняет худые файлы; loc-счётчики Crossroad/TutorialIntro/ForestPath/EmberWatch/Tavern.

---

## 13) Риски и контроль

### R1. Потеря данных при сохранении
- [x] Контроль: roundtrip-скрипт `TEA/WEB/scripts/roundtrip.ts` (15.3).
- [x] Мера: `Save As` всегда доступен; confirm на unsaved changes.

### R2. Несовместимость JSON
- [x] Контроль: vitest roundtrip на 5 JSON + synthetic golden в `tests/fixtures`.
- [x] Мера: единые правила сериализации/нормализации в `TEA/WEB/src/domain`.

### R3. Различия браузеров по File System Access API
- [~] Контроль: Chromium empty state + editor UI §8; Firefox не прогоняли.
- [x] Мера: обязательный `Save As`, опциональный `Save`.

### R4. Расхождения логики с TEA
- [x] Контроль: матрица паритета (§5.3).
- [x] Мера: сверка 15.3 на реальных сценариях (домен).

### R5. Дублирование доменных правил (C# TEA vs TS Web)
- [x] Контроль: тесты на одинаковых fixtures (реальные JSON + `tests/fixtures`).
- [x] Мера: держать матрицу паритета и обновлять ее при изменениях TEA.

---

## 14) Чеклист приемки

### MVP готов
- [~] Поток "open file -> edit -> save/save as" реализован; roundtrip JSON — 15.3. UI §8 прогнан в Chromium (Crossroad, ForestPath). Граф MVP в коде (домен сверен на Crossroad/ForestPath). Save As в приёмке §8 не кликали. Приёмка полного MVP не закрыта.
- [ ] Все пункты MVP закрыты.
- [x] Страница работает локально и как static web-page (`npm run preview` / `dist/`). Удалённый CDN не выкладывали.
- [x] E2E для одного файла стабильно проходят (Playwright Chromium, 10 тестов). Firefox не в пакете.

### V2 готов
- [~] Validation/Fix/Graph MVP+V2/Localization закрыты. **CDN отложен.**
- [ ] Пользователь может выполнять полный цикл редактирования одного сценария без Unity.

---

## 15) Ближайший actionable backlog

**Модель на старт (15.1–15.2):** Grok 4.6, **High Fast**. Каркас SPA + Open/Save As + простой editor.  
**Модель на приёмку (15.3):** GPT-5.3 Codex или Grok 4.6 **High** — сверка JSON и поведения с TEA, не «дорисовать UI».

### 15.1 Старт
- [x] Зафиксировать frontend-стек и static-сборку. Решение: React + TypeScript + Vite в `TEA/WEB`.
- [x] Поднять каркас SPA (`TEA/WEB`: Vite + React + TS, empty state, toolbar-заглушка).
- [x] Перенести/описать adventure-модель в TypeScript (`src/domain`: enums, types, glossary, normalize).
- [x] Скрипты запуска: `TEA/WEB/check-node.bat`, `TEA/WEB/start-web-tea.bat`, `TEA/WEB/preview-web-tea.bat`.
- Node.js установлен у автора (v24). Dev: `start-web-tea.bat` или `cd TEA/WEB && npm install && npm run dev`. `npm run build` / `dist/` / `preview-web-tea.bat` прогнаны (2026-09-07).

### 15.2 Первая реализация
- [x] Сделать `Open File`.
- [x] Сделать базовый editor (meta + scenes).
- [x] Сделать `Save As` (и `Save`: handle либо fallback в Save As).
- [x] Подключить валидацию: список issues, без Fix и без отдельного окна.
- Граф сцен — не в этом этапе (нужен в MVP позже, для обзора сцен и переходов).

### 15.3 Первая приемка
- [x] Прогнать на 3-5 реальных adventure JSON (Crossroad, EmberWatch, _TutorialIntro, AdventureTavernByMartha, _FirstTutorial). Скрипт `TEA/WEB/scripts/roundtrip.ts`.
- [~] Chromium: empty state (15.3), UI §8 (Crossroad/ForestPath), Fix UI, Playwright §12 (Save As download, TSV, confirm Cancel/Continue). Firefox не автоматизировали.
- [x] Исправить расхождения с TEA: доменных потерь данных нет. Ожидаемые отличия Save A: на «худых» JSON появляются `Disabled` / `IgnoredTags` / `AdventureLinks`. Issues валидации совпадают с правилами TEA (`tutorial` vs `TUTORIAL`, `KEY_1` как loc-ключ).
- [x] Утвердить MVP baseline **15.2** (open → meta/scenes → save, валидация без Fix). Этап 3 UI (§8) закрыт и проверен в Chromium: content/choices/DiceCheck/actions/restrictions. Граф MVP (§9.3) сделан. Граф V2 (фильтры All/Start/Unreachable/Broken) сделан: домен `filterAdventureGraph`, UI `GraphPanel`, Playwright `graph-filter.json`. Fix (§9.2) сделан и проверен в Chromium (фикстура: один Fix + Fix All). Validation UI (§9.1) сделан и проверен в Chromium (поиск / Fixable / Unfixable / клик к scene/content/choice/action). Локализация (§10) сделана и проверена (домен + Chromium Generate/Apply). Local `dist/` (§11.1) собран и проверен через `npm run preview`. Тесты (§12) закрыты: `npm test` = vitest + 10 Playwright Chromium (2026-09-07). **CDN (§11.2) отложен** — не в очереди, пока не будет явной команды и хоста. Плановых этапов дальше нет.
