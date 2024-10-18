
using Graphs;
using System.Collections.Generic;
using UnityEngine;

public class GridMapManager : Singleton<GridMapManager>
{
    [Header("Grid Map Parameters")]
    [SerializeField] private int seed = 1234;
    [SerializeField] private Vector2Int size;
    [SerializeField] private float tileSize = 10;
    [SerializeField] private int minRoomSize = 3;
    [SerializeField] private int maxRoomSize = 20;
    [SerializeField] private int roomCount = 5;
    [Space(10)]
    [Header("Options")]
    [SerializeField] private bool spawnCollectionOrigins;
    [SerializeField] private bool drawRoomConnections;
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
    private Grid2D<MapTile> gridMap;
    private GameObject debugHolder;

    public float TileSize { get { return tileSize; } }
    public GameObject Floor { get { return floor; } }
    public GameObject Wall { get { return wall; } }

    public static bool Roll(float chance)
    {
        return Random.Range(0, 100) < chance;
    }

    public void InitializeParameters(int seed, Vector2Int size, float tileSize, int minRoomSize, int maxRoomSize, int roomCount)
    {
        this.seed = seed;
        this.size = size;
        this.tileSize = tileSize;
        this.minRoomSize = minRoomSize;
        this.maxRoomSize = maxRoomSize;
        this.roomCount = roomCount;
    }

    public void ResetGridMap()
    {
        if (gridMap != null)
        {
            int gridMapSize = size.x * size.y;
            for (int i = 0; i < gridMapSize; i++)
            {
                MapTile tile = gridMap[i];
                if (tile != null)
                {
                    Destroy(tile.gameObject);
                }
            }
        }
        mapTileCollections.Clear();
        Destroy(debugHolder);
        debugHolder = new GameObject("Debug Holder");
    }

    public void InitializeGridMap()
    {
        ResetGridMap();
        Random.InitState(seed);
        gridMap = new Grid2D<MapTile>(size, Vector2Int.zero);
    }

    public void GenerateRooms()
    {
        for (int i = 0; i < roomCount; i++)
        {
            if (Roll(30))
            {
                GenerateRandomRoom();
            }
            else
            {
                GenerateRectangularRoom();
            }
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

            Vector2Int roomPosition = new(Random.Range(0, size.x - roomLength), Random.Range(0, size.y - roomWidth));
            mapTileCollection.CollectionOrigin = roomPosition;

            List<Vector2Int> mapTilePositions = new List<Vector2Int>();
            for (int y = 0; y < roomWidth; y++)
            {
                for (int x = 0; x < roomLength; x++)
                {
                    mapTilePositions.Add(new Vector2Int(roomPosition.x + x, roomPosition.y + y));
                }
            }

            success = PlaceMapTiles(mapTileCollection, mapTilePositions);
        }

        if (success)
        {
            mapTileCollections.Add(mapTileCollection);
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
        MapTileCollection mapTileCollection = new(MapTileCollectionType.None, mapTileCollections.Count, Vector2Int.zero);

        int attempt = 0;
        while (!success && attempt < maxRoomAttempts)
        {
            attempt++;
            Vector2Int roomPosition = new(Random.Range(0, size.x), Random.Range(0, size.y));
            mapTileCollection.CollectionOrigin.Set(roomPosition.x, roomPosition.y);
            success = PlaceMapTile(mapTileCollection, roomPosition);
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

        if (success)
        {
            mapTileCollections.Add(mapTileCollection);
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
        foreach (MapTileCollection mapTileCollection in mapTileCollections)
        {
            points.Add(new Vertex(mapTileCollection.CollectionOrigin));
        }

        Delaunay2D delaunay = Delaunay2D.Triangulate(points);

        List<Prim.Edge> edges = new List<Prim.Edge>();

        foreach (var edge in delaunay.Edges)
        {
            edges.Add(new Prim.Edge(edge.U, edge.V));
        }

        List<Prim.Edge> mst = Prim.MinimumSpanningTree(edges, edges[0].U);

        var remainingEdges = new HashSet<Prim.Edge>(edges);
        remainingEdges.ExceptWith(mst);

        foreach (var edge in remainingEdges)
        {
            if (Roll(12.5f))
            {
                mst.Add(edge);
            }
        }

        EdgeVisualizer edgeVisualizer = GetComponent<EdgeVisualizer>();
        if (drawRoomConnections)
        {
            edgeVisualizer.DrawEdges(mst, true);
        }
        else
        {
            edgeVisualizer.ClearLines();
        }

        PathfindHallways(mst);
    }

    private void PathfindHallways(List<Prim.Edge> edges)
    {
        Pathfinder2D pathfinder2D = new Pathfinder2D(size);

        foreach (Prim.Edge edge in edges)
        {
            Vector2Int start = new Vector2Int((int)edge.U.Position.x, (int)edge.U.Position.y);
            Vector2Int end = new Vector2Int((int)edge.V.Position.x, (int)edge.V.Position.y);
            int startCollectionId = gridMap[start].ParentCollection.Id;
            int endCollectionId = gridMap[end].ParentCollection.Id;

            List<Vector2Int> path = pathfinder2D.FindPath(start, end, (Pathfinder2D.Node a, Pathfinder2D.Node b) =>
            {
                Pathfinder2D.PathInfo pathInfo = new Pathfinder2D.PathInfo();

                pathInfo.cost = Vector2Int.Distance(b.Position, end);

                if (gridMap[b.Position])
                {
                    pathInfo.isStart = gridMap[b.Position].ParentCollection.Id == startCollectionId;
                    pathInfo.isGoal = gridMap[b.Position].ParentCollection.Id == endCollectionId;
                    pathInfo.traversable = gridMap[b.Position].ParentCollection.MapTileCollectionType == MapTileCollectionType.Hallway;
                }
                else
                {
                    pathInfo.traversable = true;
                }

                return pathInfo;
            });

            if (path != null)
            {
                MapTileCollection mapTileCollection = new(MapTileCollectionType.Hallway, mapTileCollections.Count, path[1]);
                mapTileCollections.Add(mapTileCollection);
                PlaceMapPossibleTiles(mapTileCollection, path);
            }
        }
    }

    public void DecorateMapTiles()
    {

    }

    public void SpawnTiles()
    {
        int gridMapSize = size.x * size.y;
        for (int i = 0; i < gridMapSize; i++)
        {
            MapTile tile = gridMap[i];
            if (tile != null)
            {
                tile.Spawn();
            }
        }

        if (spawnCollectionOrigins)
        {
            foreach (MapTileCollection mapTileCollection in mapTileCollections)
            {
                Instantiate(debugMarker, new Vector3(mapTileCollection.CollectionOrigin.x * tileSize, 0, mapTileCollection.CollectionOrigin.y * tileSize), Quaternion.identity, debugHolder.transform);
            }
        }
    }

    public bool PlaceMapTile(MapTileCollection mapTileCollection, Vector2Int position)
    {
        if (CanPlaceMapTile(position))
        {
            GameObject spawnedMapTile = Instantiate(mapTilePrefab, new Vector3(position.x * tileSize, 0, position.y * tileSize), Quaternion.identity, transform);
            MapTile mapTile = spawnedMapTile.GetComponent<MapTile>();
            mapTile.SetMapTile(mapTileCollection, position);
            gridMap[position] = mapTile;
            mapTileCollection.MapTiles.Add(mapTile);
            return true;
        }
        return false;
    }

    public void ForcePlaceMapTile(MapTileCollection mapTileCollection, Vector2Int position)
    {
        GameObject spawnedMapTile = Instantiate(mapTilePrefab, new Vector3(position.x * tileSize, 0, position.y * tileSize), Quaternion.identity, transform);
        MapTile mapTile = spawnedMapTile.GetComponent<MapTile>();
        mapTile.SetMapTile(mapTileCollection, position);
        gridMap[position] = mapTile;
        mapTileCollection.MapTiles.Add(mapTile);
    }

    public void ForcePlaceMapTiles(MapTileCollection mapTileCollection, List<Vector2Int> positions)
    {
        foreach (Vector2Int position in positions)
        {
            ForcePlaceMapTile(mapTileCollection, position);
        }
    }

    public void ForcePlaceMapTiles(MapTileCollection mapTileCollection, HashSet<Vector2> positions)
    {
        foreach (Vector2 position in positions)
        {
            ForcePlaceMapTile(mapTileCollection, new Vector2Int((int)position.x, (int)position.y));
        }
    }

    public bool PlaceMapTiles(MapTileCollection mapTileCollection, List<Vector2Int> positions)
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

    public void PlaceMapPossibleTiles(MapTileCollection mapTileCollection, List<Vector2Int> positions)
    {
        foreach (Vector2Int position in positions)
        {
            PlaceMapTile(mapTileCollection, position);
        }
    }

    public bool CanPlaceMapTile(Vector2Int position)
    {
        if (gridMap.InBounds(position))
        {
            return gridMap[position] == null;
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

    public MapTile GetMapTile(Vector2Int position)
    {
        return gridMap[position];
    }
}