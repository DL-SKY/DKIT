# Web TEA — как запустить

Браузерный редактор приключений (TEA). Только frontend: один JSON-файл, без backend и без доступа к папке Unity-проекта.

Код приложения: `TEA/WEB` в корне репозитория DKIT  
(полный путь на этой машине: `D:\GIT\DKIT\TEA\WEB`).

---

## Нужен ли Node.js

**Да.** Node.js нужен, чтобы:

1. поставить зависимости (`npm install`);
2. запустить локальный сервер разработки (`npm run dev`);
3. собрать статическую папку `dist/` (`npm run build`).

В браузере крутится уже собранный JavaScript. Node.js — это инструмент на компьютере разработчика, не сервер игры.

Нужна версия **Node.js 20 LTS или новее** (вместе с ней ставится `npm`).

---

## 1. Проверить, установлен ли Node.js

Открой **PowerShell** или **cmd** и выполни:

```bat
node -v
npm -v
```

Если видишь номера вроде `v22.x.x` и `10.x.x` — Node.js уже есть, переходи к разделу «Запуск редактора».

Если ошибка вроде `не является внутренней или внешней командой` / `CommandNotFoundException` — Node.js нет в PATH (скорее всего не установлен). Ставь по инструкции ниже.

После установки **закрой и заново открой** терминал (и Cursor), иначе `node` может не находиться.

---

## 2. Установка Node.js (Windows)

Выбери один способ.

### Способ A — официальный установщик (проще всего)

1. Открой https://nodejs.org/
2. Скачай **LTS** (рекомендуемая, не Current).
3. Запусти установщик, везде Next.
4. Оставь галочку **Add to PATH** (по умолчанию включена).
5. Дождись конца установки.
6. Закрой все окна терминала и Cursor.
7. Открой новый PowerShell и снова проверь:

```bat
node -v
npm -v
```

### Способ B — winget (если команда `winget` работает)

В PowerShell **от имени обычного пользователя** (или администратора, если установщик попросит):

```bat
winget install OpenJS.NodeJS.LTS --accept-package-agreements --accept-source-agreements
```

После установки закрой терминал, открой новый и проверь `node -v`.

### Если `node` всё ещё не находится

- Перезапусти Cursor целиком.
- Проверь, что существует `C:\Program Files\nodejs\node.exe`.
- В новом PowerShell:

```powershell
$env:Path -split ';' | Select-String node
```

Должна быть строка с `nodejs`. Если её нет — доустанови Node.js с сайта и не снимай Add to PATH.

---

## 3. Запуск редактора (режим разработки)

Самый простой способ на Windows — двойной клик в папке `TEA\WEB`:

| Файл | Что делает |
|---|---|
| `check-node.bat` | Проверяет, что Node.js 20+ и npm есть в PATH. Если нет — пишет, как поставить. |
| `start-web-tea.bat` | Сначала вызывает проверку Node, при отсутствии `node_modules` делает `npm install`, затем поднимает **dev**-сервер и открывает браузер. **Окно не закрывать**, пока работаешь в редакторе. Стоп: `Ctrl+C`. |
| `preview-web-tea.bat` | Проверка Node, при необходимости `npm install`, если нет `dist/` — `npm run build`, затем **static preview** собранного `dist/` (обычно `http://localhost:4173/`). |

Скрипты **не устанавливают** Node.js сами, только проверяют и запускают проект.

Если удобнее руками в терминале:

1. Открой терминал.
2. Перейди в папку приложения.

**PowerShell:**

```powershell
cd D:\GIT\DKIT\TEA\WEB
```

**Если репозиторий лежит в другом месте** — замени путь, конечная папка всегда `TEA\WEB`.

3. Первый раз (и после обновления `package.json`) поставь пакеты:

```powershell
npm install
```

Дождись окончания без ошибок. Появится папка `TEA\WEB\node_modules` (в git она не коммитится).

4. Запусти dev-сервер:

```powershell
npm run dev
```

5. В выводе Vite будет адрес, обычно:

```text
http://localhost:5173/
```

6. Открой эту ссылку в браузере:
   - **Google Chrome / Edge** (Chromium) — можно `Save` в тот же файл;
   - **Firefox** — `Save` работает как `Save As` (скачивание).

7. Остановить сервер: в том же терминале `Ctrl+C`.

Сервер должен **оставаться запущенным**, пока ты работаешь в браузере.

Тесты (из `TEA\WEB`): `npm run test:unit` — домен; `npx playwright install chromium` один раз, затем `npm test` — unit + E2E Chromium.

---

## 4. Что делать на странице

1. Нажми **Open File** (или **New File**, чтобы создать новый сценарий: имя файла + `start scene id`, дефолт `start`).
2. Для **Open File** выбери один adventure JSON, например из:

`Assets/Modules/Definitions/Resources/Definitions/_ADVENTURES_/Adventures/`

Примеры: `Locations\Crossroad.json`, `Quests\AdventureEmberWatch.json`.

3. Редактируй meta, сцены, content, choices и actions. Обзор переходов: **Scene Graph** (zoom/pan, клик по узлу выбирает сцену; фильтры All / Start / Unreachable / Broken). **Localization**: Generate показывает preview ключей, Apply пишет их в текущий файл (нужен Save / Save As), Download TSV качает `.txt` для Google Sheets. В панели Validation: поиск, фильтры All/Fixable/Unfixable, клик по issue открывает место проблемы. У исправляемых проблем есть **Fix**; **Fix All** применяет все доступные фиксы и снова гоняет валидацию.
4. Сохрани:
   - **Save As** — всегда доступен;
   - **Save** — в тот же файл, если браузер дал доступ (Chromium + https/localhost).

Редактор **не видит** папку проекта сам. Файл нужно выбрать вручную. После Save As положи обновлённый JSON обратно в папку Adventures, если его должен подхватить Unity.

В toolbar смотри режим: `Same file` или `Download only`.

---

## 5. Сборка без постоянного `npm run dev`

Если нужен набор файлов для открытия через любой static-сервер:

```powershell
cd D:\GIT\DKIT\TEA\WEB
npm install
npm run build
```

Результат: папка `TEA\WEB\dist\` (`index.html` + `assets/`). В git она не коммитится.

Просмотр этой сборки локально (тот же static-хост, что и после публикации):

- двойной клик `preview-web-tea.bat`;
- или:

```powershell
npm run preview
```

И снова открой URL из терминала (обычно `http://localhost:4173/`).

Голый двойной клик по `dist\index.html` (`file://`) обычно **хуже**, чем localhost: File System Access API там чаще недоступен, остаётся только выбор файла и download.

### Как опубликовать

Скопируй **содержимое** `dist/` на любой static-хостинг: nginx, IIS, GitHub Pages, Netlify, S3 и т.п. Backend не нужен, API нет. Для `Save` в тот же файл нужен Chromium и https (или localhost); иначе пользуйся Save As.

Перед выкладкой пересобери `npm run build`. Удалённый CDN **отложен**: в репозитории не настроен, сам не выкладывать, пока не будет хоста.

---

## 6. Частые проблемы

| Симптом | Что сделать |
|---|---|
| `node` / `npm` не найдены | Установить Node.js LTS, перезапустить терминал и Cursor |
| `npm install` ругается на сеть | Проверить интернет, прокси, повтор команды |
| Страница не открывается | Убедиться, что `npm run dev` ещё запущен; открыть именно URL из терминала |
| Порт 5173 занят | Vite напишет другой порт — открой его |
| Save не пишет в тот же файл | Это нормально в Firefox и на `file://`. Пользуйся Save As |
| Unity не видит правки | Файл сохранён в Downloads, а не в `Assets/...`. Скопируй JSON в папку Adventures |

---

## 7. Чего редактор не делает

- Нет backend и нет доступа к `Assets/...` сам по себе.
- Нет list/create/rename/delete adventure-файлов в папке проекта (только один текущий файл в сессии).

Подробный план работ: `TEA_browser_editor_план_2026-08-10.md` в этой же папке.
