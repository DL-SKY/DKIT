import { useMemo, useState } from "react";
import { validateAdventure } from "./domain";
import { AdventureMetaForm } from "./ui/AdventureMetaForm";
import { ChoicesPanel } from "./ui/ChoicesPanel";
import { ContentPanel } from "./ui/ContentPanel";
import { GraphPanel } from "./ui/GraphPanel";
import { LocalizationPanel } from "./ui/LocalizationPanel";
import { ScenesPanel } from "./ui/ScenesPanel";
import { ValidationPanel } from "./ui/ValidationPanel";
import { useEditorSession } from "./ui/useEditorSession";

export default function App() {
  const session = useEditorSession();
  const [graphOpen, setGraphOpen] = useState(false);
  const [localizationOpen, setLocalizationOpen] = useState(false);
  const issues = useMemo(
    () => (session.adventure == null ? [] : validateAdventure(session.adventure)),
    [session.adventure],
  );
  const sceneIds = session.adventure == null ? [] : Object.keys(session.adventure.Scenes);
  const selectedScene =
    session.adventure != null && session.selectedSceneId != null
      ? (session.adventure.Scenes[session.selectedSceneId] ?? null)
      : null;

  const statusText =
    session.adventure == null ? "No file" : session.dirty ? "Modified" : "Saved";
  const saveModeText = session.adventure == null ? "—" : session.saveMode === "same-file" ? "Same file" : "Download only";

  return (
    <div className="app">
      <header className="toolbar">
        <strong className="brand">Web TEA</strong>
        <button type="button" data-testid="toolbar-open" onClick={() => void session.openFile()}>
          Open File
        </button>
        <button type="button" data-testid="toolbar-new-file" onClick={() => session.createNewFile()}>
          New File
        </button>
        <button
          type="button"
          data-testid="toolbar-save"
          onClick={() => void session.save()}
          disabled={session.adventure == null}
        >
          Save
        </button>
        <button
          type="button"
          data-testid="toolbar-save-as"
          onClick={() => void session.saveAs()}
          disabled={session.adventure == null}
        >
          Save As
        </button>
        <button
          type="button"
          data-testid="toolbar-graph"
          className={graphOpen ? "active" : undefined}
          onClick={() => {
            setGraphOpen((open) => !open);
            setLocalizationOpen(false);
          }}
          disabled={session.adventure == null}
        >
          Scene Graph
        </button>
        <button
          type="button"
          data-testid="toolbar-localization"
          className={localizationOpen ? "active" : undefined}
          onClick={() => {
            setLocalizationOpen((open) => !open);
            setGraphOpen(false);
          }}
          disabled={session.adventure == null}
        >
          Localization
        </button>
        <span className="file-name" data-testid="file-name">
          {session.fileName || "No file selected"}
        </span>
        <span className="toolbar-spacer" />
        <span className={session.dirty ? "status modified" : "status saved"} data-testid="file-status">
          {statusText}
        </span>
        <span className="save-mode" data-testid="save-mode">
          {saveModeText}
        </span>
        <input
          ref={session.fileInputRef}
          className="file-input"
          data-testid="file-input"
          type="file"
          accept=".json,application/json"
          onChange={(event) => void session.onFileInputChange(event)}
        />
      </header>
      {session.error != null ? <div className="error-banner">{session.error}</div> : null}
      {session.adventure == null ? (
        <main className="empty-state">
          <h1>Adventure Editor</h1>
          <p>Open a single adventure JSON file or create a new one to start editing.</p>
          <div className="button-row">
            <button type="button" className="primary" data-testid="empty-open-file" onClick={() => void session.openFile()}>
              Open File
            </button>
            <button type="button" className="primary" data-testid="empty-new-file" onClick={() => session.createNewFile()}>
              New File
            </button>
          </div>
          <p className="hint">
            Save writes to the same file in Chromium when allowed. Firefox uses Save As / download.
            The editor cannot see the Unity project folder.
          </p>
        </main>
      ) : (
        <main className="workspace">
          <ScenesPanel
            sceneIds={sceneIds}
            selectedSceneId={session.selectedSceneId}
            selectedScene={selectedScene}
            onSelect={session.setSelectedSceneId}
            onAddEmpty={session.addEmptyScene}
            onAddText={session.addTextScene}
            onRename={session.renameSelectedScene}
            onDuplicate={session.duplicateSelectedScene}
            onDelete={session.removeSelectedScene}
            onChangeScene={session.updateSelectedScene}
          />
          <ContentPanel
            content={selectedScene?.Content ?? []}
            selectedIndex={session.selectedContentIndex}
            onSelect={session.setSelectedContentIndex}
            onChange={(content) => session.updateSelectedScene({ Content: content })}
          />
          <ChoicesPanel
            choices={selectedScene?.Choices ?? []}
            selectedIndex={session.selectedChoiceIndex}
            selectedActionIndex={session.selectedActionIndex}
            diceActionSelection={session.diceActionSelection}
            sceneIds={sceneIds}
            onSelect={session.setSelectedChoice}
            onSelectAction={session.setSelectedActionIndex}
            onSelectDiceAction={session.setDiceActionIndex}
            onChange={(choices) => session.updateSelectedScene({ Choices: choices })}
          />
          <AdventureMetaForm adventure={session.adventure} onChange={session.updateMeta} />
          <ValidationPanel
            issues={issues}
            onFix={session.applyFix}
            onFixAll={session.applyAllFixes}
            onNavigate={session.selectIssueLocation}
          />
          {graphOpen ? (
            <GraphPanel
              key={session.fileName}
              adventure={session.adventure}
              selectedSceneId={session.selectedSceneId}
              onSelectScene={session.setSelectedSceneId}
              onClose={() => setGraphOpen(false)}
            />
          ) : null}
          {localizationOpen ? (
            <LocalizationPanel
              key={`loc:${session.fileName}`}
              adventure={session.adventure}
              fileName={session.fileName}
              onApply={session.applyLocalization}
              onClose={() => setLocalizationOpen(false)}
            />
          ) : null}
        </main>
      )}
    </div>
  );
}
