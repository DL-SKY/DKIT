# Модуль Localization

## Назначение

`Localization` предоставляет базовые абстракции для смены языка и выдачи локализованных строк, а также UI-прокси для автоматического обновления текста при смене языка.

## Краткая логика работы

1. Реализация менеджера на базе `LocalizationManagerBase` инициализируется через `Init(...)`.
2. Менеджер выбирает язык (`GetCurrentLanguage` / `TrySetLanguage`) и загружает `LocalizationData`.
3. UI-компонент на базе `LocalizationTextProxy` хранит `key` (+ optional args), получает строку через `GetString`.
4. При `OnChangeLanguage` прокси повторно применяет перевод к `TextMeshProUGUI`.
5. Если ключ не найден или формат плейсхолдеров некорректен, модуль пишет warning и выводит безопасное значение.

## Основные классы

- `LocalizationManagerBase`  
  Базовый менеджер языка: хранит активный `SystemLanguage`, загруженные данные и событие `OnChangeLanguage`.

- `LocalizationData`  
  DTO словаря локализации (`Version`, `Description`, `Dictionary<string, string> Locals`).

- `LocalizationTextProxy`  
  Базовый UI-прокси для `TextMeshProUGUI` с автоперерисовкой при смене языка и поддержкой `string.Format`.

## Как добавить новую локализацию (язык)

1. Создать/обновить конкретную реализацию менеджера, наследованную от `LocalizationManagerBase`.
2. Реализовать обязательные методы:
   - `GetCurrentLanguage()`;
   - `CheckAvailableLanguage(SystemLanguage language)`;
   - `LoadLanguage(SystemLanguage language)` (заполнение `LocalizationData`).
3. Добавить словарь ключей для нового языка в источник данных (JSON/definitions/ресурс проекта).
4. Обновить `CheckAvailableLanguage`, чтобы новый язык считался поддерживаемым.
5. Зарегистрировать менеджер в DI (в `ProjectInstaller` сейчас отмечен TODO для localization).
6. Проверить смену языка и корректность ключей на экранах с локализуемым UI.

## Как добавить новый локализуемый UI-текст

1. Создать конкретный компонент, наследованный от `LocalizationTextProxy`, и в `Init()` получить ссылку на менеджер локализации.
2. Повесить компонент на объект с `TextMeshProUGUI`.
3. Вызывать `SetText("<KEY>", args...)` при инициализации/обновлении UI.
4. Добавить ключ `<KEY>` во все поддерживаемые языки.
5. Проверить:
   - текст меняется при `TrySetLanguage(...)`;
   - формат с аргументами не вызывает `FormatException`.

