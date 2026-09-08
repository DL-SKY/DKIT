import { ChoiceActions } from "./glossary";
import type { ChoiceActionType } from "./enums";
import type { ChoiceActionData } from "./types";

const LEGACY_SCENE_ID_KEY = "sceneId";

export function isSceneTransitionAction(type: ChoiceActionType): boolean {
  return type === "GoToScene" || type === "None";
}

export function isGraphSceneEdgeAction(type: ChoiceActionType): boolean {
  return isSceneTransitionAction(type) || type === "GoToRandomScene";
}

export function getSceneId(action: ChoiceActionData): string {
  const strings = action.Params?.Strings;
  if (strings == null) {
    return "";
  }

  const current = strings[ChoiceActions.SCENE_ID];
  if (current != null && current.trim().length > 0) {
    return current;
  }

  const legacy = strings[LEGACY_SCENE_ID_KEY];
  if (legacy != null && legacy.trim().length > 0) {
    return legacy;
  }

  return "";
}

export function getGraphSceneTargets(action: ChoiceActionData): string[] {
  const raw = getSceneId(action);
  if (raw.trim().length === 0) {
    return [];
  }

  if (action.Type !== "GoToRandomScene") {
    return [raw];
  }

  const unique = new Set<string>();
  const targets: string[] = [];
  for (const part of raw.split(ChoiceActions.SCENE_IDS_SEPARATOR)) {
    const sceneId = part.trim();
    if (sceneId.length === 0 || unique.has(sceneId)) {
      continue;
    }
    unique.add(sceneId);
    targets.push(sceneId);
  }
  return targets;
}

export function setGoToSceneId(action: ChoiceActionData, sceneId: string): void {
  action.Type = "GoToScene";
  action.Params = {
    Strings: { ...(action.Params?.Strings ?? {}) },
    Ints: { ...(action.Params?.Ints ?? {}) },
    Bools: { ...(action.Params?.Bools ?? {}) },
  };
  action.Params.Strings[ChoiceActions.SCENE_ID] = sceneId;
  delete action.Params.Strings[LEGACY_SCENE_ID_KEY];
}
