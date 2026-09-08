import { basename } from "node:path";
import { describe, expect, it } from "vitest";
import {
  buildLocalizationExportFileName,
  formatLocalizationTsv,
  generateLocalizationKeys,
} from "../src/domain/localization";
import { loadAdventure, loadFixture, REAL_ADVENTURE_FILES } from "./helpers";

describe("generateLocalizationKeys", () => {
  it("generates, reuses, skips existing keys, and leaves Image / param ids alone", () => {
    const first = generateLocalizationKeys(loadFixture("localization-demo.json"), "DemoFile.json");
    expect(first.adventurePrefix).toBe("DEMO_FILE");
    expect(first.adventure.Title).toBe("DEMO_FILE_ADV_TITLE");
    expect(first.adventure.Description).toBe("DEMO_FILE_ADV_TITLE");
    expect(first.reusedKeysCount).toBe(2);
    expect(first.generatedKeysCount).toBe(5);
    expect(first.updatedFieldsCount).toBe(7);
    expect(first.exportEntries).toHaveLength(5);

    const scene = first.adventure.Scenes.scene_01;
    expect(scene?.Content[0]?.Value).toBe("not a loc field");
    expect(scene?.Content[1]?.Value).toBe("DEMO_FILE_SCENE_01_CNT_2_TEXT");
    expect(scene?.Content[2]?.Value).toBe("DEMO_FILE_SCENE_01_CNT_3_TEXT_ARG_2");
    expect(scene?.Choices[0]?.Text).toBe("DEMO_FILE_SCENE_01_CH_1_TEXT");
    expect(scene?.Choices[0]?.VisualOptions?.ParameterOverrideDescription).toBe("Thievery");
    expect(scene?.Choices[1]?.Description).toBe("DEMO_FILE_SCENE_01_CH_1_TEXT");
    expect(scene?.Choices[1]?.Text).toBe("DEMO_FILE_SCENE_01_CH_2_TEXT");

    const tsv = formatLocalizationTsv(first.exportEntries);
    expect(tsv).toContain("DEMO_FILE_SCENE_01_CH_2_TEXT\tStay here with a newline and more\n");
    expect(buildLocalizationExportFileName("DEMO_FILE", new Date(2026, 8, 7, 15, 4, 5))).toBe(
      "DEMO_FILE_localization_20260907_150405.txt",
    );

    const existing = loadFixture("localization-demo.json");
    existing.Title = "ALREADY_A_KEY";
    existing.Description = "loc:ALREADY_A_KEY";
    const skipped = generateLocalizationKeys(existing, "DemoFile.json");
    expect(skipped.adventure.Title).toBe("ALREADY_A_KEY");
    expect(skipped.adventure.Description).toBe("loc:ALREADY_A_KEY");

    const collisionBase = loadFixture("localization-demo.json");
    collisionBase.Title = "One two";
    collisionBase.Description = "Two words";
    collisionBase.Scenes.scene_01!.Choices[0]!.Text = "DEMO_FILE_ADV_TITLE";
    const unique = generateLocalizationKeys(collisionBase, "DemoFile.json");
    expect(unique.adventure.Title).toBe("DEMO_FILE_ADV_TITLE_2");

    const second = generateLocalizationKeys(first.adventure, "DemoFile.json");
    expect(second.updatedFieldsCount).toBe(0);
    expect(second.exportEntries).toHaveLength(0);

    expect(generateLocalizationKeys(loadFixture("localization-demo.json"), "AdventureEmberWatch.json").adventurePrefix).toBe(
      "ADVENTURE_EMBER_WATC",
    );
  });

  it("matches real-file generate counts and does not mutate on a second pass", () => {
    const expected: Record<string, { updated: number; generated: number; reused: number; prefix?: string }> = {
      "Crossroad.json": { updated: 14, generated: 12, reused: 2 },
      "_TutorialIntro.json": { updated: 3, generated: 3, reused: 0, prefix: "TUTORIAL_INTRO" },
      "_ForestPath.json": { updated: 57, generated: 57, reused: 0 },
      "AdventureEmberWatch.json": {
        updated: 201,
        generated: 188,
        reused: 13,
        prefix: "ADVENTURE_EMBER_WATC",
      },
      "AdventureTavernByMartha.json": { updated: 72, generated: 54, reused: 18 },
    };

    const files = [
      REAL_ADVENTURE_FILES.Crossroad,
      REAL_ADVENTURE_FILES.TutorialIntro,
      REAL_ADVENTURE_FILES.ForestPath,
      REAL_ADVENTURE_FILES.EmberWatch,
      REAL_ADVENTURE_FILES.Tavern,
    ];

    for (const filePath of files) {
      const name = basename(filePath);
      const result = generateLocalizationKeys(loadAdventure(filePath), name);
      const again = generateLocalizationKeys(result.adventure, name);
      const want = expected[name];
      expect(want, name).toBeDefined();
      expect(result.updatedFieldsCount, name).toBe(want.updated);
      expect(result.generatedKeysCount, name).toBe(want.generated);
      expect(result.reusedKeysCount, name).toBe(want.reused);
      expect(again.updatedFieldsCount, `${name} second pass`).toBe(0);
      if (want.prefix != null) {
        expect(result.adventurePrefix).toBe(want.prefix);
      }
    }

    const tutorial = generateLocalizationKeys(
      loadAdventure(REAL_ADVENTURE_FILES.TutorialIntro),
      "_TutorialIntro.json",
    );
    expect(tutorial.adventure.Title).toBe("TUTORIAL_INTRO_ADV_TITLE");
    expect(tutorial.adventure.Description).toBe("TUTORIAL_INTRO_ADV_DESCR");
    expect(tutorial.adventure.Scenes.intro?.Content[0]?.Value).toBe("TUTORIAL_INTRO_INTRO_CNT_1_TEXT");

    const crossroad = generateLocalizationKeys(loadAdventure(REAL_ADVENTURE_FILES.Crossroad), "Crossroad.json");
    expect(crossroad.adventure.Title).toBe("CROSSROAD_ADV_TITLE");
    expect(crossroad.adventure.Scenes.scene_01?.Content[0]?.Type).toBe("Image");
    expect(crossroad.adventure.Scenes.scene_01?.Content[0]?.Value).toBe("");
    expect(crossroad.adventure.Scenes.scene_01?.Content[1]?.Value).toBe("CROSSROAD_SCENE_01_CNT_2_TEXT");
  });
});
