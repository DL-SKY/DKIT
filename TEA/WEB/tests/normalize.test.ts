import { describe, expect, it } from "vitest";
import { normalizeAdventure } from "../src/domain/normalize";
import { serializeAdventure } from "../src/domain/serialize";
import { loadAdventure, REAL_ADVENTURE_FILES, stableEqual } from "./helpers";

describe("normalizeAdventure", () => {
  it("fills missing collections and canonical defaults", () => {
    const adventure = normalizeAdventure({
      Title: "Thin",
      StartScenes: ["hub"],
      Scenes: { hub: { Id: "other" } },
    });

    expect(adventure.Disabled).toBe(false);
    expect(adventure.Tags).toEqual([]);
    expect(adventure.IgnoredTags).toEqual([]);
    expect(adventure.AdventureLinks).toEqual([]);
    expect(adventure.Restrictions).toEqual([]);
    expect(adventure.IsRepeatable).toBe(false);
    expect(adventure.Type).toBe("Adventure");
    expect(adventure.Scenes.hub?.Id).toBe("hub");
    expect(adventure.Scenes.hub?.Content).toEqual([]);
    expect(adventure.Scenes.hub?.Choices).toEqual([]);
    expect(adventure.Scenes.hub?.NotClearScene).toBe(false);
  });

  it("reads string enums and legacy numeric enums", () => {
    const fromStrings = normalizeAdventure({
      Type: "Location",
      StartScenes: ["a"],
      Scenes: {
        a: {
          Content: [{ Type: "Image", Value: "img" }],
          Choices: [
            {
              Id: "go",
              Type: "DiceCheck",
              DiceCheck: { DiceType: "D6", DifficultyClass: 12 },
              Actions: [{ Type: "GoToScene", Params: { Strings: { SceneId: "a" } } }],
            },
          ],
        },
      },
    });
    expect(fromStrings.Type).toBe("Location");
    expect(fromStrings.Scenes.a?.Content[0]?.Type).toBe("Image");
    expect(fromStrings.Scenes.a?.Choices[0]?.Type).toBe("DiceCheck");
    expect(fromStrings.Scenes.a?.Choices[0]?.DiceCheck?.DiceType).toBe("D6");
    expect(fromStrings.Scenes.a?.Choices[0]?.Actions[0]?.Type).toBe("GoToScene");

    const fromNumbers = normalizeAdventure({
      Type: 10,
      StartScenes: ["a"],
      Scenes: {
        a: {
          Content: [{ Type: 12, Values: ["one"] }],
          Choices: [
            {
              Id: "go",
              Type: 1,
              Actions: [{ Type: 100, Params: { Strings: { SceneId: "a" } } }],
            },
          ],
        },
      },
    });
    expect(fromNumbers.Type).toBe("Location");
    expect(fromNumbers.Scenes.a?.Content[0]?.Type).toBe("Slideshow");
    expect(fromNumbers.Scenes.a?.Choices[0]?.Type).toBe("DiceCheck");
    expect(fromNumbers.Scenes.a?.Choices[0]?.Actions[0]?.Type).toBe("GoToScene");
  });

  it("keeps Default DiceCheck when present so validation can flag it", () => {
    const adventure = normalizeAdventure({
      StartScenes: ["a"],
      Scenes: {
        a: {
          Choices: [
            {
              Id: "stay",
              Type: "Default",
              DiceCheck: { DifficultyClass: 15 },
              Actions: [],
            },
          ],
        },
      },
    });
    expect(adventure.Scenes.a?.Choices[0]?.Type).toBe("Default");
    expect(adventure.Scenes.a?.Choices[0]?.DiceCheck).not.toBeNull();
    expect(adventure.Scenes.a?.Choices[0]?.DiceCheck?.DifficultyClass).toBe(15);
  });

  it("skips empty scene keys and does not write adventure Id", () => {
    const adventure = normalizeAdventure({
      Id: "should-not-exist",
      StartScenes: ["keep"],
      Scenes: {
        "  ": { Id: "" },
        keep: { Id: "keep" },
      },
    });
    expect(Object.keys(adventure.Scenes)).toEqual(["keep"]);
    expect("Id" in adventure).toBe(false);
  });

  it("drops unknown fields on canonical save", () => {
    const serialized = JSON.parse(
      serializeAdventure(
        normalizeAdventure({
          Title: "Keep",
          ExtraTop: true,
          StartScenes: ["a"],
          Scenes: {
            a: {
              Id: "a",
              UnknownSceneField: 1,
              Content: [{ Type: "Text", Value: "x", Extra: "nope" }],
            },
          },
        }),
      ),
    ) as Record<string, unknown>;

    expect(serialized.ExtraTop).toBeUndefined();
    expect(serialized.Title).toBe("Keep");
    const scene = (serialized.Scenes as Record<string, Record<string, unknown>>).a;
    expect(scene.UnknownSceneField).toBeUndefined();
    expect((scene.Content as Record<string, unknown>[])[0]?.Extra).toBeUndefined();
  });

  it("roundtrips a thin file without losing normalized data", () => {
    const first = normalizeAdventure({
      Title: "Thin",
      StartScenes: ["start"],
      Scenes: { start: { Choices: [{ Id: "next", Type: "Default" }] } },
    });
    const second = normalizeAdventure(JSON.parse(serializeAdventure(first)) as unknown);
    expect(stableEqual(first, second)).toBe(true);
    expect(JSON.parse(serializeAdventure(first)).Disabled).toBe(false);
    expect(JSON.parse(serializeAdventure(first)).IgnoredTags).toEqual([]);
  });

  it("roundtrips the 15.3 real adventure JSON set", () => {
    const files = [
      REAL_ADVENTURE_FILES.Crossroad,
      REAL_ADVENTURE_FILES.EmberWatch,
      REAL_ADVENTURE_FILES.TutorialIntro,
      REAL_ADVENTURE_FILES.Tavern,
      REAL_ADVENTURE_FILES.FirstTutorial,
    ];
    for (const filePath of files) {
      const first = loadAdventure(filePath);
      const second = normalizeAdventure(JSON.parse(serializeAdventure(first)) as unknown);
      expect(stableEqual(first, second), filePath).toBe(true);
      expect(Object.keys(first.Scenes).length).toBeGreaterThan(0);
    }
  });
});
