import type { SceneData } from "../domain";
import { joinLines, parseLines } from "./lists";
import { useScrollSelectedIntoView } from "./useScrollSelectedIntoView";

type Props = {
  sceneIds: string[];
  selectedSceneId: string | null;
  selectedScene: SceneData | null;
  onSelect: (sceneId: string) => void;
  onAddEmpty: () => void;
  onAddText: () => void;
  onRename: () => void;
  onDuplicate: () => void;
  onDelete: () => void;
  onChangeScene: (patch: Partial<Pick<SceneData, "Tags" | "NotClearScene">>) => void;
};

function SceneListItem({
  sceneId,
  selected,
  onSelect,
}: {
  sceneId: string;
  selected: boolean;
  onSelect: (sceneId: string) => void;
}) {
  const ref = useScrollSelectedIntoView<HTMLButtonElement>(selected);
  return (
    <button
      ref={ref}
      type="button"
      className={selected ? "scene-item selected" : "scene-item"}
      onClick={() => onSelect(sceneId)}
    >
      {sceneId}
    </button>
  );
}

export function ScenesPanel({
  sceneIds,
  selectedSceneId,
  selectedScene,
  onSelect,
  onAddEmpty,
  onAddText,
  onRename,
  onDuplicate,
  onDelete,
  onChangeScene,
}: Props) {
  return (
    <section className="panel scenes-panel">
      <h2>Scenes</h2>
      <div className="button-row">
        <button type="button" onClick={onAddEmpty}>
          Empty Scene
        </button>
        <button type="button" onClick={onAddText}>
          Text Scene
        </button>
      </div>
      <div className="scene-list">
        {sceneIds.map((sceneId) => (
          <SceneListItem
            key={sceneId}
            sceneId={sceneId}
            selected={sceneId === selectedSceneId}
            onSelect={onSelect}
          />
        ))}
      </div>
      <div className="button-row">
        <button type="button" onClick={onRename} disabled={selectedSceneId == null}>
          Rename
        </button>
        <button type="button" onClick={onDuplicate} disabled={selectedSceneId == null}>
          Duplicate
        </button>
        <button type="button" onClick={onDelete} disabled={selectedSceneId == null}>
          Delete
        </button>
      </div>
      {selectedScene != null ? (
        <div className="scene-editor">
          <h3>Selected Scene</h3>
          <p className="hint">Id: {selectedScene.Id}</p>
          <label className="field checkbox">
            <input
              type="checkbox"
              checked={selectedScene.NotClearScene}
              onChange={(event) => onChangeScene({ NotClearScene: event.target.checked })}
            />
            <span>Not Clear Scene</span>
          </label>
          <label className="field">
            <span>Tags</span>
            <textarea
              rows={2}
              value={joinLines(selectedScene.Tags)}
              onChange={(event) => onChangeScene({ Tags: parseLines(event.target.value) })}
            />
          </label>
        </div>
      ) : (
        <p className="hint">Select a scene.</p>
      )}
    </section>
  );
}
