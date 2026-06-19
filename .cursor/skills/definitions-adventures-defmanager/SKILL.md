---
name: definitions-adventures-defmanager
description: Добавляет новый тип Definition только для adventures-подсистемы DKIT (класс, JSON и загрузка в adventures DefinitionsManager). Использовать только при явном вызове этого skill оператором/программистом/пользователем.
disable-model-invocation: true
---

# Definitions Adventures DefManager

## Назначение

Skill для добавления нового adventure-дефа в `Modules.Definitions.Scripts.Implementation.Adventures`:
- создание C#-класса;
- создание JSON в `_ADVENTURES_`;
- подключение загрузки в adventures `DefinitionsManager`.

## Критичное правило запуска

Выполняй этот Skill **только** при явном указании пользователя (оператора/программиста).

## Обязательное уточнение перед работой

Если при запуске не указан формат хранения в менеджере, **сначала уточни в чате**:
- `single` (одиночный def через `LoadSingle<T>()`);
- `dictionary` (коллекция через `LoadCollection<T>()` и `Dictionary<string, T>`).

До получения ответа в файлы ничего не записывать.

## Область применения (строго adventures)

- C#-классы: `Assets/Modules/Definitions/Scripts/Implementation/Adventures/`
- JSON: `Assets/Modules/Definitions/Resources/Definitions/_ADVENTURES_/`
- Менеджер: adventures-реализация `DefinitionsManager` в модуле Definitions.

Если задача относится к Match3 или другому подпроекту, остановись и попроси пользователя запустить более подходящий Skill.

## Workflow

1. Определи целевой тип adventure-дефа и ближайший существующий аналог (`AdventureDef`, `ClassDef`, `AncestryDef`, `FeatDef`, `ItemDef`, `SpellDef`).
2. Создай новый C#-класс:
   - по умолчанию наследуй от `AbstractDefinition`;
   - исключение: использовать другую базу только если это явно следует из контракта adventures.
3. Создай JSON-файл(ы) в `_ADVENTURES_`:
   - `single`: конкретный файл под `LoadSingle<T>()`;
   - `dictionary`: папка/подпапки под `LoadCollection<T>()` (рекурсивная загрузка).
4. Обнови adventures `DefinitionsManager`:
   - поле хранения (`T` или `Dictionary<string, T>`);
   - метод загрузки;
   - регистрация в `LoadAll()` в правильном порядке зависимостей.
5. Выполни проверки:
   - `Id` берется из имени JSON-файла;
   - JSON-ключи совпадают с именами публичных полей;
   - для коллекций нет конфликтов id в пределах дерева папки.

## Ограничения проекта

- Не создавать/редактировать/удалять `MpGenerated` и MessagePack-generated файлы.
- Не изменять файлы в `Assets/Scripts/Meta/SharedLogic/MergeGame/Models/Implementation/`.
- Не создавать и не редактировать Unity `.meta` файлы.

## Формат отчета после выполнения

После выполнения сообщи:
- выбранный формат (`single` или `dictionary`);
- список измененных/добавленных файлов;
- где подключен новый adventure-def в `DefinitionsManager`.
