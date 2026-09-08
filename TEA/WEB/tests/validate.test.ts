import { describe, expect, it } from "vitest";
import { normalizeAdventure } from "../src/domain/normalize";
import {
  applyAllValidationFixes,
  applyValidationFix,
  canFixIssue,
  canNavigateToIssue,
  filterValidationIssues,
  validateAdventure,
} from "../src/domain/validate";
import { loadAdventure, loadFixture, REAL_ADVENTURE_FILES } from "./helpers";

function messages(adventure = loadFixture("validation-fix.json")): string[] {
  return validateAdventure(adventure).map((issue) => issue.message);
}

describe("validateAdventure", () => {
  it("reports the synthetic fixture as 10 issues / 9 fixable / 8 navigable", () => {
    const issues = validateAdventure(loadFixture("validation-fix.json"));
    expect(issues).toHaveLength(10);
    expect(issues.filter(canFixIssue)).toHaveLength(9);
    expect(issues.filter(canNavigateToIssue)).toHaveLength(8);
    expect(issues.filter((issue) => issue.location == null)).toHaveLength(0);
    expect(issues.filter((issue) => issue.location?.scope === "adventure")).toHaveLength(2);
    expect(issues.some((issue) => issue.message.includes("KEY_1"))).toBe(true);
    expect(issues.some((issue) => issue.message.includes("PARAM_OVERRIDE"))).toBe(false);
    expect(issues.some((issue) => issue.message.includes("ghost_scene"))).toBe(false);
  });

  it("does not treat ParameterOverrideDescription as loc text", () => {
    const adventure = normalizeAdventure({
      StartScenes: ["scene_01"],
      Scenes: {
        scene_01: {
          Choices: [
            {
              Id: "go",
              Type: "Default",
              VisualOptions: { ParameterOverrideDescription: "PARAM_OVERRIDE" },
              Actions: [],
            },
          ],
        },
      },
    });
    expect(validateAdventure(adventure).some((issue) => issue.message.includes("PARAM_OVERRIDE"))).toBe(
      false,
    );
  });

  it("filters by query and fixable/unfixable", () => {
    const issues = validateAdventure(loadFixture("validation-fix.json"));
    expect(filterValidationIssues(issues, "go", "all")).toHaveLength(3);
    expect(filterValidationIssues(issues, "", "fixable")).toHaveLength(9);
    expect(filterValidationIssues(issues, "", "unfixable")).toHaveLength(1);
    expect(filterValidationIssues(issues, "", "unfixable")[0]?.message).toContain("KEY_1");
  });

  it("applies one tag Fix without mutating the original", () => {
    const original = loadFixture("validation-fix.json");
    const issue = validateAdventure(original).find((item) => item.message.includes("'tutorial'"));
    expect(issue?.applyFix).toBeTypeOf("function");
    const next = applyValidationFix(original, issue!);
    expect(original.Tags).toEqual(["tutorial", ""]);
    expect(next.Tags[0]).toBe("TUTORIAL");
    expect(next.Tags[1]).toBe("");
  });

  it("Fix All leaves only the KEY_1 loc issue", () => {
    const fixed = applyAllValidationFixes(loadFixture("validation-fix.json"));
    const leftover = validateAdventure(fixed);
    expect(leftover).toHaveLength(1);
    expect(leftover[0]?.applyFix).toBeUndefined();
    expect(leftover[0]?.message).toContain("KEY_1");
    expect(fixed.Tags).toEqual(["TUTORIAL"]);
    expect(fixed.Scenes.scene_01?.Tags).toEqual(["DEBUG"]);
    expect(fixed.Scenes.scene_01?.Choices[0]?.Tags).toEqual(["TEST"]);
    expect(fixed.Scenes.scene_01?.Choices[0]?.DiceCheck).toBeNull();
    expect(fixed.Scenes.scene_01?.Choices[0]?.Actions[0]?.Params.Strings.SceneId).toBe("scene_01");
    expect(fixed.Scenes.scene_01?.Choices[0]?.Actions[0]?.Params.Strings.sceneId).toBeUndefined();
    expect(fixed.Scenes.scene_01?.Choices[1]?.DiceCheck?.DifficultyClass).toBe(0);
    expect(fixed.Scenes.scene_01?.Choices[1]?.Actions).toEqual([]);
    expect(fixed.Scenes.scene_01?.Choices[2]?.DiceCheck).not.toBeNull();
    expect(fixed.Scenes.scene_01?.Choices[3]?.Tags).toEqual(["NAV"]);
    expect(fixed.Scenes.scene_01?.Choices[3]?.Actions[0]?.Params.Strings.SceneId).toBe("scene_01");
    expect(fixed.Scenes.scene_01?.Choices[3]?.Actions[0]?.Params.Strings.Extra).toBeUndefined();
  });

  it("matches known real-file issue counts", () => {
    const tutorial = validateAdventure(loadAdventure(REAL_ADVENTURE_FILES.TutorialIntro));
    expect(tutorial).toHaveLength(1);
    expect(tutorial.filter(canFixIssue)).toHaveLength(1);
    expect(tutorial[0]?.message).toContain("tutorial");

    const tutorialFixed = applyAllValidationFixes(loadAdventure(REAL_ADVENTURE_FILES.TutorialIntro));
    expect(tutorialFixed.Tags).toEqual(["TUTORIAL"]);
    expect(validateAdventure(tutorialFixed)).toHaveLength(0);

    const forest = validateAdventure(loadAdventure(REAL_ADVENTURE_FILES.ForestPath));
    expect(forest).toHaveLength(3);
    expect(forest.filter(canFixIssue)).toHaveLength(3);
    expect(forest.map((issue) => issue.message).join("\n")).toContain("debug");
    expect(forest.map((issue) => issue.message).join("\n")).toContain("test");
    expect(forest.map((issue) => issue.message).join("\n")).toContain("navigation");

    const tavern = validateAdventure(loadAdventure(REAL_ADVENTURE_FILES.Tavern));
    expect(tavern.some((issue) => issue.message.includes("KEY_1"))).toBe(true);
    expect(tavern.find((issue) => issue.message.includes("KEY_1"))?.applyFix).toBeUndefined();
    expect(tavern.find((issue) => issue.message.includes("KEY_1"))?.location).toEqual({
      scope: "content",
      sceneId: "scene_01",
      contentIndex: 1,
    });

    expect(validateAdventure(loadAdventure(REAL_ADVENTURE_FILES.Crossroad))).toHaveLength(0);
  });

  it("flags Default vs DiceCheck contracts and missing scene targets on Default actions only", () => {
    const adventure = normalizeAdventure({
      StartScenes: ["start"],
      Scenes: {
        start: {
          Choices: [
            {
              Id: "broken",
              Type: "Default",
              Actions: [{ Type: "GoToScene", Params: { Strings: { SceneId: "missing" } } }],
            },
            {
              Id: "roll",
              Type: "DiceCheck",
              DiceCheck: {
                DifficultyClass: 10,
                OnSuccess: [{ Type: "GoToScene", Params: { Strings: { SceneId: "also_missing" } } }],
              },
              Actions: [],
            },
          ],
        },
      },
    });
    const text = messages(adventure).join("\n");
    expect(text).toContain("missing scene 'missing'");
    expect(text).not.toContain("also_missing");

    const missingBlock = structuredClone(adventure);
    missingBlock.Scenes.start!.Choices[1]!.Type = "DiceCheck";
    missingBlock.Scenes.start!.Choices[1]!.DiceCheck = null;
    const filled = applyAllValidationFixes(missingBlock);
    expect(filled.Scenes.start?.Choices[1]?.DiceCheck).not.toBeNull();
    expect(filled.Scenes.start?.Choices[1]?.DiceCheck?.DifficultyClass).toBe(15);
  });

  it("requires at least one scene and start scene", () => {
    expect(validateAdventure(null)[0]?.message).toBe("Adventure is null.");
    const empty = validateAdventure(normalizeAdventure({}));
    expect(empty.some((issue) => issue.message.includes("at least one scene"))).toBe(true);
    expect(empty.some((issue) => issue.message.includes("at least one start scene"))).toBe(true);
  });
});
