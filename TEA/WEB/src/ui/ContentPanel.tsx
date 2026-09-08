import {
  SCENE_CONTENT_CREATE_OPTIONS,
  SCENE_CONTENT_TYPES,
  createSceneContent,
  moveItem,
  selectionAfterSwap,
  type SceneContentData,
  type SceneContentType,
} from "../domain";
import { RestrictionsEditor } from "./RestrictionsEditor";
import { useScrollSelectedIntoView } from "./useScrollSelectedIntoView";

type Props = {
  content: SceneContentData[];
  selectedIndex: number | null;
  onSelect: (index: number | null) => void;
  onChange: (next: SceneContentData[]) => void;
};

function isValuesType(type: SceneContentType): boolean {
  return type === "RandomImage" || type === "Slideshow";
}

function ContentListItem({
  index,
  type,
  selected,
  onSelect,
}: {
  index: number;
  type: SceneContentType;
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

export function ContentPanel({ content, selectedIndex, onSelect, onChange }: Props) {
  const selected =
    selectedIndex != null && selectedIndex >= 0 && selectedIndex < content.length
      ? content[selectedIndex]
      : null;

  return (
    <section className="panel content-panel">
      <h2>Content</h2>
      <div className="button-row wrap">
        {SCENE_CONTENT_CREATE_OPTIONS.map((option) => (
          <button
            key={option.id}
            type="button"
            onClick={() => {
              const next = [...content, createSceneContent(option.type)];
              onChange(next);
              onSelect(next.length - 1);
            }}
          >
            {option.label}
          </button>
        ))}
      </div>
      <div className="item-list">
        {content.map((item, index) => (
          <div key={index} className="item-row">
            <ContentListItem
              index={index}
              type={item.Type}
              selected={index === selectedIndex}
              onSelect={onSelect}
            />
            <button
              type="button"
              disabled={index === 0}
              onClick={() => {
                const moved = moveItem(content, index, -1);
                onChange(moved.items);
                onSelect(selectionAfterSwap(selectedIndex, index, moved.selectedIndex));
              }}
            >
              ↑
            </button>
            <button
              type="button"
              disabled={index === content.length - 1}
              onClick={() => {
                const moved = moveItem(content, index, 1);
                onChange(moved.items);
                onSelect(selectionAfterSwap(selectedIndex, index, moved.selectedIndex));
              }}
            >
              ↓
            </button>
            <button
              type="button"
              onClick={() => {
                onChange(content.filter((_, itemIndex) => itemIndex !== index));
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
      {selected != null && selectedIndex != null ? (
        <div className="selected-editor">
          <h3>Selected Content</h3>
          <label className="field">
            <span>Type</span>
            <select
              value={selected.Type}
              onChange={(event) => {
                const nextType = event.target.value as SceneContentType;
                onChange(
                  content.map((item, itemIndex) =>
                    itemIndex === selectedIndex ? { ...item, Type: nextType } : item,
                  ),
                );
              }}
            >
              {SCENE_CONTENT_TYPES.map((type) => (
                <option key={type} value={type}>
                  {type}
                </option>
              ))}
            </select>
          </label>
          {isValuesType(selected.Type) ? (
            <div>
              <h4>Values</h4>
              {(selected.Values ?? []).map((value, valueIndex) => (
                <div key={valueIndex} className="item-row">
                  <label className="field grow">
                    <span>Value {valueIndex + 1}</span>
                    <input
                      value={value}
                      onChange={(event) => {
                        const values = [...(selected.Values ?? [])];
                        values[valueIndex] = event.target.value;
                        onChange(
                          content.map((item, itemIndex) =>
                            itemIndex === selectedIndex ? { ...item, Values: values } : item,
                          ),
                        );
                      }}
                    />
                  </label>
                  <button
                    type="button"
                    onClick={() => {
                      const values = (selected.Values ?? []).filter((_, itemIndex) => itemIndex !== valueIndex);
                      onChange(
                        content.map((item, itemIndex) =>
                          itemIndex === selectedIndex ? { ...item, Values: values } : item,
                        ),
                      );
                    }}
                  >
                    X
                  </button>
                </div>
              ))}
              <div className="button-row">
                <button
                  type="button"
                  onClick={() => {
                    const values = [...(selected.Values ?? []), ""];
                    onChange(
                      content.map((item, itemIndex) =>
                        itemIndex === selectedIndex ? { ...item, Values: values } : item,
                      ),
                    );
                  }}
                >
                  Add Value
                </button>
              </div>
            </div>
          ) : (
            <label className="field">
              <span>Value</span>
              <textarea
                rows={4}
                value={selected.Value ?? ""}
                onChange={(event) =>
                  onChange(
                    content.map((item, itemIndex) =>
                      itemIndex === selectedIndex ? { ...item, Value: event.target.value } : item,
                    ),
                  )
                }
              />
            </label>
          )}
          <RestrictionsEditor
            restrictions={selected.Restrictions}
            onChange={(restrictions) =>
              onChange(
                content.map((item, itemIndex) =>
                  itemIndex === selectedIndex ? { ...item, Restrictions: restrictions } : item,
                ),
              )
            }
          />
          <div className="button-row">
            <button
              type="button"
              onClick={() => {
                onChange(content.filter((_, itemIndex) => itemIndex !== selectedIndex));
                onSelect(null);
              }}
            >
              Remove Selected Content
            </button>
          </div>
        </div>
      ) : (
        <p className="hint">Select a content block.</p>
      )}
    </section>
  );
}
