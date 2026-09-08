import { readFileSync } from "node:fs";
import { resolve } from "node:path";
import { normalizeAdventure } from "../src/domain/normalize";
import type { AdventureData } from "../src/domain/types";

export const REPO_ROOT = resolve(import.meta.dirname, "../../..");
export const WEB_ROOT = resolve(import.meta.dirname, "..");
export const ADVENTURES_ROOT = resolve(
  REPO_ROOT,
  "Assets/Modules/Definitions/Resources/Definitions/_ADVENTURES_/Adventures",
);
export const FIXTURES_ROOT = resolve(import.meta.dirname, "fixtures");

export const REAL_ADVENTURE_FILES = {
  Crossroad: resolve(ADVENTURES_ROOT, "Locations/Crossroad.json"),
  EmberWatch: resolve(ADVENTURES_ROOT, "Quests/AdventureEmberWatch.json"),
  TutorialIntro: resolve(ADVENTURES_ROOT, "Debug/_TutorialIntro.json"),
  Tavern: resolve(ADVENTURES_ROOT, "Locations/AdventureTavernByMartha.json"),
  FirstTutorial: resolve(ADVENTURES_ROOT, "Tutorials/_FirstTutorial.json"),
  ForestPath: resolve(ADVENTURES_ROOT, "Debug/_ForestPath.json"),
} as const;

export function loadRawJson(filePath: string): unknown {
  return JSON.parse(readFileSync(filePath, "utf8")) as unknown;
}

export function loadAdventure(filePath: string): AdventureData {
  return normalizeAdventure(loadRawJson(filePath));
}

export function loadFixture(name: string): AdventureData {
  return loadAdventure(resolve(FIXTURES_ROOT, name));
}

export function stable(value: unknown): unknown {
  if (Array.isArray(value)) {
    return value.map(stable);
  }
  if (value !== null && typeof value === "object") {
    const result: Record<string, unknown> = {};
    for (const key of Object.keys(value as Record<string, unknown>).sort()) {
      result[key] = stable((value as Record<string, unknown>)[key]);
    }
    return result;
  }
  return value;
}

export function stableEqual(left: unknown, right: unknown): boolean {
  return JSON.stringify(stable(left)) === JSON.stringify(stable(right));
}
