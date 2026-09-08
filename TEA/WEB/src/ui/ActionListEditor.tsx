import {
  CHOICE_ACTION_CREATE_OPTIONS,
  ChoiceActions,
  KNOWN_WINDOW_IDS,
  getSceneId,
  isSceneTransitionAction,
  moveItem,
  selectionAfterSwap,
  setGoToSceneId,
  type ChoiceActionData,
} from "../domain";
import { BoolMapEditor, IntMapEditor, StringMapEditor } from "./ParamsMapEditor";
import { useScrollSelectedIntoView } from "./useScrollSelectedIntoView";

export type ActionListProps = {
  title: string;
  actions: ChoiceActionData[];
  selectedIndex: number | null;
  sceneIds: string[];
  onSelect: (index: number | null) => void;
  onChange: (next: ChoiceActionData[]) => void;
};

function patchStrings(action: ChoiceActionData, strings: Record<string, string>): ChoiceActionData {
  return {
    ...action,
    Params: { ...action.Params, Strings: strings },
  };
}

function patchInts(action: ChoiceActionData, ints: Record<string, number>): ChoiceActionData {
  return {
    ...action,
    Params: { ...action.Params, Ints: ints },
  };
}

function patchStringKey(action: ChoiceActionData, key: string, value: string): ChoiceActionData {
  return patchStrings(action, { ...action.Params.Strings, [key]: value });
}

function patchIntKey(action: ChoiceActionData, key: string, value: number): ChoiceActionData {
  return patchInts(action, { ...action.Params.Ints, [key]: value });
}

function windowOptions(current: string): string[] {
  if (current.length > 0 && !KNOWN_WINDOW_IDS.includes(current as (typeof KNOWN_WINDOW_IDS)[number])) {
    return [current, ...KNOWN_WINDOW_IDS];
  }
  return [...KNOWN_WINDOW_IDS];
}

function ActionListItem({
  index,
  type,
  selected,
  onSelect,
}: {
  index: number;
  type: string;
  selected: boolean;
  onSelect: (index: number | null) => void;
}) {
  const ref = useScrollSelectedIntoView<HTMLButtonElement>(selected);
  return (
    <button
      ref={ref}
      type="button"
      className={selected ? "scene-item selected" : "scene-item"}
      onClick={() => onSelect(index)}
    >
      {index}: {type}
    </button>
  );
}

function SelectedActionEditor({
  action,
  sceneIds,
  onChange,
  onRemove,
}: {
  action: ChoiceActionData;
  sceneIds: string[];
  onChange: (next: ChoiceActionData) => void;
  onRemove: () => void;
}) {
  const type = action.Type;
  return (
    <div className="selected-editor">
      <h3>Selected Action</h3>
      <p className="hint">Type: {type}</p>
      {type === "SetWorldParams" || type === "SetAdventureParams" || type === "SetGlobalParams" ? (
        <div>
          <h4>Params</h4>
          <StringMapEditor
            title="Strings"
            values={action.Params.Strings}
            onChange={(strings) => onChange(patchStrings(action, strings))}
          />
          <IntMapEditor
            title="Ints"
            values={action.Params.Ints}
            onChange={(ints) => onChange(patchInts(action, ints))}
          />
          <BoolMapEditor
            title="Bools"
            values={action.Params.Bools}
            onChange={(bools) => onChange({ ...action, Params: { ...action.Params, Bools: bools } })}
          />
        </div>
      ) : null}
      {type === "SetCharacterParameter" ? (
        <>
          <label className="field">
            <span>Parameter Key</span>
            <input
              list="tea-character-parameter-keys"
              value={action.Params.Strings[ChoiceActions.PARAMETER_KEY] ?? ""}
              onChange={(event) => onChange(patchStringKey(action, ChoiceActions.PARAMETER_KEY, event.target.value))}
            />
          </label>
          <label className="field">
            <span>Parameter Value</span>
            <input
              type="number"
              value={action.Params.Ints[ChoiceActions.PARAMETER_VALUE] ?? 0}
              onChange={(event) =>
                onChange(patchIntKey(action, ChoiceActions.PARAMETER_VALUE, Number.parseInt(event.target.value, 10) || 0))
              }
            />
          </label>
        </>
      ) : null}
      {type === "AddCharacterParameter" ? (
        <>
          <label className="field">
            <span>Parameter Key</span>
            <input
              list="tea-character-parameter-keys"
              value={action.Params.Strings[ChoiceActions.PARAMETER_KEY] ?? ""}
              onChange={(event) => onChange(patchStringKey(action, ChoiceActions.PARAMETER_KEY, event.target.value))}
            />
          </label>
          <label className="field">
            <span>Parameter Delta</span>
            <input
              type="number"
              value={action.Params.Ints[ChoiceActions.PARAMETER_DELTA] ?? 0}
              onChange={(event) =>
                onChange(patchIntKey(action, ChoiceActions.PARAMETER_DELTA, Number.parseInt(event.target.value, 10) || 0))
              }
            />
          </label>
        </>
      ) : null}
      {type === "GoToAdventure" ? (
        <label className="field">
          <span>Target Adventure</span>
          <input
            value={action.Params.Strings[ChoiceActions.ADVENTURE_ID] ?? ""}
            onChange={(event) => onChange(patchStringKey(action, ChoiceActions.ADVENTURE_ID, event.target.value))}
          />
        </label>
      ) : null}
      {type === "GoToRandomAdventure" ? (
        <p className="hint">No params. Runtime picks a random eligible adventure (excludes HUB tag; more filters TBD).</p>
      ) : null}
      {type === "GoToRandomScene" ? (
        <label className="field">
          <span>Scene Ids ('{ChoiceActions.SCENE_IDS_SEPARATOR}')</span>
          <input
            value={action.Params.Strings[ChoiceActions.SCENE_ID] ?? ""}
            onChange={(event) => onChange(patchStringKey(action, ChoiceActions.SCENE_ID, event.target.value))}
          />
        </label>
      ) : null}
      {type === "OpenWindow" ? (
        <label className="field">
          <span>Window Id</span>
          <select
            value={action.Params.Strings[ChoiceActions.WINDOW_ID] ?? KNOWN_WINDOW_IDS[0]}
            onChange={(event) => onChange(patchStringKey(action, ChoiceActions.WINDOW_ID, event.target.value))}
          >
            {windowOptions(action.Params.Strings[ChoiceActions.WINDOW_ID] ?? "").map((windowId) => (
              <option key={windowId} value={windowId}>
                {windowId}
              </option>
            ))}
          </select>
        </label>
      ) : null}
      {isSceneTransitionAction(type) ? (
        <label className="field">
          <span>Target Scene</span>
          {sceneIds.length > 0 ? (
            <select
              value={getSceneId(action)}
              onChange={(event) => {
                const next = structuredClone(action);
                setGoToSceneId(next, event.target.value);
                onChange(next);
              }}
            >
              {getSceneId(action).length === 0 || !sceneIds.includes(getSceneId(action)) ? (
                <option value={getSceneId(action)}>{getSceneId(action) || "(empty)"}</option>
              ) : null}
              {sceneIds.map((sceneId) => (
                <option key={sceneId} value={sceneId}>
                  {sceneId}
                </option>
              ))}
            </select>
          ) : (
            <input
              value={getSceneId(action)}
              onChange={(event) => {
                const next = structuredClone(action);
                setGoToSceneId(next, event.target.value);
                onChange(next);
              }}
            />
          )}
        </label>
      ) : null}
      <div className="button-row">
        <button type="button" onClick={onRemove}>
          Remove Selected Action
        </button>
      </div>
    </div>
  );
}

export function ActionListEditor({
  title,
  actions,
  selectedIndex,
  sceneIds,
  onSelect,
  onChange,
}: ActionListProps) {
  const selectedAction =
    selectedIndex != null && selectedIndex >= 0 && selectedIndex < actions.length
      ? actions[selectedIndex]
      : null;

  return (
    <div className="action-list-editor">
      <h3>{title}</h3>
      <div className="button-row wrap">
        {CHOICE_ACTION_CREATE_OPTIONS.map((option) => (
          <button
            key={option.id}
            type="button"
            title={option.label}
            onClick={() => {
              const next = [...actions, option.create()];
              onChange(next);
              onSelect(next.length - 1);
            }}
          >
            {option.label}
          </button>
        ))}
      </div>
      <div className="item-list">
        {actions.map((action, index) => (
          <div key={index} className="item-row">
            <ActionListItem
              index={index}
              type={action.Type}
              selected={index === selectedIndex}
              onSelect={onSelect}
            />
            <button
              type="button"
              disabled={index === 0}
              onClick={() => {
                const moved = moveItem(actions, index, -1);
                onChange(moved.items);
                onSelect(selectionAfterSwap(selectedIndex, index, moved.selectedIndex));
              }}
            >
              ↑
            </button>
            <button
              type="button"
              disabled={index === actions.length - 1}
              onClick={() => {
                const moved = moveItem(actions, index, 1);
                onChange(moved.items);
                onSelect(selectionAfterSwap(selectedIndex, index, moved.selectedIndex));
              }}
            >
              ↓
            </button>
            <button
              type="button"
              onClick={() => {
                onChange(actions.filter((_, itemIndex) => itemIndex !== index));
                if (selectedIndex == null) {
                  return;
                }
                if (selectedIndex === index) {
                  onSelect(null);
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
      {selectedAction != null && selectedIndex != null ? (
        <SelectedActionEditor
          action={selectedAction}
          sceneIds={sceneIds}
          onChange={(nextAction) =>
            onChange(actions.map((item, itemIndex) => (itemIndex === selectedIndex ? nextAction : item)))
          }
          onRemove={() => {
            onChange(actions.filter((_, itemIndex) => itemIndex !== selectedIndex));
            onSelect(null);
          }}
        />
      ) : null}
    </div>
  );
}
