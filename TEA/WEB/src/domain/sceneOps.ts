import { isSceneTransitionAction, getSceneId, setGoToSceneId } from "./sceneTargets";
import type { AdventureData, ChoiceData, SceneData } from "./types";

export function uniqueId(existing: Set<string>, requestedId: string, fallback: string): string {
  const base = requestedId.trim().length === 0 ? fallback : requestedId.trim();
  if (!existing.has(base)) {
    return base;
  }
  let suffix = 2;
  while (existing.has(`${base}_${suffix}`)) {
    suffix += 1;
  }
  return `${base}_${suffix}`;
}

export function uniqueSceneId(scenes: Record<string, SceneData>, requestedId: string): string {
  return uniqueId(new Set(Object.keys(scenes)), requestedId, "new_scene");
}

export function uniqueChoiceId(choices: ChoiceData[], requestedId: string): string {
  return uniqueId(new Set(choices.map((choice) => choice.Id)), requestedId, "new_choice");
}

export function moveItem<T>(items: T[], index: number, delta: -1 | 1): { items: T[]; selectedIndex: number } {
  const nextIndex = index + delta;
  if (index < 0 || index >= items.length || nextIndex < 0 || nextIndex >= items.length) {
    return { items, selectedIndex: index };
  }
  const next = [...items];
  const current = next[index];
  const swapped = next[nextIndex];
  if (current === undefined || swapped === undefined) {
    return { items, selectedIndex: index };
  }
  next[index] = swapped;
  next[nextIndex] = current;
  return { items: next, selectedIndex: nextIndex };
}

export function selectionAfterSwap(
  selectedIndex: number | null,
  fromIndex: number,
  toIndex: number,
): number | null {
  if (selectedIndex == null) {
    return null;
  }
  if (selectedIndex === fromIndex) {
    return toIndex;
  }
  if (selectedIndex === toIndex) {
    return fromIndex;
  }
  return selectedIndex;
}

export function createEmptyScene(id: string): SceneData {
  return {
    Id: id,
    Tags: [],
    NotClearScene: true,
    Content: [],
    Choices: [],
  };
}

export function createTextScene(id: string): SceneData {
  return {
    Id: id,
    Tags: [],
    NotClearScene: true,
    Content: [
      {
        Type: "Text",
        Restrictions: [],
        Value: "",
        Values: null,
      },
    ],
    Choices: [],
  };
}

export function createNewAdventure(startSceneId = "start"): AdventureData {
  const sceneId = startSceneId.trim().length > 0 ? startSceneId.trim() : "start";
  return {
    Disabled: false,
    Tags: [],
    IgnoredTags: [],
    IsRepeatable: false,
    Type: "Adventure",
    AdventureLinks: [],
    Title: "",
    Description: "",
    Restrictions: [],
    StartScenes: [sceneId],
    Scenes: {
      [sceneId]: createEmptyScene(sceneId),
    },
  };
}

export function addScene(adventure: AdventureData, scene: SceneData): AdventureData {
  const id = uniqueSceneId(adventure.Scenes, scene.Id);
  const nextScene = { ...scene, Id: id };
  const startScenes =
    adventure.StartScenes.length === 0 ? [id] : [...adventure.StartScenes];
  return {
    ...adventure,
    StartScenes: startScenes,
    Scenes: {
      ...adventure.Scenes,
      [id]: nextScene,
    },
  };
}

export function deleteScene(adventure: AdventureData, sceneId: string): AdventureData {
  const scenes = { ...adventure.Scenes };
  delete scenes[sceneId];
  return {
    ...adventure,
    StartScenes: adventure.StartScenes.filter((id) => id !== sceneId),
    Scenes: scenes,
  };
}

export function duplicateScene(adventure: AdventureData, sceneId: string): AdventureData | null {
  const source = adventure.Scenes[sceneId];
  if (source == null) {
    return null;
  }
  const copy = structuredClone(source) as SceneData;
  copy.Id = `${sceneId}_copy`;
  return addScene(adventure, copy);
}

export function renameScene(
  adventure: AdventureData,
  oldSceneId: string,
  requestedSceneId: string,
): { ok: true; adventure: AdventureData; newSceneId: string } | { ok: false; error: string } {
  const newSceneId = requestedSceneId.trim();
  if (newSceneId.length === 0) {
    return { ok: false, error: "Scene id cannot be empty." };
  }
  if (newSceneId === oldSceneId) {
    return { ok: true, adventure, newSceneId };
  }
  if (adventure.Scenes[newSceneId] != null) {
    return { ok: false, error: `Scene '${newSceneId}' already exists.` };
  }
  const next = structuredClone(adventure) as AdventureData;
  const sceneData = next.Scenes[oldSceneId];
  if (sceneData == null) {
    return { ok: false, error: `Scene '${oldSceneId}' was not found.` };
  }

  const scenes: Record<string, SceneData> = {};
  for (const key of Object.keys(next.Scenes)) {
    if (key === oldSceneId) {
      scenes[newSceneId] = { ...sceneData, Id: newSceneId };
    } else {
      scenes[key] = next.Scenes[key];
    }
  }

  next.StartScenes = next.StartScenes.map((id) => (id === oldSceneId ? newSceneId : id));
  next.Scenes = scenes;

  for (const scene of Object.values(next.Scenes)) {
    for (const choice of scene.Choices) {
      for (const action of choice.Actions) {
        if (!isSceneTransitionAction(action.Type)) {
          continue;
        }
        if (getSceneId(action) === oldSceneId) {
          setGoToSceneId(action, newSceneId);
        }
      }
    }
  }

  return { ok: true, adventure: next, newSceneId };
}

export function updateScene(
  adventure: AdventureData,
  sceneId: string,
  patch: Partial<Pick<SceneData, "Tags" | "NotClearScene" | "Content" | "Choices">>,
): AdventureData {
  const scene = adventure.Scenes[sceneId];
  if (scene == null) {
    return adventure;
  }
  return {
    ...adventure,
    Scenes: {
      ...adventure.Scenes,
      [sceneId]: {
        ...scene,
        ...patch,
      },
    },
  };
}
