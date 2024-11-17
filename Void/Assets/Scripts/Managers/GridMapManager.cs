using System.Collections.Generic;
using UnityEngine;
using Graphs;

public class GridMapManager : Singleton<GridMapManager>
{
    [Header("Map Generation Parameters")]
    [SerializeField] private int seed = 1234;
    [SerializeField] private Vector2Int gridSize = new Vector2Int(50, 50);
    [SerializeField] private int minRoomSize = 3;
    [SerializeField] private int maxRoomSize = 20;
    [SerializeField] private int roomCount = 5;
    [Range(0, 100)]
    [SerializeField] private float additionalHallwayChance = 12.5f;
    [Range(0, 100)]
    [SerializeField] private float randomRoomChance = 30f;
    [SerializeField] private int maxRoomAttempts = 100;
    [Space(10)]
    [Header("Tile Spawning Parameters")]
    [SerializeField] private float tileSize = 10;
    [Space(10)]
    [Header("Interior Generation Parameters")]
    [SerializeField] private int interiorTilesPerMapTile = 3;
    [SerializeField] private int maxFixtureAttempts = 100;
    public List<FixtureData> fixtures;
    [Space(10)]
    [Header("Visualization Options")]
    [SerializeField] private bool spawnCollectionAveragePositions;
    [SerializeField] private bool drawRoomConnectionTree;
    [SerializeField] private GameObject debugMarker;
    [Space(10)]
    [Header("Prefabs")]
    [SerializeField] private GameObject floor;
    [SerializeField] private GameObject wall;

    private Grid2D<MapTile> gridMap;
    private Grid2D<InteriorTile> interiorGridMap;
    private readonly List<TileCollection> tileCollections = new List<TileCollection>();
    private readonly List<FixtureInstance> fixtureInstances = new List<FixtureInstance>();

    private GameObject debugHolder;
    private GameObject levelHolder;

    public float TileSize { get { return tileSize; } }
    public GameObject Floor { get { return floor; } }
    public GameObject Wall { get { return wall; } }

    public static bool Roll(float chance)
    {
        return Random.Range(0, 100) < chance;
    }

    public void InitializeGridMap()
    {
        ResetGridMap();
        Random.InitState(seed);
        gridMap = new Grid2D<MapTile>(gridSize, Vector2Int.zero);
        levelHolder = new GameObject("Level Holder");
        debugHolder = new GameObject("Debug Holder");
    }

    public void InitializeGridMap(int seed, Vector2Int gridSize, float tileSize, int minRoomSize, int maxRoomSize, int roomCount)
    {
        this.seed = seed;
        this.gridSize = gridSize;
        this.tileSize = tileSize;
        this.minRoomSize = minRoomSize;
        this.maxRoomSize = maxRoomSize;
        this.roomCount = roomCount;
        InitializeGridMap();
    }

    public void ResetGridMap()
    {
        gridMap = null;
        tileCollections.Clear();
        Destroy(levelHolder);
        Destroy(debugHolder);
    }

    public void InitializeInteriorGridMap()
    {
        ResetInteriorGridMap();
        interiorGridMap = new Grid2D<InteriorTile>(gridSize * interiorTilesPerMapTile, Vector2Int.zero);
        foreach (TileCollection collection in tileCollections)
        {
            foreach (Vector2Int position in collection.mapTilePositions)
            {
                for (int y = 0; y < interiorTilesPerMapTile; y++)
                {
                    for (int x = 0; x < interiorTilesPerMapTile; x++)
                    {
                        interiorGridMap[interiorTilesPerMapTile * position + new Vector2Int(x, y)] = new InteriorTile(InteriorTile.InteriorTileType.None, null);
                    }
                }
            }
        }
    }

    public void InitializeInteriorGridMap(int interiorTilesPerMapTile)
    {
        this.interiorTilesPerMapTile = interiorTilesPerMapTile;
        InitializeInteriorGridMap();
    }

    public void ResetInteriorGridMap()
    {
        interiorGridMap = null;
        fixtureInstances.Clear();
    }

    public void GenerateRooms()
    {
        for (int i = 0; i < roomCount; i++)
        {
            if (Roll(randomRoomChance))
            {
                GenerateRandomRoom();
            }
            else
            {
                GenerateRectangularRoom();
            }
        }
    }

    public void PlacePredefinedRoom()
    {

    }

    private void GenerateRectangularRoom()
    {
        bool success = false;
        TileCollection tileCollection = new(TileCollection.TileCollectionType.Room, tileCollections.Count);

        int attempt = 0;
        while (!success && attempt < maxRoomAttempts)
        {
            attempt++;
            int roomLength = Random.Range(minRoomSize, maxRoomSize);
            int roomWidth = Random.Range(minRoomSize, maxRoomSize);

            Vector2Int roomPosition = new(Random.Range(0, gridSize.x - roomLength), Random.Range(0, gridSize.y - roomWidth));

            List<Vector2Int> mapTilePositions = new List<Vector2Int>();
            for (int y = 0; y < roomWidth; y++)
            {
                for (int x = 0; x < roomLength; x++)
                {
                    mapTilePositions.Add(new Vector2Int(roomPosition.x + x, roomPosition.y + y));
                }
            }

            success = PlaceMapTiles(tileCollection, MapTile.MapTileType.Room, mapTilePositions);
        }

        if (success)
        {
            tileCollections.Add(tileCollection);
            Debug.Log($"Successfully Generated Rectangular Room [{attempt} Attempt(s)]");
        }
        else
        {
            Debug.LogWarning($"Failed To Generate Rectangular Room After {attempt} Attempts");
        }
    }

    private void GenerateRandomRoom()
    {
        bool success = false;
        TileCollection tileCollection = new(TileCollection.TileCollectionType.Room, tileCollections.Count);

        int attempt = 0;
        while (!success && attempt < maxRoomAttempts)
        {
            attempt++;
            Vector2Int roomPosition = new(Random.Range(0, gridSize.x), Random.Range(0, gridSize.y));
            success = PlaceMapTile(tileCollection, MapTile.MapTileType.Room, roomPosition);
        }

        if (!success)
        {
            Debug.LogWarning($"Failed To Select Room Location After {attempt} Attempts");
            return;
        }

        attempt = 0;
        success = false;
        int maxTiles = Random.Range(minRoomSize * minRoomSize, maxRoomSize * 2);
        int tilesPlaced = 0;
        while (!success && attempt < maxRoomAttempts)
        {
            attempt++;
            Vector2Int position = tileCollection.GetRandomMapTilePosition();

            int direction = Random.Range(0, 4);

            if (direction == 0)
            {
                if (PlaceMapTile(tileCollection, MapTile.MapTileType.Room, position + new Vector2Int(1, 0)))
                {
                    tilesPlaced++;
                }
            }
            else if (direction == 1)
            {
                if (PlaceMapTile(tileCollection, MapTile.MapTileType.Room, position + new Vector2Int(0, 1)))
                {
                    tilesPlaced++;
                }
            }
            else if (direction == 2)
            {
                if (PlaceMapTile(tileCollection, MapTile.MapTileType.Room, position - new Vector2Int(1, 0)))
                {
                    tilesPlaced++;
                }
            }
            else if (direction == 3)
            {
                if (PlaceMapTile(tileCollection, MapTile.MapTileType.Room, position - new Vector2Int(0, 1)))
                {
                    tilesPlaced++;
                }
            }

            success = tilesPlaced >= maxTiles;
        }

        if (success)
        {
            tileCollections.Add(tileCollection);
            Debug.Log($"Successfully Generated Random Room [{attempt} Attempt(s)]");
        }
        else
        {
            Debug.LogWarning($"Successfully Generated Random Room, [Reached Max Tile Place Attempts]");
        }
    }

    public void GenerateHallways()
    {
        List<Vertex> points = new List<Vertex>();
        foreach (TileCollection tileCollection in tileCollections)
        {
            // By Using the Collection Average Position, it is not guarenteed that that position contains a goal tile, which could void the path/connection to the room in question.
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

        var remainingEdges = new HashSet<Prim.Edge>(edges);
        remainingEdges.ExceptWith(chosenEdges);

        foreach (var edge in remainingEdges)
        {
            if (Roll(additionalHallwayChance))
            {
                chosenEdges.Add(edge);
            }
        }

        EdgeVisualizer edgeVisualizer = GetComponent<EdgeVisualizer>();
        if (drawRoomConnectionTree)
        {
            edgeVisualizer.DrawEdges(chosenEdges, true);
        }
        else
        {
            edgeVisualizer.ClearLines();
        }

        PathfindHallways(chosenEdges);
    }

    private void PathfindHallways(List<Prim.Edge> edges)
    {
        Pathfinder2D pathfinder2D = new Pathfinder2D(gridSize);
        TileCollection tileCollection = new(TileCollection.TileCollectionType.Hallway, tileCollections.Count);
        tileCollections.Add(tileCollection);

        foreach (Prim.Edge edge in edges)
        {
            Vector2Int start = new Vector2Int((int)edge.U.Position.x, (int)edge.U.Position.y);
            Vector2Int end = new Vector2Int((int)edge.V.Position.x, (int)edge.V.Position.y);
            int startCollectionId = gridMap[start].Collection.Id;
            int endCollectionId = gridMap[end].Collection.Id;

            List<Vector2Int> path = pathfinder2D.FindPath(start, end, (Pathfinder2D.Node a, Pathfinder2D.Node b) =>
            {
                Pathfinder2D.PathInfo pathInfo = new Pathfinder2D.PathInfo();

                pathInfo.cost = Vector2Int.Distance(b.Position, end);
                pathInfo.traversable = true;

                MapTile mapTile = gridMap[b.Position];

                if (mapTile.Type == MapTile.MapTileType.None)
                {
                    pathInfo.cost += 10;
                }
                else if (mapTile.Type == MapTile.MapTileType.Room)
                {
                    pathInfo.traversable = false;
                    pathInfo.isStart = mapTile.Collection.Id == startCollectionId;
                    pathInfo.isGoal = mapTile.Collection.Id == endCollectionId;
                }
                else if (mapTile.Type == MapTile.MapTileType.Hallway)
                {
                    pathInfo.cost += 5;
                }

                return pathInfo;
            });

            if (path != null)
            {
                MapTile startConnection = gridMap[path[0]];
                MapTile endConnection = gridMap[path[path.Count - 1]];

                startConnection.Collection.AddConnection(path[0], path[1]);
                endConnection.Collection.AddConnection(path[path.Count - 1], path[path.Count - 2]);

                tileCollection.AddConnection(path[1], path[0]);
                tileCollection.AddConnection(path[path.Count - 2], path[path.Count - 1]);

                PlacePossibleMapTiles(tileCollection, MapTile.MapTileType.Hallway, path);
            }
        }

        if (tileCollection.mapTilePositions.Count <= 0)
        {
            tileCollections.Remove(tileCollection);
        }
    }

    public void GenerateWalkways()
    {

    }

    public void GenerateTasks()
    {

    }

    public void GenerateInteriors()
    {
        foreach (TileCollection collection in tileCollections)
        {
            int attempt = 0;
            int start = fixtureInstances.Count;
            while (attempt < maxFixtureAttempts)
            {
                FixtureData fixtureData = fixtures[0];
                Vector2Int position = interiorTilesPerMapTile * collection.GetRandomMapTilePosition() + new Vector2Int(Random.Range(0, interiorTilesPerMapTile), Random.Range(0, interiorTilesPerMapTile));
                FixtureInstance fixtureInstance = new FixtureInstance();
                fixtureInstance.Data = fixtureData;
                fixtureInstance.Position = ((Vector2)(position) - interiorTilesPerMapTile * 0.5f * Vector2.one) / interiorTilesPerMapTile;
                fixtureInstance.Forward = Vector2Int.up;
                List<Vector2Int> positions = new List<Vector2Int>();
                foreach (Vector2Int offset in fixtureData.tilePositions)
                {
                    positions.Add(position + offset);
                }
                if (PlaceInteriorTiles(InteriorTile.InteriorTileType.Fixture, fixtureInstance, positions))
                {
                    SpawnFixture(fixtureInstance);
                }
                attempt++;
            }
        }
    }

    public void SpawnTiles()
    {
        Vector2Int[] neighbors =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right,
        };

        foreach (TileCollection collection in tileCollections)
        {
            foreach (Vector2Int position in collection.mapTilePositions)
            {
                GameObject newTile = new GameObject("Tile");
                newTile.transform.parent = levelHolder.transform;
                Instantiate(floor, new Vector3(position.x, 0, position.y), Quaternion.identity, newTile.transform);

                foreach (Vector2Int neighbor in neighbors)
                {
                    if (gridMap.InBounds(position + neighbor))
                    {
                        if (collection.connections.Contains((position, position + neighbor)))
                        {
                            continue;
                        }
                        
                        if (gridMap[position].Collection == gridMap[position + neighbor].Collection)
                        {
                            continue;
                        }
                    }

                    float angle = 0;
                    if (neighbor == Vector2Int.up)
                    {
                        angle = 0;
                    }
                    else if (neighbor == Vector2Int.right)
                    {
                        angle = 90;
                    }
                    else if (neighbor == Vector2Int.down)
                    {
                        angle = 180;
                    }
                    else
                    {
                        angle = 270;
                    }

                    Instantiate(wall, new Vector3(position.x, 0, position.y), Quaternion.Euler(0, angle, 0), newTile.transform);
                }

                newTile.transform.localScale = tileSize * Vector3.one;
            }

            if (spawnCollectionAveragePositions)
            {
                Vector2Int position = collection.GetClosestMapTilePositionToAverage();
                Instantiate(debugMarker, tileSize * new Vector3(position.x, 0, position.y), Quaternion.identity, debugHolder.transform);
            }
        }
    }
    public void SpawnFixture(FixtureInstance fixtureInstance)
    {
        Instantiate(fixtureInstance.Data.FixturePrefab, tileSize * new Vector3(fixtureInstance.Position.x, 0, fixtureInstance.Position.y), Quaternion.identity, levelHolder.transform);
    }

    public bool PlaceMapTile(TileCollection tileCollection, MapTile.MapTileType type, Vector2Int position)
    {
        if (CanPlaceMapTile(position))
        {
            ForcePlaceMapTile(tileCollection, type, position);
            return true;
        }
        return false;
    }

    public void ForcePlaceMapTile(TileCollection tileCollection, MapTile.MapTileType type, Vector2Int position)
    {
        MapTile mapTile = new MapTile(type, tileCollection);
        gridMap[position] = mapTile;
        tileCollection.AddMapTilePosition(position);
    }

    public void ForcePlaceMapTiles(TileCollection tileCollection, MapTile.MapTileType type, List<Vector2Int> positions)
    {
        foreach (Vector2Int position in positions)
        {
            ForcePlaceMapTile(tileCollection, type, position);
        }
    }

    public bool PlaceMapTiles(TileCollection tileCollection, MapTile.MapTileType type, List<Vector2Int> positions)
    {
        if (!CanPlaceMapTiles(positions))
        {
            return false;
        }
        ForcePlaceMapTiles(tileCollection, type, positions);
        return true;
    }

    public void PlacePossibleMapTiles(TileCollection tileCollection, MapTile.MapTileType type, List<Vector2Int> positions)
    {
        foreach (Vector2Int position in positions)
        {
            PlaceMapTile(tileCollection, type, position);
        }
    }

    public bool CanPlaceMapTile(Vector2Int position)
    {
        if (gridMap.InBounds(position))
        {
            return gridMap[position].Type == MapTile.MapTileType.None;
        }
        return false;
    }

    public bool CanPlaceMapTiles(List<Vector2Int> positions)
    {
        foreach (Vector2Int position in positions)
        {
            if (!CanPlaceMapTile(position))
            {
                return false;
            }
        }
        return true;
    }

//-------------------------------------------------------------------------------------------------------------------------

    public bool PlaceInteriorTile(InteriorTile.InteriorTileType type, FixtureInstance fixture, Vector2Int position)
    {
        if (CanPlaceInteriorTile(position))
        {
            ForcePlaceInteriorTile(type, fixture, position);
            return true;
        }
        return false;
    }

    public void ForcePlaceInteriorTile(InteriorTile.InteriorTileType type, FixtureInstance fixture, Vector2Int position)
    {
        InteriorTile interiorTile = new InteriorTile(type, fixture);
        interiorGridMap[position] = interiorTile;
        if (fixture != null)
        {
            fixtureInstances.Add(fixture);
        }
    }

    public void ForcePlaceInteriorTiles(InteriorTile.InteriorTileType type, FixtureInstance fixture, List<Vector2Int> positions)
    {
        foreach (Vector2Int position in positions)
        {
            ForcePlaceInteriorTile(type, fixture, position);
        }
    }

    public bool PlaceInteriorTiles(InteriorTile.InteriorTileType type, FixtureInstance fixture, List<Vector2Int> positions)
    {
        if (!CanPlaceInteriorTiles(positions))
        {
            return false;
        }
        ForcePlaceInteriorTiles(type, fixture, positions);
        return true;
    }

    public void PlacePossibleInteriorTiles(InteriorTile.InteriorTileType type, FixtureInstance fixture, List<Vector2Int> positions)
    {
        foreach (Vector2Int position in positions)
        {
            PlaceInteriorTile(type, fixture, position);
        }
    }

    public bool CanPlaceInteriorTile(Vector2Int position)
    {
        if (interiorGridMap.InBounds(position))
        {
            return interiorGridMap[position].Type == InteriorTile.InteriorTileType.None;
        }
        return false;
    }

    public bool CanPlaceInteriorTiles(List<Vector2Int> positions)
    {
        foreach (Vector2Int position in positions)
        {
            if (!CanPlaceInteriorTile(position))
            {
                Debug.Log($"Tile Not Empty {position}");
                return false;
            }
        }
        return true;
    }

    public bool CanPlaceFixture(Vector2Int position)
    {
        if (interiorGridMap.InBounds(position))
        {
            return interiorGridMap[position].Type == InteriorTile.InteriorTileType.None;
        }
        return false;
    }
}