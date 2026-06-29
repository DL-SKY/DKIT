# Модуль Cheats

**Последнее обновление:** 2026-06-29 11:50:00 (+03:00)

## Назначение

`Cheats` — это редакторский модуль (Unity Editor), который добавляет окно `Tools/Cheats` для служебных действий и быстрой диагностики во время разработки.

## Краткая логика работы

1. Unity открывает окно `CheatWindow` через пункт меню `Tools/Cheats`.
2. Окно запрашивает список секций у `CheatSectionsProvider`.
3. Провайдер создает экземпляры секций из заранее зарегистрированных типов, валидирует их и сортирует по `Order`, затем по `Id`.
4. `CheatWindow` рисует toolbar (фильтр + reload) и список секций в scroll view.
5. Каждая секция отображается как foldout; состояние раскрытия хранится в `EditorPrefs`.
6. Рендер содержимого секции делегируется в `DrawContent()` конкретной реализации `CheatSectionBase`.

## Основные классы

- `CheatWindow`  
  Главное окно читов. Отвечает за жизненный цикл окна, фильтрацию, перезагрузку секций и UI-отрисовку foldout-блоков.

- `CheatSectionBase`  
  Базовый абстрактный класс секции. Контракт:
  - `Id` — отображаемый идентификатор секции;
  - `Order` — порядок сортировки;
  - `IsVisible(filter)` — фильтрация по строке поиска;
  - `DrawContent()` — отрисовка и логика секции.

- `CheatSectionsProvider`  
  Реестр секций и фабрика их создания. Содержит список `RegisteredSectionTypes`, создает инстансы через `Activator.CreateInstance`, обрабатывает ошибки и возвращает отсортированный список.

- `SaveCheatSection`  
  Текущая рабочая секция (Implementation). Показывает пути и параметры сохранений на основе `GlobalSettings.json`, вычисляет ожидаемый путь save-файла и дает кнопку открытия директории сохранений.

- `DefinitionsCheatSection`  
  Секция диагностики adventure-definitions. В рантайме показывает загруженные single-def/коллекции из `Adventures.DefinitionsManager`. Если игра не запущена или менеджер не найден — показывает `HelpBox`.

- `LocalizationCheatSection`  
  Секция локализации. В рантайме показывает текущий язык и кнопки переключения, которые генерируются динамически из `LocalizationSettingsDef.LanguageFolders`.  
  По кнопке секция:
  - вызывает `LocalizationManager.TrySetLanguage(...)`;
  - сохраняет язык в state через `SetLocalizationLanguageStateAction` + `AdventureStateLogic.ProcessAction(..., forceBatch: true)`.  
  Если рантайм-объекты недоступны — показывает `HelpBox`.

- `GeneralCheatSection`  
  Пример/шаблон секции (Examples). Используется как образец для новых реализаций.

## Как добавить новый раздел (подменю/подокно) в Cheat-меню

Ниже обязательный минимальный процесс, чтобы новая секция появилась в `Tools/Cheats`:

1. Создать новый класс секции, наследованный от `CheatSectionBase`  
   Рекомендуемый путь: `Assets/Modules/Cheats/Scripts/Editor/Implementation/<Feature>/<FeatureCheatSection>.cs`.

2. Реализовать базовый контракт секции  
   - задать `Id` (имя секции в окне);
   - при необходимости задать `Order` (чем меньше значение, тем выше секция в списке);
   - реализовать `DrawContent()` с нужными контролами и действиями.

3. Зарегистрировать тип в `CheatSectionsProvider`  
   Добавить `typeof(<НовыйКлассСекции>)` в `RegisteredSectionTypes`.  
   Без этого шага секция не будет создана и не отобразится в окне.

4. Открыть/перезагрузить окно `Tools/Cheats`  
   Нажать `Reload` в окне или переоткрыть окно, чтобы перечитать реестр секций.

5. Проверить отображение и фильтр  
   Убедиться, что секция видна по `Id`, корректно раскрывается в foldout и отрабатывает действия из `DrawContent()`.

## Текущие зарегистрированные секции

В `CheatSectionsProvider.RegisteredSectionTypes` сейчас подключены:
- `SaveCheatSection`
- `DefinitionsCheatSection`
- `LocalizationCheatSection`

### Мини-шаблон новой секции

```csharp
using Modules.Cheats.Scripts.Editor.Core;
using UnityEditor;

namespace Modules.Cheats.Scripts.Editor.Implementation.Sample
{
    public sealed class SampleCheatSection : CheatSectionBase
    {
        public override string Id => "Sample";
        public override int Order => 100;

        public override void DrawContent()
        {
            DrawSectionHeader("Sample actions");
            if (GUILayout.Button("Run sample action", GUILayout.Height(24f)))
            {
                UnityEngine.Debug.Log("[SampleCheatSection] Action executed.");
            }
        }
    }
}
```
