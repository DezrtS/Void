using System.Collections.Generic;
using UnityEngine;
using Graphs;
using Unity.Netcode;

public class GridMapManager : Singleton<GridMapManager>
{
    [Header("Map Generation Parameters")]
    [SerializeField] private bool generateOnStart = true;
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
    public List<RoomData> rooms;
    [Space(10)]
    [Header("Tile Spawning Parameters")]
    [SerializeField] private float tileSize = 10;
    [Space(10)]
    [Header("Interior Generation Parameters")]
    [SerializeField] private int interiorTilesPerMapTile = 3;
    [Range(0, 100)]
    [SerializeField] private int emptySpacePercentage = 50;
    [SerializeField] private int maxFixturesPerTileCollection = 25;
    [SerializeField] private int maxTaskItemsPerTileCollection = 2;
    [SerializeField] private int maxFixtureAttempts = 100;
    [SerializeField] private bool doThroughWallCheck = true;
    public List<FixtureData> fixtures;
    public List<ItemData> taskItems;
    private List<Item> items = new List<Item>();
    [Space(10)]
    [Header("Visualization Options")]
    [SerializeField] private bool spawnCollectionAveragePositions;
    [SerializeField] private bool spawnFixturePositions;
    [SerializeField] private bool spawnFixtureRelationships;
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
    private List<Prim.Edge> relationshipEdges = new List<Prim.Edge>();

    private GameObject debugHolder;
    private GameObject levelHolder;

    private TileCollection elevatorRoom;

    public float TileSize { get { return tileSize; } }
    public GameObject Floor { get { return floor; } }
    public GameObject Wall { get { return wall; } }

    public static bool Roll(float chance)
    {
        return Random.Range(0, 100) < chance;
    }

    public static Matrix4x4 GetRotationMatrix(RotationPreset rotationPreset)
    {
        return rotationPreset switch
        {
            RotationPreset.Zero => new Matrix4x4(
                new Vector4(1, 0, 0, 0),
                new Vector4(0, 1, 0, 0),
                new Vector4(0, 0, 1, 0),
                new Vector4(0, 0, 0, 1)
            ),
            RotationPreset.Ninety => new Matrix4x4(
                new Vector4(0, -1, 0, 0),
                new Vector4(1, 0, 0, 0),
                new Vector4(0, 0, 1, 0),
                new Vector4(0, 0, 0, 1)
            ),
            RotationPreset.OneEighty => new Matrix4x4(
                new Vector4(-1, 0, 0, 0),
                new Vector4(0, -1, 0, 0),
                new Vector4(0, 0, 1, 0),
                new Vector4(0, 0, 0, 1)
            ),
            RotationPreset.TwoSeventy => new Matrix4x4(
                new Vector4(0, 1, 0, 0),
                new Vector4(-1, 0, 0, 0),
                new Vector4(0, 0, 1, 0),
                new Vector4(0, 0, 0, 1)
            ),
            _ => new Matrix4x4(
                new Vector4(1, 0, 0, 0),
                new Vector4(0, 1, 0, 0),
                new Vector4(0, 0, 1, 0),
                new Vector4(0, 0, 0, 1)
            ),
        };
    }

    public Vector3 GetElevatorRoomPosition()
    {
        Vector2 position = (Vector2)elevatorRoom.GetRandomMapTilePosition() * tileSize;
        return new Vector3(position.x, 2, position.y);
    }

    public Vector3 GetMonsterSpawnPosition()
    {
        TileCollection maxCollection = elevatorRoom;
        float distance = float.MinValue;
        foreach (TileCollection collection in tileCollections)
        {
            if (collection.Type == TileCollection.TileCollectionType.Hallway) continue;

            float tempDistance = (collection.AveragePosition - elevatorRoom.AveragePosition).magnitude;
            if (tempDistance > distance)
            {
                distance = tempDistance;
                maxCollection = collection;
            }
        }

        return new Vector3(maxCollection.AveragePosition.x, 2, maxCollection.AveragePosition.y);
    }

    private void Start()
    {
        if (generateOnStart)
        {
            GenerateNewGridMap();
        }
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

    public void GenerateNewGridMap()
    {
        InitializeGridMap();
        GenerateRooms();
        GenerateHallways();

        InitializeInteriorGridMap();
        //GenerateTasks();
        GenerateInteriors();

        SpawnTiles();
        SpawnFixtures();
    }

    public void GenerateRooms()
    {
        PlacePredefinedRooms();

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

    public void PlacePredefinedRooms()
    {
        RoomData roomData = rooms[0];
        Vector2Int position = new Vector2Int(gridSize.x / 2 - roomData.GridSize.x / 2, gridSize.x / 2 - roomData.GridSize.x / 2);
        elevatorRoom = new TileCollection(TileCollection.TileCollectionType.Room, 0);
        List<Vector2Int> newPositions = new List<Vector2Int>(roomData.TilePositions);
        for (int i = 0; i < newPositions.Count; i++)
        {
            newPositions[i] += position;
        }
        foreach (Connection connection in roomData.Connections)
        {
            elevatorRoom.AddConnection(connection.from + position, connection.to + position);
        }
        elevatorRoom.InitiatlizeRoom(roomData);
        tileCollections.Add(elevatorRoom);
        PlaceMapTiles(elevatorRoom, MapTile.MapTileType.Room, newPositions);
        Instantiate(roomData.RoomPrefab, interiorTilesPerMapTile * new Vector3(position.x, 0, position.y), Quaternion.identity, levelHolder.transform);
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
            //RoomData roomData = tileCollection.RoomData;
            //if (roomData)
            //{
            //    foreach (Connection connection in roomData.connections)
            //    {
            //        points.Add(new Vertex(connection.from + tileCollection.Bounds.lowerLeft));
            //    }
            //}
            //else
            //{
                points.Add(new Vertex(tileCollection.GetClosestMapTilePositionToAverage()));
            //}
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
            MapTile startTile = gridMap[start];
            MapTile endTile = gridMap[end];
            bool containsRoomData = false;
            //if (endTile.Collection != null)
            //{
            //    containsRoomData = endTile.Collection.RoomData != null;
            //}
            
            bool startTileExists = startTile.Type != MapTile.MapTileType.None;
            bool endTileExists = endTile.Type != MapTile.MapTileType.None;

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
                    if (startTileExists && !containsRoomData)
                    {
                        pathInfo.isStart = mapTile.Collection.Id == startTile.Collection.Id;
                    }
                    else
                    {
                        pathInfo.isStart = false;
                    }
                 
                    if (endTileExists && !containsRoomData)
                    {
                        pathInfo.isGoal = mapTile.Collection.Id == endTile.Collection.Id;
                    }
                    else
                    {
                        pathInfo.isGoal = b.Position == end;
                    }
                }
                else if (mapTile.Type == MapTile.MapTileType.Hallway)
                {
                    pathInfo.cost += 5;
                }

                return pathInfo;
            });

            if (path != null)
            {
                if (startTileExists && endTileExists)
                {
                    MapTile startConnection = gridMap[path[0]];
                    MapTile endConnection = gridMap[path[path.Count - 1]];

                    startConnection.Collection.AddConnection(path[0], path[1]);
                    if (!containsRoomData)
                    {
                        endConnection.Collection.AddConnection(path[path.Count - 1], path[path.Count - 2]);
                    }

                    tileCollection.AddConnection(path[1], path[0]);
                    tileCollection.AddConnection(path[path.Count - 2], path[path.Count - 1]);
                }

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
        //foreach (TileCollection collection in tileCollections)
        //{

        //}
    }

    public void GenerateTasks()
    {
        foreach (TileCollection collection in tileCollections)
        {
            if (collection.Type == TileCollection.TileCollectionType.Hallway)
            {
                continue;
            }

            RoomData roomData = collection.RoomData;
            if (roomData)
            {
                if (roomData.HasInterior)
                {
                    continue;
                }
            }

            for (int i = 0; i < maxTaskItemsPerTileCollection; i++)
            {
                int index = Random.Range(0, taskItems.Count);
                ItemData itemData = taskItems[index];
                Vector2Int position = collection.GetRandomMapTilePosition();
                GameObject spawnedItem = Instantiate(itemData.ItemPrefab, new Vector3(position.x, 2, position.y), Quaternion.identity);
                spawnedItem.GetComponent<Item>().NetworkObject.Spawn();
            }
        }
    }

    public void GenerateInteriors()
    {
        foreach (TileCollection collection in tileCollections)
        {
            if (collection.Type == TileCollection.TileCollectionType.Hallway)
            {
                continue;
            }

            RoomData roomData = collection.RoomData;
            if (roomData)
            {
                if (roomData.HasInterior)
                {
                    continue;
                }
            }

            bool establishedOriginFixture = false;
            int attempt = 0;
            float maxRoomTileCount = collection.mapTilePositions.Count * interiorTilesPerMapTile * interiorTilesPerMapTile;
            float roomTileCount = maxRoomTileCount;
            while (!establishedOriginFixture && attempt < maxFixtureAttempts)
            {
                int start = fixtureInstances.Count;
                int validRelationshipsStart = start;
                FixtureInstance originFixture = PlaceRandomFixture(collection, start);
                if (originFixture != null)
                {
                    roomTileCount -= originFixture.Data.GridSize.magnitude;
                    establishedOriginFixture = true;
                    for (int i = 0; i < maxFixturesPerTileCollection; i++)
                    {
                        if ((roomTileCount / maxRoomTileCount) * 100 < emptySpacePercentage)
                        {
                            break;
                        }

                        int range = fixtureInstances.Count - validRelationshipsStart;
                        int selectedFixtureIndex = Random.Range(0, range);
                        int startIndex = selectedFixtureIndex;
                        attempt = 0;

                        while (attempt < maxFixtureAttempts)
                        {
                            FixtureInstance selectedFixture = fixtureInstances[validRelationshipsStart + selectedFixtureIndex];
                            // TODO : FIX OUT OF RANGE ERROR
                            FixtureInstance placedFixture = PlaceRandomRelationship(collection, selectedFixture, start);

                            if (placedFixture == null)
                            {
                                selectedFixtureIndex = (selectedFixtureIndex + 1) % range;

                                if (selectedFixtureIndex == startIndex)
                                {
                                    validRelationshipsStart += range;
                                    establishedOriginFixture = false;
                                    attempt = 0;
                                    while (!establishedOriginFixture && attempt < maxFixtureAttempts)
                                    {
                                        originFixture = PlaceRandomFixture(collection, start);

                                        if (originFixture != null)
                                        {
                                            roomTileCount -= originFixture.Data.GridSize.magnitude;
                                            establishedOriginFixture = true;
                                            break;
                                        }

                                        attempt++;
                                    }

                                    if (!establishedOriginFixture)
                                    {
                                        Debug.LogWarning($"Failed to Establish New Origin Fixture Within {maxFixtureAttempts} Attempts");
                                        //return;
                                    }
                                    break;
                                }
                            }
                            else
                            {
                                roomTileCount -= placedFixture.Data.GridSize.magnitude;
                                //Debug.Log("RelationshipFound");
                                break;
                            }

                            attempt++;

                            if (attempt >= maxFixtureAttempts - 15)
                            {
                                Debug.LogWarning("Reached Max Fixture Place Attepmts, Skipping Over Fixture Placement");
                            }
                        }
                    }
                }
                //else
                //{
                //    Debug.LogWarning("Failed to Place Origin Fixture");
                //}
                attempt++;
            }
        }

        if (spawnFixtureRelationships)
        {
            EdgeVisualizer edgeVisualizer = GetComponent<EdgeVisualizer>();
            edgeVisualizer.DrawEdges(relationshipEdges, true);
        }
    }

    private FixtureInstance PlaceRandomFixture(TileCollection collection, int verifyFrom)
    {
        int attempt = 0;
        int selectedFixtureIndex = Random.Range(0, fixtures.Count);
        while (attempt < maxFixtureAttempts)
        {
            FixtureData fixture = fixtures[selectedFixtureIndex];
            Vector2Int randomPosition = interiorTilesPerMapTile * collection.GetRandomMapTilePosition() + new Vector2Int(Random.Range(0, interiorTilesPerMapTile), Random.Range(0, interiorTilesPerMapTile));
            RotationPreset randomRotationPreset = (RotationPreset)Random.Range(0, 4);

            Matrix4x4 rotationMatrix = GetRotationMatrix(randomRotationPreset);

            FixtureInstance fixtureInstance = new FixtureInstance();
            fixtureInstance.Data = fixture;
            fixtureInstance.ParentCollection = collection;
            fixtureInstance.Position = randomPosition;
            fixtureInstance.RotationMatrix = rotationMatrix;
            fixtureInstance.RotationPreset = randomRotationPreset;

            bool forceQuit = false;
            List<Vector2Int> positions = new List<Vector2Int>();
            foreach (Vector2Int tilePosition in fixture.TilePositions)
            {
                Vector2 rotatedPosition = rotationMatrix * (Vector2)tilePosition;
                Vector2Int newPosition = new Vector2Int((int)rotatedPosition.x, (int)rotatedPosition.y) + randomPosition;
                if (doThroughWallCheck)
                {
                    Vector2 scaledPosition = (rotatedPosition + randomPosition) / interiorTilesPerMapTile;
                    // TODO SCALED POSITION IS SOMETIMES OUT OF BOUNDS (AT MAX X OR Y)
                    if (!gridMap.InBounds(new Vector2Int((int)scaledPosition.x, (int)scaledPosition.y)))
                    {
                        forceQuit = true;
                        //Debug.LogWarning("Fixture Placed Through Wall");
                        break;
                    }

                    if (gridMap[scaledPosition].Collection != collection)
                    {
                        forceQuit = true;
                        //Debug.LogWarning("Fixture Placed Through Wall");
                        break;
                    }
                }
                positions.Add(newPosition);
            }

            if (!forceQuit && CanPlaceInteriorTiles(positions))
            {
                if (VerifyRestrictions(fixtureInstance))
                {
                    ForcePlaceInteriorTiles(InteriorTile.InteriorTileType.Fixture, fixtureInstance, positions);
                    for (int i = verifyFrom; i < fixtureInstances.Count; i++)
                    {
                        if (!VerifyRestrictions(fixtureInstances[i]))
                        {
                            forceQuit = true;
                            break;
                        }

                        // POTENTIAL OPTIMIZATION : SEND POSITIONS TO VERIFY FUNCTION TO CHECK IF THOSE POSITIONS ARE INSIDE OF THE BOUNDS OF THE RESTRICTIONS AND IF SO, IF THEY NEGATE ITS RESTRICTIONS.
                    }

                    if (!forceQuit)
                    {
                        //SpawnFixture(fixtureInstance);
                        return fixtureInstance;
                    }
                    else
                    {
                        fixtureInstances.RemoveAt(fixtureInstances.Count - 1);
                        ForcePlaceInteriorTiles(InteriorTile.InteriorTileType.None, null, positions);
                    }
                }
            }

            attempt++;
        }

        return null;
    }

    private FixtureInstance PlaceRandomRelationship(TileCollection collection, FixtureInstance fixtureInstance, int verifyFrom)
    {
        if (fixtureInstance.Data.Relationships.Count == 0)
        {
            return null;
        }

        int attempt = 0;
        int range = fixtureInstance.Data.Relationships.Count;
        int selectedRelationshipIndex = Random.Range(0, range);
        int startIndex = selectedRelationshipIndex;
        while (attempt < maxFixtureAttempts)
        {
            FixtureRelationshipData relationship = fixtureInstance.Data.Relationships[selectedRelationshipIndex];

            if (!relationship.Enabled)
            {
                selectedRelationshipIndex = (selectedRelationshipIndex + 1) % range;
                if (selectedRelationshipIndex == startIndex)
                {
                    return null;
                }
                continue;
            }

            FixtureData fixture = relationship.OtherFixture;
            Vector3 originRotatedPosition = fixtureInstance.RotationMatrix * (Vector2)relationship.Position;
            Vector2Int position = (new Vector2Int((int)originRotatedPosition.x, (int)originRotatedPosition.y) + fixtureInstance.Position);

            RotationPreset newRotationPreset = (RotationPreset)(((int)relationship.RotationPreset + (int)fixtureInstance.RotationPreset) % 4);

            FixtureInstance newFixtureInstance = new FixtureInstance();
            newFixtureInstance.Data = fixture;
            newFixtureInstance.ParentCollection = collection;
            newFixtureInstance.Position = position;
            newFixtureInstance.RotationMatrix = GetRotationMatrix(newRotationPreset);
            newFixtureInstance.RotationPreset = newRotationPreset;

            bool forceQuit = false;
            List<Vector2Int> positions = new List<Vector2Int>();
            foreach (Vector2Int tilePosition in fixture.TilePositions)
            {
                Vector2 rotatedPosition = newFixtureInstance.RotationMatrix * (Vector2)tilePosition;
                Vector2Int newPosition = new Vector2Int((int)rotatedPosition.x, (int)rotatedPosition.y) + newFixtureInstance.Position;
                if (doThroughWallCheck)
                {
                    Vector2 scaledPosition = (rotatedPosition + newFixtureInstance.Position) / interiorTilesPerMapTile;

                    if (!gridMap.InBounds(new Vector2Int((int)scaledPosition.x, (int)scaledPosition.y)))
                    {
                        forceQuit = true;
                        //Debug.LogWarning("Fixture Placed Through Wall");
                        break;
                    }

                    if (gridMap[scaledPosition].Collection != collection)
                    {
                        forceQuit = true;
                        //Debug.LogWarning("Fixture Placed Through Wall");
                        break;
                    }
                }
                positions.Add(newPosition);
            }

            if (!forceQuit && CanPlaceInteriorTiles(positions))
            {
                if (VerifyRestrictions(newFixtureInstance))
                {
                    ForcePlaceInteriorTiles(InteriorTile.InteriorTileType.Fixture, newFixtureInstance, positions);
                    for (int i = verifyFrom; i < fixtureInstances.Count; i++)
                    {
                        if (!VerifyRestrictions(fixtureInstances[i]))
                        {
                            forceQuit = true;
                            break;
                        }
                    }

                    if (!forceQuit)
                    {
                        if (spawnFixtureRelationships)
                        {

                            Prim.Edge edge = new Prim.Edge(new Vertex((Vector2)fixtureInstance.Position / tileSize), new Vertex((Vector2)newFixtureInstance.Position / tileSize));
                            Instantiate(debugMarker,  new Vector3(fixtureInstance.Position.x, 3, fixtureInstance.Position.y), Quaternion.identity, debugHolder.transform);
                            relationshipEdges.Add(edge);
                        }
                        //SpawnFixture(newFixtureInstance);
                        return newFixtureInstance;
                    }
                    else
                    {
                        fixtureInstances.RemoveAt(fixtureInstances.Count - 1);
                        ForcePlaceInteriorTiles(InteriorTile.InteriorTileType.None, null, positions);
                    }
                }
            }

            selectedRelationshipIndex = (selectedRelationshipIndex + 1) % range;
            if (selectedRelationshipIndex == startIndex)
            {
                return null;
            }
            attempt++;
        }

        return null;
    }

    public bool VerifyRestrictions(FixtureInstance fixtureInstance)
    {
        if (fixtureInstance.Data.Restrictions.Count == 0)
        {
            return true;
        }

        bool allDisabled = true;
        foreach (RestrictionData restrictionData in fixtureInstance.Data.Restrictions)
        {
            if (!restrictionData.Enabled)
            {
                continue;
            }
            else
            {
                allDisabled = false;
            }

            bool manditoryVsProhibited;
            bool condition = true;
            foreach (Restriction restriction in restrictionData.Restrictions)
            {
                manditoryVsProhibited = restriction.Type == Restriction.RestrictionType.Manditory;
                condition = true;

                if (restriction.HasInteriorTileType && condition)
                {
                    foreach (Vector2Int position in restriction.Positions)
                    {
                        Vector3 rotatedPosition = fixtureInstance.RotationMatrix * (Vector2)position;
                        Vector2Int offsetPosition = fixtureInstance.Position + new Vector2Int((int)rotatedPosition.x, (int)rotatedPosition.y);
                        InteriorTile.InteriorTileType interiorTileType = InteriorTile.InteriorTileType.Wall;

                        if (interiorGridMap.InBounds(offsetPosition))
                        {
                            interiorTileType = interiorGridMap[offsetPosition].Type;
                        }

                        if (interiorTileType == restriction.InteriorTileType)
                        {
                            if (!manditoryVsProhibited)
                            {
                                condition = false;
                                break;
                            }
                        }
                        else
                        {
                            if (manditoryVsProhibited)
                            {
                                condition = false;
                                break;
                            }
                        }
                    }
                }

                if (restriction.HasPathToWalkableTile && condition)
                {
                    Vector2Int size = fixtureInstance.ParentCollection.Bounds.upperRight - fixtureInstance.ParentCollection.Bounds.lowerLeft;
                    Pathfinder2D pathfinder2D = new Pathfinder2D(size * interiorTilesPerMapTile);
                    Vector2Int end = fixtureInstance.ParentCollection.connections[0].from;

                    foreach (Vector2Int position in restriction.Positions)
                    {
                        Vector3 rotatedPosition = fixtureInstance.RotationMatrix * (Vector2)position;
                        Vector2Int offsetPosition = fixtureInstance.Position + new Vector2Int((int)rotatedPosition.x, (int)rotatedPosition.y);
                        if (!interiorGridMap.InBounds(offsetPosition))
                        {
                            if (manditoryVsProhibited)
                            {
                                condition = false;
                                break;
                            }
                        }

                        List<Vector2Int> path = pathfinder2D.FindPath(position, end, (Pathfinder2D.Node a, Pathfinder2D.Node b) =>
                        {
                            Pathfinder2D.PathInfo pathInfo = new Pathfinder2D.PathInfo();

                            pathInfo.cost = Vector2Int.Distance(b.Position, end);
                            pathInfo.traversable = true;

                            InteriorTile interiorTile = interiorGridMap[b.Position];

                            if (interiorTile.Type == InteriorTile.InteriorTileType.None)
                            {
                                pathInfo.cost += 5;
                            }
                            else if (interiorTile.Type == InteriorTile.InteriorTileType.Walkway)
                            {
                                pathInfo.traversable = false;
                                pathInfo.isGoal = true;
                            }
                            else
                            {
                                pathInfo.traversable = false;
                            }

                            return pathInfo;
                        });

                        if (path.Count == 0)
                        {
                            if (manditoryVsProhibited)
                            {
                                condition = false;
                                break;
                            }
                        }
                        else
                        {
                            if (!manditoryVsProhibited)
                            {
                                condition = false;
                                break;
                            }
                        }
                    }
                }

                if (!condition)
                {
                    break;
                }
            }

            if (condition)
            {
                return true;
            }
        }

        if (allDisabled)
        {
            return true;
        }

        return false;
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
            RoomData roomData = collection.RoomData;
            bool spawnFloors = true;
            bool spawnWalls = true;
            if (roomData)
            {
                spawnFloors = roomData.SpawnFloors;
                spawnWalls = roomData.SpawnWalls;

                if (!spawnFloors && !spawnWalls)
                {
                    continue;
                }

            }

            foreach (Vector2Int position in collection.mapTilePositions)
            {
                GameObject newTile = new GameObject("Tile");
                newTile.transform.parent = levelHolder.transform;
                if (spawnFloors)
                {
                    Instantiate(floor, new Vector3(position.x, 0, position.y), Quaternion.identity, newTile.transform);
                }

                if (spawnWalls)
                {
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
    public void SpawnFixtures()
    {
        foreach (FixtureInstance fixtureInstance in fixtureInstances)
        {
            Vector2 spawnPosition = ((Vector2)(fixtureInstance.Position) - (interiorTilesPerMapTile / 2) * Vector2.one) / interiorTilesPerMapTile;
            float rotation = 90 * (int)fixtureInstance.RotationPreset;
            /*GameObject fixture = */
            Instantiate(fixtureInstance.Data.FixturePrefab, tileSize * new Vector3(spawnPosition.x, 0, spawnPosition.y), Quaternion.Euler(0, rotation, 0), levelHolder.transform);
            //fixture.AddComponent<DebugFixture>().SetupValues(fixtureInstance);

            if (spawnFixturePositions)
            {
                foreach (Vector2Int position in fixtureInstance.Data.TilePositions)
                {
                    Vector3 rotatedPosition = fixtureInstance.RotationMatrix * (Vector2)position;
                    Vector2 newSpawnPosition = (((Vector2)rotatedPosition + fixtureInstance.Position) - (interiorTilesPerMapTile / 2) * Vector2.one) / interiorTilesPerMapTile;
                    Instantiate(debugMarker, tileSize * new Vector3(newSpawnPosition.x, 0, newSpawnPosition.y), Quaternion.identity, levelHolder.transform);
                }
            }
        }
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
    }

    public void ForcePlaceInteriorTiles(InteriorTile.InteriorTileType type, FixtureInstance fixture, List<Vector2Int> positions)
    {
        foreach (Vector2Int position in positions)
        {
            ForcePlaceInteriorTile(type, fixture, position);
        }
        if (fixture != null)
        {
            fixtureInstances.Add(fixture);
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
        if (fixture != null)
        {
            fixtureInstances.Add(fixture);
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
                return false;
            }
        }
        return true;
    }

    // TODO Change to allow Fixtures placed out of bounds that only have tiles in bounds and take into accoutn Rotation Matrix
    //public bool CanPlaceFixture(FixtureInstance fixture)
    //{
    //    if (interiorGridMap.InBounds(fixture.Position))
    //    {
    //        List<Vector2Int> positions = new List<Vector2Int>();
    //        foreach (Vector2Int tilePosition in fixture.Data.tilePositions)
    //        {
    //            positions.Add(tilePosition);
    //        }

    //        return CanPlaceInteriorTiles(positions);
    //    }
    //    return false;
    //}

    //public bool CanPlaceFixture(FixtureInstance fixture, List<Vector2Int> positions)
    //{
    //    if (interiorGridMap.InBounds(fixture.Position))
    //    {
    //        return CanPlaceInteriorTiles(positions);
    //    }
    //    return false;
    //}
}