# Документация проекта DKIT

## Состав проекта

Проект построен на Unity и организован по стандартной структуре:

- `Assets` — основной код, ресурсы, сцены и игровые модули.
- `Packages` — зависимости и пакеты Unity.
- `ProjectSettings` — настройки проекта Unity.
- `UserSettings` — локальные пользовательские настройки редактора.
- `Library`, `Logs`, `obj` — служебные директории, генерируются инструментами.

Ключевая прикладная логика сосредоточена в `Assets/Modules`.

## Модули (`Assets/Modules`)

- `Cheats` — редакторские инструменты и секции читов для отладки/тестирования.
- `Debug` — инфраструктура логирования и отладочной диагностики.
- `Definitions` — загрузка и работа с конфигурационными definitions-данными.
- `Dices` — механики бросков кубиков и представление результатов.
- `ECS` — ECS-слой с компонентами, системами и событиями.
- `Initializer` — пайплайн инициализации проекта и стартовых задач.
- `Localization` — локализационные данные и прокси-компоненты UI-текстов.
- `Match3` — доменная логика игрового режима Match-3.
- `Restrictions` — система проверок ограничений и условий доступа.
- `RPG` — RPG/adventure-подсистема (приключения, выборы, действия).
- `Save` — управление сохранениями и модели сохраняемых данных.
- `State` — менеджмент состояния игры и данных профиля/сессии.

См. также ранний прототип боевой системы: [modules/Battle.md](modules/Battle.md) (runtime боя ещё не начат; статблоки `CreatureDef` + factory уже есть).  
Практическое руководство по чертам: [modules/Feats.md](modules/Feats.md) («Как использовать Feats»).  
Как заполнять дефы создания персонажа: [modules/CharacterCreationDefs.md](modules/CharacterCreationDefs.md) (`ClassDef` / `AncestryDef` / `BackgroundDef` / `FeatDef` / `ConditionDef`).  
Практическое руководство по противникам: [modules/Creatures.md](modules/Creatures.md) (`CreatureDef` → `CharacterStateData`).
- `Utils` — общие утилиты, вспомогательные компоненты и инструменты.
- `Windows` — базовая UI-архитектура окон (View/ViewModel), стек/`Esc`, toast-hints (`IHintManager`), Adventure runtime UI; см. [modules/Windows.md](modules/Windows.md).
