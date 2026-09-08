import type { ChoiceActionType, SceneContentType } from "./enums";
import { ChoiceActions, Windows } from "./glossary";
import { getGraphSceneTargets, isGraphSceneEdgeAction } from "./sceneTargets";
import type { AdventureData, ChoiceActionData, ChoiceData } from "./types";

export const GRAPH_NODE_WIDTH = 220;
export const GRAPH_NODE_TOP_BLOCK_HEIGHT = 58;
export const GRAPH_CHOICE_ROW_HEIGHT = 18;
export const GRAPH_LAYER_SPACING_X = 320;
export const GRAPH_LAYER_SPACING_Y = 34;
export const GRAPH_CANVAS_PADDING = 24;
export const GRAPH_ZOOM_MIN = 0.45;
export const GRAPH_ZOOM_MAX = 2.5;
export const GRAPH_RESET_PAN = { x: 40, y: 40 };

export type GraphNodeStatus = "broken" | "start" | "unreachable" | "regular";
export type GraphNodeFilter = "all" | "start" | "unreachable" | "broken";

export const GRAPH_NODE_FILTERS: { id: GraphNodeFilter; label: string }[] = [
  { id: "all", label: "All" },
  { id: "start", label: "Start" },
  { id: "unreachable", label: "Unreachable" },
  { id: "broken", label: "Broken" },
];

export type GraphPoint = {
  x: number;
  y: number;
};

export type GraphChoiceRow = {
  id: string;
  type: ChoiceData["Type"];
  actionTypes: ChoiceActionType[];
  exitsScenario: boolean;
};

export type GraphNode = {
  id: string;
  tagsLabel: string;
  contentTypes: SceneContentType[];
  choices: GraphChoiceRow[];
  isExistingScene: boolean;
  isBrokenTarget: boolean;
  isStart: boolean;
  isReachable: boolean;
};

export type GraphEdge = {
  fromNodeId: string;
  toNodeId: string;
  fromChoiceIndex: number;
};

export type AdventureGraph = {
  nodes: GraphNode[];
  nodeById: Record<string, GraphNode>;
  edges: GraphEdge[];
  existingNodesCount: number;
  brokenNodesCount: number;
  unreachableNodesCount: number;
};

export function graphNodeHeight(node: GraphNode): number {
  return GRAPH_NODE_TOP_BLOCK_HEIGHT + GRAPH_CHOICE_ROW_HEIGHT * Math.max(0, node.choices.length);
}

export function graphNodeStatus(node: GraphNode): GraphNodeStatus {
  if (node.isBrokenTarget) {
    return "broken";
  }
  if (node.isStart) {
    return "start";
  }
  if (!node.isReachable) {
    return "unreachable";
  }
  return "regular";
}

export function nodeMatchesGraphFilter(node: GraphNode, filter: GraphNodeFilter): boolean {
  if (filter === "all") {
    return true;
  }
  return graphNodeStatus(node) === filter;
}

export function filterAdventureGraph(graph: AdventureGraph, filter: GraphNodeFilter): AdventureGraph {
  if (filter === "all") {
    return graph;
  }

  const nodes = graph.nodes.filter((node) => nodeMatchesGraphFilter(node, filter));
  const visibleIds = new Set(nodes.map((node) => node.id));
  const nodeById: Record<string, GraphNode> = {};
  for (const node of nodes) {
    nodeById[node.id] = node;
  }

  let existingNodesCount = 0;
  let brokenNodesCount = 0;
  let unreachableNodesCount = 0;
  for (const node of nodes) {
    if (node.isExistingScene) {
      existingNodesCount += 1;
    }
    if (node.isBrokenTarget) {
      brokenNodesCount += 1;
    }
    if (!node.isBrokenTarget && !node.isReachable) {
      unreachableNodesCount += 1;
    }
  }

  return {
    nodes,
    nodeById,
    edges: graph.edges.filter(
      (edge) => visibleIds.has(edge.fromNodeId) && visibleIds.has(edge.toNodeId),
    ),
    existingNodesCount,
    brokenNodesCount,
    unreachableNodesCount,
  };
}

export function compareOrdinal(a: string, b: string): number {
  if (a < b) {
    return -1;
  }
  if (a > b) {
    return 1;
  }
  return 0;
}

export function buildAdventureGraph(adventure: AdventureData): AdventureGraph {
  const nodes = new Map<string, GraphNode>();
  const edges: GraphEdge[] = [];

  const startSet = new Set<string>();
  for (const startSceneId of adventure.StartScenes) {
    startSet.add(startSceneId);
  }

  for (const [id, sceneData] of Object.entries(adventure.Scenes)) {
    if (id.trim().length === 0) {
      continue;
    }

    nodes.set(id, {
      id,
      tagsLabel: buildTagsLabel(sceneData?.Tags),
      contentTypes: buildContentTypes(sceneData?.Content),
      choices: buildChoiceRows(sceneData?.Choices),
      isExistingScene: true,
      isBrokenTarget: false,
      isStart: startSet.has(id),
      isReachable: false,
    });
  }

  for (const [fromScene, sceneData] of Object.entries(adventure.Scenes)) {
    const choices = sceneData?.Choices;
    if (choices == null) {
      continue;
    }

    for (let choiceIndex = 0; choiceIndex < choices.length; choiceIndex += 1) {
      const choice = choices[choiceIndex];
      const actions = choice?.Actions;
      if (actions == null) {
        continue;
      }

      for (const action of actions) {
        if (action == null || !isGraphSceneEdgeAction(action.Type)) {
          continue;
        }

        for (const target of getGraphSceneTargets(action)) {
          if (!nodes.has(target)) {
            nodes.set(target, {
              id: target,
              tagsLabel: "tags: -",
              contentTypes: [],
              choices: [],
              isExistingScene: false,
              isBrokenTarget: true,
              isStart: false,
              isReachable: false,
            });
          }

          edges.push({
            fromNodeId: fromScene,
            toNodeId: target,
            fromChoiceIndex: choiceIndex,
          });
        }
      }
    }
  }

  markReachableNodes(nodes, edges, startSet);

  let existingNodesCount = 0;
  let brokenNodesCount = 0;
  let unreachableNodesCount = 0;
  const nodeList: GraphNode[] = [];
  const nodeById: Record<string, GraphNode> = {};

  for (const node of nodes.values()) {
    nodeList.push(node);
    nodeById[node.id] = node;
    if (node.isExistingScene) {
      existingNodesCount += 1;
    }
    if (node.isBrokenTarget) {
      brokenNodesCount += 1;
    }
    if (!node.isBrokenTarget && !node.isReachable) {
      unreachableNodesCount += 1;
    }
  }

  return {
    nodes: nodeList,
    nodeById,
    edges,
    existingNodesCount,
    brokenNodesCount,
    unreachableNodesCount,
  };
}

export function autoLayoutGraph(graph: AdventureGraph): Record<string, GraphPoint> {
  const positions: Record<string, GraphPoint> = {};
  const normalDepths = new Map<string, number>();
  const queue: string[] = [];
  const leftColumnNodes: string[] = [];

  for (const node of graph.nodes) {
    if (node.isBrokenTarget || !node.isReachable) {
      leftColumnNodes.push(node.id);
      continue;
    }

    if (node.isStart) {
      normalDepths.set(node.id, 0);
      queue.push(node.id);
    }
  }

  if (queue.length === 0) {
    for (const node of graph.nodes) {
      if (node.isBrokenTarget || !node.isReachable) {
        continue;
      }
      if (node.isExistingScene) {
        normalDepths.set(node.id, 0);
        queue.push(node.id);
        break;
      }
    }
  }

  while (queue.length > 0) {
    const current = queue.shift();
    if (current == null) {
      break;
    }
    const currentDepth = normalDepths.get(current);
    if (currentDepth == null) {
      continue;
    }

    for (const edge of graph.edges) {
      if (edge.fromNodeId !== current) {
        continue;
      }
      const targetNode = graph.nodeById[edge.toNodeId];
      if (targetNode == null) {
        continue;
      }
      if (targetNode.isBrokenTarget || !targetNode.isReachable) {
        continue;
      }
      if (normalDepths.has(edge.toNodeId)) {
        continue;
      }

      normalDepths.set(edge.toNodeId, currentDepth + 1);
      queue.push(edge.toNodeId);
    }
  }

  let maxDepth = 0;
  for (const depth of normalDepths.values()) {
    if (depth > maxDepth) {
      maxDepth = depth;
    }
  }

  for (const node of graph.nodes) {
    if (node.isBrokenTarget || !node.isReachable) {
      continue;
    }
    if (!normalDepths.has(node.id)) {
      maxDepth += 1;
      normalDepths.set(node.id, maxDepth);
    }
  }

  const normalLayers = new Map<number, string[]>();
  for (const [nodeId, depth] of normalDepths) {
    const list = normalLayers.get(depth);
    if (list == null) {
      normalLayers.set(depth, [nodeId]);
    } else {
      list.push(nodeId);
    }
  }

  leftColumnNodes.sort(compareOrdinal);

  const depthKeys = [...normalLayers.keys()].sort((a, b) => a - b);
  const hasLeftColumn = leftColumnNodes.length > 0;
  const leftColumnX = GRAPH_CANVAS_PADDING;

  let leftY = GRAPH_CANVAS_PADDING;
  for (const nodeId of leftColumnNodes) {
    positions[nodeId] = { x: leftColumnX, y: leftY };
    const node = graph.nodeById[nodeId];
    leftY += (node == null ? GRAPH_CHOICE_ROW_HEIGHT : graphNodeHeight(node)) + GRAPH_LAYER_SPACING_Y;
  }

  for (const depth of depthKeys) {
    const layerNodes = normalLayers.get(depth) ?? [];
    layerNodes.sort(compareOrdinal);

    const columnIndex = hasLeftColumn ? depth + 1 : depth;
    const x = GRAPH_CANVAS_PADDING + columnIndex * GRAPH_LAYER_SPACING_X;
    let y = GRAPH_CANVAS_PADDING;
    for (const nodeId of layerNodes) {
      positions[nodeId] = { x, y };
      const node = graph.nodeById[nodeId];
      y += (node == null ? GRAPH_CHOICE_ROW_HEIGHT : graphNodeHeight(node)) + GRAPH_LAYER_SPACING_Y;
    }
  }

  return positions;
}

function markReachableNodes(
  nodes: Map<string, GraphNode>,
  edges: GraphEdge[],
  starts: Set<string>,
): void {
  for (const node of nodes.values()) {
    node.isReachable = false;
  }

  const queue: string[] = [];
  const visited = new Set<string>();

  for (const start of starts) {
    if (!nodes.has(start)) {
      continue;
    }
    queue.push(start);
    visited.add(start);
    const startNode = nodes.get(start);
    if (startNode != null) {
      startNode.isReachable = true;
    }
  }

  while (queue.length > 0) {
    const current = queue.shift();
    if (current == null) {
      break;
    }

    for (const edge of edges) {
      if (edge.fromNodeId !== current) {
        continue;
      }
      if (visited.has(edge.toNodeId)) {
        continue;
      }
      visited.add(edge.toNodeId);

      const node = nodes.get(edge.toNodeId);
      if (node != null) {
        node.isReachable = true;
      }
      queue.push(edge.toNodeId);
    }
  }
}

function buildTagsLabel(tags: string[] | null | undefined): string {
  if (tags == null || tags.length === 0) {
    return "tags: -";
  }

  const preparedTags: string[] = [];
  for (const tag of tags) {
    const trimmed = (tag ?? "").trim();
    if (trimmed.length > 0) {
      preparedTags.push(trimmed);
    }
  }

  return preparedTags.length === 0 ? "tags: -" : `tags: ${preparedTags.join(", ")}`;
}

function buildContentTypes(
  content: AdventureData["Scenes"][string]["Content"] | null | undefined,
): SceneContentType[] {
  const result: SceneContentType[] = [];
  if (content == null) {
    return result;
  }

  for (const item of content) {
    if (item == null) {
      continue;
    }
    result.push(item.Type);
  }
  return result;
}

function buildChoiceRows(choices: ChoiceData[] | null | undefined): GraphChoiceRow[] {
  const result: GraphChoiceRow[] = [];
  if (choices == null) {
    return result;
  }

  for (let i = 0; i < choices.length; i += 1) {
    const choice = choices[i];
    const actionTypes: ChoiceActionType[] = [];
    if (choice?.Actions != null) {
      for (const action of choice.Actions) {
        if (action == null) {
          continue;
        }
        actionTypes.push(action.Type);
      }
    }

    let id = choice?.Id;
    if (id == null || id.trim().length === 0) {
      id = `choice_${i}`;
    }

    result.push({
      id,
      type: choice?.Type ?? "Default",
      actionTypes,
      exitsScenario: choiceLeadsToScenarioExit(choice),
    });
  }

  return result;
}

function choiceLeadsToScenarioExit(choice: ChoiceData | null | undefined): boolean {
  if (choice == null) {
    return false;
  }

  if (hasScenarioExitAction(choice.Actions)) {
    return true;
  }

  const diceCheck = choice.DiceCheck;
  if (diceCheck == null) {
    return false;
  }

  return (
    hasScenarioExitAction(diceCheck.OnCriticalSuccess) ||
    hasScenarioExitAction(diceCheck.OnSuccess) ||
    hasScenarioExitAction(diceCheck.OnFailure) ||
    hasScenarioExitAction(diceCheck.OnCriticalFailure)
  );
}

function hasScenarioExitAction(actions: ChoiceActionData[] | null | undefined): boolean {
  if (actions == null) {
    return false;
  }
  for (const action of actions) {
    if (isScenarioExitAction(action)) {
      return true;
    }
  }
  return false;
}

function isScenarioExitAction(action: ChoiceActionData | null | undefined): boolean {
  if (action == null) {
    return false;
  }

  if (action.Type === "GoToAdventure" || action.Type === "GoToRandomAdventure") {
    return true;
  }

  if (action.Type !== "OpenWindow") {
    return false;
  }

  const windowId = action.Params?.Strings?.[ChoiceActions.WINDOW_ID];
  return windowId === Windows.ADVENTURE_LIST;
}
