import { describe, expect, it } from "vitest";
import { getGraphSceneTargets, isGraphSceneEdgeAction, setGoToSceneId } from "../src/domain/sceneTargets";
import type { ChoiceActionData } from "../src/domain/types";

function action(type: ChoiceActionData["Type"], sceneId?: string, extra?: Record<string, string>): ChoiceActionData {
  const strings: Record<string, string> = { ...(extra ?? {}) };
  if (sceneId != null) {
    strings.SceneId = sceneId;
  }
  return { Type: type, Params: { Strings: strings, Ints: {}, Bools: {} } };
}

describe("sceneTargets", () => {
  it("treats GoToScene, GoToRandomScene and legacy None as graph edges", () => {
    expect(isGraphSceneEdgeAction("GoToScene")).toBe(true);
    expect(isGraphSceneEdgeAction("GoToRandomScene")).toBe(true);
    expect(isGraphSceneEdgeAction("None")).toBe(true);
    expect(isGraphSceneEdgeAction("GoToAdventure")).toBe(false);
  });

  it("splits GoToRandomScene on ';' and keeps unique order", () => {
    expect(getGraphSceneTargets(action("GoToRandomScene", "a; b ;a;;c"))).toEqual(["a", "b", "c"]);
    expect(getGraphSceneTargets(action("GoToScene", "a;b"))).toEqual(["a;b"]);
  });

  it("reads legacy sceneId and rewrites it to SceneId", () => {
    const legacy: ChoiceActionData = {
      Type: "None",
      Params: { Strings: { sceneId: "hub" }, Ints: {}, Bools: {} },
    };
    expect(getGraphSceneTargets(legacy)).toEqual(["hub"]);
    setGoToSceneId(legacy, "next");
    expect(legacy.Type).toBe("GoToScene");
    expect(legacy.Params.Strings.SceneId).toBe("next");
    expect(legacy.Params.Strings.sceneId).toBeUndefined();
  });
});
