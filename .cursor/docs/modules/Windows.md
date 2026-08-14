# Модуль Windows

**Последнее обновление:** 2026-08-14 09:28:00 (+03:00)

## Назначение

`Windows` реализует базовую UI-архитектуру окон (View/ViewModel), управление стеком открытых окон, сортировкой и закрытием по `Esc`, очередь toast-подсказок (`IHintManager`), runtime UI приключений (Adventure Main / Scroll / content items) и общий сервис загрузки картинок по path/URL.

## Краткая логика работы

1. Клиентский код создает VM через `ViewModelFactory`.
2. `WindowsManager.OpenView<V, M>(path, model)` грузит prefab из `Resources`, инстанцирует `View`, инициализирует через `Init(model)`.
3. View получает уникальный `Handle`, подписывается на события VM и отображается.
4. `WindowsManager` хранит открытые окна в словаре и историю (если `Options.HideInHistory == false`).
5. По `Esc` менеджер пытается закрыть последнее окно в истории, если `Options.CanCloseOnEsc == true`.
6. При закрытии `View` уничтожается, VM вызывает `Dispose()`, менеджер очищает ссылки и историю.
7. Toast-подсказки идут отдельно через `IHintManager` (очередь VM, одно видимое окно на `SortingOrderLayer.HINT`) — см. секцию **Hints** ниже.

## Основные классы

- `WindowsManager`  
  Открытие/закрытие окон, история, сортировка, обработка `Esc`. Prefab: `Resources/Prefabs/Main/WindowsManager`.

- `IHintManager` / `HintManager`  
  Очередь toast-подсказок; реализация — MonoBehaviour на том же prefab, что и `WindowsManager`. Подробнее: секция **Hints** ниже.

- `ViewBase<TViewModel>`  
  Базовый класс View: `Init`, `Subscribe/Unsubscribe`, `Show/Hide`, `SetSortingOrder`, жизненный цикл destroy.

- `ViewModelBase`  
  Базовый VM: события `OnChange`/`OnChangeCustom`, `Options`, хранение `ViewHandle`, `Dispose()`.

- `IView`, `IViewModel`  
  Контракты слоя окон.

- `Options`, `SortingOrderLayer`  
  Настройки поведения окна (закрытие по Esc, скрытие из истории, слой сортировки). Слои: `COMMON` (100), `DIALOGUE` (1_000), `HINT` (10_000), `PRELOADER` (20_000), `DEBUG` (32_000).

- Примеры реализаций: `MainLoadView`/`MainLoadViewModel`, `DefaultMatch3View`/`DefaultMatch3ViewModel`, `AdventureMainView`/`AdventureMainViewModel`, `HintView`/`HintViewModel`, `CreateCharacterView`/`CreateCharacterViewModel`.

- Компоненты: `ProgressBar`, `SafeAreaRect`, `CachedPathImage`.

- Сервисы: `IImageCache` / `CachedPathImageService` (Zenject singleton).

## Hints (`IHintManager`)

Папка: `Assets/Modules/Windows/Scripts/Implementation/Hints/`  
Менеджер: `Assets/Modules/Windows/Scripts/Managers/HintManager.cs`  
Prefab окна: `Resources/Prefabs/Views/Hints/HintView` (`HintView.Path`).

### Назначение

Короткоживущие toast-окна (подложка + локализованный текст). Высота панели зависит от объёма текста (VerticalLayoutGroup + ContentSizeFitter). В один момент видна только одна подсказка; остальные ждут в очереди VM.

### API

| Метод | Поведение |
|---|---|
| `Show(string localizationKey)` | В очередь; hold = `_defaultDurationSeconds` на `HintManager` (по умолчанию 3 с, в инспекторе prefab) |
| `Show(string localizationKey, float durationSeconds)` | В очередь с явным hold после fade-in |
| `Clear()` | Dispose всех ожидающих VM + закрытие текущей подсказки **без** показа следующих |

Потребители инжектят интерфейс: `[Inject] IHintManager _hints`.

`durationSeconds` — время **Hold** (полностью видимый toast), не включая fade-in/out и ожидание в очереди. Минимум 1 с (`Mathf.Max(1, durationSeconds)` в `Init`).

### Разделение View / VM

Анимация и таймер жизни живут в `HintViewModel` (`Updater`, не `Update` на View и не корутины).

`HintView` — тонкий биндинг:

- `Show()` / `Hide()` передают длительности fade из prefab (`_showAnimationSeconds` / `_hideAnimationSeconds`, по умолчанию 0.25 с)
- `OnChange` пишет `CanvasGroup.alpha` из `HintViewModel.Alpha`
- `HideFinished` → `base.Hide()` (`Destroy`)
- на `Closed` View **не** подписан

`HintView.Hide()` не уничтожает окно сразу (в отличие от дефолта `ViewBase.Hide`). Destroy только после окончания hide-анимации.

### Состояния (`HintViewStateType`)

`None` → `Show` (fade к 1) → `Hold` (тикает жизнь) → `Hide` (fade к 0) → `None`.

- Fade идёт от **текущего** `Alpha` к цели (Hide во время Show не прыгает к 1).
- `Hide()` идемпотентен: повторный вызов в состоянии `Hide` — no-op.
- Шаг часов клампится до `MAX_DELTA_TIME` (`1/30` с), чтобы hitch-кадр (`Time.maximumDeltaTime` ≈ 0.33 с) не съедал fade 0.25 с.

### События

| Событие | Кто шлёт | Кто слушает | Когда |
|---|---|---|---|
| `HideFinished` | VM, один раз, после fade-out | `HintView` | пора `Destroy` |
| `Closed` | VM из `Dispose`, один раз, только если `ViewHandle != 0` | `HintManager` | окно реально открывалось и уничтожено; очередь может идти дальше |

### Поток

1. `IHintManager.Show` → `ViewModelFactory.Create<HintViewModel>()` → `Init(key, holdDuration)` → enqueue. VM в `None`, жизнь ещё не тикает.
2. Если активной нет — `WindowsManager.OpenView<HintView, HintViewModel>(...)`.
3. `HintView.Show` → `HintViewModel.Show(showSeconds)` → состояние `Show`, fade-in.
4. Fade-in закончился → `Hold`. Тикает `_lifeTime`. Затем `RequestClose()` → `WindowsManager.CloseView` → `HintView.Hide` → `HintViewModel.Hide(hideSeconds)` → `Hide`.
5. Fade-out закончился → `HideFinished` → `Destroy` → `Dispose` → `Closed`.
6. `HintManager` снимает active и открывает следующую из очереди (если не было `Clear`).

`Clear` ставит `_suppressAdvanceOnClose`, Dispose очереди без показа, текущую закрывает через `RequestClose` (hide доигрывается). После `Closed` очередь не продолжается.

### Options окна

`HintViewModel.CreateOptions()`:

- `CanCloseOnEsc = false`
- `HideInHistory = true` (Esc / history `WindowsManager` не затрагивают toast)
- `SortingOrderLayer.HINT`

Клики не перехватывает: `CanvasGroup.blocksRaycasts = false`, `interactable = false`.

### Локализация

View вызывает `_text.SetText(localizationKey)` (`LocalizationText`); ключ хранится в VM, резолв строки — на стороне View (как в остальных окнах модуля).

### DI и prefab

- Компонент `HintManager` (или другая реализация `IHintManager`) располагается **на том же GameObject/prefab**, что и `WindowsManager`.
- Adventure и Match3 `ProjectInstaller`:  
  `Container.Bind<IHintManager>().FromMethod(...)` → `windowsManager.GetComponent<IHintManager>()`.  
  Если компонента нет — исключение с подсказкой про меню Wire/Build.
- Zenject инжектит зависимости в sibling-компоненты при `FromComponentInNewPrefab` для `WindowsManager`.

### Editor

`Tools/Cursor/Build Hint Prefabs` (`HintPrefabBuilder`):

- собирает `HintView.prefab` из `TemplateView`;
- вешает `HintManager` на `WindowsManager.prefab`.

Отдельно: `Tools/Cursor/Wire HintManager On WindowsManager`.

На компоненте `HintManager` в Play Mode: контекстное меню **Hints/Show Test Hint** (`HINT_TEST`). Если ключа нет в локализации, на экране покажется сам ключ.

`.meta` не создаёт и не правит — после импорта скриптов в Unity.

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

## CreateCharacterView (создание персонажа)

Папка: `Assets/Modules/Windows/Scripts/Implementation/Adventure/Characters/CreateCharacter/`  
Prefab: `Resources/Prefabs/Views/Adventure/Characters/CreateCharacterView/CreateCharacterView`  
DTO черновика: `CreateCharacterRequestData` ([State.md](State.md))  
Запись в черновик: `CreateCharacterRequestApplicator` ([State.md](State.md))  
Накат черт в производное: `CharacterRequestData.ApplyFeat` ([Feats.md](Feats.md), [CharacterCreationDefs.md](CharacterCreationDefs.md))

Открытие: `OpenWindowChoiceActionExecutor` (`Glossary.Windows.CREATE_CHARACTER`) → `ViewModelFactory.Create<CreateCharacterViewModel>()` → `Init(addToActiveParty)` → `WindowsManager.OpenView`.

Окно в работе: View/VM и prefab ещё неполные; контракт ниже — целевой, его держать при дописывании экрана и подокон.

### Слои данных

Два слоя, не смешивать:

| Слой | Где | Роль |
|---|---|---|
| Черновик | `CreateCharacterViewModel` владеет `CreateCharacterRequestData _request` | единственный source of truth до `CreateCharacterStateAction` |
| Презентация | публичные свойства VM (`Name`, `Avatar`, `HitPoints`, `Ancestry`, …) | снимок для View |

`CreateCharacterStateAction` **не** считает статы: персистит то, что уже лежит в `_request`. Пересчёт производного — ответственность UI-потока **до** Create.

### Триггеры vs производное

**Триггеры** (выбор игрока): `Ancestry`, `Class`, `Background`, `Name` / `Avatar` / `Gender`, ручные пики (свободные boosts, skills, option-feat).

**Производное** (собирается из триггеров): `CharacterData.Parameters`, `EquippedItems`, `Spells`, `StatusEffects`. Экранные HP / Level / формулы — через `CreatedCharacterParametersProxy` по этому словарю, не отдельное хранилище.

### Главный экран — зеркало, не пересчётчик

`CreateCharacterViewModel` в `Init` создаёт `_request`, инстанс `CreateCharacterRequestApplicator` и **подписывается** на `CreateCharacterRequestData.OnUpdate`.

Аппликатор передаётся в VM подокон, которые меняют персонажа. Подокна **не** пишут в `_request` напрямую.

Обработчик `OnUpdate`:

1. Проецирует `_request` в свойства VM (`UpdateCharacterRequestData`).
2. Шлёт `SendOnChange(ON_CHANGE_CHARACTER)`.
3. Пересчитывает `CanCreate` и при смене шлёт `ON_CHANGE_CAN_CREATE`.

В этом обработчике **нет** `ApplyFeat`, очистки `Parameters`, rebuild инвентаря. Главный VM не знает, какой триггер изменился — он читает уже согласованный черновик.

View на теги: `ON_CHANGE_CHARACTER` → панели персонажа; `ON_CHANGE_CAN_CREATE` → active/disabled Create. Теги раздельные: кнопка Create не обязана перерисовывать все панели.

`Init` VM вызывается **до** `OpenView` / `Subscribe` View. Первый `RebuildDerived(notify: false)` и проекция свойств — без расчёта на `SendOnChange` (событие в этот момент никто не слушает). `Dispose` — отписка от `OnUpdate` и `Applicator.Dispose()`.

### Аппликатор — единственный писатель

`CreateCharacterRequestApplicator` (не статика; lifetime = сессия создания):

- `SetName` / `SetAvatar` / `SetGender` — запись триггера, без rebuild, один `NotifyUpdated`.
- `SetAncestry` / `SetClass` / `SetBackground` — запись триггера → wipe + reapply всех валидных Features / starting equipment → **один** `NotifyUpdated`.
- `TryApplyAbilityBoost` / `TryRefundAbilityBoost` / `SetSkillProficiencyRank` / `ApplyOptionFeat` — правка уже собранного `CharacterData`, без полного wipe, один `NotifyUpdated`.

`CreateCharacterRequestData.NotifyUpdated()` поднимает `OnUpdate`. Конвенция: зовёт только аппликатор, не из проектора VM и не mid-rebuild.

Подокно класса: `applicator.SetClass(id)` — всё. Транзакция wipe+reapply внутри `SetClass`.

На смене `Ancestry` / `Class` / `Background` сносятся ручные пики (свободные boosts, option-feat): они живут только до следующего `RebuildDerived`. Имя / аватар rebuild не делают.

Чтение формул (HP и т.п.) — `CreatedCharacterParametersProxy`, не аппликатор.

### Что не делать

- Rebuild / `ApplyFeat` / очистку `Parameters` внутри подписки главного VM на `OnUpdate` — скрытый второй пересчёт и гонка с аппликатором.
- `NotifyUpdated` из проектора главного VM — цикл.
- `NotifyUpdated` на каждый `ApplyFeat` внутри `RebuildDerived`.
- Прямую запись в поля `_request` из подокон и отдельный callback «перерисуйся»: один канал — `OnUpdate`.

### Best practice: как пользоваться аппликатором

Целевой код (подокон у `CreateCharacter` ещё нет). Главный VM уже создаёт `Applicator` в `Init` и слушает `OnUpdate` — подокну остаётся вызвать метод аппликатора.

**1. Выбор класса / вида / предыстории** (`ListDialog` уже есть). Callback пишет только через аппликатор: wipe+reapply и `OnUpdate` внутри `SetClass`.

```csharp
public void OnEditClass()
{
    if (_isDisposed)
        return;

    var vm = _viewModelFactory.Create<DefinitionListDialogViewModel>();
    vm.Init(
        title: "Класс",
        mode: DefinitionListDialogMode.Class,
        selectedId: Class,
        onSelected: OnClassSelected);
    _windowsManager.OpenView<ListDialogView, ListDialogViewModel>(
        ListDialogView.Path, vm);
}

private void OnClassSelected(string classId)
{
    if (_isDisposed)
        return;

    Applicator.SetClass(classId);
    // не трогать _request.Class
    // не звать RebuildDerived / NotifyUpdated / SendOnChange — это сделает аппликатор → OnUpdate
}
```

То же для вида / предыстории: `SetAncestry` / `SetBackground`. Имя и аватар — `SetName` / `SetAvatar` (без rebuild).

**2. Подокно, которое крутит параметры** (характеристики / навыки). В `Init` передаём аппликатор, не `_request`.

```csharp
public void Init(CreateCharacterRequestApplicator applicator)
{
    _applicator = applicator;
}

public void OnIncreaseStrength()
{
    _applicator.TryApplyAbilityBoost(Glossary.Characters.STR);
}

public void OnDecreaseStrength()
{
    _applicator.TryRefundAbilityBoost(Glossary.Characters.STR);
}

public void OnCycleAthletics()
{
    int next = _applicator.GetSkillProficiencyRank(Glossary.Characters.ATHLETICS) + 1;
    _applicator.SetSkillProficiencyRank(Glossary.Characters.ATHLETICS, next);
}
```

Главный экран под подокном живой: каждый успешный вызов уже шлёт `OnUpdate`, HP/boosts на главном экране обновятся сами.

**3. Читать итоги (HP, Perception, сейвы) — proxy, не сырой ключ.**

```csharp
var proxy = new CreatedCharacterParametersProxy(
    _request,
    ruleDef,
    _definitionsManager);

int maxHp = proxy.GetTotalValue(Glossary.Characters.MAX_HIT_POINTS);
int perception = proxy.GetTotalValue(Glossary.Characters.PERCEPTION);
int strRaw = Applicator.GetRawParameter(Glossary.Characters.STR);
```

Не писать и не читать итог как `_request.CharacterData.Parameters["Perception"]`.

Антипаттерн:

```csharp
_request.Class = classId;                          // без OnUpdate, без rebuild
_request.CharacterData.Parameters["STR"] = 3;      // минуя аппликатор
_request.NotifyUpdated();                          // Raise не из аппликатора
Applicator.RebuildDerived();                       // второй полный wipe из VM подокна
```

### Логические панели главного экрана

Контент (9) + хром (1). Prefab пока заглушка (`Background` + пустой `Frame`); состав — контракт View/VM.

| # | Блок | Тип |
|---|---|---|
| 1 | Avatar | триггер |
| 2 | Name | триггер |
| 3 | Hit Points | производное (показ) |
| 4 | Level | производное (показ) |
| 5 | Ancestry | триггер |
| 6 | Class | триггер |
| 7 | Background | триггер |
| 8 | Ability Scores (+ boost points) | производное + ручной пик |
| 9 | Skills (+ boost points) | производное + ручной пик |
| 10 | Buttons (Close / Create active|disabled) | хром; `CanCreate` с `_request` |

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
