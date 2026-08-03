# Как заполнять дефы создания персонажа

**Последнее обновление:** 2026-08-03 09:50:00 (+03:00)

Пошаговая инструкция для контент-авторов: какие JSON писать, в каком порядке и как связать `ClassDef` / `AncestryDef` / `BackgroundDef` с `FeatDef` (и при необходимости `ConditionDef`), чтобы при создании и прокачке персонажа стейт (`CharacterStateData.Parameters`) заполнялся корректно.

Связанные документы:

- [Definitions.md](Definitions.md) — справочник полей дефов
- [Feats.md](Feats.md) — runtime Apply / Unapply / Condition / прокачка
- [State.md](State.md) — `CharacterParametersOperator`, создание / update персонажа

---

## 1. Идея в одном абзаце

Класс, происхождение и предыстория **не пишут статы напрямую**. Они только перечисляют id черт в `Features`.  
Механика живёт в `FeatDef.Apply` (`Add` / `Set` / `AlsoApplyFeatIds` / `ConditionDuration`).  
Runtime вызывает `ApplyFeat` по этим id → сырые ключи попадают в `CharacterStateData.Parameters`.  
Итоги вроде `MaxHitPoints` / `Athletics` **не кладите** в `Apply` — их считает `CharacterParametersProxy`.

```mermaid
flowchart LR
  ancestry["AncestryDef.Features[level]"]
  classDef["ClassDef.Features[level]"]
  background["BackgroundDef.Features"]
  feat["FeatDef.Apply"]
  params["CharacterStateData.Parameters"]
  proxy["CharacterParametersProxy"]
  ancestry --> feat
  classDef --> feat
  background --> feat
  feat -->|"ApplyFeat"| params
  params --> proxy
```

---

## 2. Рекомендуемый порядок работы

Делайте контент **снизу вверх**: сначала атомарные черты, потом обёртки выбора, потом таблицы прокачки на дефах.

| Шаг | Что создавать | Зачем |
|-----|---------------|--------|
| 1 | Атомарные `FeatDef` с `Apply` | реальные изменения `Parameters` |
| 2 | Choice-обёртки `FeatDef` с `Options` | UI-выбор (heritage, key ability, class feat) |
| 3 | `AncestryDef` / `ClassDef` / `BackgroundDef` | ссылки на id из шагов 1–2 в `Features` |
| 4 | (опционально) `ConditionDef` | tick-логика статусов с тем же id, что у Condition-feat |

Не наоборот: если сначала заполнить `Features: ["_MyCoolThing"]`, а файла черты ещё нет — `ApplyFeat` даст warning / no-op, и стейт останется «пустым».

---

## 3. Где лежат файлы

| Def | Папка | Id |
|-----|-------|----|
| `ClassDef` | `Definitions/_ADVENTURES_/Classes/` | имя файла без `.json` |
| `AncestryDef` | `Definitions/_ADVENTURES_/Ancestries/` | то же |
| `BackgroundDef` | `Definitions/_ADVENTURES_/Backgrounds/` | то же |
| `FeatDef` | `Definitions/_ADVENTURES_/Feats/` (+ подпапки `Ancestry`, `Class`, `ClassFeature`, `Condition`, `General`, …) | то же (подпапка на id **не** влияет) |
| `ConditionDef` | `Definitions/_ADVENTURES_/Conditions/` | **должен совпадать** с id парного Condition-`FeatDef` |

`Id` в JSON **не пишется** — его задаёт загрузчик из имени файла.

Подпапки у `Feats` — только для удобства авторов (`ClassFeature/_FighterKeyAttribute.json` → id `_FighterKeyAttribute`).

---

## 4. Базовые правила `FeatDef.Apply`

Патч `CharacterParamsPatchData`:

| Поле | Что делает при Apply | Типичное использование |
|------|----------------------|------------------------|
| `Add` | `Parameters[key] += value` | boost характеристик (`STR` +1), бонусы (`.Bonus`) |
| `Set` | `Parameters[key] = value` | proficiency (`.ProfRank`), флаги «черта взята» |
| `AlsoApplyFeatIds` | каскадный `ApplyFeat` по списку | редко; обычно выбор идёт через `Options` + UI |
| `ConditionDuration` | таймер в `StatusEffects` | только для `Type: Condition` |

### Ключи параметров

- Abilities: `STR`, `DEX`, `CON`, `INT`, `WIS`, `CHA`
- Уровень: `Level`
- Владение: `{Id}.ProfRank` — значения enum `ProficiencyType`: `0` Untrained, `1` Trained, `2` Expert, `3` Master, `4` Legendary
- Бонусы к **вычисляемым** параметрам: `{Id}.Bonus`, `{Id}.ItemsBonus`, `{Id}.PerLevel`  
  **Не** пишите в `Add` ключ вроде `"Perception": -1`, если `Perception` считается формулой — кладите `"Perception.Bonus": -1`

### Флаг «черта взята»

Почти у каждой механической черты в `Set` ставьте флаг с id самой черты:

```json
"Set": {
  "_ExactingStrike": 1
}
```

Так Restrictions / UI / читы могут проверить, что черта уже есть. При `UnapplyFeat` флаг станет `0`.

### Choice-обёртка vs child

| Роль | `Options` | `Apply` | Кто попадает в стейт |
|------|-----------|---------|----------------------|
| Обёртка выбора (`_FighterKeyAttribute`, `_DwarfHeritage`) | список child id | **нет / пусто** | сама обёртка **не** Apply’ится как механика |
| Выбранный child (`_FighterKeyStrength`, `_RockDwarf`) | нет | есть | UI/`ApplyFeat` применяет **child**, не обёртку |

В `ClassDef.Features` / `AncestryDef.Features` обычно лежит **id обёртки**. Runtime при создании/уровне:

1. видит обёртку с `Options`;
2. спрашивает игрока;
3. вызывает `ApplyFeat(chosenChildId)`.

Если обёртку Apply’нуть как есть (без выбора) — warning / no-op по патчу.

---

## 5. `FeatType` — какую роль выбрать

| `Type` | Когда |
|--------|--------|
| `ClassFeature` | автоматические фичи класса/ancestry, initial proficiencies, key ability children, heritage children |
| `ClassFeat` | выбираемые классовые черты (`_ExactingStrike`, `_PowerAttack`) |
| `AncestryFeat` | выбираемые ancestry feats |
| `BackgroundSkillFeat` | skill feat от background |
| `SkillFeat` | skill feat на прокачке |
| `GeneralFeat` | general feat |
| `Boost` | свободные / attribute boosts с `Options` или атомарные boost-children |
| `Condition` | статусы / дебафы (часто с `ConditionDuration`) |

`Type` сам по себе стейт не меняет — меняет только `Apply`. Но его используют UI-фильтры и Restrictions.

---

## 6. Шаг за шагом: атомарная черта с механикой

**Пример:** фиксированные boosts дварфа.

Файл: `Feats/Ancestry/_DwarfAttributeBoosts.json`

```json
{
  "Disabled": false,
  "Type": "ClassFeature",
  "Level": 1,
  "Tags": ["player-core", "ancestry-dwarf"],
  "Title": "Повышения и изъян дварфа",
  "Description": "Вы получаете повышения Телосложения и Мудрости и изъян Харизмы.",
  "Restrictions": [],
  "Apply": {
    "Add": {
      "CON": 1,
      "WIS": 1,
      "CHA": -1
    },
    "Set": {
      "_DwarfAttributeBoosts": 1
    }
  }
}
```

После `ApplyFeat("_DwarfAttributeBoosts")` в `Parameters`:

| Ключ | Значение |
|------|----------|
| `CON` | было + 1 |
| `WIS` | было + 1 |
| `CHA` | было − 1 |
| `_DwarfAttributeBoosts` | `1` |

**Пример:** начальные proficiency класса (без выбора).

Файл: `Feats/ClassFeature/_FighterInitialProficiencies.json`

```json
{
  "Disabled": false,
  "Type": "ClassFeature",
  "Level": 1,
  "Tags": ["player-core", "class-fighter"],
  "Title": "Начальное владение",
  "Description": "Базовые ранги владения воина.",
  "Restrictions": [],
  "Apply": {
    "Set": {
      "Perception.ProfRank": 2,
      "Fortitude.ProfRank": 2,
      "Reflex.ProfRank": 2,
      "Will.ProfRank": 1,
      "Weapon.Simple.ProfRank": 2,
      "Weapon.Martial.ProfRank": 2,
      "Weapon.Advanced.ProfRank": 1,
      "Unarmed.ProfRank": 2,
      "Armor.ProfRank": 1,
      "Unarmored.ProfRank": 1,
      "ClassDC.ProfRank": 1
    }
  }
}
```

Здесь флаг «черта взята» можно не дублировать на каждый ключ — достаточно самих `.ProfRank`. При необходимости добавьте `"_FighterInitialProficiencies": 1` в `Set`.

**Пример:** обучаемость навыку (child для skill-choice).

```json
{
  "Disabled": false,
  "Type": "ClassFeature",
  "Level": 1,
  "Tags": ["player-core", "skill-athletics"],
  "Title": "Обученность: Атлетика",
  "Description": "Вы становитесь обучены в навыке Атлетика.",
  "Restrictions": [],
  "Apply": {
    "Set": {
      "Athletics.ProfRank": 1,
      "_TrainAthletics": 1
    }
  }
}
```

---

## 7. Шаг за шагом: choice-обёртка + options

### 7.1. Обёртка (без `Apply`)

```json
{
  "Disabled": false,
  "Type": "ClassFeature",
  "Level": 1,
  "Tags": ["player-core", "class-fighter"],
  "Title": "Ключевая характеристика",
  "Description": "На 1-м уровне класс даёт повышение Силы или Ловкости.",
  "Restrictions": [],
  "Options": [
    "_FighterKeyStrength",
    "_FighterKeyDexterity"
  ]
}
```

### 7.2. Каждый option — отдельный FeatDef с `Apply`

```json
{
  "Disabled": false,
  "Type": "ClassFeature",
  "Level": 1,
  "Tags": ["player-core", "class-fighter", "key-strength"],
  "Title": "Ключевая характеристика: Сила",
  "Description": "Повышение Силы от классовой ключевой характеристики.",
  "Restrictions": [],
  "Apply": {
    "Add": { "STR": 1 },
    "Set": { "_FighterKeyStrength": 1 }
  }
}
```

### 7.3. Свободный boost происхождения

Обёртка (`Type: Boost`):

```json
{
  "Disabled": false,
  "Type": "Boost",
  "Level": 1,
  "Tags": ["player-core", "ancestry-dwarf", "boost-free"],
  "Title": "Свободное повышение характеристики",
  "Description": "Выберите одну характеристику для свободного повышения происхождения.",
  "Restrictions": [],
  "Options": [
    "_DwarfBoostSTR",
    "_DwarfBoostDEX",
    "_DwarfBoostCON",
    "_DwarfBoostINT",
    "_DwarfBoostWIS",
    "_DwarfBoostCHA"
  ]
}
```

Child:

```json
{
  "Disabled": false,
  "Type": "Boost",
  "Level": 1,
  "Tags": ["player-core", "ancestry-dwarf"],
  "Title": "Повышение: Сила",
  "Description": "Свободное повышение Силы от происхождения дварфа.",
  "Restrictions": [],
  "Apply": {
    "Add": { "STR": 1 },
    "Set": { "_DwarfBoostSTR": 1 }
  }
}
```

### 7.4. Heritage / class feat choice

То же правило:

- `_DwarfHeritage` → `Options: ["_RockDwarf", …]` без `Apply`
- `_RockDwarf` → `Apply.Set: { "_RockDwarf": 1 }` (+ механика, если есть числа)
- `_FighterFeat1` → `Options: ["_ExactingStrike", …]`
- `_ExactingStrike` (`Type: ClassFeat`) → `Apply.Set: { "_ExactingStrike": 1 }`

Narrative-only child (пока нет чисел) всё равно должен иметь флаг в `Set`, чтобы стейт фиксировал выбор.

---

## 8. `AncestryDef` — происхождение и его `Features`

Файл: `Ancestries/Dwarf.json` (id = `Dwarf`).

### Поля, важные для персонажа

| Поле | Смысл |
|------|--------|
| `HitPoints` | ancestry HP **один раз** при создании (не за уровень) |
| `Speed` | базовая скорость (часто ещё пишут в Parameters через feat/скелет) |
| `Size` | `Small` / `Medium` / `Large` |
| `Features` | **словарь** `"уровень" → [feat id, …]` |
| `EquippedItems` | слоты происхождения (`Finger` / `Neck` / `Tail`, …) |
| `Names` | пул имён по `Male` / `Female` |

### Как заполнять `Features`

Ключи — **уровень персонажа**, на котором выдаётся фича (строки в JSON: `"1"`, `"5"`).

На **1 уровне** обычно:

1. фиксированные ancestry boosts / flaw (`_DwarfAttributeBoosts`);
2. свободный boost-обёртка (`_DwarfFreeBoost`);
3. автоматические особенности (`_Darkvision`, `_ClanDagger`);
4. heritage-обёртка (`_DwarfHeritage`);
5. ancestry feat choice 1 (`_DwarfAncestryFeat1`).

На более высоких уровнях — только то, что ancestry даёт сама (часто choice ancestry feat):

```json
{
  "Disabled": false,
  "Size": "Medium",
  "Speed": 20,
  "HitPoints": 10,
  "Tags": ["player-core", "ancestry-dwarf"],
  "Title": "Дварф",
  "Description": "…",
  "Features": {
    "1": [
      "_DwarfAttributeBoosts",
      "_DwarfFreeBoost",
      "_Darkvision",
      "_ClanDagger",
      "_DwarfHeritage",
      "_DwarfAncestryFeat1"
    ],
    "5": [
      "_DwarfAncestryFeat5"
    ]
  },
  "Names": {
    "Male": ["Dolgrin", "Agnar"],
    "Female": ["Agna", "Brunhild"]
  }
}
```

**Правило:** в списке уровня кладите id так, как их должен видеть UI/runtime слева направо (сначала авто-механику, потом выборы). Порядок Apply для авто-feats имеет значение, если они трогают одни и те же ключи.

---

## 9. `ClassDef` — класс и таблица прокачки

Файл: `Classes/Fighter.json` (id = `Fighter`).

### Поля

| Поле | Смысл |
|------|--------|
| `HitPointsPerLevel` | HP класса **за каждый уровень** (до CON) |
| `Features` | словарь уровень → список feat id |
| `EquippedItems` | базовые слоты + стартовый лут (`Hand`, `Body`, `Bag`, …) |

### Как проектировать `Features` по уровням

Каждый уровень = **пакет** того, что персонаж получает при достижении этого уровня.

Пример воина (укороченный):

```json
{
  "Disabled": false,
  "HitPointsPerLevel": 10,
  "Tags": ["player-core", "role-martial"],
  "Title": "Воин",
  "Description": "…",
  "Features": {
    "1": [
      "_FighterKeyAttribute",
      "_FighterInitialProficiencies",
      "_FighterInitialSkill",
      "_ReactiveStrike",
      "_ShieldBlock",
      "_FighterFeat1"
    ],
    "2": [
      "_FighterFeat2",
      "_SkillFeatChoice"
    ],
    "3": [
      "_Bravery",
      "_GeneralFeatChoice",
      "_SkillIncrease"
    ],
    "5": [
      "_AncestryFeatChoice",
      "_AttributeBoosts",
      "_FighterWeaponMastery",
      "_SkillIncrease"
    ]
  },
  "EquippedItems": [
    { "Slot": "Hand", "ItemId": "_Longsword" },
    { "Slot": "Hand", "ItemId": "_SteelShield" },
    { "Slot": "Body", "ItemId": "_Breastplate" },
    { "Slot": "Bag", "ItemId": "_Backpack" },
    { "Slot": "Bag", "ItemId": "" }
  ]
}
```

### Типы записей в `Features[level]`

| Вид записи | Пример | Что должно быть в FeatDef |
|------------|--------|----------------------------|
| Авто-фича с механикой | `_FighterInitialProficiencies`, `_Bravery` | `Apply` заполнен |
| Выбор ключевой характеристики | `_FighterKeyAttribute` | `Options`, без `Apply` |
| Выбор навыка | `_FighterInitialSkill` | `Options` → `_TrainAthletics` / `_TrainAcrobatics` |
| Выбор классовой черты | `_FighterFeat1` | `Options` → `ClassFeat` children |
| Универсальный слот выбора | `_SkillFeatChoice`, `_GeneralFeatChoice`, `_AncestryFeatChoice` | `Options` или отдельная UI-логика по `Type`/`Restrictions` |
| Mastery / escalation | `_FighterWeaponMastery` | часто снова обёртка с Options по группам оружия |

### Прокачка (уровень N)

Runtime (см. [Feats.md](Feats.md)):

1. snapshot персонажа;
2. `Level += 1` (патч);
3. `ApplyFeat` для каждого id из `ClassDef.Features[N]` (и при необходимости `AncestryDef.Features[N]`);
4. для choice — Apply **выбранного** child;
5. `UpdateCharacterStateAction`.

Поэтому:

- **не дублируйте** в `Features[2]` то, что уже выдали на 1;
- escalation (Expert → Master) оформляйте **новым** feat id с `Set` нового `.ProfRank`, либо отдельным патчем — не полагайтесь, что повторный Apply того же Set «поднимет» ранг сам по себе без продуманного Unapply/Replace.

---

## 10. `BackgroundDef` — предыстория

Файл: `Backgrounds/_Farmhand.json`.

`Features` здесь — **плоский список** (не словарь по уровням). Всё выдаётся при создании персонажа (эквивалент «с 1 уровня»).

```json
{
  "Disabled": false,
  "Restrictions": [],
  "Tags": [
    "player-core",
    "boost-strength",
    "boost-constitution",
    "skill-athletics"
  ],
  "Icon": "",
  "Title": "Батрак",
  "Description": "…",
  "Features": [
    "_AssuranceAthletics"
  ]
}
```

### Что обычно кладут в Background Features

- skill feat (`BackgroundSkillFeat`);
- при необходимости — отдельные boost-feats / lore / trained skill.

Если background даёт **выбор** (два skill feat на выбор) — положите обёртку с `Options`, а не оба child сразу.

Boosts характеристик от background в текущем контенте часто ещё живут в Tags как пометки для UI; механику всё равно лучше оформить явным `FeatDef` с `Apply.Add`, иначе стейт их не получит.

---

## 11. Как правильно собирать `FeatDef` под Features-таблицы

Чеклист на каждую запись в `Features`:

1. **Существует ли JSON** с таким id?
2. Это **авто** или **choice**?
   - авто → обязателен осмысленный `Apply` (или осознанный narrative-only флаг);
   - choice → `Options` непустой, у **каждого** child есть свой файл.
3. Child’ы не обязаны (и обычно **не должны**) дублироваться в `ClassDef.Features` / `AncestryDef.Features` — туда идёт только обёртка.
4. `Level` у FeatDef = минимальный уровень взятия (для UI/Restrictions), не путать с ключом словаря Features.
5. `Restrictions` на child ограничивают, **когда** его можно выбрать (класс, навык trained, и т.д.).
6. Не пишите итоговые формулы в `Apply` — только сырые ключи.

### Паттерны (шпаргалка)

```text
[Авто-механика]
FeatDef { Apply.Add / Apply.Set }

[Выбор 1 из N]
FeatDef wrapper { Options: [A, B, C] }          ← без Apply
FeatDef A/B/C   { Apply: ... }                  ← механика

[Каскад без UI]
FeatDef { Apply.AlsoApplyFeatIds: [X, Y] }      ← редко; порядок Apply прямой

[Статус]
FeatDef Type=Condition { Apply + ConditionDuration }
ConditionDef тот же id { Tick? }                ← опционально
```

---

## 12. `ConditionDef` — когда нужен и как заполнять

Слой ролей:

| Файл | Роль |
|------|------|
| `Feats/Condition/_ConditionPoisoned.json` | **обязателен**: механика в `Parameters` + таймер |
| `Conditions/_ConditionPoisoned.json` | **опционален**: tick в конце хода (урон / TickPatch) |

**Id обоих файлов должны совпадать.**

### FeatDef (минимум)

```json
{
  "Disabled": false,
  "Type": "Condition",
  "Level": 0,
  "Tags": ["condition", "poison"],
  "Title": "Отравление",
  "Description": "Периодический урон ядом, пока активен таймер.",
  "Restrictions": [],
  "Apply": {
    "Set": { "_ConditionPoisoned": 1 },
    "ConditionDuration": 3
  }
}
```

### ConditionDef (tick)

```json
{
  "Disabled": false,
  "Tags": ["condition", "poison"],
  "Title": "Отравление",
  "Description": "…",
  "Tick": {
    "Disabled": false,
    "FlatDamage": 2,
    "DamageDice": [ { "Count": 1, "DiceType": "D6" } ],
    "DamageType": "Poison",
    "TickPatch": {
      "Set": { "_ConditionPoisonedTick": 1 }
    }
  }
}
```

Без `ConditionDef` статус всё равно работает (таймер + Unapply), но без периодического урона/тик-патча.

Condition **не** кладут в `ClassDef.Features` / `AncestryDef.Features` / `BackgroundDef.Features` — их вешает бой / проверки / предметы.

---

## 13. Сквозной пример: дварф-воин-батрак 1 уровня

### Контент (ссылки)

| Источник | Features |
|----------|----------|
| `BackgroundDef._Farmhand` | `_AssuranceAthletics` |
| `AncestryDef.Dwarf` lvl 1 | `_DwarfAttributeBoosts`, `_DwarfFreeBoost`, `_Darkvision`, `_ClanDagger`, `_DwarfHeritage`, `_DwarfAncestryFeat1` |
| `ClassDef.Fighter` lvl 1 | `_FighterKeyAttribute`, `_FighterInitialProficiencies`, `_FighterInitialSkill`, `_ReactiveStrike`, `_ShieldBlock`, `_FighterFeat1` |

### Что делает игрок (выборы)

| Обёртка | Выбор (Apply child) |
|---------|---------------------|
| `_DwarfFreeBoost` | например `_DwarfBoostSTR` |
| `_DwarfHeritage` | например `_RockDwarf` |
| `_DwarfAncestryFeat1` | например `_BoulderRoll` |
| `_FighterKeyAttribute` | например `_FighterKeyStrength` |
| `_FighterInitialSkill` | например `_TrainAthletics` |
| `_FighterFeat1` | например `_ExactingStrike` |

### Что уходит в Parameters (упрощённо)

| Источник | Эффект |
|----------|--------|
| `_DwarfAttributeBoosts` | `CON+1`, `WIS+1`, `CHA-1` |
| `_DwarfBoostSTR` | `STR+1` |
| `_FighterKeyStrength` | `STR+1` |
| `_FighterInitialProficiencies` | набор `.ProfRank` |
| `_TrainAthletics` | `Athletics.ProfRank = 1` |
| `_RockDwarf` / `_ExactingStrike` / … | флаги `Set` |
| `_AssuranceAthletics` | флаг / механика skill feat |

Стартовый скелет до черт обычно содержит как минимум `Level = 1` и нулевые abilities — см. симуляцию в [Feats.md](Feats.md#кейс-a-создание-персонажа).

---

## 14. Чеклист перед коммитом контента

- [ ] У каждой ссылки в `Features` / `Options` / `AlsoApplyFeatIds` есть JSON-файл с тем же id
- [ ] Choice-обёртки **без** `Apply` (или с пустым), children **с** `Apply`
- [ ] В `ClassDef`/`AncestryDef` в `Features` лежат обёртки, а не все children скопом
- [ ] `BackgroundDef.Features` — плоский список; нет словаря уровней
- [ ] Сырые ключи согласованы с `Glossary.Characters` и суффиксами `.ProfRank` / `.Bonus` / `.ItemsBonus` / `.PerLevel`
- [ ] Нет записи итоговых `MaxHitPoints` / skill totals в `Apply`
- [ ] У механических черт есть флаг в `Set` (обычно id самой черты = 1)
- [ ] `EquippedItems`: класс — оружие/броня/`Bag`; ancestry — украшения; пустой `ItemId` = свободный слот
- [ ] Condition: парный `FeatDef` обязателен; `ConditionDef` — только если нужен tick; id совпадают
- [ ] Новый уровень класса: в `Features[N]` только **новые** выдачи этого уровня

---

## 15. Частые ошибки

| Ошибка | Результат | Как правильно |
|--------|-----------|---------------|
| В `Features` указан id, файла нет | warning, стейт не меняется | создать FeatDef до ссылки |
| В `Features` положили все Options-children | выдадутся все сразу / UI сломан | только обёртка |
| Обёртку Apply’нули без выбора | no-op | Apply выбранный child |
| `Add: { "Athletics": 1 }` для формулы | ломает контракт proxy | `Athletics.ProfRank` / `.Bonus` |
| Background boosts только в Tags | красивые теги, нулевой стейт | отдельный FeatDef + id в `Features` |
| Повторно Apply тот же Set-prof на level-up | ранг не «повысится» сам | новый feat / новый Set-ключ / продуманный Unapply |
| `ConditionDef` без `FeatDef` | tick не к чему привязать | сначала Condition-feat |

---

## См. также

- Runtime Apply / Unapply / EndCharacterTurn: [Feats.md](Feats.md)
- Поля дефов C# ↔ JSON: [Definitions.md](Definitions.md)
- Write API и state-actions персонажа: [State.md](State.md)
- Restrictions на выбор черт: [Restrictions.md](Restrictions.md)
