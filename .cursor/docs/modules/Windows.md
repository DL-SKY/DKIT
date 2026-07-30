# Модуль Windows

**Последнее обновление:** 2026-07-30 12:37:00 (+03:00)

## Назначение

`Windows` реализует базовую UI-архитектуру окон (View/ViewModel), управление стеком открытых окон, сортировкой и закрытием по `Esc`, а также runtime UI приключений (Adventure Main / Scroll / content items) и общий сервис загрузки картинок по path/URL.

## Краткая логика работы

1. Клиентский код создает VM через `ViewModelFactory`.
2. `WindowsManager.OpenView<V, M>(path, model)` грузит prefab из `Resources`, инстанцирует `View`, инициализирует через `Init(model)`.
3. View получает уникальный `Handle`, подписывается на события VM и отображается.
4. `WindowsManager` хранит открытые окна в словаре и историю (если `Options.HideInHistory == false`).
5. По `Esc` менеджер пытается закрыть последнее окно в истории, если `Options.CanCloseOnEsc == true`.
6. При закрытии `View` уничтожается, VM вызывает `Dispose()`, менеджер очищает ссылки и историю.

## Основные классы

- `WindowsManager`  
  Открытие/закрытие окон, история, сортировка, обработка `Esc`.

- `ViewBase<TViewModel>`  
  Базовый класс View: `Init`, `Subscribe/Unsubscribe`, `Show/Hide`, `SetSortingOrder`, жизненный цикл destroy.

- `ViewModelBase`  
  Базовый VM: события `OnChange`/`OnChangeCustom`, `Options`, хранение `ViewHandle`, `Dispose()`.

- `IView`, `IViewModel`  
  Контракты слоя окон.

- `Options`, `SortingOrderLayer`  
  Настройки поведения окна (закрытие по Esc, скрытие из истории, слой сортировки).

- Примеры реализаций: `MainLoadView`/`MainLoadViewModel`, `DefaultMatch3View`/`DefaultMatch3ViewModel`, `AdventureMainView`/`AdventureMainViewModel`.

- Компоненты: `ProgressBar`, `SafeAreaRect`, `CachedPathImage`.

- Сервисы: `IImageCache` / `CachedPathImageService` (Zenject singleton).

## Adventure runtime UI

Корневая папка скриптов:

`Assets/Modules/Windows/Scripts/Implementation/Adventure/`

Prefab главного окна: `Resources/Prefabs/Views/Adventure/AdventureMainView`.

### Слои

| Слой | Тип | Назначение |
|---|---|---|
| `AdventureMainView` / `AdventureMainViewModel` | `ViewBase` / `ViewModelBase` | Главный экран; `Init()` создаёт и инициализирует Scroll / TopPanel / BottomPanel VM |
| `AdventureScrollView` / `AdventureScrollViewModel` | MonoBehaviour sub-view | Скролл контента сцены + плашки choices: подписка на `AdventuresManager`, factory VM, spawn prefab’ов, sequencer |
| `AdventureTopPanelView` / `AdventureTopPanelViewModel` | MonoBehaviour sub-view | Верхняя панель (character / abilities / menu); `Init` создаёт Parameter VM на каждый слот `_parameterViews` |
| `AdventureParameterView` / `AdventureParameterViewModel` | MonoBehaviour sub-view | Слот параметра в TopPanel; View вызывает `VM.Init(_parameterName)` |
| `AdventureBottomPanelView` / `AdventureBottomPanelViewModel` | MonoBehaviour sub-view | Нижняя панель; каркас `Init` / `Subscribe` |
| Content items | MonoBehaviour + VM | Элементы `SceneContentType` в скролле |
| Choice items | MonoBehaviour + VM | Плашки `ChoiceData` внизу скролла |

Sub-views (Scroll / TopPanel / BottomPanel / Parameter) и Content / Choice item View **не** наследуют `ViewBase`: паттерн `Init(vm)`, `Subscribe`/`Unsubscribe`. Lifetime panel VM владеет `AdventureMainViewModel`; lifetime parameter VM — `AdventureTopPanelViewModel`; lifetime content/choice VM — `AdventureScrollViewModel` (`Dispose` идемпотентен).

База:

- `AdventureContentViewModelBase` — `Init(SceneContentData)`, `Data`, `IsContentReady` / `ContentReady`, `Dispose` / `DisposeImplementation`
- `AdventureContentViewBase` (non-generic) — prefab refs, `Animator`, `Init(AdventureContentViewModelBase)`
- `AdventureContentViewBase<TViewModel>` — typed `_viewModel`, `Subscribe` / `InitImplementation`
- `AdventureChoiceViewModelBase` — `Init(ChoiceData)`; резолв `MainIcon` / `Text` / `Description` / `DescriptionParam` / `DescriptionIcon` из `VisualOptions` + `VisualSettingsDef` / стейта персонажа; `Select()`, `Dispose`
- `AdventureChoiceViewBase` / `AdventureChoiceViewBase<TViewModel>` — тот же паттерн, что у content

VM создаются через DiContainer (`Instantiate` + `Init(data)`), зависимости — `[Inject]`.

### Scroll ↔ AdventuresManager

1. `AdventureMainViewModel.Init()` → factory + `Init()` для `Scroll`, `TopPanel`, `BottomPanel`; `AdventureMainView` прокидывает VM в соответствующие sub-view.
2. `AdventureScrollViewModel` подписан на `AdventuresManager.ChangedContent` и `ChangedChoices`.
3. На `ChangedContent` / стартовый `Init`: `AppendContent` — factory новых content VM по `SceneContentType` **в конец** списка → `ON_APPEND_CONTENT`. Уже показанные content-плашки не трогаются; **choice-плашки уничтожаются** и появятся снова после sequencer.
4. На `ChangedChoices` / стартовый `Init`: `RebuildChoices` — `ON_CLEAR_CHOICES` (View уничтожает choice panels) → dispose старых choice VM → factory новых из `GetCurrentChoices()` → `ON_REFRESH_CHOICES`.
5. Очистка только явно: `ClearContent()` → `ON_CLEAR_CONTENT` (View уничтожает content **и** choice children) → dispose content + choice VM. Также из `Dispose`.
6. `AdventureScrollView` держит prefab’ы Text / Image / Splitter / Item / Choice, `_contentRoot`.
7. Sequencer контента: spawn pending → ждать `IsContentReady` → `Animator.Play` → `Completed` → следующий pending. После последнего контента (или если pending нет) — `PresentChoices`: все плашки создаются сразу, у каждой свой `Animator.Play` **параллельно**.
8. `ON_REFRESH_CHOICES` во время sequencer — откладывает показ до конца контента; если sequencer не бежит — сразу parallel present (после `ON_CLEAR_CHOICES`).
9. `SkipAllShowAnimation()`: `Skip` текущего content-аниматора + остальные pending content сразу с `Skip` + choice-плашки появляются с `Skip` (без анимации). Триггер: `ScreenInputService.OnInput` (`ScreenInputType.PointerDown`) — любой pointer down по экрану.

`Initializer` после загрузки вызывает `adventureMainViewModel.Init()` перед `OpenView`.

### Content items (View + VM)

Три View на типы контента:

| View | `SceneContentType` | VM |
|---|---|---|
| `AdventureTextContentView` | `Text` | `AdventureTextContentViewModel` |
| `AdventureImageContentView` | `Image`, `RandomImage`, `Slideshow` | `AdventureImageContentViewModel`, `AdventureRandomImageContentViewModel`, `AdventureSlideshowContentViewModel` |
| `AdventureSplitterContentView` | `Splitter` | `AdventureSplitterContentViewModel` (статика в prefab, без path) |
| `AdventureItemContentView` | `Item` | `AdventureItemContentViewModel` (`DefinitionsManager` → `ItemDef` по `Value`) |

### Choice items (View + VM)

| View | Данные | VM |
|---|---|---|
| `AdventureChoiceView` | `ChoiceData` (`Default` / `DiceCheck`) | `AdventureChoiceViewModel` |

- Prefab: `ChoiceContentView` (слот `_choiceContentPrefab` на `AdventureScrollView`) + `FadeInContentAnimator`.
- UI: `Text` / `Description` (+ `DescriptionParam`) / иконки / `Button` → `Select()`.
- `ChoiceType.Default`: прогон `Actions` через `ChoiceActionExecutorFactory`.
- `ChoiceType.DiceCheck`: runtime outcome пока stub (warning log).

#### Резолв полей в `AdventureChoiceViewModelBase`

`Init(ChoiceData)` выставляет свойства через приватные `Get*()` (`DefinitionsManager`, `AdventureStateManager` — `[Inject]`):

| Свойство | Логика |
|---|---|
| `MainIcon` | `VisualOptions.MainIcon`, если не пусто; иначе константа по `ChoiceType`: `DEFAULT_TYPE_MAIN_ICON` (`Adventures/Icons/click`) / `DICE_CHECK_TYPE_MAIN_ICON` (`Adventures/Icons/dice_20`) |
| `Text` | `ChoiceData.Text` |
| `Description` | если `VisualOptions.ParameterOverrideDescription` не пусто — ключ локализации из `VisualSettingsDef.ParameterLocalizations` по `{param}.TextParams`; иначе `ChoiceData.Description` |
| `DescriptionParam` | при parameter override — значение параметра активного персонажа через `CharacterParametersProxy.GetTotalValue` (формат `"+N"` / `"0"` / `"-N"`); иначе `""` |
| `DescriptionIcon` | если parameter override не пусто — путь из `VisualSettingsDef.ParameterIcons` по ключу параметра; иначе `VisualOptions.DescriptionIcon` |
| `EnabledDescription` | `!string.IsNullOrEmpty(Description)` |

`ParameterOverrideDescription` — **id параметра** (например `"Thievery"`, `"STR"`), не локализуемый текст. View: `_localDescription.SetText(Description, DescriptionParam)`.

Image-группа:

- VM держит только **path/URL** (`CurrentPath`, `ON_CHANGE_PATH`), без `Sprite`.
- View прокидывает path в `CachedPathImage`.
- `Slideshow` — цикл `Values` раз в **1 с** через подписку VM на `Updater` (не `Update` во View).
- Skip show-анимаций скролла — через project-wide `ScreenInputService` (`Modules.Utils.Scripts.Input`, бинд в `ProjectInstaller`).
- `Splitter` — отдельный View/VM; картинка статична в prefab, анимация через `FadeInContentAnimator` + `CanvasGroup`, без path.

### Анимация появления (`IContentAnimator`)

Папка: `.../Scroll/Items/Animation/`

Контракт:

- `Play()` / `Skip()` / `IsPlaying` / `event Completed` (один раз — естественный конец или Skip).
- Оркестрация последовательности контента, parallel choices и global skip — в `AdventureScrollView` (`SkipAllShowAnimation`).

Реализации:

- `FadeInContentAnimator` — `CanvasGroup.alpha` 0→1 за `_duration`.
- `TypewriterContentAnimator` — TMP `maxVisibleCharacters`, скорость `_charsPerSecond`; RTF-теги не считаются (`textInfo.characterCount` после `ForceMeshUpdate`), `\n` учитывается.

Ожидаемая связка на префабах: Text + Typewriter; Image/Item/Splitter/Choice + FadeIn.

### `CachedPathImage` + `IImageCache`

- UI-компонент: `Modules.Windows.Scripts.Components.CachedPathImage` (`SetPath`, placeholder/throbber).
- Сервис: `Modules.Windows.Scripts.Services.IImageCache` / `CachedPathImageService`.
- Bind: Adventure и Match3 `ProjectInstaller` → `IImageCache` → `CachedPathImageService` AsSingle.
- Загрузки через `CoroutineHolder`.
- Локальный path → `Resources.Load`.
- URL → memory cache → disk (`persistentDataPath/CachedPathImages/<sha256>`) → сеть.
- Prefetch: `EnsureRemoteLoading(url)` — очередь, **лимит 5**.
- Отображение UI: `EnsureRemoteLoading(url, prioritize: true)` — старт сразу, лимит игнорируется; если URL был в очереди — promote.
- `ClearMemoryCache()` — чистит memory (диск остаётся).
- Prefab View через `Resources.Instantiate` может резолвить сервис через `ProjectContext`, если Zenject inject не сработал.

## Как добавить новое окно (View + ViewModel)

1. Создать VM, наследованный от `ViewModelBase`:
   - добавить состояние и команды;
   - при необходимости — метод `Init(...)` с входными параметрами (вызывать после `ViewModelFactory.Create`, до `OpenView`);
   - реализовать `Dispose()` (отписки и cleanup).
2. Создать View, наследованный от `ViewBase<YourViewModel>`:
   - реализовать `Subscribe`, `Unsubscribe`, `InitImplementation`;
   - при необходимости переопределить `Show`/`Hide`.
3. Подготовить prefab окна и положить его под `Resources` (путь использовать в `Path` константе View).
4. Открывать окно через `WindowsManager.OpenView<YourView, YourViewModel>(YourView.Path, vm)`.
5. Для конфигурации поведения окна переопределить `CreateOptions()` в VM и вернуть нужный `Options`.
6. Проверить:
   - корректное открытие/закрытие;
   - реакцию UI на `OnChange`/`OnChangeCustom`;
   - очистку подписок после уничтожения окна.
