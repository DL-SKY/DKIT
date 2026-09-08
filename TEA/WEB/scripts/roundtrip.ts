import { readFileSync } from "node:fs";
import { basename } from "node:path";
import { normalizeAdventure } from "../src/domain/normalize.ts";
import { serializeAdventure } from "../src/domain/serialize.ts";
import { validateAdventure } from "../src/domain/validate.ts";

const files = process.argv.slice(2);
if (files.length === 0) {
  console.error("Usage: node --experimental-strip-types scripts/roundtrip.ts <json...>");
  process.exit(1);
}

function stable(value: unknown): unknown {
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

let failed = 0;
for (const filePath of files) {
  const rawText = readFileSync(filePath, "utf8");
  const original = JSON.parse(rawText) as unknown;
  const first = normalizeAdventure(original);
  const serialized = serializeAdventure(first);
  const second = normalizeAdventure(JSON.parse(serialized) as unknown);
  const equal = JSON.stringify(stable(first)) === JSON.stringify(stable(second));
  const issues = validateAdventure(first);
  const originalKeys = original !== null && typeof original === "object" ? Object.keys(original as object) : [];
  const serializedObj = JSON.parse(serialized) as Record<string, unknown>;
  const addedTop = Object.keys(serializedObj).filter((key) => !originalKeys.includes(key));

  console.log(`--- ${basename(filePath)} ---`);
  console.log(`scenes: ${Object.keys(first.Scenes).length}`);
  console.log(`roundtrip: ${equal ? "OK" : "FAIL"}`);
  console.log(`validation issues: ${issues.length}`);
  if (issues.length > 0) {
    for (const issue of issues.slice(0, 8)) {
      console.log(`  - ${issue.message}`);
    }
    if (issues.length > 8) {
      console.log(`  ... ${issues.length - 8} more`);
    }
  }
  if (addedTop.length > 0) {
    console.log(`canonical extra top-level keys: ${addedTop.join(", ")}`);
  }
  if (!equal) {
    failed += 1;
  }
}

if (failed > 0) {
  console.error(`\nFailed files: ${failed}`);
  process.exit(1);
}

console.log("\nAll roundtrips OK.");
