# Reference: Definitions New Def

Эта памятка дополняет `SKILL.md` и содержит практические шаблоны.

## A. Шаблон C# класса def

```csharp
using Modules.Definitions.Scripts.Implementation.Defs;

namespace Modules.Definitions.Scripts.Implementation.<TargetNamespace>
{
    public class MyFeatureDef : AbstractDefinition
    {
        public bool Disabled;
        public string Title;
        public string Description;
        public string[] Tags;
    }
}
```

Примечания:
- `Id` обычно не хранится в JSON и задается загрузчиком через имя файла.
- Имена JSON-ключей должны совпадать с именами публичных полей.

## B. Шаблон JSON (single)

```json
{
  "Disabled": false,
  "Title": "Starter Feature",
  "Description": "Base setup for feature.",
  "Tags": ["core", "starter"]
}
```

Пример пути:

`Assets/Modules/Definitions/Resources/Definitions/<Folder>/MyFeature.json`

## C. Шаблон JSON (collection)

Размещайте несколько файлов в папке:

`Assets/Modules/Definitions/Resources/Definitions/<Folder>/`

Важно:
- допускаются подпапки;
- `LoadCollection()` читает рекурсивно;
- id должен быть уникален в рамках всего дерева папки.

## D. Шаблон интеграции в DefinitionsManager

```csharp
public Dictionary<string, MyFeatureDef> MyFeatures;

private IEnumerator LoadMyFeatures()
{
    MyFeatures = _loader.LoadCollection<MyFeatureDef>("Definitions/<Folder>");
    yield break;
}
```

И зарегистрировать в `LoadAll()`:

```csharp
var loadMethods = new Func<IEnumerator>[]
{
    // ...
    LoadMyFeatures,
    // ...
};
```

## E. Проверка после внедрения

1. Сборка проходит без ошибок.
2. JSON корректно десериализуется.
3. Доступ к `MyFeatures[id]` или `TryGetValue` возвращает ожидаемые данные.
4. Порядок загрузки учитывает зависимости на другие def-типы.
5. Нет изменений в автогенерируемых MessagePack артефактах.
