# Боевая система (ранний прототип)

**Последнее обновление:** 2026-07-27 11:54:36 (+03:00)

> **Статус:** ранний прототип / design draft.  
> Runtime-слой пошагового боя **не начат**. Документ фиксирует целевые очертания механики, контракт дефов и границы ответственности модулей.  
> Реализовано сейчас: зачаток `BattleRuleDef` (MAP) в [Definitions](Definitions.md); прокси атаки/урона оружия (`WeaponProxy`) в [State](State.md). Всё остальное ниже — план.

## Цель

Упрощённый пошаговый бой в духе классических d20 НРИ (D&D / Pathfinder 2e), без клеток и перемещения: сражение сторон «отряд vs отряд» с общим пулом действий на ход стороны и переключением между членами отряда.

## Связанные модули

| Модуль | Роль в бое |
|---|---|
| [Definitions](Definitions.md) | Статический контент: `BattleRuleDef`, планируемая коллекция `BattleActionDef`, оружие/заклинания/черты |
| [State](State.md) | Персистентные итоги боя (HP, постоянные эффекты); runtime-прокси `WeaponProxy` / `CharacterParametersProxy` |
| [Restrictions](Restrictions.md) | Доступность боевых действий (нужен планируемый `CharacterParams`) |
| [RPG](RPG.md) | Точка входа в бой из adventure (например, будущий `StartCombat` choice-action) |
| `Dices` | Броски d20 / кости урона |

## Базовые упрощения относительно PF2e / D&D

| Классика | Наш прототип |
|---|---|
| Клетки, дистанция, перемещение | Нет. Цели — только по стороне / персонажу |
| Индивидуальная инициатива каждого существа | Ход **стороны** (игрок / соперник) |
| 3 действия на персонажа (PF2e) | Общий пул `N` действий на **сторону** за раунд |
| Strike / Cast a Spell как встроенные правила | Отдельные дефы коллекции `BattleActions` |
| Полный набор реакций / AoO / укрытий | Вне MVP |

## Модель боя

### Стороны и отряды

- Бой — это дуэль **двух сторон**.
- У каждой стороны есть пул персонажей (у игрока — `CharactersStateData.ActivePartyCharacterIds` или снимок на старт боя; у соперника — encounter-состав из контента).
- Условно «1 на 1» по сторонам, но внутри стороны несколько бойцов.

### Ход стороны

1. Начало хода: `RemainingActions = ActionsPerTurn` (из `BattleRuleDef`).
2. Пока есть действия:
   - выбрать **исполнителя** (любой живой персонаж своей стороны);
   - выбрать **боевое действие** (`BattleActionDef`);
   - проверить доступность (`Restrictions`, ресурсы, оружие/заклинание);
   - выполнить броски и эффекты;
   - списать `ActionCost`.
3. Конец хода: тики длительности статусов / кулдаунов.
4. Переход хода к другой стороне.

Переключение между членами отряда **само по себе** либо бесплатно (`SwitchCharacterCost = 0`), либо стоит действие — параметр в `BattleRuleDef`.

### Таргетинг

Действия могут бить не только «текущего» противника:

| `TargetType` | Смысл |
|---|---|
| `Self` | Исполнитель |
| `AllySingle` | Один союзник |
| `AllyAll` | Весь свой отряд |
| `EnemySingle` | Один противник |
| `EnemyAll` | Весь отряд противника |
| `RandomEnemy` | Случайный живой противник |

## Где живёт состояние

| Слой | Что хранит | Персистентность |
|---|---|---|
| **Runtime battle session** | Очередь хода, `RemainingActions`, счётчики MAP, кулдауны раунда, временные статусы боя | Нет (не в сейв профиля) |
| **State (`CharacterStateData`)** | `HitPoints`, устойчивые параметры, экипировка, `StatusEffects` после боя | Да |
| **Defs** | Правила, каталог действий, оружие, черты | Статика |

Технические счётчики боя **не** пишутся в `StateData`, чтобы не раздувать сейвы.

## Дефы: уже есть

### `BattleRuleDef`

Путь: `Definitions/_ADVENTURES_/BattleRules`.  
Активный id: `RuleSettingsDef.BattleRule` (сейчас `GeneralBattleRule`).

| Поле | Назначение |
|---|---|
| `UseMultipleAttackPenalty` | Включить MAP |
| `AttackTags` | Теги действия, по которым оно считается атакой (например `ATTACK`) |
| `MultipleAttackPenaltyTable` | Таблица штрафа с **1-й** атаки: индекс `0` → первая атака |
| `MultipleAttackPenaltyTableByWeaponTag` | Override таблицы по тегу оружия (`AGILE` → сниженный MAP) |

Пример (`GeneralBattleRule`):

```json
"MultipleAttackPenaltyTable": [0, -5, -10, -10, -10, -10],
"MultipleAttackPenaltyTableByWeaponTag": {
  "AGILE": [0, -4, -8, -8, -8, -8]
}
```

Индексация: атака номер `n` → `table[min(n - 1, Count - 1)]`. За пределами списка — последнее значение.

Тег оружия `Glossary.Weapons.AGILE` (`"AGILE"`) — «Быстрое».

### Планируемые поля `BattleRuleDef`

| Поле | Черновик | Зачем |
|---|---|---|
| `ActionsPerTurn` | `int` | Пул действий стороны |
| `SwitchCharacterCost` | `int` (0/1) | Стоимость смены исполнителя |
| `MapCounterScope` | `PerActor` / `PerSide` | Счётчик MAP на персонажа или на всю сторону |
| `DefaultBattleActionIds` | `List<string>` | Базовые действия, доступные всем |

**Рекомендация прототипа:** `MapCounterScope = PerActor` (ближе к d20: воин копит свой MAP, жрец — свой).

## Дефы: каталог действий

### Коллекция `BattleActions` → `BattleActionDef`

Путь: `Definitions/_ADVENTURES_/BattleActions`.  
Загрузка: `DefinitionsManager.BattleActions` + `LoadBattleActions()` (после `LoadSpells`).

Каталог допустимых опций в бою (`Strike`, `CastSpell`, классовые приёмы и т.д.). **Контракт данных загружается; runtime resolver ещё не реализован.**

Поля:

| Поле | Тип | Назначение |
|---|---|---|
| `Disabled` | `bool` | Выключить контент |
| `Tags` | `List<string>` | В т.ч. `ATTACK` / `SPELL` для MAP и классификации |
| `Title` / `Description` / `Icon` | meta | UI |
| `ActionCost` | `int` | Стоимость в действиях стороны |
| `TargetType` | enum | См. таблицу таргетинга |
| `AvailabilityRestrictions` | `List<Restriction>` | Условия показа/использования |
| `RequiredWeaponTagsAny` | `List<string>` | Опционально: нужно оружие с одним из тегов |
| `RequiredSpellId` | `string` | Опционально: id `SpellDef` (`""` = выбор в runtime) |
| `Effects` | `List<BattleActionEffectData>` | Эффекты при выполнении / попадании |

### `BattleActionEffectData`

| Поле | Назначение |
|---|---|
| `Type` | `Damage` / `Heal` / `AddStatus` / `RemoveStatus` / `ModifyParam` |
| `Value` / `ValueFormula` | Число или формула; пустая формула + `Damage` → урон оружия в runtime |
| `StatusId` / `StatusValue` / `DurationRounds` | Для статусов |
| `OnHitOnly` | Только при успешном hit-check |

Для атак: hit-check против защиты цели (обычно AC из `Glossary.Characters.ARMOR_CLASS`), модификатор атаки через `WeaponProxy` + MAP из `BattleRuleDef`.

### Стартовый набор контента

Тестовые id с префиксом `_` (не финальные):

| Id | Tags | Target | Заметки |
|---|---|---|---|
| `_Strike` | `ATTACK` | `EnemySingle` | Базовая атака оружием |
| `_CastSpell` | `SPELL` | `EnemySingle` | Обёртка каста; spell выбирается runtime |
| `_RaiseShield` | — | `Self` | MVP-защита через статус `RaisedShield` |
| `_Cleave` | `ATTACK` | `EnemyAll` | AoE по отряду противника, `ActionCost = 2` |
| `_PaladinSuperPuperAttack` | `ATTACK` | `EnemySingle` | `CharacterParams` ≥ 1 на `feat.paladin.super_puper_attack` |

## Доступность действий и черты

Паттерн:

1. `FeatDef.Apply.Set` (или `Add`) пишет флаг в `CharacterStateData.Parameters`, например `feat.paladin.super_puper_attack = 1`.
2. У соответствующего `BattleActionDef` в `AvailabilityRestrictions` — условие на этот ключ.

Для этого в [Restrictions](Restrictions.md) используется:

- `RestrictionType.CharacterParams`
- `CharacterParamsRestrictionChecker` читает `CharacterStateData.Parameters` целевого персонажа
- формат: `StringValues[0]` = ключ, `IntValues[0]` + `CompareOptions`
- цель: `CharacterRestrictionContext` (override исполнителя или первый живой в активном отряде)

Базовые действия из `BattleRuleDef.DefaultBattleActionIds` доступны без feat-флага (пустые / пройденные restrictions).

Опциональные действия (классовые приёмы) видны только при выполнении restrictions.

## Multiple Attack Penalty (MAP) в runtime

1. Действие участвует в MAP, если `BattleActionDef.Tags` пересекается с `BattleRuleDef.AttackTags`.
2. Выбрать таблицу:
   - если у оружия исполнителя есть тег из `MultipleAttackPenaltyTableByWeaponTag` — эта таблица;
   - иначе `MultipleAttackPenaltyTable`.
3. Взять штраф по номеру атаки этого счётчика в текущем раунде (см. `MapCounterScope`).
4. Прибавить штраф к модификатору атаки перед броском d20.

Не-атакующие действия (лечение, баф) MAP не копят и не получают.

## Мини-симуляция (зачем нужны эти настройки)

Условия: `ActionsPerTurn = 3`, `SwitchCharacterCost = 0`, MAP `PerActor`, таблицы как в `GeneralBattleRule`.

| # | Исполнитель | Действие | MAP | Остаток AP |
|---|---|---|---|---|
| 1 | Воин | `_Strike` → враг A | `0` (1-я атака воина) | 2 |
| 2 | Жрец | баф на `AllyAll` | нет | 1 |
| 3 | Воин | `_Strike` → враг B | `-5` (2-я атака воина; Agile → `-4`) | 0 |

Выводы симуляции:

- нужен общий пул AP стороны и смена исполнителя;
- MAP должен считаться **на актёра**, иначе баф жреца «ломал» бы логику атак воина при `PerSide`;
- нужны `EnemyAll` / `AllyAll` для AoE-эффектов;
- нужны статусы с длительностью в раундах;
- нужна привязка опциональных действий к `CharacterStateData.Parameters`.

## Границы ответственности (когда появится код)

| Компонент | Модуль | Задача |
|---|---|---|
| `BattleActionDef` / JSON | Definitions | Каталог действий — **готово** (resolver — нет) |
| Расширение `BattleRuleDef` | Definitions | Economy / MAP scope |
| `CharacterParams` restriction | Restrictions | Доступность по параметрам персонажа — **готово** |
| `BattleSession` / resolver | новый слой в RPG (или отдельный Combat-модуль) | Оркестрация раунда, выбор действий, применение эффектов |
| Hit / damage rolls | Dices + State proxies | d20 + `WeaponProxy` + MAP |
| Запись HP / статусов после удара | State state-actions | Не прямая запись в `StateData` |
| Старт боя из сцены | RPG choice-action | Будущий `StartCombat` (сейчас в enum закомментирован) |

## Этапы реализации (ориентир)

### Phase 1 — MVP

- ~~Коллекция `BattleActionDef` + стартовые действия~~ — контракт и JSON загружаются.
- Runtime session: две стороны, пул AP, смена исполнителя, `EnemySingle` / `Self`.
- MAP из текущего `BattleRuleDef`.
- ~~`RestrictionType.CharacterParams`~~ — реализован (`CharacterParamsRestrictionChecker` + `CharacterRestrictionContext`).
- Применение урона/лечения в HP через state-actions.

### Phase 2

- `AllyAll` / `EnemyAll`, статусы с длительностью, кулдауны.
- Каст заклинаний через `RequiredSpellId` + `SpellDef`.
- `ActionsPerTurn` / `SwitchCharacterCost` / `MapCounterScope` в `BattleRuleDef`.

### Phase 3

- AI соперника, реакции, более богатые эффекты и триггеры.
- Encounter-дефы (состав врагов, награды) — отдельный контракт при необходимости.

## Открытые вопросы прототипа

1. Encounter как отдельная коллекция дефов или inline в adventure scene/choice?
2. Смерть / нокдаун / dying — упростить до `IsDead` или ввести промежуточные статусы?
3. Писать ли временные боевые статусы сразу в `CharacterStateData.StatusEffects` или держать только в session до конца боя?
4. Стоимость переключения исполнителя: 0 (гибкий отряд) или 1 (жёстче баланс)?

Решения по этим пунктам не зафиксированы; при реализации MVP их нужно явно выбрать и обновить этот документ.
