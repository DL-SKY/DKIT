import { describe, expect, it } from "vitest";
import { buildAdventureGraph, filterAdventureGraph, graphNodeStatus } from "../src/domain/graph";
import { normalizeAdventure } from "../src/domain/normalize";
import { loadAdventure, loadFixture, REAL_ADVENTURE_FILES } from "./helpers";

describe("buildAdventureGraph", () => {
  it("builds edges only from Default GoToScene / GoToRandomScene / legacy None", () => {
    const graph = buildAdventureGraph(
      normalizeAdventure({
        StartScenes: ["start"],
        Scenes: {
          start: {
            Choices: [
              {
                Id: "next",
                Type: "Default",
                Actions: [
                  { Type: "GoToScene", Params: { Strings: { SceneId: "mid" } } },
                  { Type: "GoToRandomScene", Params: { Strings: { SceneId: "a;b;a" } } },
                  { Type: "None", Params: { Strings: { SceneId: "legacy" } } },
                  { Type: "GoToAdventure", Params: { Strings: { AdventureId: "other" } } },
                ],
              },
              {
                Id: "roll",
                Type: "DiceCheck",
                DiceCheck: {
                  OnSuccess: [{ Type: "GoToScene", Params: { Strings: { SceneId: "from_dice" } } }],
                },
                Actions: [],
              },
            ],
          },
          mid: { Choices: [] },
          a: { Choices: [] },
          b: { Choices: [] },
          island: { Choices: [] },
        },
      }),
    );

    const pairs = graph.edges.map((edge) => `${edge.fromNodeId}->${edge.toNodeId}`).sort();
    expect(pairs).toEqual(["start->a", "start->b", "start->legacy", "start->mid"]);
    expect(graph.nodeById.from_dice).toBeUndefined();
    expect(graph.nodeById.legacy?.isBrokenTarget).toBe(true);
    expect(graphNodeStatus(graph.nodeById.start!)).toBe("start");
    expect(graphNodeStatus(graph.nodeById.island!)).toBe("unreachable");
    expect(graphNodeStatus(graph.nodeById.legacy!)).toBe("broken");
    expect(graph.unreachableNodesCount).toBe(1);
    expect(graph.brokenNodesCount).toBe(1);

    expect(filterAdventureGraph(graph, "all")).toBe(graph);
    expect(filterAdventureGraph(graph, "start").nodes.map((node) => node.id)).toEqual(["start"]);
    expect(filterAdventureGraph(graph, "unreachable").nodes.map((node) => node.id)).toEqual(["island"]);
    expect(filterAdventureGraph(graph, "broken").nodes.map((node) => node.id)).toEqual(["legacy"]);
    expect(filterAdventureGraph(graph, "start").edges).toHaveLength(0);
    expect(filterAdventureGraph(graph, "broken").edges).toHaveLength(0);
  });

  it("matches Crossroad and ForestPath domain counts from graph MVP", () => {
    const crossroad = buildAdventureGraph(loadAdventure(REAL_ADVENTURE_FILES.Crossroad));
    expect(crossroad.edges).toHaveLength(6);
    expect(crossroad.unreachableNodesCount).toBe(0);
    expect(crossroad.brokenNodesCount).toBe(0);

    const forest = buildAdventureGraph(loadAdventure(REAL_ADVENTURE_FILES.ForestPath));
    expect(forest.edges).toHaveLength(37);
    expect(forest.nodeById.scene_11?.isReachable).toBe(false);
    expect(graphNodeStatus(forest.nodeById.scene_11!)).toBe("unreachable");
    expect(forest.unreachableNodesCount).toBeGreaterThanOrEqual(1);

    const unreachableOnly = filterAdventureGraph(forest, "unreachable");
    expect(unreachableOnly.nodeById.scene_11).toBeDefined();
    expect(unreachableOnly.nodes.every((node) => graphNodeStatus(node) === "unreachable")).toBe(true);
    expect(unreachableOnly.nodes.every((node) => !node.isStart)).toBe(true);
  });

  it("does not create DiceCheck-outcome edges on the validation fixture", () => {
    const graph = buildAdventureGraph(loadFixture("validation-fix.json"));
    expect(graph.nodeById.ghost_scene).toBeUndefined();
    expect(graph.edges.every((edge) => edge.toNodeId !== "ghost_scene")).toBe(true);
  });
});
