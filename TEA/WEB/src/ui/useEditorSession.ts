import { useCallback, useEffect, useMemo, useRef, useState, type ChangeEvent } from "react";
import {
  addScene,
  applyAllValidationFixes,
  applyValidationFix,
  createEmptyScene,
  createNewAdventure,
  createTextScene,
  deleteScene,
  duplicateScene,
  renameScene,
  updateScene,
  type AdventureData,
  type ValidationIssue,
} from "../domain";
import {
  canUseFileSystemAccess,
  isAbortError,
  openAdventureFromFile,
  openAdventureWithPicker,
  saveAdventureAs,
  saveAdventureToHandle,
  type OpenedAdventureFile,
} from "../io/adventureFile";

const UNSAVED_MESSAGE = "Current adventure has unsaved changes. Continue without saving?";

export type SaveMode = "same-file" | "download-only";

function confirmLoseUnsaved(dirty: boolean): boolean {
  if (!dirty) {
    return true;
  }
  return window.confirm(UNSAVED_MESSAGE);
}

function firstSceneId(adventure: AdventureData): string | null {
  if (adventure.StartScenes[0] != null && adventure.Scenes[adventure.StartScenes[0]] != null) {
    return adventure.StartScenes[0];
  }
  const ids = Object.keys(adventure.Scenes);
  return ids[0] ?? null;
}

function ensureJsonFileName(value: string): string {
  const trimmed = value.trim();
  if (trimmed.length === 0) {
    return "new_adventure.json";
  }
  return trimmed.toLowerCase().endsWith(".json") ? trimmed : `${trimmed}.json`;
}

function ensureSceneId(value: string): string {
  const trimmed = value.trim();
  return trimmed.length > 0 ? trimmed : "start";
}

export function useEditorSession() {
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [adventure, setAdventure] = useState<AdventureData | null>(null);
  const [fileName, setFileName] = useState("");
  const [handle, setHandle] = useState<FileSystemFileHandle | null>(null);
  const [dirty, setDirty] = useState(false);
  const [selectedSceneId, setSelectedSceneIdState] = useState<string | null>(null);
  const [selectedContentIndex, setSelectedContentIndex] = useState<number | null>(null);
  const [selectedChoiceIndex, setSelectedChoiceIndex] = useState<number | null>(null);
  const [selectedActionIndex, setSelectedActionIndex] = useState<number | null>(null);
  const [diceActionSelection, setDiceActionSelection] = useState({
    OnCriticalSuccess: null as number | null,
    OnSuccess: null as number | null,
    OnFailure: null as number | null,
    OnCriticalFailure: null as number | null,
  });
  const [error, setError] = useState<string | null>(null);
  const fileAccess = useMemo(() => canUseFileSystemAccess(), []);

  const saveMode: SaveMode = handle != null && fileAccess ? "same-file" : "download-only";

  useEffect(() => {
    const onBeforeUnload = (event: BeforeUnloadEvent) => {
      if (!dirty) {
        return;
      }
      event.preventDefault();
      event.returnValue = "";
    };
    window.addEventListener("beforeunload", onBeforeUnload);
    return () => window.removeEventListener("beforeunload", onBeforeUnload);
  }, [dirty]);

  const applyOpened = useCallback((opened: OpenedAdventureFile) => {
    setAdventure(opened.adventure);
    setFileName(opened.fileName);
    setHandle(opened.handle);
    setDirty(false);
    setSelectedSceneIdState(firstSceneId(opened.adventure));
    setSelectedContentIndex(null);
    setSelectedChoiceIndex(null);
    setSelectedActionIndex(null);
    setDiceActionSelection({
      OnCriticalSuccess: null,
      OnSuccess: null,
      OnFailure: null,
      OnCriticalFailure: null,
    });
    setError(null);
  }, []);

  const patchAdventure = useCallback((next: AdventureData) => {
    setAdventure(next);
    setDirty(true);
  }, []);

  const openFile = useCallback(async () => {
    setError(null);
    try {
      if (fileAccess) {
        if (!confirmLoseUnsaved(dirty)) {
          return;
        }
        const opened = await openAdventureWithPicker();
        if (opened != null) {
          applyOpened(opened);
        }
        return;
      }
      fileInputRef.current?.click();
    } catch (caught) {
      if (isAbortError(caught)) {
        return;
      }
      setError(caught instanceof Error ? caught.message : "Failed to open file.");
    }
  }, [applyOpened, dirty, fileAccess]);

  const createNewFile = useCallback(() => {
    if (!confirmLoseUnsaved(dirty)) {
      return;
    }
    const requested = window.prompt("File name", "new_adventure.json");
    if (requested == null) {
      return;
    }
    const requestedSceneId = window.prompt("Start scene id", "start");
    if (requestedSceneId == null) {
      return;
    }
    const nextFileName = ensureJsonFileName(requested);
    const startSceneId = ensureSceneId(requestedSceneId);
    const freshAdventure = createNewAdventure(startSceneId);
    setAdventure(freshAdventure);
    setFileName(nextFileName);
    setHandle(null);
    setDirty(true);
    setSelectedSceneIdState(startSceneId);
    setSelectedContentIndex(null);
    setSelectedChoiceIndex(null);
    setSelectedActionIndex(null);
    setDiceActionSelection({
      OnCriticalSuccess: null,
      OnSuccess: null,
      OnFailure: null,
      OnCriticalFailure: null,
    });
    setError(null);
  }, [dirty]);

  const onFileInputChange = useCallback(
      async (event: ChangeEvent<HTMLInputElement>) => {
      const file = event.target.files?.[0];
      event.target.value = "";
      if (file == null) {
        return;
      }
      if (!confirmLoseUnsaved(dirty)) {
        return;
      }
      try {
        const opened = await openAdventureFromFile(file);
        applyOpened(opened);
      } catch (caught) {
        setError(caught instanceof Error ? caught.message : "Failed to open file.");
      }
    },
    [applyOpened, dirty],
  );

  const saveAs = useCallback(async () => {
    if (adventure == null) {
      return;
    }
    setError(null);
    try {
      const result = await saveAdventureAs(adventure, fileName || "adventure.json");
      setFileName(result.fileName);
      setHandle(result.handle);
      setDirty(false);
    } catch (caught) {
      if (isAbortError(caught)) {
        return;
      }
      setError(caught instanceof Error ? caught.message : "Failed to save file.");
    }
  }, [adventure, fileName]);

  const save = useCallback(async () => {
    if (adventure == null) {
      return;
    }
    if (handle == null) {
      await saveAs();
      return;
    }
    setError(null);
    try {
      await saveAdventureToHandle(handle, adventure);
      setDirty(false);
    } catch (caught) {
      if (isAbortError(caught)) {
        return;
      }
      setError(caught instanceof Error ? caught.message : "Failed to save file.");
    }
  }, [adventure, handle, saveAs]);

  const resetNestedSelection = useCallback(() => {
    setSelectedContentIndex(null);
    setSelectedChoiceIndex(null);
    setSelectedActionIndex(null);
    setDiceActionSelection({
      OnCriticalSuccess: null,
      OnSuccess: null,
      OnFailure: null,
      OnCriticalFailure: null,
    });
  }, []);

  const setSelectedSceneId = useCallback(
    (sceneId: string) => {
      setSelectedSceneIdState(sceneId);
      resetNestedSelection();
    },
    [resetNestedSelection],
  );

  const addEmptyScene = useCallback(() => {
    if (adventure == null) {
      return;
    }
    const requested = window.prompt("Scene id", "new_scene");
    if (requested == null) {
      return;
    }
    const next = addScene(adventure, createEmptyScene(requested));
    const createdId = Object.keys(next.Scenes).find((id) => adventure.Scenes[id] == null) ?? requested;
    patchAdventure(next);
    setSelectedSceneIdState(createdId);
    resetNestedSelection();
  }, [adventure, patchAdventure, resetNestedSelection]);

  const addTextScene = useCallback(() => {
    if (adventure == null) {
      return;
    }
    const requested = window.prompt("Scene id", "new_scene");
    if (requested == null) {
      return;
    }
    const next = addScene(adventure, createTextScene(requested));
    const createdId = Object.keys(next.Scenes).find((id) => adventure.Scenes[id] == null) ?? requested;
    patchAdventure(next);
    setSelectedSceneIdState(createdId);
    resetNestedSelection();
  }, [adventure, patchAdventure, resetNestedSelection]);

  const removeSelectedScene = useCallback(() => {
    if (adventure == null || selectedSceneId == null) {
      return;
    }
    if (!window.confirm(`Delete scene '${selectedSceneId}'?`)) {
      return;
    }
    const next = deleteScene(adventure, selectedSceneId);
    patchAdventure(next);
    setSelectedSceneIdState(firstSceneId(next));
    resetNestedSelection();
  }, [adventure, patchAdventure, resetNestedSelection, selectedSceneId]);

  const duplicateSelectedScene = useCallback(() => {
    if (adventure == null || selectedSceneId == null) {
      return;
    }
    const next = duplicateScene(adventure, selectedSceneId);
    if (next == null) {
      return;
    }
    const createdId = Object.keys(next.Scenes).find((id) => adventure.Scenes[id] == null);
    patchAdventure(next);
    if (createdId != null) {
      setSelectedSceneIdState(createdId);
      resetNestedSelection();
    }
  }, [adventure, patchAdventure, resetNestedSelection, selectedSceneId]);

  const renameSelectedScene = useCallback(() => {
    if (adventure == null || selectedSceneId == null) {
      return;
    }
    const requested = window.prompt("Rename scene", selectedSceneId);
    if (requested == null) {
      return;
    }
    const result = renameScene(adventure, selectedSceneId, requested);
    if (!result.ok) {
      setError(result.error);
      return;
    }
    patchAdventure(result.adventure);
    setSelectedSceneIdState(result.newSceneId);
  }, [adventure, patchAdventure, selectedSceneId]);

  const updateMeta = useCallback(
    (patch: Partial<AdventureData>) => {
      if (adventure == null) {
        return;
      }
      patchAdventure({ ...adventure, ...patch });
    },
    [adventure, patchAdventure],
  );

  const updateSelectedScene = useCallback(
    (patch: Parameters<typeof updateScene>[2]) => {
      if (adventure == null || selectedSceneId == null) {
        return;
      }
      patchAdventure(updateScene(adventure, selectedSceneId, patch));
    },
    [adventure, patchAdventure, selectedSceneId],
  );

  const setSelectedChoice = useCallback((index: number | null) => {
    setSelectedChoiceIndex(index);
    setSelectedActionIndex(null);
    setDiceActionSelection({
      OnCriticalSuccess: null,
      OnSuccess: null,
      OnFailure: null,
      OnCriticalFailure: null,
    });
  }, []);

  const setDiceActionIndex = useCallback((list: keyof typeof diceActionSelection, index: number | null) => {
    setDiceActionSelection((current) => ({ ...current, [list]: index }));
  }, []);

  const applyFix = useCallback(
    (issue: ValidationIssue) => {
      if (adventure == null || issue.applyFix == null) {
        return;
      }
      patchAdventure(applyValidationFix(adventure, issue));
    },
    [adventure, patchAdventure],
  );

  const applyAllFixes = useCallback(() => {
    if (adventure == null) {
      return;
    }
    patchAdventure(applyAllValidationFixes(adventure));
  }, [adventure, patchAdventure]);

  const applyLocalization = useCallback(
    (next: AdventureData) => {
      patchAdventure(next);
    },
    [patchAdventure],
  );

  const selectIssueLocation = useCallback(
    (issue: ValidationIssue) => {
      const location = issue.location;
      if (adventure == null || location == null || location.scope === "adventure") {
        return;
      }
      if (adventure.Scenes[location.sceneId] == null) {
        return;
      }
      setSelectedSceneIdState(location.sceneId);
      if (location.scope === "scene") {
        resetNestedSelection();
        return;
      }
      if (location.scope === "content") {
        setSelectedContentIndex(location.contentIndex);
        setSelectedChoiceIndex(null);
        setSelectedActionIndex(null);
        setDiceActionSelection({
          OnCriticalSuccess: null,
          OnSuccess: null,
          OnFailure: null,
          OnCriticalFailure: null,
        });
        return;
      }
      setSelectedContentIndex(null);
      setSelectedChoiceIndex(location.choiceIndex);
      setSelectedActionIndex(location.scope === "action" ? location.actionIndex : null);
      setDiceActionSelection({
        OnCriticalSuccess: null,
        OnSuccess: null,
        OnFailure: null,
        OnCriticalFailure: null,
      });
    },
    [adventure, resetNestedSelection],
  );

  return {
    fileInputRef,
    adventure,
    fileName,
    dirty,
    selectedSceneId,
    setSelectedSceneId,
    selectedContentIndex,
    setSelectedContentIndex,
    selectedChoiceIndex,
    setSelectedChoice,
    selectedActionIndex,
    setSelectedActionIndex,
    diceActionSelection,
    setDiceActionIndex,
    error,
    saveMode,
    fileAccess,
    openFile,
    createNewFile,
    onFileInputChange,
    save,
    saveAs,
    addEmptyScene,
    addTextScene,
    removeSelectedScene,
    duplicateSelectedScene,
    renameSelectedScene,
    updateMeta,
    updateSelectedScene,
    applyFix,
    applyAllFixes,
    applyLocalization,
    selectIssueLocation,
  };
}
