# Модуль Windows

**Последнее обновление:** 2026-06-29 11:50:00 (+03:00)

## Назначение

`Windows` реализует базовую UI-архитектуру окон (View/ViewModel), управление стеком открытых окон, сортировкой и закрытием по `Esc`.

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

