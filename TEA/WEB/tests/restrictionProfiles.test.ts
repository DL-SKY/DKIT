import { describe, expect, it } from "vitest";
import { getVisibleRestrictionFields } from "../src/domain/restrictionProfiles";

describe("restriction editor profiles", () => {
  it("matches Unity TEA field profiles", () => {
    expect(getVisibleRestrictionFields("TimeNow")).toEqual(["CompareOptions", "LongValues"]);
    expect(getVisibleRestrictionFields("ActivePartyCount")).toEqual(["CompareOptions", "IntValues"]);
    expect(getVisibleRestrictionFields("CharacterParams")).toEqual([
      "CompareOptions",
      "StringValues",
      "IntValues",
    ]);
    expect(getVisibleRestrictionFields("WorldParams")).toEqual([
      "CompareOptions",
      "StringValues",
      "IntValues",
      "LongValues",
      "BoolValues",
    ]);
  });
});
