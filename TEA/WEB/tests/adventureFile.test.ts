import { readFileSync } from "node:fs";
import { resolve } from "node:path";
import { describe, expect, it } from "vitest";
import { openAdventureFromFile } from "../src/io/adventureFile";
import { FIXTURES_ROOT } from "./helpers";

async function openNamed(fileName: string): Promise<Awaited<ReturnType<typeof openAdventureFromFile>>> {
  const text = readFileSync(resolve(FIXTURES_ROOT, fileName), "utf8");
  return openAdventureFromFile(new File([text], fileName, { type: "application/json" }));
}

describe("openAdventureFromFile", () => {
  it("opens a valid adventure and keeps the file name", async () => {
    const opened = await openNamed("minimal.json");
    expect(opened.fileName).toBe("minimal.json");
    expect(opened.handle).toBeNull();
    expect(opened.adventure.Title).toBe("Preview build");
    expect(opened.adventure.Scenes.scene_01).toBeDefined();
  });

  it("rejects broken JSON syntax", async () => {
    await expect(openNamed("invalid-syntax.json")).rejects.toThrow(/Failed to parse JSON/);
  });

  it("rejects a non-object adventure root", async () => {
    await expect(openNamed("invalid-array.json")).rejects.toThrow("JSON root must be an adventure object.");
  });
});
