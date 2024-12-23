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

    [Header("Debugging")]
    [SerializeField] private bool drawDelaunayTriangulation;
    [SerializeField] private bool drawHallwayConnections;

    public void GenerateHallways(FacilityFloor facilityFloor)
    {
        List<Vertex> points = new List<Vertex>();
        foreach (TileCollection tileCollection in facilityFloor.TileCollections)
        {
            // TODO - Add support for predefined room hallways
            points.Add(new Vertex(tileCollection.GetClosestMapTilePositionToAverage()));
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
        
        //if (drawDelaunayTriangulation)
        //{
            
        //}

        //if (drawHallwayConnections)
        //{

        //}

        PathfindHallways(facilityFloor, chosenEdges);
    }

    // TODO - Add support for predefined rooms
    private void PathfindHallways(FacilityFloor facilityFloor, List<Prim.Edge> edges)
    {
        Pathfinder2D pathfinder2D = new Pathfinder2D(facilityFloor.Size);
        TileCollection tileCollection = new TileCollection(TileCollection.TileCollectionType.Hallway, facilityFloor.TileCollections.Count);
        facilityFloor.TileCollections.Add(tileCollection);

        foreach (Prim.Edge edge in edges)
        {
            Vector2Int start = facilityFloor.FloorMap[new Vector2Int((int)edge.U.Position.x, (int)edge.U.Position.y)].Collection.GetClosestMapTileTowardsPosition(edge.V.Position);
            Vector2Int end = facilityFloor.FloorMap[new Vector2Int((int)edge.V.Position.x, (int)edge.V.Position.y)].Collection.GetClosestMapTileTowardsPosition(edge.U.Position);

            List<Vector2Int> path = pathfinder2D.FindPath(start, (Pathfinder2D.Node from, Pathfinder2D.Node to) =>
            {
                return CalculatePathCost(facilityFloor, end, from, to);
            }, 
            (Pathfinder2D.Node node) =>
            {
                if (useExactEndPosition) return node.Position == end;

                return facilityFloor.FloorMap[node.Position].Collection == facilityFloor.FloorMap[end].Collection;
            });

            if (path != null)
            {
                facilityFloor.FloorMap[start].Collection.AddConnection(path[0], path[1]);
                facilityFloor.FloorMap[end].Collection.AddConnection(path[^1], path[^2]);

                if (path.Count >= 3)
                {
                    tileCollection.AddConnection(path[1], path[0]);
                    tileCollection.AddConnection(path[^2], path[^1]);

                    Debug.Log($"Path 1 - FROM: {path[1]} TO: {path[0]}");
                    Debug.Log($"Path 2 - FROM: {path[^2]} TO: {path[^1]}");
                }

                MapGeneration.PlacePossibleMapTiles(facilityFloor, tileCollection, MapTile.MapTileType.Hallway, path);
            }
        }

        if (tileCollection.mapTilePositions.Count <= 0)
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
        if (facilityFloor.FloorMap[to.Position].Collection == facilityFloor.FloorMap[end].Collection)
        {
            if (useExactEndPosition)
            {
                if (to.Position != end) pathInfo.traversable = false;
            }

            pathInfo.cost += 5;
            return pathInfo;
        }

        MapTile mapTile = facilityFloor.FloorMap[to.Position];

        if (mapTile.Type == MapTile.MapTileType.None)
        {
            pathInfo.cost += 10;
        }
        else if (mapTile.Type == MapTile.MapTileType.Hallway)
        {
            pathInfo.cost += 5;
        }
        else
        {
            pathInfo.traversable = false;
        }

        return pathInfo;
    }
}