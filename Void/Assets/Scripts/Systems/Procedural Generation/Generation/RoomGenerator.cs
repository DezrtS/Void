using System.Collections.Generic;
using UnityEngine;

public class RoomGenerator : MonoBehaviour
{
    [Header("Room Amount")]
    [SerializeField] private int roomCount = 25;
    [SerializeField] private int maxRoomAttempts = 100;

    [Header("Room Size")]
    [SerializeField] private int minRoomSize = 3;
    [SerializeField] private int maxRoomSize = 6;

    [Header("Room Type")]
    [Range(0, 100)]
    [SerializeField] private float randomRoomChance = 15f;

    [Header("Predefined Rooms")]
    [SerializeField] private List<RoomData> predefinedRooms;

    [Header("Debugging")]
    [SerializeField] private bool spawnRoomMarkers;

    public int RoomCount => roomCount;

    public void GenerateRooms(FacilityFloor facilityFloor)
    {
        //PlacePredefinedRooms(facilityFloor);

        for (int i = 0; i < roomCount; i++)
        {
            if (FacilityGenerationManager.Roll(randomRoomChance))
            {
                GenerateRandomRoom(facilityFloor);
            }
            else
            {
                GenerateRectangularRoom(facilityFloor);
            }
        }
    }

    public void PlacePredefinedRooms(FacilityFloor facilityFloor)
    {
        RoomData roomData = predefinedRooms[0];
        Vector2Int position = new Vector2Int(facilityFloor.Size.x / 2 - roomData.GridSize.x / 2, facilityFloor.Size.x / 2 - roomData.GridSize.x / 2);
        TileCollection elevatorRoom = new TileCollection(TileCollection.TileCollectionType.Room, 0);
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
        facilityFloor.TileCollections.Add(elevatorRoom);
        MapGeneration.PlaceMapTiles(facilityFloor, elevatorRoom, MapTile.MapTileType.Room, newPositions);
        //Instantiate(roomData.RoomPrefab, interiorTilesPerMapTile * new Vector3(position.x, 0, position.y), Quaternion.identity, levelHolder.transform);
    }

    private void GenerateRectangularRoom(FacilityFloor facilityFloor)
    {
        bool success = false;
        TileCollection tileCollection = new(TileCollection.TileCollectionType.Room, facilityFloor.TileCollections.Count);

        int attempt = 0;
        while (!success && attempt < maxRoomAttempts)
        {
            attempt++;
            int roomLength = Random.Range(minRoomSize, maxRoomSize);
            int roomWidth = Random.Range(minRoomSize, maxRoomSize);

            Vector2Int roomPosition = new(Random.Range(0, facilityFloor.Size.x - roomLength), Random.Range(0, facilityFloor.Size.y - roomWidth));

            List<Vector2Int> mapTilePositions = new List<Vector2Int>();
            for (int y = 0; y < roomWidth; y++)
            {
                for (int x = 0; x < roomLength; x++)
                {
                    mapTilePositions.Add(new Vector2Int(roomPosition.x + x, roomPosition.y + y));
                }
            }

            success = MapGeneration.TryPlaceMapTiles(facilityFloor, tileCollection, MapTile.MapTileType.Room, mapTilePositions);
        }

        if (success)
        {
            facilityFloor.TileCollections.Add(tileCollection);
            //Debug.Log($"Successfully Generated Rectangular Room [{attempt} Attempt(s)]");
        }
        else
        {
            Debug.LogWarning($"Failed To Generate Rectangular Room After {attempt} Attempts");
        }
    }

    private void GenerateRandomRoom(FacilityFloor facilityFloor)
    {
        bool success = false;
        TileCollection tileCollection = new(TileCollection.TileCollectionType.Room, facilityFloor.TileCollections.Count);

        int attempt = 0;
        while (!success && attempt < maxRoomAttempts)
        {
            attempt++;
            Vector2Int roomPosition = new(Random.Range(0, facilityFloor.Size.x), Random.Range(0, facilityFloor.Size.y));
            success = MapGeneration.TryPlaceMapTile(facilityFloor, tileCollection, MapTile.MapTileType.Room, roomPosition);
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
                if (MapGeneration.TryPlaceMapTile(facilityFloor, tileCollection, MapTile.MapTileType.Room, position + new Vector2Int(1, 0)))
                {
                    tilesPlaced++;
                }
            }
            else if (direction == 1)
            {
                if (MapGeneration.TryPlaceMapTile(facilityFloor, tileCollection, MapTile.MapTileType.Room, position + new Vector2Int(0, 1)))
                {
                    tilesPlaced++;
                }
            }
            else if (direction == 2)
            {
                if (MapGeneration.TryPlaceMapTile(facilityFloor, tileCollection, MapTile.MapTileType.Room, position - new Vector2Int(1, 0)))
                {
                    tilesPlaced++;
                }
            }
            else if (direction == 3)
            {
                if (MapGeneration.TryPlaceMapTile(facilityFloor, tileCollection, MapTile.MapTileType.Room, position - new Vector2Int(0, 1)))
                {
                    tilesPlaced++;
                }
            }

            success = tilesPlaced >= maxTiles;
        }

        if (success)
        {
            facilityFloor.TileCollections.Add(tileCollection);
            //Debug.Log($"Successfully Generated Random Room [{attempt} Attempt(s)]");
        }
        else
        {
            Debug.LogWarning($"Successfully Generated Random Room, [Reached Max Tile Place Attempts]");
        }
    }
}