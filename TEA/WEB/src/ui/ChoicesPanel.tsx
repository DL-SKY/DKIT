import {
  CHOICE_CREATE_OPTIONS,
  CHOICE_TYPES,
  CHARACTER_PARAMETER_KEYS,
  DICE_OPTION_FLAGS,
  DICE_TYPES,
  createDefaultDiceCheck,
  createEmptyVisualOptions,
  formatDiceOptions,
  moveItem,
  parseDiceOptionFlags,
  selectionAfterSwap,
  uniqueChoiceId,
  type ChoiceActionData,
  type ChoiceData,
  type ChoiceDiceCheckData,
  type ChoiceType,
  type DiceType,
  type VisualOptions,
} from "../domain";
import { ActionListEditor } from "./ActionListEditor";
import { joinLines, parseLines } from "./lists";
import { RestrictionsEditor } from "./RestrictionsEditor";
import { useScrollSelectedIntoView } from "./useScrollSelectedIntoView";

export type DiceActionListKey = "OnCriticalSuccess" | "OnSuccess" | "OnFailure" | "OnCriticalFailure";

type DiceActionSelection = Record<DiceActionListKey, number | null>;

type Props = {
  choices: ChoiceData[];
  selectedIndex: number | null;
  selectedActionIndex: number | null;
  diceActionSelection: DiceActionSelection;
  sceneIds: string[];
  onSelect: (index: number | null) => void;
  onSelectAction: (index: number | null) => void;
  onSelectDiceAction: (list: DiceActionListKey, index: number | null) => void;
  onChange: (next: ChoiceData[]) => void;
};

function patchChoice(choices: ChoiceData[], index: number, patch: Partial<ChoiceData>): ChoiceData[] {
  return choices.map((item, itemIndex) => {
    if (itemIndex !== index) {
      return item;
    }
    const next: ChoiceData = { ...item, ...patch };
    if (next.VisualOptions == null) {
      next.VisualOptions = createEmptyVisualOptions();
    }
    if (next.Type === "DiceCheck" && next.DiceCheck == null) {
      next.DiceCheck = createDefaultDiceCheck();
    }
    return next;
  });
}

function visualOf(choice: ChoiceData): VisualOptions {
  return choice.VisualOptions ?? createEmptyVisualOptions();
}

function ChoiceListItem({
  index,
  label,
  selected,
  onSelect,
}: {
  index: number;
  label: string;
  selected: boolean;
  onSelect: () => void;
}) {
  const ref = useScrollSelectedIntoView<HTMLButtonElement>(selected);
  return (
    <button
      ref={ref}
      type="button"
      className={selected ? "scene-item selected" : "scene-item"}
      onClick={onSelect}
    >
      {index}: {label}
    </button>
  );
}

function DiceCheckEditor({
  diceCheck,
  sceneIds,
  selection,
  onSelectDiceAction,
  onChange,
}: {
  diceCheck: ChoiceDiceCheckData;
  sceneIds: string[];
  selection: DiceActionSelection;
  onSelectDiceAction: (list: DiceActionListKey, index: number | null) => void;
  onChange: (next: ChoiceDiceCheckData) => void;
}) {
  const selectedFlags = parseDiceOptionFlags(diceCheck.DiceOptions);

  const patchActions = (list: DiceActionListKey, next: ChoiceActionData[]) => {
    onChange({ ...diceCheck, [list]: next });
  };

  return (
    <div className="dice-check-editor">
      <h3>Dice Check</h3>
      <label className="field">
        <span>Difficulty Class</span>
        <input
          type="number"
          value={diceCheck.DifficultyClass}
          onChange={(event) =>
            onChange({ ...diceCheck, DifficultyClass: Number.parseInt(event.target.value, 10) || 0 })
          }
        />
      </label>
      <label className="field">
        <span>Dice Type</span>
        <select
          value={diceCheck.DiceType}
          onChange={(event) => onChange({ ...diceCheck, DiceType: event.target.value as DiceType })}
        >
          {DICE_TYPES.map((type) => (
            <option key={type} value={type}>
              {type}
            </option>
          ))}
        </select>
      </label>
      <fieldset className="field">
        <legend>Dice Options</legend>
        {DICE_OPTION_FLAGS.map((flag) => (
          <label key={flag} className="field checkbox">
            <input
              type="checkbox"
              checked={selectedFlags.includes(flag)}
              onChange={(event) => {
                const nextFlags = event.target.checked
                  ? [...selectedFlags, flag]
                  : selectedFlags.filter((item) => item !== flag);
                onChange({ ...diceCheck, DiceOptions: formatDiceOptions(nextFlags) });
              }}
            />
            <span>{flag}</span>
          </label>
        ))}
      </fieldset>
      <label className="field">
        <span>Dice Check Param</span>
        <input
          list="tea-character-parameter-keys"
          value={diceCheck.DiceCheckParam}
          onChange={(event) => onChange({ ...diceCheck, DiceCheckParam: event.target.value })}
        />
      </label>
      <ActionListEditor
        title="On Critical Success Actions"
        actions={diceCheck.OnCriticalSuccess}
        selectedIndex={selection.OnCriticalSuccess}
        sceneIds={sceneIds}
        onSelect={(index) => onSelectDiceAction("OnCriticalSuccess", index)}
        onChange={(next) => patchActions("OnCriticalSuccess", next)}
      />
      <ActionListEditor
        title="On Success Actions"
        actions={diceCheck.OnSuccess}
        selectedIndex={selection.OnSuccess}
        sceneIds={sceneIds}
        onSelect={(index) => onSelectDiceAction("OnSuccess", index)}
        onChange={(next) => patchActions("OnSuccess", next)}
      />
      <ActionListEditor
        title="On Failure Actions"
        actions={diceCheck.OnFailure}
        selectedIndex={selection.OnFailure}
        sceneIds={sceneIds}
        onSelect={(index) => onSelectDiceAction("OnFailure", index)}
        onChange={(next) => patchActions("OnFailure", next)}
      />
      <ActionListEditor
        title="On Critical Failure Actions"
        actions={diceCheck.OnCriticalFailure}
        selectedIndex={selection.OnCriticalFailure}
        sceneIds={sceneIds}
        onSelect={(index) => onSelectDiceAction("OnCriticalFailure", index)}
        onChange={(next) => patchActions("OnCriticalFailure", next)}
      />
    </div>
  );
}

export function ChoicesPanel({
  choices,
  selectedIndex,
  selectedActionIndex,
  diceActionSelection,
  sceneIds,
  onSelect,
  onSelectAction,
  onSelectDiceAction,
  onChange,
}: Props) {
  const selected =
    selectedIndex != null && selectedIndex >= 0 && selectedIndex < choices.length
      ? choices[selectedIndex]
      : null;
  const visual = selected == null ? null : visualOf(selected);

  return (
    <section className="panel choices-panel">
      <h2>Choices</h2>
      <div className="button-row wrap">
        {CHOICE_CREATE_OPTIONS.map((option) => (
          <button
            key={option.id}
            type="button"
            onClick={() => {
              const created = option.create(uniqueChoiceId(choices, "new_choice"));
              const next = [...choices, created];
              onChange(next);
              onSelect(next.length - 1);
              onSelectAction(null);
            }}
          >
            {option.label}
          </button>
        ))}
      </div>
      <div className="item-list">
        {choices.map((choice, index) => (
          <div key={index} className="item-row">
            <ChoiceListItem
              index={index}
              label={choice.Id.trim().length === 0 ? `choice_${index}` : choice.Id}
              selected={index === selectedIndex}
              onSelect={() => {
                onSelect(index);
                onSelectAction(null);
              }}
            />
            <button
              type="button"
              disabled={index === 0}
              onClick={() => {
                const moved = moveItem(choices, index, -1);
                onChange(moved.items);
                onSelect(selectionAfterSwap(selectedIndex, index, moved.selectedIndex));
              }}
            >
              ↑
            </button>
            <button
              type="button"
              disabled={index === choices.length - 1}
              onClick={() => {
                const moved = moveItem(choices, index, 1);
                onChange(moved.items);
                onSelect(selectionAfterSwap(selectedIndex, index, moved.selectedIndex));
              }}
            >
              ↓
            </button>
            <button
              type="button"
              onClick={() => {
                onChange(choices.filter((_, itemIndex) => itemIndex !== index));
                if (selectedIndex == null) {
                  return;
                }
                if (selectedIndex === index) {
                  onSelect(null);
                  onSelectAction(null);
                } else if (selectedIndex > index) {
                  onSelect(selectedIndex - 1);
                }
              }}
            >
              X
            </button>
          </div>
        ))}
      </div>
      {selected != null && selectedIndex != null && visual != null ? (
        <div className="selected-editor">
          <h3>Selected Choice</h3>
          <label className="field">
            <span>Choice Id</span>
            <input
              value={selected.Id}
              onChange={(event) => onChange(patchChoice(choices, selectedIndex, { Id: event.target.value }))}
            />
          </label>
          <label className="field">
            <span>Type</span>
            <select
              value={selected.Type}
              onChange={(event) => {
                const nextType = event.target.value as ChoiceType;
                const patch: Partial<ChoiceData> = { Type: nextType };
                if (nextType === "DiceCheck" && selected.DiceCheck == null) {
                  patch.DiceCheck = createDefaultDiceCheck();
                }
                onChange(patchChoice(choices, selectedIndex, patch));
                onSelectAction(null);
              }}
            >
              {CHOICE_TYPES.map((type) => (
                <option key={type} value={type}>
                  {type}
                </option>
              ))}
            </select>
          </label>
          <label className="field">
            <span>Text</span>
            <input
              value={selected.Text ?? ""}
              onChange={(event) => onChange(patchChoice(choices, selectedIndex, { Text: event.target.value }))}
            />
          </label>
          <label className="field">
            <span>Description</span>
            <input
              value={selected.Description ?? ""}
              onChange={(event) =>
                onChange(patchChoice(choices, selectedIndex, { Description: event.target.value }))
              }
            />
          </label>
          <label className="field checkbox">
            <input
              type="checkbox"
              checked={selected.AlwaysShow}
              onChange={(event) =>
                onChange(patchChoice(choices, selectedIndex, { AlwaysShow: event.target.checked }))
              }
            />
            <span>Always Show</span>
          </label>
          <label className="field">
            <span>Tags</span>
            <textarea
              rows={2}
              value={joinLines(selected.Tags)}
              onChange={(event) => onChange(patchChoice(choices, selectedIndex, { Tags: parseLines(event.target.value) }))}
            />
          </label>
          <h3>Visual Options</h3>
          <label className="field">
            <span>Main Icon</span>
            <input
              value={visual.MainIcon}
              onChange={(event) =>
                onChange(
                  patchChoice(choices, selectedIndex, {
                    VisualOptions: { ...visual, MainIcon: event.target.value },
                  }),
                )
              }
            />
          </label>
          <label className="field">
            <span>Description Icon</span>
            <input
              value={visual.DescriptionIcon}
              onChange={(event) =>
                onChange(
                  patchChoice(choices, selectedIndex, {
                    VisualOptions: { ...visual, DescriptionIcon: event.target.value },
                  }),
                )
              }
            />
          </label>
          <label className="field">
            <span>Parameter Override Description</span>
            <input
              list="tea-character-parameter-keys"
              value={visual.ParameterOverrideDescription}
              onChange={(event) =>
                onChange(
                  patchChoice(choices, selectedIndex, {
                    VisualOptions: { ...visual, ParameterOverrideDescription: event.target.value },
                  }),
                )
              }
            />
          </label>
          <RestrictionsEditor
            restrictions={selected.Restrictions}
            onChange={(restrictions) => onChange(patchChoice(choices, selectedIndex, { Restrictions: restrictions }))}
          />
          {selected.Type === "DiceCheck" ? (
            <DiceCheckEditor
              diceCheck={selected.DiceCheck ?? createDefaultDiceCheck()}
              sceneIds={sceneIds}
              selection={diceActionSelection}
              onSelectDiceAction={onSelectDiceAction}
              onChange={(diceCheck) => onChange(patchChoice(choices, selectedIndex, { DiceCheck: diceCheck }))}
            />
          ) : (
            <ActionListEditor
              title="Actions"
              actions={selected.Actions}
              selectedIndex={selectedActionIndex}
              sceneIds={sceneIds}
              onSelect={onSelectAction}
              onChange={(actions) => onChange(patchChoice(choices, selectedIndex, { Actions: actions }))}
            />
          )}
          <div className="button-row">
            <button
              type="button"
              onClick={() => {
                onChange(choices.filter((_, itemIndex) => itemIndex !== selectedIndex));
                onSelect(null);
                onSelectAction(null);
              }}
            >
              Remove Selected Choice
            </button>
          </div>
        </div>
      ) : (
        <p className="hint">Select a choice.</p>
      )}
      <datalist id="tea-character-parameter-keys">
        {CHARACTER_PARAMETER_KEYS.map((key) => (
          <option key={key} value={key} />
        ))}
      </datalist>
    </section>
  );
}
