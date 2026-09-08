import { ADVENTURE_TYPES, type AdventureData, type AdventureType } from "../domain";
import { joinLines, parseLines } from "./lists";
import { RestrictionsEditor } from "./RestrictionsEditor";

type Props = {
  adventure: AdventureData;
  onChange: (patch: Partial<AdventureData>) => void;
};

export function AdventureMetaForm({ adventure, onChange }: Props) {
  return (
    <section className="panel meta-panel">
      <h2>Adventure</h2>
      <label className="field">
        <span>Type</span>
        <select
          value={adventure.Type}
          onChange={(event) => onChange({ Type: event.target.value as AdventureType })}
        >
          {ADVENTURE_TYPES.map((type) => (
            <option key={type} value={type}>
              {type}
            </option>
          ))}
        </select>
      </label>
      <label className="field checkbox">
        <input
          type="checkbox"
          checked={adventure.Disabled}
          onChange={(event) => onChange({ Disabled: event.target.checked })}
        />
        <span>Disabled</span>
      </label>
      <label className="field checkbox">
        <input
          type="checkbox"
          checked={adventure.IsRepeatable}
          onChange={(event) => onChange({ IsRepeatable: event.target.checked })}
        />
        <span>Is Repeatable</span>
      </label>
      <label className="field">
        <span>Title</span>
        <input
          value={adventure.Title ?? ""}
          onChange={(event) => onChange({ Title: event.target.value })}
        />
      </label>
      <label className="field">
        <span>Description</span>
        <textarea
          rows={4}
          value={adventure.Description ?? ""}
          onChange={(event) => onChange({ Description: event.target.value })}
        />
      </label>
      <label className="field">
        <span>Tags</span>
        <textarea
          rows={2}
          value={joinLines(adventure.Tags)}
          onChange={(event) => onChange({ Tags: parseLines(event.target.value) })}
        />
      </label>
      <label className="field">
        <span>Ignored Tags</span>
        <textarea
          rows={2}
          value={joinLines(adventure.IgnoredTags)}
          onChange={(event) => onChange({ IgnoredTags: parseLines(event.target.value) })}
        />
      </label>
      <label className="field">
        <span>Adventure Links</span>
        <textarea
          rows={2}
          value={joinLines(adventure.AdventureLinks)}
          onChange={(event) => onChange({ AdventureLinks: parseLines(event.target.value) })}
        />
      </label>
      <label className="field">
        <span>Start Scenes</span>
        <textarea
          rows={3}
          value={joinLines(adventure.StartScenes)}
          onChange={(event) => onChange({ StartScenes: parseLines(event.target.value) })}
        />
      </label>
      <RestrictionsEditor
        restrictions={adventure.Restrictions}
        onChange={(Restrictions) => onChange({ Restrictions })}
      />
    </section>
  );
}
