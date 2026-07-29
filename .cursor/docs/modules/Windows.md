# Модуль Windows

**Последнее обновление:** 2026-07-22 17:47:15 (+03:00)

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
| `AdventureMainView` / `AdventureMainViewModel` | `ViewBase` / `ViewModelBase` | Главный экран; `Init()` создаёт и инициализирует Scroll VM |
| `AdventureScrollView` / `AdventureScrollViewModel` | MonoBehaviour sub-view | Скролл контента сцены: подписка на `AdventuresManager`, factory VM, spawn prefab’ов, sequencer |
| Content items | MonoBehaviour + VM | Элементы `SceneContentType` в скролле |

Content item View **не** наследуют `ViewBase`: паттерн как у `AdventureScrollView` — `Init(vm)`, `Subscribe`/`Unsubscribe`. Lifetime VM владеет `AdventureScrollViewModel` (`Dispose` идемпотентен).

База:

- `AdventureContentViewModelBase` — `Init(SceneContentData)`, `Data`, `IsContentReady` / `ContentReady`, `Dispose` / `DisposeImplementation`
- `AdventureContentViewBase` (non-generic) — prefab refs, `Animator`, `Init(AdventureContentViewModelBase)`
- `AdventureContentViewBase<TViewModel>` — typed `_viewModel`, `Subscribe` / `InitImplementation`

VM создаются через DiContainer (`Instantiate` + `Init(data)`), зависимости — `[Inject]`.

### Scroll ↔ AdventuresManager

1. `AdventureMainViewModel.Init()` → `ViewModelFactory.Create<AdventureScrollViewModel>()` → `Scroll.Init()`.
2. `AdventureScrollViewModel` подписан на `AdventuresManager.ChangedContent`, читает `GetCurrentContent()`.
3. На `ChangedContent` / стартовый `Init`: `AppendContent` — factory новых VM по `SceneContentType` **в конец** списка → `ON_APPEND_CONTENT`. Уже показанные плашки не трогаются.
4. Очистка только явно: `ClearContent()` → `ON_CLEAR_CONTENT` (View уничтожает children) → dispose content VM. Также из `Dispose`.
5. `AdventureScrollView` держит prefab’ы Text / Image / Splitter / Item (`AdventureContentViewBase`), `_contentRoot`; по `ON_APPEND_CONTENT` спавнит только ещё не созданные item’ы и продолжает sequencer.
6. Sequencer: spawn pending → ждать `IsContentReady` → `Animator.Play` → `Completed` → следующий pending.
7. `SkipAllShowAnimation()` (на VM или View): `Skip` текущего аниматора + остальные pending сразу с `Skip`. Подписка на tap/клик — снаружи.

`Initializer` после загрузки вызывает `adventureMainViewModel.Init()` перед `OpenView`.

### Content items (View + VM)

Три View на типы контента:

| View | `SceneContentType` | VM |
|---|---|---|
| `AdventureTextContentView` | `Text` | `AdventureTextContentViewModel` |
| `AdventureImageContentView` | `Image`, `RandomImage`, `Slideshow`, `Splitter` | `AdventureImageContentViewModel`, `AdventureRandomImageContentViewModel`, `AdventureSlideshowContentViewModel`, `AdventureSplitterContentViewModel` |
| `AdventureItemContentView` | `Item` | `AdventureItemContentViewModel` (`DefinitionsManager` → `ItemDef` по `Value`) |

Image-группа:

- VM держит только **path/URL** (`CurrentPath`, `ON_CHANGE_PATH`), без `Sprite`.
- View прокидывает path в `CachedPathImage`.
- `Splitter` — декоративная картинка-разделитель (тот же пайплайн, что `Image`).
- `Slideshow` — цикл `Values` раз в **1 с** через подписку VM на `Updater` (не `Update` во View).

### Анимация появления (`IContentAnimator`)

Папка: `.../Scroll/Items/Animation/`

Контракт:

- `Play()` / `Skip()` / `IsPlaying` / `event Completed` (один раз — естественный конец или Skip).
- Оркестрация последовательности и global skip — в `AdventureScrollView` (`SkipAllShowAnimation`).

Реализации:

- `FadeInContentAnimator` — `CanvasGroup.alpha` 0→1 за `_duration`.
- `TypewriterContentAnimator` — TMP `maxVisibleCharacters`, скорость `_charsPerSecond`; RTF-теги не считаются (`textInfo.characterCount` после `ForceMeshUpdate`), `\n` учитывается.

Ожидаемая связка на префабах: Text + Typewriter; Image/Item + FadeIn.

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
