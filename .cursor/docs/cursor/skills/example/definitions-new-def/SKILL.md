---
name: definitions-new-def
description: Создает новый тип definitions-данных: C# класс def, JSON контент и регистрацию загрузки в DefinitionsManager проекта или подпроекта. Использовать, когда нужно добавить новый def/definition и подключить его в pipeline загрузки.
disable-model-invocation: true
---

# Definitions New Def

## Цель

Добавить новый тип Definition в проекте DKIT от модели до runtime-загрузки.

## Рабочий процесс

1. Найти целевой контекст:
   - модуль (`Match3` или `Adventures`);
   - файл и структура `DefinitionsManager`;
   - существующие аналоги def-классов и путей JSON.
2. Создать класс def в `Assets/Modules/Definitions/Scripts/Implementation/...`:
   - по умолчанию наследовать от `AbstractDefinition`;
   - использовать публичные поля для JSON-десериализации.
3. Добавить JSON в `Assets/Modules/Definitions/Resources/Definitions/...`:
   - для одиночного дефа: один файл и `LoadSingle<T>()`;
   - для коллекции: папка с файлами и `LoadCollection<T>()`.
4. Подключить загрузку в `DefinitionsManager`:
   - добавить поле хранения данных;
   - добавить метод `LoadXxx`;
   - зарегистрировать метод в `LoadAll()` с учетом зависимостей.
5. Проверить корректность:
   - `Id` формируется из имени файла JSON;
   - все ссылки на другие def валидны;
   - нет конфликтов id в коллекции.

## Контрольный список

- [ ] C#-класс создан и компилируется.
- [ ] JSON-файлы размещены в корректной `Resources`-папке.
- [ ] В `DefinitionsManager` добавлены поле, метод загрузки и вызов в `LoadAll()`.
- [ ] Потребляющий код может получить def без `null`/ошибок lookup.
- [ ] Изменения не затрагивают автогенерируемые файлы.

## Ограничения

- Не создавать, не редактировать и не удалять MessagePack/MpGenerated файлы.
- Не менять файлы в `Assets/Scripts/Meta/SharedLogic/MergeGame/Models/Implementation/`.
- Не придумывать новые соглашения путей, если в модуле уже есть established структура.

## Дополнительно

Если не хватает деталей по шаблонам, открой [reference.md](reference.md).
