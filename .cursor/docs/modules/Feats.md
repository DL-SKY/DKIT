# Как использовать Feats

**Последнее обновление:** 2026-07-28 16:40:00 (+03:00)

Практическое руководство: когда и как применять / снимать черты (`FeatDef`) к персонажу через Write API.

Связанные документы: [Definitions.md](Definitions.md) (`FeatDef`, `CharacterParamsPatchData`, `Features` на дефах), [State.md](State.md) (`CharacterParametersOperator`, прокачка, экипировка, `EndCharacterTurn`), [Battle.md](Battle.md) (бой — прототип), [Creatures.md](Creatures.md) (статусы на NPC через тот же `CharacterStateData`).

---

## Идея в одном абзаце

`FeatDef` — **статический** эффект в контенте (`Apply`: `Add` / `Set` / `AlsoApplyFeatIds`, опционально `ConditionDuration`).  
В runtime механика пишется в сырые `CharacterStateData.Parameters` через **`CharacterParametersOperator.ApplyFeat` / `UnapplyFeat`** (или `ApplyPatch` / `UnapplyPatch`).  
Для `FeatType.Condition` с `Apply.ConditionDuration > 0` оператор дополнительно пишет таймер в `CharacterStateData.StatusEffects` (`featId` → оставшиеся ходы).  
Итоги вроде `MaxHitPoints` / навыков **не пишутся** в словарь — их читает `CharacterParametersProxy.GetTotalValue`.

```mermaid
flowchart TD
  defSource["ClassDef / AncestryDef / BackgroundDef / ItemDef.Features / combat"]
  featDef["FeatDef.Apply"]
  operator["CharacterParametersOperator"]
  parameters["CharacterStateData.Parameters"]
  status["CharacterStateData.StatusEffects"]
  proxy["CharacterParametersProxy.GetTotalValue"]
  turnEnd["EndCharacterTurnStateAction"]
  defSource -->|"список feat id"| applyCall["ApplyFeat / UnapplyFeat"]
  applyCall --> operator
  featDef --> operator
  operator --> parameters
  operator -->|"Condition + Duration > 0"| status
  parameters --> proxy
  turnEnd -->|"декремент / Unapply при 0"| status
  turnEnd --> operator
```

---

## Базовый API

| Вызов | Когда |
|-------|--------|
| `ApplyFeat(parameters\|character, featId, definitionsManager)` | Выдать черту / feature / condition персонажу |
| `ApplyFeat(parameters, statusEffects, featId, definitionsManager)` | То же + явная передача словаря таймеров (нужно для condition) |
| `ApplyFeat(..., equippedItems, inventoryItems, ...)` | Полный путь Apply, когда нужно обработать `FeatDef.AdditionalSlots` и перенос предметов |
| `UnapplyFeat(...)` | Снять ту же черту (симметричный откат патча + снятие таймера) |
| `ApplyPatch` / `UnapplyPatch` | Ручная правка без отдельного `FeatDef` (boost при прокачке и т.п.) |
| `CreateCharacterRequestSnapshot(character)` | Слепок mutable-блока перед прокачкой |
| `EndCharacterTurnStateAction` | Конец хода: декремент таймеров condition, тик урона, Unapply при `0` |

Семантика патча:

| Поле | Apply | Unapply |
|------|-------|---------|
| `Add` | `+= value` | `+= -value` |
| `Set` | `= value` | ≠0 → `0`; `0` → `1` |
| `AlsoApplyFeatIds` | каскад Apply (прямой порядок) | каскад Unapply (обратный порядок) |
| `ConditionDuration` | если feat `Type=Condition` и значение `> 0` → `StatusEffects[featId] = Duration` (refresh без повторного `Add`) | удалить `StatusEffects[featId]` |

Обёртки на слепке прокачки: `CharacterRequestData` / `UpdateCharacterRequestData` — те же `ApplyFeat` / `UnapplyFeat` / `ApplyPatch` / `UnapplyPatch` (`ApplyFeat`/`UnapplyFeat` на DTO передают и `Parameters`, и `StatusEffects`).

---

## AdditionalSlots (теперь поддерживается в Apply/Unapply)

`FeatDef.AdditionalSlots` больше не является «декларацией без рантайма» — оператор применяет её напрямую:

| Событие | Что происходит |
|---------|----------------|
| `ApplyFeat` c `AdditionalSlots` | в `EquippedItems` добавляются новые записи слотов (`ItemId = null`) |
| `UnapplyFeat` c `AdditionalSlots` | соответствующие слоты удаляются (в обратном порядке списка) |

Если удаляемый слот занят предметом, перед удалением запускается перенос:

1. Сначала пытается переложить предмет в **свободный `Bag`**-слот этого же персонажа.
2. Если свободного `Bag` нет и доступен `inventoryItems` — переносит в **общий инвентарь**.
3. Если никуда перенести нельзя — слот не удаляется, пишется warning (чтобы не потерять предмет).

Правила `ItemDef.Features` при таком переносе соблюдаются:
- носимый → `Bag` / инвентарь: `Unapply` фич предмета;
- `Bag` → `Bag`: без изменений фич.

Пример JSON:

```json
{
  "Type": "Condition",
  "Level": 0,
  "Title": "Мутация: дополнительная рука",
  "Description": "Даёт дополнительный слот Hand, пока эффект активен.",
  "Apply": {
    "Set": { "_MutationExtraHand": 1 },
    "ConditionDuration": 3
  },
  "AdditionalSlots": [ "Hand" ]
}
```

> Для корректного fallback в общий инвентарь при `Unapply` вызывайте overload с `inventoryItems` (или путь через `CharacterRequestData`, который теперь передаёт `EquippedItems`).

### FAQ по AdditionalSlots

**Q: Что если снимается дополнительный слот, а он занят предметом?**  
`UnapplyFeat` сначала пытается переложить предмет в свободный `Bag`-слот, и только потом удалить слот.

**Q: Что если свободного `Bag`-слота нет?**  
Если передан `inventoryItems`, предмет переносится в общий инвентарь.

**Q: Что если нет ни свободного `Bag`, ни `inventoryItems`?**  
Слот не удаляется, пишется warning в лог. Это защита от потери предмета.

**Q: Если удаляемый слот сам `Bag`, что происходит?**  
Логика такая же: поиск другого свободного `Bag`; если его нет — перенос в общий инвентарь.

**Q: Нужно ли вручную снимать `ItemDef.Features` при переносе из носимого слота?**  
Нет, оператор делает это автоматически: при уходе из носимого слота фичи unapply, при переносе `Bag` → `Bag` фичи не трогаются.

---

## 1. Создание персонажа: ancestry / background / class

Контент уже ссылается на feat id:

| Источник | Поле | Форма |
|----------|------|--------|
| `AncestryDef` | `Features` | `Dictionary<int, List<string>>` — уровень → id |
| `ClassDef` | `Features` | то же |
| `BackgroundDef` | `Features` | `List<string>` (обычно «с 1 уровня») |

**Поток (целевой):** при финализации создания собрать стартовый `CharacterRequestData.Parameters`, затем последовательно `ApplyFeat` для всех выдаваемых id (ancestry lvl 1, background, class lvl 1, выбран игрока из `Options`), затем `CreateCharacterStateAction`.

Пример (упрощённо):

```csharp
var characterData = new CharacterRequestData
{
    Parameters = new Dictionary<string, int>
    {
        // базовые сырые ключи до черт: Level, abilities-скелет и т.д.
        [Glossary.Characters.LEVEL] = 1,
    },
};

// Background: плоский список
foreach (string featId in backgroundDef.Features)
    characterData.ApplyFeat(featId, definitionsManager);

// Ancestry / Class: features 1-го уровня
if (ancestryDef.Features != null
    && ancestryDef.Features.TryGetValue(1, out List<string> ancestryFeats))
{
    foreach (string featId in ancestryFeats)
        characterData.ApplyFeat(featId, definitionsManager);
}

if (classDef.Features != null
    && classDef.Features.TryGetValue(1, out List<string> classFeats))
{
    foreach (string featId in classFeats)
        characterData.ApplyFeat(featId, definitionsManager);
}

// Черта с Options (например Key Attribute): игрок выбрал один id
characterData.ApplyFeat(playerChosenOptionFeatId, definitionsManager);

stateLogic.ProcessAction(new CreateCharacterStateAction(new CreateCharacterRequestData
{
    Name = name,
    Ancestry = ancestryDef.Id,
    Class = classDef.Id,
    Background = backgroundDef.Id,
    CharacterData = characterData,
    AddToActiveParty = true,
}));
```

**Unapply** при создании обычно не нужен: игрок ещё не «снял» черту. Если в UI отменяют выбор option-feat до подтверждения — вызовите `UnapplyFeat` на том же слепке.

Черты с пустым / отсутствующим `Apply` (чисто narrative / choice-обёртки вроде `_FighterFeat1`) — `ApplyFeat` сделает warning и no-op по патчу; смысл таких id — выбор дочернего feat из `Options`.

---

## 2. Новый уровень (прокачка)

Контракт: только через слепок + `UpdateCharacterStateAction` (экшен **не** крутит Apply сам — персистит готовый `CharacterData`).

```csharp
CharacterStateData character = state.Characters.Characters[characterId];

var request = new UpdateCharacterRequestData
{
    CharacterId = characterId,
    CharacterData = CharacterParametersOperator.CreateCharacterRequestSnapshot(character),
};

// 1) поднять сырой Level (ручной патч или отдельный feat-boost)
request.ApplyPatch(new CharacterParamsPatchData
{
    Add = new Dictionary<string, int> { [Glossary.Characters.LEVEL] = 1 },
}, definitionsManager);

int newLevel = request.CharacterData.Parameters[Glossary.Characters.LEVEL];

// 2) feats класса / ancestry на этот уровень
if (classDef.Features != null
    && classDef.Features.TryGetValue(newLevel, out List<string> levelFeats))
{
    foreach (string featId in levelFeats)
    {
        // choice-feat: вместо id-обёртки Apply выбранный Options-id
        if (IsChoiceWrapper(featId, out string chosenChildId))
            request.ApplyFeat(chosenChildId, definitionsManager);
        else
            request.ApplyFeat(featId, definitionsManager);
    }
}

// 3) ручные boosts (какие параметры качать)
request.ApplyPatch(playerAbilityBoostPatch, definitionsManager);

stateLogic.ProcessAction(new UpdateCharacterStateAction(request));
```

Откат выбора в UI прокачки до `ProcessAction`: `request.UnapplyFeat(...)` / `UnapplyPatch` на том же слепке.

---

## 3. Инвентарь / экипировка персонажа

У `ItemDef` есть `Features` (`List<string>` — id `FeatDef`).  
Проводка в state-actions: `CharacterItemFeaturesOperator` внутри Equip / Unequip / Move / Remove (в конструктор передаётся `DefinitionsManager`).

**Правило Bag:** Features действуют только на **носимых** слотах (`Hand`, `Body`, `Finger`, `Neck`, `Tail`, …). Слот `Bag` — это «карман» на персонаже: предмет там **не** бафает. Хелпер: `Glossary.Items.GrantsItemFeatures(slot)`.

| Событие | Действие |
|---------|----------|
| `EquipItemFromInventory` в носимый слот | Unapply старого (если был) → Apply нового |
| `EquipItemFromInventory` в `Bag` | Features не трогать |
| `UnequipItemToInventory` / `RemoveCharacterEquippedItem` с носимого | Unapply |
| `MoveEquippedItemBetweenSlots` носимый → `Bag` | Unapply |
| `MoveEquippedItemBetweenSlots` `Bag` → носимый | Apply |
| Move между двумя носимыми | Features не трогать |
| `AddCharacterItem` (только `Bag` / общий инвентарь) | Features не применять |
| Предмет только в общем `Inventory.Items` | Features не применять |

Слоты украшений (`Finger`, `Neck`, `Tail`) задаются обычно в `AncestryDef.EquippedItems`; оружие/броня/`Bag` — в `ClassDef.EquippedItems`.

Пример вызова:

```csharp
stateLogic.ProcessAction(new EquipItemFromInventoryStateAction(
    new EquipItemFromInventoryRequestData
    {
        CharacterId = characterId,
        SlotIndex = neckSlotIndex,
        ItemId = "_AmuletOfMightyFists",
    },
    definitionsManager));
```

Бафы/дебафы предмета описывайте обычным `FeatDef.Apply` (например `Add: { "Athletics.ItemsBonus": 1 }` или `MaxHitPoints.Bonus`).

---

## 4. Condition-feats: статусы с таймером

Временный / условный эффект оформляется как обычный `FeatDef` с `Type: "Condition"`.  
Механика — в `Apply` (`Add` / `Set`); длительность — в `Apply.ConditionDuration`.

| Слой | Что хранит |
|------|------------|
| `Parameters` | механический эффект (штрафы, флаги) |
| `StatusEffects` | таймер: **ключ = id feat**, **value = оставшиеся ходы** |
| `EndCharacterTurnStateAction` | декремент таймеров, тик (например ongoing damage), `UnapplyFeat` при `0` |

Правила timed-condition (`Type == Condition` и `ConditionDuration > 0`):

1. Первый `ApplyFeat` → патч в `Parameters` + `StatusEffects[featId] = ConditionDuration`.
2. Повторный `ApplyFeat`, пока ключ уже есть → **только refresh таймера** (повторный `Add` не накручивается).
3. `UnapplyFeat` → откат патча + удаление ключа из `StatusEffects`.
4. Конец хода персонажа → `EndCharacterTurnStateAction` (не вручную править словарь таймеров в геймплее).

Типичные триггеры наложения: крит / провал спасброска / skill check / боевой outcome ([Battle.md](Battle.md)).

### Как заполнять деф (JSON)

Файл: `Definitions/_ADVENTURES_/Feats/_ConditionFrightened.json` (имя файла = id).

```json
{
  "Disabled": false,
  "Restrictions": [],
  "Tags": [],
  "Icon": "",
  "Title": "Испуган",
  "Description": "Штраф к проверкам. Снимается по истечении ConditionDuration или UnapplyFeat.",
  "Type": "Condition",
  "Level": 0,
  "Apply": {
    "Add": {
      "Perception.Bonus": -1
    },
    "Set": {
      "_ConditionFrightened": 1
    },
    "ConditionDuration": 3
  }
}
```

Для сложной логики (урон по тикам, доп-патчи, тип урона) используется отдельный `ConditionDef`
с **тем же id**, что и у `FeatDef`.

```json
{
  "Disabled": false,
  "Tags": [],
  "Title": "Продолжительный урон",
  "Description": "5 + 1d4 урона в конце каждого хода, пока активен таймер Condition.",
  "Tick": {
    "FlatDamage": 5,
    "DamageDice": [
      { "Count": 1, "DiceType": "D4" }
    ],
    "DamageType": "Fire"
  }
}
```

`EndCharacterTurnStateAction` по ключу из `StatusEffects` сначала находит `FeatDef` (для таймера/unapply),
затем опционально `ConditionDef` (для тика). Если `ConditionDef` отсутствует — выполняется только
декремент таймера без особой tick-логики.

Поддержка тика через `ConditionDef.Tick`:

| Поле | Назначение |
|------|------------|
| `FlatDamage` | фиксированный урон за ход |
| `DamageDice` | кости урона за ход (`Count` + `DiceType`) |
| `DamageType` | семантический тип урона (ключ/тег для UI/других систем) |
| `TickPatch` | дополнительный `CharacterParamsPatchData` на каждом тике |

Урон вычитается из сырого `HitPoints` (clamp до `0`).

Condition **без** таймера (`ConditionDuration` отсутствует / `0`): только `Add`/`Set` в `Parameters`; в `StatusEffects` не пишется; снимать вручную через `UnapplyFeat` (лечение, Escape и т.п.).

> **Важно:** ключи вроде `"Perception": -1` в `Add` меняют **сырое** значение. Если параметр считается формулой в `RuleDef.ParameterFormulas`, кладите штраф в сырой ключ с суффиксом (например `Perception.Bonus`) — иначе ломаете контракт «итог только через proxy».

### Как наложить / снять / тикнуть счётчик

**Наложить** (после провала проверки / крита):

```csharp
const string FEAR_FEAT_ID = "_ConditionFrightened";

// Предпочтительно: overload на CharacterStateData
// (сам прокинет Parameters + StatusEffects)
CharacterParametersOperator.ApplyFeat(character, FEAR_FEAT_ID, definitionsManager);

// Эквивалент на слепке / DTO:
characterData.ApplyFeat(FEAR_FEAT_ID, definitionsManager);
```

После Apply:

| Словарь | Ключ | Значение |
|---------|------|----------|
| `Parameters` | `Perception.Bonus` | было −1 (Add) |
| `Parameters` | `_ConditionFrightened` | `1` (Set) |
| `StatusEffects` | `_ConditionFrightened` | `3` (= `ConditionDuration`) |

**Refresh** (повторное наложение, пока эффект ещё висит):

```csharp
CharacterParametersOperator.ApplyFeat(character, FEAR_FEAT_ID, definitionsManager);
// StatusEffects["_ConditionFrightened"] снова = 3
// Perception.Bonus НЕ уменьшается второй раз
```

То же для Condition с уроном (через `ConditionDef.Tick`):

- урон за ход задаётся полями `ConditionDef.Tick`, а не копится в `Parameters`;
- повторный `ApplyFeat` **не удваивает** урон и не добавляет второй тик;
- обновляется только таймер до `ConditionDuration` из настроек дефа;
- каждый `EndCharacterTurn` по-прежнему снимает те же N HP, просто статус живёт дольше.

**Снять вручную** (лечение / успешный save / Escape):

```csharp
CharacterParametersOperator.UnapplyFeat(character, FEAR_FEAT_ID, definitionsManager);
// Parameters: откат Add/Set; StatusEffects: ключ удалён
```

**Декремент счётчика в конце хода** — только через state-action:

```csharp
stateLogic.ProcessAction(new EndCharacterTurnStateAction(
    new EndCharacterTurnRequestData
    {
        CharacterId = characterId,
    },
    definitionsManager));
```

Что делает `EndCharacterTurnStateAction` для каждой записи в `StatusEffects`:

1. Резолвит `FeatDef` по ключу; пропускает не-timed / не-Condition.
2. Если есть `ConditionDef` с тем же id — выполняет его tick-обработчики (урон/патч и т.п.).
3. `remaining = value - 1`.
4. Если `remaining > 0` → пишет обратно в `StatusEffects`.
5. Если `remaining <= 0` → `UnapplyFeat(character, featId, …)` (откат механики + удаление ключа).

Пример с `_ConditionFrightened` (`Duration = 3`):

| Момент | `StatusEffects[_ConditionFrightened]` | `Parameters` |
|--------|----------------------------------------|--------------|
| После Apply | `3` | штраф / флаг наложены |
| После 1-го `EndCharacterTurn` | `2` | без изменений (кроме тиков) |
| После 2-го | `1` | без изменений |
| После 3-го | ключ удалён | `UnapplyFeat`: штраф/флаг сняты |

Не делайте так:

```csharp
// Плохо: ручной декремент в обход Unapply / тиков
character.StatusEffects[featId]--;
```

Персистентность: если эффект должен пережить выход из боя — меняйте живой `CharacterStateData` через state-actions (или слепок + `UpdateCharacter`). Эфемерные боевые копии — Apply/Unapply на session-персонаже без записи в профиль (см. [Battle.md](Battle.md)).

---

## 5. Краткая шпаргалка «когда что вызывать»

| Ситуация | Apply | Unapply / тик |
|----------|-------|----------------|
| Выбрали ancestry / background / class feature | да, по спискам `Features` | при отмене выбора в UI |
| Выбрали option у feat с `Options` | Apply **выбранного** child id | отмена выбора |
| Новый уровень | Apply feats этого уровня (+ ручные `ApplyPatch`) | правка слепка до `ProcessAction` |
| Надели предмет с `ItemDef.Features` в **носимый** слот (не `Bag`) | Apply всех Features | при снятии / переносе в `Bag` / удалении |
| Крит / провал / боевой дебаф (Condition) | `ApplyFeat` condition-id | `EndCharacterTurn` (таймер) / `UnapplyFeat` (лечение) |
| Refresh того же Condition | повторный `ApplyFeat` (только таймер) | — |
| Преген / чит с готовым словарём | можно не вызывать Apply, если `Parameters` уже содержат итог сырых ключей | — |

---

## Чего оператор пока не делает

- Не пишет в `Spells`.
- Для **не-Condition** feat / `ConditionDuration <= 0` — не трогает `StatusEffects` (только `Parameters`).
- Не clamp’ит текущие `HitPoints` при падении `MaxHitPoints` — при необходимости делайте в вызывающем коде после Apply/Unapply.
- `ItemDef.Features` в equip-экшенах уже подключены; create/level-up UI и бой — вызывайте ApplyFeat явно.
- `EndCharacterTurn` нужно вызывать из turn-loop боя / adventure (автоподписки на ход пока нет).

---

## См. также

- Контракт патча и JSON: [Definitions.md](Definitions.md) (`FeatDef`, `CharacterParamsPatchData`)
- Read / Write API и пример `UpdateCharacter`: [State.md](State.md)
- Ограничения по параметрам персонажа: [Restrictions.md](Restrictions.md)

---

## Разбор симуляции (пошагово)

Ниже — пошаговая симуляция на реальных дефах (дварф + воин + батрак) и на иллюстративных предметах/статусах. У текущих item JSON поле `Features` ещё не заполнено — кейсы C/D показывают целевой контракт.

### Общие правила симуляции

Каждый `ApplyFeat(id)`:
1. читает `FeatDef.Apply`;
2. для timed Condition, если ключ уже есть в `StatusEffects` — только refresh таймера и выход;
3. делает `Add` (`+=`), потом `Set` (`=`);
4. каскадит `AlsoApplyFeatIds`;
5. если `Type=Condition` и `ConditionDuration > 0` — пишет `StatusEffects[featId] = ConditionDuration`.

`UnapplyFeat` — наоборот: сначала каскад с конца, потом `Add` с минусом, `Set`: ненулевое → `0`, `0` → `1`; для timed Condition удаляет ключ из `StatusEffects`.

Итог `MaxHitPoints` / `Athletics` **не лежит** в словаре — его даёт `CharacterParametersProxy` после изменения сырых ключей.

```mermaid
flowchart TD
  create["Create: Features ancestry/bg/class + Options"]
  level["Level-up: snapshot + Apply feats level N"]
  equip["Equip: ItemDef.Features Apply"]
  unequip["Unequip/swap: Unapply then Apply"]
  combat["Fail/crit: Condition Feat Apply"]
  turnEnd["EndCharacterTurn"]
  create --> params["Parameters"]
  level --> params
  equip --> params
  unequip --> params
  combat --> params
  combat --> status["StatusEffects timer"]
  turnEnd --> status
  turnEnd -->|"Unapply at 0"| params
  params --> proxy["Proxy totals"]
```

### Кейс A. Создание персонажа

**Стартовый скелет** (до черт), упрощённо — все abilities = 0, `Level = 1`:

| Ключ | Значение |
|------|----------|
| `Level` | 1 |
| `STR`…`CHA` | 0 |

#### A1. Background `_Farmhand` → `Features: ["_AssuranceAthletics"]`

Допустим у `_AssuranceAthletics` есть флаг в `Set`. После Apply:

| Ключ | Значение |
|------|----------|
| `_AssuranceAthletics` | 1 |

Если в `Apply` пусто — warning, словарь не меняется.

#### A2. Ancestry `Dwarf` level 1 → `_DwarfAttributeBoosts`

`Add: CON+1, WIS+1, CHA-1` + `Set: _DwarfAttributeBoosts=1`

| Ключ | Было | Стало |
|------|------|-------|
| `CON` | 0 | **1** |
| `WIS` | 0 | **1** |
| `CHA` | 0 | **-1** |
| `_DwarfAttributeBoosts` | — | **1** |

Дальше `_Darkvision` → только `Set: _Darkvision=1`.  
Choice-обёртки вроде `_DwarfHeritage` / `_DwarfAncestryFeat1` без `Apply` — no-op; игрок отдельно Apply’ит выбранный child из `Options`.

#### A3. Class `Fighter` level 1

Список включает `_FighterKeyAttribute` — **обёртка с Options**, без `Apply`. Игрок выбирает `_FighterKeyStrength`:

`Add: STR+1`, `Set: _FighterKeyStrength=1`

| Ключ | Было | Стало |
|------|------|-------|
| `STR` | 0 | **1** |
| `_FighterKeyStrength` | — | **1** |

`_TrainAthletics` (если выдан через initial skill):

`Set: Athletics.ProfRank=1`, `_TrainAthletics=1`

| Ключ | Стало |
|------|-------|
| `Athletics.ProfRank` | **1** (`Trained`) |
| `_TrainAthletics` | **1** |

#### A4. Persist

`CreateCharacterStateAction` кладёт **готовый** словарь `Parameters` в state.  
Proxy: `GetTotalValue("Athletics")` ≈ `STR + PROFICIENCY + ITEMS` — уже ненулевой из‑за STR и ProfRank.

### Кейс B. Повышение уровня (1 → 2, потом к 3)

Персонаж уже в state. UI:

```text
snapshot = CreateCharacterRequestSnapshot(character)
ApplyPatch(Level += 1)     // Level: 1 → 2
ApplyFeat для Features[2]  // _FighterFeat2 (choice), _SkillFeatChoice (choice)
ProcessAction(UpdateCharacter)
```

На 2 уровне у Fighter в JSON в основном **choice-обёртки** — механический Apply часто только у выбранного child.

**Переход на 3:** среди Features есть `_Bravery`:

`Set: Will.ProfRank=2`, `_Bravery=1`

| Ключ | Было | После Apply `_Bravery` |
|------|------|------------------------|
| `Level` | 3 | 3 |
| `Will.ProfRank` | 0/1 | **2** (`Expert`) |
| `_Bravery` | — | **1** |

Если в UI игрок «отменил» выбор `_Bravery` до `ProcessAction`:

`UnapplyFeat("_Bravery")` → `Will.ProfRank=0`, `_Bravery=0` (инверсия `Set`).

Wholesale `UpdateCharacter` потом просто записывает слепок как есть.

### Кейс C. Экипировка магического предмета

Предмет (иллюстрация; в текущих item JSON `Features` ещё нет):

```json
"_AmuletOfMightyFists": {
  "AvailableSlots": ["Neck"],
  "Features": ["_ItemBonusAthletics1"]
}
```

`_ItemBonusAthletics1.Apply`:

```json
"Add": { "Athletics.ItemsBonus": 1 },
"Set": { "_ItemBonusAthletics1": 1 }
```

**Надеть в `Neck`:** Apply Features.

| Ключ | До | После |
|------|----|-------|
| `Athletics.ItemsBonus` | 0 | **1** |
| `_ItemBonusAthletics1` | — | **1** |

**Положить в `Bag` (с `Neck` или из общего инвентаря без ношения):** Features **не** применяются / при уходе с `Neck` в `Bag` — **Unapply**.

**Снять в общий инвентарь** с `Neck` → `UnapplyFeat`.

### Кейс D. Проклятый предмет + swap / перенос в Bag

Проклятие (иллюстрация):

```json
"_CursedRingOfWeakness": {
  "AvailableSlots": ["Finger"],
  "Features": ["_CurseStrengthPenalty"]
}
```

`_CurseStrengthPenalty.Apply`: `Add: { "STR": -2 }`, `Set: { "_CurseStrengthPenalty": 1 }`

**Надели на `Finger`:** `STR` 1 → **-1**, флаг проклятия = 1.

**Перенос `Finger` → `Bag`:** `UnapplyFeat("_CurseStrengthPenalty")` → `STR` обратно, флаг → 0. В мешке кольцо «молчит».

**Swap** носимый↔носимый (например два `Finger`): Unapply старого в целевом слоте (если был) → Apply нового; уходящий в другой носимый слот Features сохраняет (или при полном swap — симметрично пересчитать оба).

**Swap** носимый ↔ `Bag`: уходящий в `Bag` — Unapply; приходящий из `Bag` в носимый — Apply.

### Кейс E. Провал проверки / боевой дебаф (Condition)

Триггер: провал Will save → `_ConditionFrightened` (`Type=Condition`, `ConditionDuration=3`):

```json
"Type": "Condition",
"Apply": {
  "Add": { "Perception.Bonus": -1 },
  "Set": { "_ConditionFrightened": 1 },
  "ConditionDuration": 3
}
```

| Шаг | Parameters | StatusEffects |
|-----|------------|---------------|
| До | … | — |
| `ApplyFeat("_ConditionFrightened")` | `Perception.Bonus` −= 1, флаг = 1 | `_ConditionFrightened` = **3** |
| Повторный `ApplyFeat` (refresh) | без изменений | снова **3** |
| `EndCharacterTurn` ×1 | без изменений (если нет tick-настроек в `ConditionDef`) | **2** |
| `EndCharacterTurn` ×2 | без изменений | **1** |
| `EndCharacterTurn` ×3 | Unapply: Bonus обратно, флаг = 0 | ключ удалён |

Оператор сам пишет/снимает таймер в `StatusEffects` для timed Condition. Декремент — только через `EndCharacterTurnStateAction`.

### Кейс F. Откат ошибочного Apply (симметрия)

Было после `_DwarfAttributeBoosts`: `CON=1, WIS=1, CHA=-1`.

`UnapplyFeat("_DwarfAttributeBoosts")`:

| Ключ | Расчёт | Стало |
|------|--------|-------|
| `CON` | 1 + (−1) | **0** |
| `WIS` | 1 + (−1) | **0** |
| `CHA` | −1 + (−(−1)) = −1+1 | **0** |
| `_DwarfAttributeBoosts` | Set был 1 → | **0** |

Словарь вернулся к состоянию до этой черты (если других патчей на те же ключи не было).

### Сводка: где крутится Apply и как попадает в сейв

| Кейс | Где крутится Apply | Как попадает в сейв |
|------|--------------------|---------------------|
| Создание | на `CharacterRequestData` до Create | `CreateCharacterStateAction` |
| Уровень | на слепке `UpdateCharacterRequestData` | `UpdateCharacterStateAction` wholesale |
| Экипировка | `CharacterItemFeaturesOperator` в Equip/Unequip/Move/Remove | тот же state-action |
| Бой/проверка | на живом `CharacterStateData` или session-копии | отдельный state-action / эфемерно |
| Конец хода | `EndCharacterTurnStateAction` | тот же ProcessAction → сейв state |
