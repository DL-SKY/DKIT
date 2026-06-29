# Модуль Localization

## Назначение

`Localization` предоставляет рабочую реализацию локализации:
- базовый абстрактный менеджер языков;
- DTO данных локализации;
- конкретный `LocalizationManager` с загрузкой из `Resources`;
- базовый и готовый UI-прокси для `TextMeshProUGUI`.

## Текущее состояние реализации

### Что реализовано

- `LocalizationManagerBase`:
  - хранит текущий `SystemLanguage` (`Language`);
  - хранит загруженные данные (`LocalizationData`);
  - публикует событие `OnChangeLanguage`;
  - умеет `Init()` / `Init(SystemLanguage)`, вызывая `TrySetLanguage(...)`;
  - умеет вернуть строку по ключу через `GetString(...)`.

- `LocalizationTextProxy`:
  - требует `TextMeshProUGUI` через `[RequireComponent]`;
  - хранит ключ и аргументы форматирования (`SetText(key, args...)`);
  - при смене языка повторно применяет текст;
  - при ошибке форматирования пишет warning и использует неформатированную строку.

- `LocalizationText`:
  - конкретный компонент поверх `LocalizationTextProxy`;
  - получает `LocalizationManager` через DI и сразу готов к использованию в UI.

- `LocalizationManager`:
  - получает single-def `LocalizationSettingsDef` из `DefinitionsManager`;
  - поддерживает fallback: язык из state -> системный язык -> язык по умолчанию из настроек;
  - загружает язык из `Resources/Langs/<folder>/Localization`;
  - переносит данные словаря построчно с проверкой дубликатов ключей.

- `LocalizationData`:
  - содержит `Version`, `Description`, `Dictionary<string, string> Locals`.

### Где лежат данные

- `Assets/Modules/Definitions/Resources/Definitions/LocalizationSettings/LocalizationSettings.json`
- `Assets/Modules/Definitions/Resources/Definitions/_ADVENTURES_/LocalizationSettings/LocalizationSettings.json`

Формат:
  - `DefaultLanguage`;
  - `LanguageFolders` (`SystemLanguage -> folder`), например `English -> eng`, `Russian -> rus`.

- `Assets/Modules/Localization/Resources/Langs/eng/Localization.json`
- `Assets/Modules/Localization/Resources/Langs/rus/Localization.json`

Формат файла языка:
- `Version` (int),
- `Description` (string),
- `Locals` (`Dictionary<string, string>`).

## Фактическая логика работы базовых классов

1. `LocalizationInitTask` берет язык из `Adventure State` (`LocalizationStateData.Language`).
2. Если в state `SystemLanguage.Unknown`:
   - берется `Application.systemLanguage`;
   - если язык не поддержан/невалиден, берется `DefaultLanguage` из `LocalizationSettings`.
3. `TrySetLanguage(...)`:
   - валидирует язык через `CheckAvailableLanguage(...)`;
   - загружает `LocalizationData` через `LoadLanguage(...)`;
   - вызывает `OnChangeLanguage`.
4. После успешного применения языка task сохраняет выбранный язык обратно в state через `SetLocalizationLanguageStateAction`.
5. `GetString(key)`:
   - возвращает строку из `_data.Locals`;
   - при отсутствии данных/ключа пишет warning и возвращает `string.Empty`.
6. `LocalizationTextProxy` применяет строку к `TextMeshProUGUI`:
   - без аргументов — напрямую;
   - с аргументами — через `string.Format(...)`;
   - при `FormatException` — warning + fallback на исходную локализованную строку.

## Интеграция со State

В `Modules.State.Scripts.Implementation.Adventure` добавлены:
- `StateDatas/LocalizationStateData` (`SystemLanguage Language`, default `Unknown`);
- `Interfaces/ILocalizationStateDataOwner`;
- поле `Localization` в `StateData`;
- `Actions/SetLocalizationLanguageStateAction`.

`AdventureStateDataFactory` теперь создает `LocalizationStateData` по умолчанию.

## DI и инициализация

- `LocalizationManager` зарегистрирован в DI в обоих `ProjectInstaller`.
- Также зарегистрирована привязка `LocalizationManagerBase -> LocalizationManager`.
- `LocalizationInitTask` выполняет фактическую инициализацию локализации на старте.

