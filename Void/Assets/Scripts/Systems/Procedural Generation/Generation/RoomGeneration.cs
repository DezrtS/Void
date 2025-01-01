using System.Collections.Generic;
using UnityEngine;

public static class RoomGeneration
{
    public static bool TryPlaceRectangularRoom(FacilityFloor facilityFloor, TileCollection tileCollection, Vector2Int position, Vector2Int size)
    {
        Vector2Int wallPosition = position - Vector2Int.one;
        Vector2Int wallSize = size + Vector2Int.one * 2;

        Vector2Int[] wallPositions = new Vector2Int[(wallSize.x + wallSize.y - 2) * 2];

        // Create wall positions
        int index = 0;
        // Top wall
        for (int x = 0; x < wallSize.x; x++)
        {
            wallPositions[index++] = new Vector2Int(wallPosition.x + x, wallPosition.y);
        }
        // Right wall
        for (int y = 1; y < wallSize.y; y++)
        {
            wallPositions[index++] = new Vector2Int(wallPosition.x + wallSize.x - 1, wallPosition.y + y);
        }
        // Bottom wall
        for (int x = wallSize.x - 2; x >= 0; x--)
        {
            wallPositions[index++] = new Vector2Int(wallPosition.x + x, wallPosition.y + wallSize.y - 1);
        }
        // Left wall
        for (int y = wallSize.y - 2; y > 0; y--)
        {
            wallPositions[index++] = new Vector2Int(wallPosition.x, wallPosition.y + y);
        }

        if (!FacilityGeneration.CanPlaceTiles(facilityFloor, wallPositions, new FacilityGeneration.TilePlaceParams(FacilityGeneration.TileType.Wall, false))) return false;

        Vector2Int[] floorPositions = new Vector2Int[size.x * size.y];

        for (int y = 0; y < size.y; y++)
        {
            for (int x = 0; x < size.x; x++)
            {
                floorPositions[x + size.x * y] = new Vector2Int(position.x + x, position.y + y);
            }
        }

        if (!FacilityGeneration.TryPlaceTiles(facilityFloor, tileCollection, floorPositions, new FacilityGeneration.TilePlaceParams(FacilityGeneration.TileType.Room, false))) return false;

        //FacilityGeneration.PlaceTiles(facilityFloor, tileCollection, null, new FacilityGeneration.TilePlaceParams(FacilityGeneration.TileType.Wall, false), wallPositions);
        return true;
    }
}