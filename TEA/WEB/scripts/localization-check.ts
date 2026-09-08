import { readFileSync } from "node:fs";
import { basename } from "node:path";
import {
  buildLocalizationExportFileName,
  formatLocalizationTsv,
  generateLocalizationKeys,
} from "../src/domain/localization.ts";
import { normalizeAdventure } from "../src/domain/normalize.ts";
import type { AdventureData } from "../src/domain/types.ts";

function assert(condition: boolean, message: string): void {
  if (!condition) {
    throw new Error(message);
  }
}

function makeAdventure(partial: Record<string, unknown>): AdventureData {
  return normalizeAdventure({
    Title: "Hello world",
    Description: "Hello world",
    StartScenes: ["scene_01"],
    Scenes: {
      scene_01: {
        Id: "scene_01",
        Content: [
          { Type: "Image", Value: "not a loc field" },
          { Type: "Text", Value: "First text" },
          { Type: "Text", Value: "Hello {0} and {1}" },
        ],
        Choices: [
          {
            Id: "go",
            Text: "Go there",
            Description: "",
            Type: "Default",
            VisualOptions: {
              MainIcon: "",
              DescriptionIcon: "",
              ParameterOverrideDescription: "Thievery",
            },
            Actions: [],
          },
          {
            Id: "stay",
            Text: "Stay here with a newline\nand more",
            Description: "Go there",
            Type: "Default",
            Actions: [],
          },
        ],
      },
    },
    ...partial,
  });
}

const synthetic = makeAdventure({});
const first = generateLocalizationKeys(synthetic, "DemoFile.json");
assert(first.adventurePrefix === "DEMO_FILE", `prefix ${first.adventurePrefix}`);
assert(first.adventure.Title === "DEMO_FILE_ADV_TITLE", `title ${first.adventure.Title}`);
assert(first.adventure.Description === "DEMO_FILE_ADV_TITLE", "description should reuse title key");
assert(first.reusedKeysCount === 2, `reused ${first.reusedKeysCount}`);
assert(first.generatedKeysCount === 5, `generated ${first.generatedKeysCount}`);
assert(first.updatedFieldsCount === 7, `updated ${first.updatedFieldsCount}`);
assert(first.exportEntries.length === 5, `export ${first.exportEntries.length}`);

const scene = first.adventure.Scenes.scene_01;
assert(scene != null, "scene_01 missing");
assert(scene.Content[0]?.Value === "not a loc field", "Image Value must stay");
assert(scene.Content[1]?.Value === "DEMO_FILE_SCENE_01_CNT_2_TEXT", `cnt2 ${scene.Content[1]?.Value}`);
assert(scene.Content[2]?.Value === "DEMO_FILE_SCENE_01_CNT_3_TEXT_ARG_2", `cnt3 ${scene.Content[2]?.Value}`);
assert(scene.Choices[0]?.Text === "DEMO_FILE_SCENE_01_CH_1_TEXT", `ch1 ${scene.Choices[0]?.Text}`);
assert(scene.Choices[0]?.VisualOptions?.ParameterOverrideDescription === "Thievery", "param id must stay");
assert(scene.Choices[1]?.Description === "DEMO_FILE_SCENE_01_CH_1_TEXT", "choice descr reuses Go there");
assert(
  scene.Choices[1]?.Text === "DEMO_FILE_SCENE_01_CH_2_TEXT",
  `ch2 text ${scene.Choices[1]?.Text}`,
);

const tsv = formatLocalizationTsv(first.exportEntries);
assert(tsv.includes("DEMO_FILE_SCENE_01_CH_2_TEXT\tStay here with a newline and more\n"), "tsv newline flatten");
assert(
  buildLocalizationExportFileName("DEMO_FILE", new Date(2026, 8, 7, 15, 4, 5)) ===
    "DEMO_FILE_localization_20260907_150405.txt",
  "export file name",
);

const existing = makeAdventure({
  Title: "ALREADY_A_KEY",
  Description: "loc:ALREADY_A_KEY",
});
const skipped = generateLocalizationKeys(existing, "DemoFile.json");
assert(skipped.adventure.Title === "ALREADY_A_KEY", "existing title key stays");
assert(skipped.adventure.Description === "loc:ALREADY_A_KEY", "loc: prefix stays");

const collision = makeAdventure({ Title: "One two", Description: "Two words" });
collision.Scenes.scene_01!.Choices[0]!.Text = "DEMO_FILE_ADV_TITLE";
const unique = generateLocalizationKeys(collision, "DemoFile.json");
assert(unique.adventure.Title === "DEMO_FILE_ADV_TITLE_2", `unique ${unique.adventure.Title}`);

const second = generateLocalizationKeys(first.adventure, "DemoFile.json");
assert(second.updatedFieldsCount === 0, "second pass must be no-op");
assert(second.exportEntries.length === 0, "second pass export empty");

const emberPrefix = generateLocalizationKeys(makeAdventure({}), "AdventureEmberWatch.json");
assert(emberPrefix.adventurePrefix === "ADVENTURE_EMBER_WATC", `ember ${emberPrefix.adventurePrefix}`);

const files = process.argv.slice(2);
for (const filePath of files) {
  const adventure = normalizeAdventure(JSON.parse(readFileSync(filePath, "utf8")) as unknown);
  const result = generateLocalizationKeys(adventure, basename(filePath));
  const again = generateLocalizationKeys(result.adventure, basename(filePath));
  console.log(`--- ${basename(filePath)} ---`);
  console.log(`prefix: ${result.adventurePrefix}`);
  console.log(
    `updated=${result.updatedFieldsCount} generated=${result.generatedKeysCount} reused=${result.reusedKeysCount} export=${result.exportEntries.length}`,
  );
  console.log(`second pass updated=${again.updatedFieldsCount}`);
  if (basename(filePath) === "_TutorialIntro.json") {
    assert(result.adventure.Title === "TUTORIAL_INTRO_ADV_TITLE", "tutorial title key");
    assert(result.adventure.Description === "TUTORIAL_INTRO_ADV_DESCR", "tutorial descr key");
    assert(
      result.adventure.Scenes.intro?.Content[0]?.Value === "TUTORIAL_INTRO_INTRO_CNT_1_TEXT",
      "tutorial content key",
    );
  }
  if (basename(filePath) === "Crossroad.json") {
    assert(result.adventure.Title === "CROSSROAD_ADV_TITLE", "crossroad title key");
    assert(result.adventure.Scenes.scene_01?.Content[0]?.Type === "Image", "image content stays type");
    assert(result.adventure.Scenes.scene_01?.Content[0]?.Value === "", "image value not keyed");
    assert(
      result.adventure.Scenes.scene_01?.Content[1]?.Value === "CROSSROAD_SCENE_01_CNT_2_TEXT",
      "crossroad first text key",
    );
  }
}

console.log("localization checks OK.");
