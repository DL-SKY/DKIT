import { useCallback, useEffect, useMemo, useRef, useState, type PointerEvent } from "react";
import {
  autoLayoutGraph,
  buildAdventureGraph,
  filterAdventureGraph,
  GRAPH_CHOICE_ROW_HEIGHT,
  GRAPH_NODE_FILTERS,
  GRAPH_NODE_TOP_BLOCK_HEIGHT,
  GRAPH_NODE_WIDTH,
  GRAPH_RESET_PAN,
  GRAPH_ZOOM_MAX,
  GRAPH_ZOOM_MIN,
  graphNodeHeight,
  graphNodeStatus,
  type AdventureData,
  type AdventureGraph,
  type GraphEdge,
  type GraphNode,
  type GraphNodeFilter,
  type GraphPoint,
} from "../domain";

type Props = {
  adventure: AdventureData;
  selectedSceneId: string | null;
  onSelectScene: (sceneId: string) => void;
  onClose: () => void;
};

type DragState =
  | { kind: "pan"; pointerId: number; lastX: number; lastY: number }
  | { kind: "node"; pointerId: number; nodeId: string; offsetX: number; offsetY: number };

function clampZoom(value: number): number {
  return Math.min(GRAPH_ZOOM_MAX, Math.max(GRAPH_ZOOM_MIN, value));
}

function mergePositions(
  graphNodeIds: string[],
  previous: Record<string, GraphPoint>,
  laidOut: Record<string, GraphPoint>,
): Record<string, GraphPoint> {
  const next: Record<string, GraphPoint> = {};
  for (const id of graphNodeIds) {
    next[id] = previous[id] ?? laidOut[id] ?? { x: GRAPH_RESET_PAN.x, y: GRAPH_RESET_PAN.y };
  }
  return next;
}

function edgePoints(
  edge: GraphEdge,
  nodesById: Record<string, GraphNode>,
  positions: Record<string, GraphPoint>,
): { from: GraphPoint; to: GraphPoint } | null {
  const fromNode = nodesById[edge.fromNodeId];
  const toNode = nodesById[edge.toNodeId];
  const fromPos = positions[edge.fromNodeId];
  const toPos = positions[edge.toNodeId];
  if (fromNode == null || toNode == null || fromPos == null || toPos == null) {
    return null;
  }

  let from: GraphPoint;
  if (edge.fromChoiceIndex >= 0 && edge.fromChoiceIndex < fromNode.choices.length) {
    from = {
      x: fromPos.x + GRAPH_NODE_WIDTH,
      y:
        fromPos.y +
        GRAPH_NODE_TOP_BLOCK_HEIGHT +
        edge.fromChoiceIndex * GRAPH_CHOICE_ROW_HEIGHT +
        GRAPH_CHOICE_ROW_HEIGHT / 2,
    };
  } else {
    from = {
      x: fromPos.x + GRAPH_NODE_WIDTH,
      y: fromPos.y + graphNodeHeight(fromNode) / 2,
    };
  }

  const to = {
    x: toPos.x,
    y: toPos.y + graphNodeHeight(toNode) / 2,
  };
  return { from, to };
}

function tangentDistance(from: GraphPoint, to: GraphPoint): number {
  const horizontalGap = Math.abs(to.x - from.x);
  if (horizontalGap <= 0.001) {
    return 24;
  }
  return Math.min(70, Math.max(24, horizontalGap * 0.45));
}

function arrowPoints(tip: GraphPoint, fromControl: GraphPoint): string {
  const dx = tip.x - fromControl.x;
  const dy = tip.y - fromControl.y;
  const length = Math.hypot(dx, dy);
  const dirX = length < 0.0001 ? 1 : dx / length;
  const dirY = length < 0.0001 ? 0 : dy / length;
  const normalX = -dirY;
  const normalY = dirX;
  const size = 6;
  const p2x = tip.x - dirX * size + normalX * (size * 0.55);
  const p2y = tip.y - dirY * size + normalY * (size * 0.55);
  const p3x = tip.x - dirX * size - normalX * (size * 0.55);
  const p3y = tip.y - dirY * size - normalY * (size * 0.55);
  return `${tip.x},${tip.y} ${p2x},${p2y} ${p3x},${p3y}`;
}

function choiceTypeLabel(type: GraphNode["choices"][number]["type"]): string {
  return type === "DiceCheck" ? "Dice" : "Def";
}

function worldBounds(
  graph: AdventureGraph,
  positions: Record<string, GraphPoint>,
): { minX: number; minY: number; width: number; height: number } {
  let minX = Infinity;
  let minY = Infinity;
  let maxX = -Infinity;
  let maxY = -Infinity;
  const include = (x: number, y: number) => {
    minX = Math.min(minX, x);
    minY = Math.min(minY, y);
    maxX = Math.max(maxX, x);
    maxY = Math.max(maxY, y);
  };

  for (const node of graph.nodes) {
    const pos = positions[node.id];
    if (pos == null) {
      continue;
    }
    include(pos.x, pos.y);
    include(pos.x + GRAPH_NODE_WIDTH, pos.y + graphNodeHeight(node));
  }

  for (const edge of graph.edges) {
    const points = edgePoints(edge, graph.nodeById, positions);
    if (points == null) {
      continue;
    }
    include(points.from.x, points.from.y);
    include(points.to.x, points.to.y);
  }

  if (!Number.isFinite(minX)) {
    return { minX: 0, minY: 0, width: 1, height: 1 };
  }

  const pad = 80;
  return {
    minX: minX - pad,
    minY: minY - pad,
    width: maxX - minX + pad * 2,
    height: maxY - minY + pad * 2,
  };
}

export function GraphPanel({ adventure, selectedSceneId, onSelectScene, onClose }: Props) {
  const canvasRef = useRef<HTMLDivElement>(null);
  const graph = useMemo(() => buildAdventureGraph(adventure), [adventure]);
  const [nodeFilter, setNodeFilter] = useState<GraphNodeFilter>("all");
  const visibleGraph = useMemo(() => filterAdventureGraph(graph, nodeFilter), [graph, nodeFilter]);
  const [positions, setPositions] = useState<Record<string, GraphPoint>>(() => autoLayoutGraph(graph));
  const [pan, setPan] = useState(GRAPH_RESET_PAN);
  const [zoom, setZoom] = useState(1);
  const [graphSelectedId, setGraphSelectedId] = useState<string | null>(selectedSceneId);
  const dragRef = useRef<DragState | null>(null);
  const panZoomRef = useRef({ pan: GRAPH_RESET_PAN, zoom: 1 });
  panZoomRef.current = { pan, zoom };
  const bounds = useMemo(() => worldBounds(visibleGraph, positions), [visibleGraph, positions]);
  const filtered = nodeFilter !== "all";
  const showingLabel = filtered
    ? `${visibleGraph.nodes.length} of ${graph.nodes.length}`
    : String(graph.nodes.length);

  useEffect(() => {
    setGraphSelectedId(selectedSceneId);
  }, [selectedSceneId]);

  useEffect(() => {
    const nodeIds = visibleGraph.nodes.map((node) => node.id);
    setPositions((current) => mergePositions(nodeIds, current, autoLayoutGraph(visibleGraph)));
  }, [visibleGraph]);

  const resetView = useCallback(() => {
    setPan(GRAPH_RESET_PAN);
    setZoom(1);
  }, []);

  const runAutoLayout = useCallback(() => {
    setPositions(autoLayoutGraph(visibleGraph));
  }, [visibleGraph]);

  const applyZoomAt = useCallback((nextZoom: number, canvasLocal: GraphPoint | null) => {
    const current = panZoomRef.current;
    const clamped = clampZoom(nextZoom);
    if (canvasLocal != null) {
      const worldBefore = {
        x: (canvasLocal.x - current.pan.x) / Math.max(current.zoom, 0.001),
        y: (canvasLocal.y - current.pan.y) / Math.max(current.zoom, 0.001),
      };
      setPan({
        x: canvasLocal.x - worldBefore.x * clamped,
        y: canvasLocal.y - worldBefore.y * clamped,
      });
    }
    setZoom(clamped);
  }, []);

  useEffect(() => {
    const canvas = canvasRef.current;
    if (canvas == null) {
      return;
    }
    const onWheel = (event: WheelEvent) => {
      event.preventDefault();
      const rect = canvas.getBoundingClientRect();
      const mouseCanvas = { x: event.clientX - rect.left, y: event.clientY - rect.top };
      const currentZoom = panZoomRef.current.zoom;
      applyZoomAt(currentZoom - event.deltaY * 0.0015, mouseCanvas);
    };
    canvas.addEventListener("wheel", onWheel, { passive: false });
    return () => canvas.removeEventListener("wheel", onWheel);
  }, [applyZoomAt]);

  const screenToWorld = useCallback(
    (clientX: number, clientY: number): GraphPoint | null => {
      const canvas = canvasRef.current;
      if (canvas == null) {
        return null;
      }
      const rect = canvas.getBoundingClientRect();
      return {
        x: (clientX - rect.left - pan.x) / Math.max(zoom, 0.001),
        y: (clientY - rect.top - pan.y) / Math.max(zoom, 0.001),
      };
    },
    [pan.x, pan.y, zoom],
  );

  const selectNode = useCallback(
    (node: GraphNode) => {
      setGraphSelectedId(node.id);
      if (node.isExistingScene) {
        onSelectScene(node.id);
      }
    },
    [onSelectScene],
  );

  const onNodePointerDown = useCallback(
    (event: PointerEvent<HTMLDivElement>, node: GraphNode) => {
      if (event.button !== 0) {
        return;
      }
      event.stopPropagation();
      const world = screenToWorld(event.clientX, event.clientY);
      const pos = positions[node.id];
      if (world == null || pos == null) {
        return;
      }
      selectNode(node);
      dragRef.current = {
        kind: "node",
        pointerId: event.pointerId,
        nodeId: node.id,
        offsetX: world.x - pos.x,
        offsetY: world.y - pos.y,
      };
      event.currentTarget.setPointerCapture(event.pointerId);
    },
    [positions, screenToWorld, selectNode],
  );

  const onCanvasPointerDown = useCallback((event: PointerEvent<HTMLDivElement>) => {
    if (event.button !== 1 && event.button !== 2 && event.button !== 0) {
      return;
    }
    if (event.button === 0 && event.target !== event.currentTarget) {
      return;
    }
    dragRef.current = {
      kind: "pan",
      pointerId: event.pointerId,
      lastX: event.clientX,
      lastY: event.clientY,
    };
    event.currentTarget.setPointerCapture(event.pointerId);
  }, []);

  const onPointerMove = useCallback((event: PointerEvent<HTMLDivElement>) => {
    const drag = dragRef.current;
    if (drag == null || drag.pointerId !== event.pointerId) {
      return;
    }
    if (drag.kind === "pan") {
      const dx = event.clientX - drag.lastX;
      const dy = event.clientY - drag.lastY;
      drag.lastX = event.clientX;
      drag.lastY = event.clientY;
      setPan((current) => ({ x: current.x + dx, y: current.y + dy }));
      return;
    }

    const world = screenToWorld(event.clientX, event.clientY);
    if (world == null) {
      return;
    }
    setPositions((current) => ({
      ...current,
      [drag.nodeId]: { x: world.x - drag.offsetX, y: world.y - drag.offsetY },
    }));
  }, [screenToWorld]);

  const onPointerUp = useCallback((event: PointerEvent<HTMLDivElement>) => {
    const drag = dragRef.current;
    if (drag != null && drag.pointerId === event.pointerId) {
      dragRef.current = null;
    }
  }, []);

  return (
    <div className="graph-overlay" role="dialog" aria-label="Scene Graph Preview">
      <div className="graph-topbar">
        <span className="graph-title">Scene Graph Preview</span>
        <span className="graph-stats" data-testid="graph-stats">
          Nodes: {graph.existingNodesCount} Broken: {graph.brokenNodesCount} Unreachable:{" "}
          {graph.unreachableNodesCount}
          {filtered ? `  Showing: ${showingLabel}` : ""}
        </span>
        <div className="graph-filters">
          {GRAPH_NODE_FILTERS.map((item) => (
            <button
              key={item.id}
              type="button"
              data-testid={`graph-filter-${item.id}`}
              className={nodeFilter === item.id ? "active" : undefined}
              onClick={() => setNodeFilter(item.id)}
            >
              {item.label}
            </button>
          ))}
        </div>
        <span className="toolbar-spacer" />
        <label className="graph-zoom">
          Zoom
          <input
            type="range"
            min={GRAPH_ZOOM_MIN}
            max={GRAPH_ZOOM_MAX}
            step={0.01}
            value={zoom}
            onChange={(event) => applyZoomAt(Number(event.target.value), null)}
          />
        </label>
        <button type="button" onClick={resetView}>
          Reset View
        </button>
        <button type="button" onClick={runAutoLayout}>
          Auto Layout
        </button>
        <button type="button" onClick={onClose}>
          Close
        </button>
      </div>
      <div className="graph-legend">
        <strong>Legend:</strong>
        <span className="graph-legend-item start">Start</span>
        <span className="graph-legend-item regular">Regular</span>
        <span className="graph-legend-item unreachable">Unreachable</span>
        <span className="graph-legend-item broken">Broken Target</span>
        <span className="graph-legend-item exit">Scenario Exit</span>
        <span className="hint">Pan: empty drag / middle / right. Zoom: wheel.</span>
      </div>
      <div
        ref={canvasRef}
        className="graph-canvas"
        onPointerDown={onCanvasPointerDown}
        onPointerMove={onPointerMove}
        onPointerUp={onPointerUp}
        onPointerCancel={onPointerUp}
        onContextMenu={(event) => event.preventDefault()}
        style={{ backgroundPosition: `${pan.x}px ${pan.y}px` }}
      >
        {visibleGraph.nodes.length === 0 ? (
          <p className="graph-empty hint">No matching nodes.</p>
        ) : null}
        <div
          className="graph-world"
          style={{ transform: `translate(${pan.x}px, ${pan.y}px) scale(${zoom})` }}
        >
          <svg
            className="graph-edges"
            aria-hidden="true"
            width={bounds.width}
            height={bounds.height}
            viewBox={`${bounds.minX} ${bounds.minY} ${bounds.width} ${bounds.height}`}
            style={{ left: bounds.minX, top: bounds.minY }}
          >
            {visibleGraph.edges.map((edge, index) => {
              const points = edgePoints(edge, visibleGraph.nodeById, positions);
              if (points == null) {
                return null;
              }
              const { from, to } = points;
              const tangent = tangentDistance(from, to);
              const controlA = { x: from.x + tangent, y: from.y };
              const controlB = { x: to.x - tangent, y: to.y };
              const toNode = visibleGraph.nodeById[edge.toNodeId];
              const color = toNode?.isBrokenTarget ? "rgba(230, 77, 77, 0.95)" : "rgba(230, 230, 230, 1)";
              const path = `M ${from.x} ${from.y} C ${controlA.x} ${controlA.y}, ${controlB.x} ${controlB.y}, ${to.x} ${to.y}`;
              return (
                <g key={`${edge.fromNodeId}-${edge.toNodeId}-${edge.fromChoiceIndex}-${index}`}>
                  <path d={path} fill="none" stroke={color} strokeWidth={2} />
                  <polygon points={arrowPoints(to, controlB)} fill={color} />
                </g>
              );
            })}
          </svg>
          {visibleGraph.nodes.map((node) => {
            const pos = positions[node.id];
            if (pos == null) {
              return null;
            }
            const status = graphNodeStatus(node);
            const selected = node.id === graphSelectedId;
            return (
              <div
                key={node.id}
                className={`graph-node ${status}${selected ? " selected" : ""}`}
                style={{
                  left: pos.x,
                  top: pos.y,
                  width: GRAPH_NODE_WIDTH,
                  height: graphNodeHeight(node),
                }}
                onPointerDown={(event) => onNodePointerDown(event, node)}
              >
                <div className="graph-node-top" style={{ height: GRAPH_NODE_TOP_BLOCK_HEIGHT }}>
                  <div className="graph-node-id">{node.id}</div>
                  <div className="graph-node-tags">{node.tagsLabel}</div>
                  {node.contentTypes.length > 0 ? (
                    <div className="graph-node-content">
                      {node.contentTypes.map((type, index) => (
                        <span key={`${type}-${index}`} className="graph-chip" title={type}>
                          {type}
                        </span>
                      ))}
                    </div>
                  ) : null}
                </div>
                {node.choices.map((choice) => (
                  <div
                    key={choice.id}
                    className={`graph-choice-row${choice.exitsScenario ? " scenario-exit" : ""}`}
                    style={{ height: GRAPH_CHOICE_ROW_HEIGHT }}
                  >
                    <span className="graph-chip" title={choice.type}>
                      {choiceTypeLabel(choice.type)}
                    </span>
                    <span className="graph-choice-id">{choice.id}</span>
                    <span className="graph-choice-actions">
                      {choice.actionTypes.map((type, index) => (
                        <span key={`${type}-${index}`} className="graph-chip action" title={type}>
                          {type}
                        </span>
                      ))}
                    </span>
                  </div>
                ))}
              </div>
            );
          })}
        </div>
      </div>
    </div>
  );
}
