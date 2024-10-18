using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GridMapManager : Singleton<GridMapManager>
{
    [Header("Grid Map Parameters")]
    [SerializeField] private int seed = 1234;
    [SerializeField] private int mapLength = 25;
    [SerializeField] private int mapWidth = 25;
    [SerializeField] private float mapTileSize = 10;
    [SerializeField] private int minRoomSize = 3;
    [SerializeField] private int maxRoomSize = 20;
    [SerializeField] private int roomCount = 5;
    [Space(10)]
    [Header("Options")]
    [SerializeField] private bool enableCollectionOriginMarkers;
    [SerializeField] private bool enableTriangulationVisualization;
    [Space(10)]
    [Header("Prefabs")]
    [SerializeField] private GameObject mapTilePrefab;
    [SerializeField] private GameObject floor;
    [SerializeField] private GameObject wall;
    [Space]
    [Header("Misc")]
    [SerializeField] private int maxRoomAttempts = 100;
    [SerializeField] private GameObject debugMarker;

    private readonly List<MapTileCollection> mapTileCollections = new();
    private MapTile[] mapTiles = new MapTile[0];
    private GameObject debugHolder;

    public float MapTileSize { get { return mapTileSize; } }
    public GameObject Floor {  get { return floor; } }
    public GameObject Wall { get { return wall; } }

    public void SetGridMapParameters(int seed, int mapLength, int mapWidth, float mapTileSize, int minRoomSize, int maxRoomSize, int roomCount)
    {
        this.seed = seed;
        this.mapLength = mapLength;
        this.mapWidth = mapWidth;
        this.mapTileSize = mapTileSize;
        this.minRoomSize = minRoomSize;
        this.maxRoomSize = maxRoomSize;
        this.roomCount = roomCount;
    }

    public void ResetGridMap()
    {
        foreach (MapTile tile in mapTiles)
        {
            if (tile)
            {
                Destroy(tile.gameObject);
            }
        }
        mapTileCollections.Clear();
        Destroy(debugHolder);
        debugHolder = new GameObject("Debug Holder");
    }

    public void GenerateGridMap()
    {
        ResetGridMap();
        Random.InitState(seed);
        mapTiles = new MapTile[mapLength * mapWidth];
    }

    public void GenerateRooms()
    {
        for (int i = 0; i < roomCount; i++)
        {
            GenerateRectangularRoom();
        }
    }

    private void GenerateRectangularRoom()
    {
        bool success = false;
        MapTileCollection mapTileCollection = new(MapTileCollectionType.None, mapTileCollections.Count, new Vector2Int(1, 1));

        int attempt = 0;
        while (!success && attempt < maxRoomAttempts)
        {
            attempt++;
            int roomLength = Random.Range(minRoomSize, maxRoomSize);
            int roomWidth = Random.Range(minRoomSize, maxRoomSize);

            Vector2Int roomPosition = new(Random.Range(0, mapLength - roomLength), Random.Range(0, mapWidth - roomWidth));
            mapTileCollection.CollectionOrigin = roomPosition;

            Vector2Int[] mapTilePositions = new Vector2Int[roomLength * roomWidth];
            for (int y = 0; y < roomWidth; y++)
            {
                for (int x = 0; x < roomLength; x++)
                {
                    mapTilePositions[x + roomLength * y] = new Vector2Int(roomPosition.x + x, roomPosition.y + y);
                }
            }

            success = PlaceMapTiles(mapTileCollection, mapTilePositions);
            if (!success)
            {
                Debug.LogWarning($"Failed to generate room, retrying (Attempt: {attempt} / {maxRoomAttempts})");
            }
        }

        if (success)
        {
            mapTileCollections.Add(mapTileCollection);
        }
    }

    private void GenerateRandomRoom()
    {
        bool success = false;
        MapTileCollection mapTileCollection = new(MapTileCollectionType.None, mapTileCollections.Count, Vector2Int.zero);

        int attempt = 0;
        while (!success && attempt < maxRoomAttempts)
        {
            attempt++;
            Vector2Int roomPosition = new(Random.Range(0, mapLength), Random.Range(0, mapWidth));
            mapTileCollection.CollectionOrigin.Set(roomPosition.x, roomPosition.y);
            Debug.Log(mapTileCollection.CollectionOrigin);
            success = PlaceMapTile(mapTileCollection, roomPosition);
        }

        if (!success)
        {
            return;
        }

        attempt = 0;
        success = false;
        int maxTiles = Random.Range(minRoomSize * minRoomSize, maxRoomSize * 2);
        int tilesPlaced = 0;
        while (!success && attempt < maxRoomAttempts)
        {
            attempt++;
            MapTile tile = mapTileCollection.MapTiles[Random.Range(0, mapTileCollection.MapTiles.Count)];

            int direction = Random.Range(0, 4);

            if (direction == 0)
            {
                if (PlaceMapTile(mapTileCollection, tile.Position + new Vector2Int(1, 0)))
                {
                    tilesPlaced++;
                }
            }
            else if (direction == 1)
            {
                if (PlaceMapTile(mapTileCollection, tile.Position + new Vector2Int(0, 1)))
                {
                    tilesPlaced++;
                }
            }
            else if (direction == 2)
            {
                if (PlaceMapTile(mapTileCollection, tile.Position - new Vector2Int(1, 0)))
                {
                    tilesPlaced++;
                }
            }
            else if (direction == 3)
            {
                if (PlaceMapTile(mapTileCollection, tile.Position - new Vector2Int(0, 1)))
                {
                    tilesPlaced++;
                }
            }

            success = tilesPlaced >= maxTiles;
        }

        mapTileCollections.Add(mapTileCollection);
    }

    public void GenerateHallways()
    {

    }

    public void DecorateMapTiles()
    {

    }

    public void SpawnTiles()
    {
        foreach (MapTile mapTile in mapTiles)
        {
            if (!mapTile)
            {
                continue;
            }
            mapTile.Spawn();
        }
        if (enableCollectionOriginMarkers)
        {
            foreach (MapTileCollection mapTileCollection in mapTileCollections)
            {
                Instantiate(debugMarker, new Vector3(mapTileCollection.CollectionOrigin.x * mapTileSize, 0, mapTileCollection.CollectionOrigin.y * mapTileSize), Quaternion.identity, debugHolder.transform);
            }
        }
    }

    public bool PlaceMapTile(MapTileCollection mapTileCollection, Vector2Int position)
    {
        if (CanPlaceMapTile(position))
        {
            int index = (int) (position.x + position.y * mapLength);
            GameObject spawnedMapTile = Instantiate(mapTilePrefab, new Vector3(position.x * mapTileSize, 0, position.y * mapTileSize), Quaternion.identity, transform);
            MapTile mapTile = spawnedMapTile.GetComponent<MapTile>();
            mapTile.SetMapTile(mapTileCollection, position);
            mapTiles[index] = mapTile;
            mapTileCollection.MapTiles.Add(mapTile);
            return true;
        }
        return false;
    }

    public void ForcePlaceMapTile(MapTileCollection mapTileCollection, Vector2Int position)
    {
        int index = (int)(position.x + position.y * mapLength);
        GameObject spawnedMapTile = Instantiate(mapTilePrefab, new Vector3(position.x * mapTileSize, 0, position.y * mapTileSize), Quaternion.identity, transform);
        MapTile mapTile = spawnedMapTile.GetComponent<MapTile>();
        mapTile.SetMapTile(mapTileCollection, position);
        mapTiles[index] = mapTile;
        mapTileCollection.MapTiles.Add(mapTile);
    }

    public void ForcePlaceMapTiles(MapTileCollection mapTileCollection, HashSet<Vector2> positions)
    {
        foreach (Vector2 position in positions)
        {
            ForcePlaceMapTile(mapTileCollection, new Vector2Int((int)position.x, (int)position.y));
        }
    }

    public bool PlaceMapTiles(MapTileCollection mapTileCollection, Vector2Int[] positions)
    {
        if (!CanPlaceMapTiles(positions))
        {
            return false;
        }
        foreach (Vector2Int position in positions)
        {
            ForcePlaceMapTile(mapTileCollection, position);
        }
        return true;
    }

    public bool CanPlaceMapTile(Vector2Int position)
    {
        int index = (int)(position.x + position.y * mapLength);
        if (index >= 0 && index < mapTiles.Length)
        {
            return mapTiles[index] == null;
        }
        return false;
    }

    public bool CanPlaceMapTiles(Vector2Int[] positions)
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

    public MapTile GetMapTile(Vector2 position)
    {
        int index = (int)(position.x + position.y * mapLength);
        if (index >= 0 && index < mapTiles.Length)
        {
            return mapTiles[index];
        }
        return null;
    }

    public void GenerateRoomConnections()
    {
        List<Vector2> points = new List<Vector2>();
        foreach (MapTileCollection mapTileCollection in mapTileCollections)
        {
            points.Add(mapTileCollection.CollectionOrigin);
        }

        if (points.Count <= 2)
        {
            return;
        }

        Delaunay delaunay = Delaunay.Triangulate(points);

        PrimMST primMST = PrimMST.Construct(delaunay.Edges, points[0]);

        foreach (Edge edge in primMST.DiscardedEdges)
        {
            if (Random.Range(1, 101) < 12.5f)
            {
                primMST.MinimumSpanningTree.Add(edge);
            }
        }

        EdgeVisualizer visualizer = GetComponent<EdgeVisualizer>();
        if (enableTriangulationVisualization)
        {
            visualizer.DrawEdges(primMST.MinimumSpanningTree, true);
        }
        else
        {
            visualizer.ClearLines();
        }

        PathfindHallways(primMST.MinimumSpanningTree);
    }

    public void PathfindHallways(List<Edge> edges)
    {
        Node[][] nodeGraph = new Node[mapLength][];

        for (int i = 0; i < nodeGraph.Length; i++)
        {
            nodeGraph[i] = new Node[mapWidth];
        }

        HashSet<Vector2> total = new HashSet<Vector2>();

        foreach (Edge edge in edges)
        {
            MapTile startMapTile = GetMapTile(edge.A);
            MapTile endMapTile = GetMapTile(edge.B);
            int startCollectionId = startMapTile.ParentCollection.Id;
            int goalCollectionId = endMapTile.ParentCollection.Id;

            for (int i = 0; i < mapTiles.Length; i++)
            {
                MapTile mapTile = mapTiles[i];
                if (mapTile != null)
                {
                    nodeGraph[mapTile.Position.x][mapTile.Position.y] = new Node(mapTile.Position, mapTile.ParentCollection.Id == goalCollectionId, mapTile.ParentCollection.Id == startCollectionId, false, 0);
                }
                else
                {
                    Vector2Int position = new Vector2Int(i % mapLength, i / mapLength);
                    nodeGraph[position.x][position.y] = new Node(position, false, false, true, 0);
                }
            }

            List<Vector2> positions = Pathfind(nodeGraph[startMapTile.Position.x][startMapTile.Position.y], nodeGraph[endMapTile.Position.x][endMapTile.Position.y], nodeGraph, mapLength, mapWidth);
            foreach (Vector2 position in positions)
            {
                total.Add(position);
            }
        }

        ForcePlaceMapTiles(new MapTileCollection(MapTileCollectionType.Hallway, -1, Vector2Int.zero), total);

    }

    public List<Vector2> Pathfind(Node startNode, Node endNode, Node[][] nodeGraph, int graphLength, int graphWidth)
    {
        Vector2[] checkPositions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };

        SearchEntry[][] searchEntries = new SearchEntry[graphLength][];
        for (int i = 0; i < graphLength; i++)
        {
            searchEntries[i] = new SearchEntry[graphWidth];

            for (int j = 0; j < graphWidth; j++)
            {
                searchEntries[i][j] = new SearchEntry(i * graphLength + j);
            }
        }

        var visited = new HashSet<Vector2>();
        var priorityQueue = new SortedSet<(float cost, Node node)>(new NodeComparer());
        priorityQueue.Add((Vector2.Distance(startNode.Position, endNode.Position), startNode));
        int limit = 0;
        while(priorityQueue.Count > 0 && limit < 100)
        {
            limit++;
            var node = priorityQueue.Min;
            priorityQueue.Remove(node);
            visited.Add(node.node.Position);

            if (node.node.IsGoal)
            {
                limit = 0;
                Vector2 previousPosition = searchEntries[(int)node.node.Position.x][(int)node.node.Position.y].PreviousNode.Position;
                List<Vector2> positions = new List<Vector2>();
                //Node previous = searchEntries[(int)node.node.Position.x][(int)node.node.Position.y].PreviousNode;
                while (searchEntries[(int)previousPosition.x][(int)previousPosition.y].PreviousNode != null && limit < 100)
                {
                    limit++;
                    Vector2 after = searchEntries[(int)previousPosition.x][(int)previousPosition.y].PreviousNode.Position;
                    Debug.Log($"BEFORE: {previousPosition} ID: {searchEntries[(int)previousPosition.x][(int)previousPosition.y].Id} --> AFTER: {after} ID: {searchEntries[(int)after.x][(int)after.y].Id}");
                    positions.Add(previousPosition);
                    previousPosition = searchEntries[(int)previousPosition.x][(int)previousPosition.y].PreviousNode.Position;
                    //return;
                }
                return positions;
            }

            foreach (Vector2 checkPosition in checkPositions)
            {
                Vector2 position = node.node.Position + checkPosition;
                if (position.x >= graphLength || position.x < 0 || position.y >= graphWidth || position.y < 0)
                {
                    continue;
                }
                float cost = Vector2.Distance(position, endNode.Position);
                if (nodeGraph[(int)position.x][(int)position.y].IsTraversable)
                {
                    if (!visited.Contains(position))
                    {
                        priorityQueue.Add((cost, nodeGraph[(int)position.x][(int)position.y]));
                        searchEntries[(int)position.x][(int)position.y].Set(searchEntries[(int)node.node.Position.x][(int)node.node.Position.y].DistanceFromStart + 1, node.node);
                    }
                    else if (searchEntries[(int)position.x][(int)position.y].DistanceFromStart > searchEntries[(int)node.node.Position.x][(int)node.node.Position.y].DistanceFromStart + 1)
                    {
                        searchEntries[(int)position.x][(int)position.y].Set(searchEntries[(int)node.node.Position.x][(int)node.node.Position.y].DistanceFromStart + 1, node.node);
                    }
                }
                else if (!visited.Contains(position))
                {
                    if (nodeGraph[(int)position.x][(int)position.y].IsStart)
                    {
                        priorityQueue.Add((cost, nodeGraph[(int)position.x][(int)position.y]));
                    }

                    if (nodeGraph[(int)position.x][(int)position.y].IsGoal)
                    {
                        priorityQueue.Add((cost, nodeGraph[(int)position.x][(int)position.y]));
                        searchEntries[(int)position.x][(int)position.y].Set(searchEntries[(int)node.node.Position.x][(int)node.node.Position.y].DistanceFromStart + 1, node.node);
                    }
                }
            }
        }

        if (limit >= 100)
        {
            Debug.LogWarning("INFINITE LOOP");
        }

        return null;
    }
}

public class Node
{
    public Vector2 Position;
    public bool IsGoal;
    public bool IsStart;
    public bool IsTraversable;
    public int CostOffset;

    public Node(Vector2 position, bool isGoal, bool isStart, bool isTraversable, int costOffset)
    {
        Position = position;
        IsGoal = isGoal;
        IsStart = isStart;
        IsTraversable = isTraversable;
        CostOffset = costOffset;
    }

    public override int GetHashCode()
    {
        return Position.GetHashCode() ^ CostOffset.GetHashCode();
    }
}

public class SearchEntry
{
    public int Id;
    public int DistanceFromStart;
    public Node PreviousNode;

    public SearchEntry(int id)
    {
        Id = id;
    }

    public void Set(int distanceFromStart, Node previousNode)
    {
        DistanceFromStart = distanceFromStart;
        PreviousNode = previousNode;
    }
}

public class NodeComparer : IComparer<(float cost, Node node)>
{
    public int Compare((float cost, Node node) x, (float cost, Node node) y)
    {
        int weightComparison = x.cost.CompareTo(y.cost);
        if (weightComparison != 0)
        {
            return weightComparison;
        }

        // If weights are equal, compare edges by their hash codes or other criteria
        return x.node.GetHashCode().CompareTo(y.node.GetHashCode());
    }
}