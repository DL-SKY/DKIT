# Web TEA

Browser frontend for the TEA adventure editor. Single-file mode: open one `.json`, edit, save. No backend.

## Requirements

- Node.js 20+

## Local run

Двойной клик в проводнике:

- `check-node.bat` — проверка, что Node.js 20+ и npm видны в PATH.
- `start-web-tea.bat` — проверка Node, при необходимости `npm install`, затем dev-сервер (окно не закрывать). Браузер откроется на `http://localhost:5173/`.

Сообщения в консоли — **на английском**. В `cmd.exe` русский из UTF-8 стабильно ломается (OEM 866 vs UTF-8). Подробная русская инструкция: этот README и `.cursor/plans/TEA/README.md`.

Вручную в терминале:

```bash
cd TEA/WEB
npm install
npm run dev
```

Open the URL printed by Vite (usually `http://localhost:5173`).

## Tests

```bash
cd TEA/WEB
npm install
npx playwright install chromium
npm test
```

`npm test` runs unit tests (vitest) then Playwright E2E in Chromium. First E2E run needs `npx playwright install chromium`.

| Script | What it runs |
|---|---|
| `npm test` | `vitest run` + `playwright test` |
| `npm run test:unit` | domain / IO / golden JSON only |
| `npm run test:watch` | vitest watch |
| `npm run test:e2e` | Playwright Chromium against `npm run dev` |

Unit tests live in `tests/`. E2E lives in `e2e/` and turns off File System Access so Open/Save use the file input and download fallback. Real adventure JSON comes from `Assets/.../Adventures/`. Firefox is not in the automated pack.

## Static build

```bash
cd TEA/WEB
npm install
npm run build
```

Output is `dist/` (`index.html` + `assets/`). `base` is relative (`./`), so the folder also works from a subpath. `dist/` is gitignored — rebuild before you publish.

Local preview of that build (static host, no backend):

- double-click `preview-web-tea.bat` (builds `dist/` if missing, then `npm run preview`);
- or `npm run preview` and open the URL (usually `http://localhost:4173/`).

Opening `dist/index.html` via `file://` is weaker: File System Access API is usually missing, so only file picker + download remain.

## Publish (any static host)

Copy the **contents** of `dist/` (not the `TEA/WEB` source) to nginx, IIS, GitHub Pages, Netlify, S3, or any other static server. There is no API and no server rewrite is required: one HTML page plus JS/CSS.

Use **https** (or localhost) if you want Chromium `Save` into the same file. On plain `http` remote hosts, `Save` falls back to `Save As` / download.

This repo does not deploy to a CDN by itself. Point your host at a freshly built `dist/`.

## Open / Save

- `Open File` picks one `.json`. Invalid JSON stays on the empty state (or keeps the current file).
- `New File` asks for file name and start scene id, creates a new in-memory adventure (canonical defaults), marks it `Modified`, and suggests the entered file name on Save As.
- `Save As` always works (file picker in Chromium, download in Firefox).
- `Save` writes to the same file when File System Access API is available; otherwise it behaves as `Save As`.
- Toolbar shows `Same file` or `Download only`.
- After open/new: edit adventure meta, scenes, content, choices (Default / DiceCheck), actions, restrictions.
- Validation lists issues. Search and All/Fixable/Unfixable filter the list; click a scene/content/choice/action issue to select that place in the editor. Adventure-level issues have no jump. `Fix` / `Fix All` apply the same auto-fixes as Unity TEA (tags, DiceCheck block, `sceneId` → `SceneId`, and the other fixable cases). Non-fixable issues stay as warnings.
- `Scene Graph` opens a canvas of scene transitions (zoom/pan). Click an existing scene node to select it in the editor. Edges come from Default-choice `GoToScene` / `GoToRandomScene` (and legacy `None`); DiceCheck outcomes are not drawn. Filters All / Start / Unreachable / Broken hide other node statuses; stats still show full Broken / Unreachable counts.
- `Localization` generates keys like Unity TEA, shows a preview, applies them to the current adventure, and downloads a tab-separated `.txt` for Google Sheets. Save / Save As to keep keys in the JSON.
- Unsaved changes ask for confirm on another Open and on tab reload.
