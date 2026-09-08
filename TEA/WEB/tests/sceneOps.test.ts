import { describe, expect, it } from "vitest";
import { createNewAdventure } from "../src/domain/sceneOps";

describe("createNewAdventure", () => {
  it("creates canonical empty adventure with one start scene", () => {
    const adventure = createNewAdventure();
    expect(adventure.Disabled).toBe(false);
    expect(adventure.Tags).toEqual([]);
    expect(adventure.IgnoredTags).toEqual([]);
    expect(adventure.AdventureLinks).toEqual([]);
    expect(adventure.Restrictions).toEqual([]);
    expect(adventure.Type).toBe("Adventure");
    expect(adventure.Title).toBe("");
    expect(adventure.Description).toBe("");
    expect(adventure.StartScenes).toEqual(["start"]);
    expect(Object.keys(adventure.Scenes)).toEqual(["start"]);
    expect(adventure.Scenes.start?.Id).toBe("start");
    expect(adventure.Scenes.start?.Content).toEqual([]);
    expect(adventure.Scenes.start?.Choices).toEqual([]);
    expect(adventure.Scenes.start?.NotClearScene).toBe(true);
  });

  it("normalizes custom start scene id", () => {
    const adventure = createNewAdventure("  intro_scene  ");
    expect(adventure.StartScenes).toEqual(["intro_scene"]);
    expect(adventure.Scenes.intro_scene?.Id).toBe("intro_scene");
  });

  it("falls back to start when scene id is blank", () => {
    const adventure = createNewAdventure("   ");
    expect(adventure.StartScenes).toEqual(["start"]);
    expect(adventure.Scenes.start?.Id).toBe("start");
  });
});
