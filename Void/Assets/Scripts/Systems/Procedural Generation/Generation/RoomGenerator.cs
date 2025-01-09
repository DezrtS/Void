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
        PlacePredefinedRooms(facilityFloor);

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
        foreach (RoomData roomData in predefinedRooms)
        {
            TileCollection tileCollection = new TileCollection(TileCollection.TileCollectionType.Room, facilityFloor.TileCollections.Count);

            for (int attempt = 0; attempt < maxRoomAttempts; attempt++)
            {
                Vector2Int randomPosition = new(Random.Range(1, facilityFloor.Size.x - roomData.Size.x - 1), Random.Range(1, facilityFloor.Size.y - roomData.Size.y - 1));
                List<Vector2Int> newPositions = new List<Vector2Int>(roomData.TilePositions);

                for (int i = 0; i < newPositions.Count; i++)
                {
                    newPositions[i] += randomPosition;
                }

                if (!FacilityGeneration.CanPlaceTiles(facilityFloor, newPositions, new FacilityGeneration.TilePlaceParams(FacilityGeneration.TileType.Room, false))) break;

                tileCollection.InitiatlizeRoom(roomData);
                facilityFloor.TileCollections.Add(tileCollection);

                FacilityGeneration.PlaceTiles(facilityFloor, tileCollection, newPositions, new FacilityGeneration.TilePlaceParams(FacilityGeneration.TileType.Room, false));
            }
        }

        //Instantiate(roomData.RoomPrefab, interiorTilesPerMapTile * new Vector3(position.x, 0, position.y), Quaternion.identity, levelHolder.transform);
    }

    private void GenerateRectangularRoom(FacilityFloor facilityFloor)
    {
        TileCollection tileCollection = new TileCollection(TileCollection.TileCollectionType.Room, facilityFloor.TileCollections.Count);

        for (int attempt = 0; attempt < maxRoomAttempts; attempt++)
        {
            Vector2Int randomSize = new Vector2Int(Random.Range(minRoomSize, maxRoomSize), Random.Range(minRoomSize, maxRoomSize));
            Vector2Int randomPosition = new(Random.Range(1, facilityFloor.Size.x - randomSize.x - 1), Random.Range(1, facilityFloor.Size.y - randomSize.y - 1));
            if (RoomGeneration.TryPlaceRectangularRoom(facilityFloor, tileCollection, randomPosition, randomSize))
            {
                facilityFloor.TileCollections.Add(tileCollection);
                return;
            }
        }

        Debug.LogWarning($"Failed to Generate Rectangular Room Within {maxRoomAttempts} Attempt(s)");
    }

    private void GenerateRandomRoom(FacilityFloor facilityFloor)
    {
        TileCollection tileCollection = new(TileCollection.TileCollectionType.Room, facilityFloor.TileCollections.Count);

        for (int attempt = 0; attempt < maxRoomAttempts; attempt++)
        {
            List<Vector2Int> closedRoomTiles = new List<Vector2Int>();
            List<Vector2Int> roomTiles = new List<Vector2Int>();
            List<Vector2Int> wallTiles = new List<Vector2Int>();

            Vector2Int randomPosition = new Vector2Int(Random.Range(1, facilityFloor.Size.x - 1), Random.Range(1, facilityFloor.Size.y - 1));
            int randomSize = Random.Range(minRoomSize, maxRoomSize) * Random.Range(minRoomSize, maxRoomSize);

            roomTiles.Add(randomPosition);

            bool success = true;
            for (int i = 0; i < randomSize; i++)
            {
                if (roomTiles.Count <= 0)
                {
                    success = false;
                    break;
                }

                int randomIndex = Random.Range(0, roomTiles.Count);
                Vector2Int selectedPosition = roomTiles[randomIndex];
                roomTiles.RemoveAt(randomIndex);

                if (!FacilityGeneration.CanPlaceTile(facilityFloor, selectedPosition, new FacilityGeneration.TilePlaceParams(FacilityGeneration.TileType.Room, false))) continue;

                List<Vector2Int> tempWallTiles = new List<Vector2Int>();
                foreach (Vector2Int offset in FacilityGeneration.Neighbors)
                {
                    Vector2Int position = selectedPosition + offset;
                    if (!closedRoomTiles.Contains(position) && !wallTiles.Contains(position))
                    {
                        tempWallTiles.Add(position);
                    }
                }

                if (!FacilityGeneration.CanPlaceTiles(facilityFloor, tempWallTiles, new FacilityGeneration.TilePlaceParams(FacilityGeneration.TileType.Wall, false))) continue;

                closedRoomTiles.Add(selectedPosition);
                wallTiles.Remove(selectedPosition);

                wallTiles.AddRange(tempWallTiles);

                foreach (Vector2Int offset in FacilityGeneration.Neighbors)
                {
                    Vector2Int position = selectedPosition + offset;
                    if (!closedRoomTiles.Contains(position) && !roomTiles.Contains(position))
                    {
                        roomTiles.Add(position);
                    }
                }
            }

            if (success)
            {
                FacilityGeneration.PlaceTiles(facilityFloor, tileCollection, closedRoomTiles, new FacilityGeneration.TilePlaceParams(FacilityGeneration.TileType.Room, false));
                //FacilityGeneration.PlaceTiles(facilityFloor, tileCollection, null, new FacilityGeneration.TilePlaceParams(FacilityGeneration.TileType.Wall, false), wallTiles);
                facilityFloor.TileCollections.Add(tileCollection);
                return;
            }
        }

        Debug.LogWarning($"Failed to Generate Random Room Within {maxRoomAttempts} Attempt(s)");
    }


}