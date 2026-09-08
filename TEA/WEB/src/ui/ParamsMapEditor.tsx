import { uniqueDictionaryKey } from "./lists";

type StringMapEditorProps = {
  title: string;
  values: Record<string, string>;
  onChange: (next: Record<string, string>) => void;
};

type IntMapEditorProps = {
  title: string;
  values: Record<string, number>;
  onChange: (next: Record<string, number>) => void;
};

type BoolMapEditorProps = {
  title: string;
  values: Record<string, boolean>;
  onChange: (next: Record<string, boolean>) => void;
};

function renameKey<T>(
  source: Record<string, T>,
  oldKey: string,
  requestedKey: string,
  value: T,
): Record<string, T> {
  const nextKey = uniqueDictionaryKey(Object.keys(source), requestedKey, oldKey);
  if (nextKey === oldKey) {
    return { ...source, [oldKey]: value };
  }
  const next: Record<string, T> = {};
  for (const key of Object.keys(source)) {
    if (key === oldKey) {
      next[nextKey] = value;
    } else {
      next[key] = source[key];
    }
  }
  return next;
}

function removeKey<T>(source: Record<string, T>, keyToRemove: string): Record<string, T> {
  const next: Record<string, T> = {};
  for (const key of Object.keys(source)) {
    if (key !== keyToRemove) {
      next[key] = source[key];
    }
  }
  return next;
}

export function StringMapEditor({ title, values, onChange }: StringMapEditorProps) {
  return (
    <div className="map-editor">
      <h4>{title}</h4>
      <div className="button-row">
        <button
          type="button"
          onClick={() => {
            const key = uniqueDictionaryKey(Object.keys(values), "key");
            onChange({ ...values, [key]: "" });
          }}
        >
          Add {title} Param
        </button>
      </div>
      {Object.keys(values).map((key) => (
        <div key={key} className="map-row">
          <input
            value={key}
            aria-label={`${title} key`}
            onChange={(event) => onChange(renameKey(values, key, event.target.value, values[key] ?? ""))}
          />
          <input
            value={values[key] ?? ""}
            aria-label={`${title} value`}
            onChange={(event) => onChange({ ...values, [key]: event.target.value })}
          />
          <button type="button" onClick={() => onChange(removeKey(values, key))}>
            X
          </button>
        </div>
      ))}
    </div>
  );
}

export function IntMapEditor({ title, values, onChange }: IntMapEditorProps) {
  return (
    <div className="map-editor">
      <h4>{title}</h4>
      <div className="button-row">
        <button
          type="button"
          onClick={() => {
            const key = uniqueDictionaryKey(Object.keys(values), "key");
            onChange({ ...values, [key]: 0 });
          }}
        >
          Add {title} Param
        </button>
      </div>
      {Object.keys(values).map((key) => (
        <div key={key} className="map-row">
          <input
            value={key}
            aria-label={`${title} key`}
            onChange={(event) => onChange(renameKey(values, key, event.target.value, values[key] ?? 0))}
          />
          <input
            type="number"
            value={values[key] ?? 0}
            aria-label={`${title} value`}
            onChange={(event) => onChange({ ...values, [key]: Number.parseInt(event.target.value, 10) || 0 })}
          />
          <button type="button" onClick={() => onChange(removeKey(values, key))}>
            X
          </button>
        </div>
      ))}
    </div>
  );
}

export function BoolMapEditor({ title, values, onChange }: BoolMapEditorProps) {
  return (
    <div className="map-editor">
      <h4>{title}</h4>
      <div className="button-row">
        <button
          type="button"
          onClick={() => {
            const key = uniqueDictionaryKey(Object.keys(values), "key");
            onChange({ ...values, [key]: false });
          }}
        >
          Add {title} Param
        </button>
      </div>
      {Object.keys(values).map((key) => (
        <div key={key} className="map-row">
          <input
            value={key}
            aria-label={`${title} key`}
            onChange={(event) => onChange(renameKey(values, key, event.target.value, values[key] ?? false))}
          />
          <label className="field checkbox">
            <input
              type="checkbox"
              checked={values[key] ?? false}
              onChange={(event) => onChange({ ...values, [key]: event.target.checked })}
            />
            <span>true</span>
          </label>
          <button type="button" onClick={() => onChange(removeKey(values, key))}>
            X
          </button>
        </div>
      ))}
    </div>
  );
}
