using System.Collections.Generic;
using UnityEngine;
using Graphs;

public class HallwayGenerator : MonoBehaviour
{
    [Header("Hallway Properties")]
    [SerializeField] private bool useExactEndPosition;
    [SerializeField] private float straightnessPenalty = 1f;
    [Range(0, 100)]
    [SerializeField] private float additionalHallwayChance = 50f;

    [Header("Hallway Size")]
    [SerializeField] private int minHallwaySize = 1;
    [SerializeField] private int maxHallwaySize = 3;

    [Header("Debugging")]
    [SerializeField] private bool drawDelaunayTriangulation;
    [SerializeField] private bool drawHallwayConnections;

    public void GenerateHallways(FacilityFloor facilityFloor)
    {
        List<Vertex> points = new List<Vertex>();
        foreach (TileCollection tileCollection in facilityFloor.TileCollections)
        {
            // TODO - Add support for predefined room hallways
            points.Add(new Vertex(tileCollection.GetClosestTilePositionToAverage()));
        }

        Delaunay2D delaunay = Delaunay2D.Triangulate(points);

        List<Prim.Edge> edges = new List<Prim.Edge>();

        foreach (var edge in delaunay.Edges)
        {
            edges.Add(new Prim.Edge(edge.U, edge.V));
        }

        if (edges.Count < 1)
        {
            return;
        }

        List<Prim.Edge> chosenEdges = Prim.MinimumSpanningTree(edges, edges[0].U);

        //chosenEdges.RemoveAll(edge =>
        //{
        //    Vector2Int from = new Vector2Int((int)edge.U.Position.x, (int)edge.U.Position.y);
        //    Vector2Int to = new Vector2Int((int)edge.V.Position.x, (int)edge.V.Position.y);
        //    return (gridMap[from].Collection == gridMap[to].Collection);
        //});

        var remainingEdges = new HashSet<Prim.Edge>(edges);
        remainingEdges.ExceptWith(chosenEdges);

        foreach (var edge in remainingEdges)
        {
            if (FacilityGenerationManager.Roll(additionalHallwayChance))
            {
                chosenEdges.Add(edge);
            }
        }

        if (drawDelaunayTriangulation)
        {
            FacilityGenerationManager.Instance.EdgeVisualizer.DrawEdges(chosenEdges, new Vector3(0, 2, 0), facilityFloor.TileSize, "DelaunayTriangulation", true);
        }

        //if (drawHallwayConnections)
        //{

        //}

        PathfindVariableHallways(facilityFloor, chosenEdges);
    }

    // TODO - Add support for predefined rooms
    private void PathfindHallways(FacilityFloor facilityFloor, List<Prim.Edge> edges)
    {
        Pathfinder2D pathfinder2D = new Pathfinder2D(facilityFloor.Size);
        TileCollection tileCollection = new TileCollection(TileCollection.TileCollectionType.Hallway, facilityFloor.TileCollections.Count);
        facilityFloor.TileCollections.Add(tileCollection);

        foreach (Prim.Edge edge in edges)
        {
            Vector2Int start = facilityFloor.TileMap[new Vector2Int((int)edge.U.Position.x, (int)edge.U.Position.y)].TileCollection.GetClosestTileTowardsPosition(edge.V.Position);
            Vector2Int end = facilityFloor.TileMap[new Vector2Int((int)edge.V.Position.x, (int)edge.V.Position.y)].TileCollection.GetClosestTileTowardsPosition(edge.U.Position);

            List<Vector2Int> path = pathfinder2D.FindPath(start, (Pathfinder2D.Node from, Pathfinder2D.Node to) =>
            {
                return CalculatePathCost(facilityFloor, end, from, to);
            },
            (Pathfinder2D.Node node) =>
            {
                if (useExactEndPosition) return node.Position == end;

                return facilityFloor.TileMap[node.Position].TileCollection == facilityFloor.TileMap[end].TileCollection;
            });

            if (path != null)
            {
                facilityFloor.TileMap[start].TileCollection.AddConnection(path[0], path[1]);
                facilityFloor.TileMap[end].TileCollection.AddConnection(path[^1], path[^2]);

                if (path.Count >= 3)
                {
                    tileCollection.AddConnection(path[1], path[0]);
                    tileCollection.AddConnection(path[^2], path[^1]);

                    Debug.Log($"Path 1 - FROM: {path[1]} TO: {path[0]}");
                    Debug.Log($"Path 2 - FROM: {path[^2]} TO: {path[^1]}");
                }

                FacilityGeneration.PlacePossibleTiles(facilityFloor, tileCollection, path, new FacilityGeneration.TilePlaceParams(FacilityGeneration.TileType.Hallway, false));
            }
        }

        if (tileCollection.tilePositions.Count <= 0)
        {
            facilityFloor.TileCollections.Remove(tileCollection);
        }
    }

    private Pathfinder2D.PathInfo CalculatePathCost(FacilityFloor facilityFloor, Vector2Int end, Pathfinder2D.Node from, Pathfinder2D.Node to)
    {
        Pathfinder2D.PathInfo pathInfo = new Pathfinder2D.PathInfo();

        pathInfo.cost = Vector2Int.Distance(to.Position, end);

        Pathfinder2D.Node previous = from.Previous;
        if (previous != null)
        {
            Vector2Int previousDifference = previous.Position - from.Position;
            Vector2Int difference = from.Position - to.Position;

            if (previousDifference != difference)
            {
                pathInfo.cost += straightnessPenalty;
            }
        }

        pathInfo.traversable = true;
        if (facilityFloor.TileMap[to.Position].TileCollection == facilityFloor.TileMap[end].TileCollection)
        {
            if (useExactEndPosition)
            {
                if (to.Position != end) pathInfo.traversable = false;
            }

            pathInfo.cost += 5;
            return pathInfo;
        }

        FacilityGeneration.Tile tile = facilityFloor.TileMap[to.Position];

        if (tile.Type == FacilityGeneration.TileType.None)
        {
            pathInfo.cost += 10;
        }
        else if (tile.Type == FacilityGeneration.TileType.Hallway)
        {
            pathInfo.cost += 5;
        }
        else
        {
            pathInfo.traversable = false;
        }

        return pathInfo;
    }

    private void PathfindVariableHallways(FacilityFloor facilityFloor, List<Prim.Edge> edges)
    {
        MultiNodePathfinder2D pathfinder2D = new MultiNodePathfinder2D(facilityFloor.Size);

        foreach (Prim.Edge edge in edges)
        {
            TileCollection tileCollection = new TileCollection(TileCollection.TileCollectionType.Hallway, facilityFloor.TileCollections.Count);
            Vector2Int start = facilityFloor.TileMap[new Vector2Int((int)edge.U.Position.x, (int)edge.U.Position.y)].TileCollection.GetClosestTileTowardsPosition(edge.V.Position);
            Vector2Int end = facilityFloor.TileMap[new Vector2Int((int)edge.V.Position.x, (int)edge.V.Position.y)].TileCollection.GetClosestTileTowardsPosition(edge.U.Position);

            int additionalHallwaySize = Random.Range(minHallwaySize, maxHallwaySize);
            List<Vector2Int> path = pathfinder2D.FindPath(start, additionalHallwaySize + 1, (MultiNodePathfinder2D.Node from, List<MultiNodePathfinder2D.Node> to) =>
            {
                return CalculatePathCost(facilityFloor, end, from, to);
            },
            (MultiNodePathfinder2D.Node node) =>
            {
                if (useExactEndPosition) return node.Position == end;

                return facilityFloor.TileMap[node.Position].TileCollection == facilityFloor.TileMap[end].TileCollection;
            });

            if (path != null)
            {
                if (path.Count <= 1)
                {
                    Debug.LogWarning("Hallway Could Not Spawn");
                    continue;
                }

                facilityFloor.TileMap[start].TileCollection.AddConnection(path[0], path[1]);
                facilityFloor.TileMap[end].TileCollection.AddConnection(path[^1], path[^2]);

                if (path.Count >= 3)
                {
                    tileCollection.AddConnection(path[1], path[0]);
                    tileCollection.AddConnection(path[^2], path[^1]);

                    //Debug.Log($"Path 1 - FROM: {path[1]} TO: {path[0]}");
                    //Debug.Log($"Path 2 - FROM: {path[^2]} TO: {path[^1]}");
                }

                if (drawHallwayConnections)
                {
                    List<Prim.Edge> connections = new List<Prim.Edge>()
                    {
                        new Prim.Edge(new Vertex((Vector3)(Vector2)path[0] + Vector3.forward), new Vertex(path[1])),
                        new Prim.Edge(new Vertex((Vector3)(Vector2)path[^1] + Vector3.forward), new Vertex(path[^2]))
                    };

                    FacilityGenerationManager.Instance.EdgeVisualizer.DrawEdges(connections, new Vector3(0, 8, 0), facilityFloor.TileSize, "HallwayConnections", true);
                }

                List<Vector2Int> newPositions = MultiNodePathfinder2D.ExpandPath(path, additionalHallwaySize);
                //path.AddRange(newPositions);
                FacilityGeneration.PlacePossibleTiles(facilityFloor, tileCollection, newPositions, new FacilityGeneration.TilePlaceParams(FacilityGeneration.TileType.Hallway, false));
            }

            if (tileCollection.tilePositions.Count > 0)
            {
                facilityFloor.TileCollections.Add(tileCollection);
            }
        }

        DebugManager.Instance.pathDebugging.Add((pathfinder2D.VisualNodeGrid, pathfinder2D.CommandInvoker));
    }

    private MultiNodePathfinder2D.EvaluationInfo CalculatePathCost(FacilityFloor facilityFloor, Vector2Int end, MultiNodePathfinder2D.Node from, List<MultiNodePathfinder2D.Node> to)
    {
        MultiNodePathfinder2D.EvaluationInfo evaluationInfo = new MultiNodePathfinder2D.EvaluationInfo();

        evaluationInfo.Cost = Mathf.Pow(Vector2Int.Distance(to[0].Position, end), 2);

        MultiNodePathfinder2D.Node previous = from.Previous;
        if (previous != null)
        {
            Vector2Int previousDifference = previous.Position - from.Position;
            Vector2Int difference = from.Position - to[0].Position;

            if (previousDifference != difference)
            {
                evaluationInfo.Cost += straightnessPenalty;
            }
        }

        evaluationInfo.Traversable = true;
        bool reachedEnd = true;
        foreach (MultiNodePathfinder2D.Node node in to)
        {
            if (facilityFloor.TileMap[node.Position].TileCollection != facilityFloor.TileMap[end].TileCollection)
            {
                reachedEnd = false;
                break;
            }
        }

        if (reachedEnd)
        {
            if (useExactEndPosition)
            {
                bool success = false;
                foreach (MultiNodePathfinder2D.Node node in to)
                {
                    if (node.Position == end)
                    {
                        success = true;
                        break;
                    }
                }
                
                if (!success) evaluationInfo.Traversable = false;
            }

            evaluationInfo.Cost += 5;
            return evaluationInfo;
        }

        FacilityGeneration.Tile tile = facilityFloor.TileMap[to[0].Position];

        if (tile.Type == FacilityGeneration.TileType.None)
        {
            evaluationInfo.Cost += 25;
        }
        else if (tile.Type == FacilityGeneration.TileType.Hallway)
        {
            evaluationInfo.Cost += 5;
        }
        else
        {
            evaluationInfo.Traversable = false;
            return evaluationInfo;
        }

        for (int i = 1; i < to.Count; i++)
        {
            if (facilityFloor.TileMap[to[i].Position].Type != tile.Type)
            {
                //if (tile.Type == FacilityGeneration.TileType.Hallway && facilityFloor.TileMap[to[i].Position].Type == FacilityGeneration.TileType.None) continue;
                evaluationInfo.Traversable = false;
                return evaluationInfo;
            }
        }

        return evaluationInfo;
    }
}