using System.Collections.Generic;
using UnityEngine;

public static class InteriorGeneration
{
    public static bool CanPlaceInteriorTile(FacilityFloor facilityFloor, Vector2Int position)
    {
        if (!facilityFloor.InteriorFloorMap.InBounds(position)) return false;

        return facilityFloor.InteriorFloorMap[position].Type == InteriorTile.InteriorTileType.None;
    }

    public static bool CanPlaceInteriorTiles(FacilityFloor facilityFloor, IEnumerable<Vector2Int> positions)
    {
        foreach (Vector2Int position in positions)
        {
            if (!CanPlaceInteriorTile(facilityFloor, position)) return false;
        }
        return true;
    }

    public static void PlaceInteriorTile(FacilityFloor facilityFloor, FixtureInstance fixtureInstance, InteriorTile.InteriorTileType type, Vector2Int position)
    {
        facilityFloor.InteriorFloorMap[position] = new InteriorTile(type, fixtureInstance);
    }

    public static void PlaceInteriorTiles(FacilityFloor facilityFloor, FixtureInstance fixtureInstance, InteriorTile.InteriorTileType type, IEnumerable<Vector2Int> positions)
    {
        foreach (Vector2Int position in positions)
        {
            PlaceInteriorTile(facilityFloor, fixtureInstance, type, position);
        }
    }

    public static bool TryPlaceInteriorTile(FacilityFloor facilityFloor, FixtureInstance fixtureInstance, InteriorTile.InteriorTileType type, Vector2Int position)
    {
        if (!CanPlaceInteriorTile(facilityFloor, position)) return false;

        PlaceInteriorTile(facilityFloor, fixtureInstance, type, position);
        return true;
    }

    public static bool TryPlaceInteriorTiles(FacilityFloor facilityFloor, FixtureInstance fixtureInstance, InteriorTile.InteriorTileType type, IEnumerable<Vector2Int> positions)
    {
        if (!CanPlaceInteriorTiles(facilityFloor, positions)) return false;

        PlaceInteriorTiles(facilityFloor, fixtureInstance, type, positions);
        return true;
    }

    public static void PlacePossibleInteriorTiles(FacilityFloor facilityFloor, FixtureInstance fixtureInstance, InteriorTile.InteriorTileType type, IEnumerable<Vector2Int> positions)
    {
        foreach (Vector2Int position in positions)
        {
            TryPlaceInteriorTile(facilityFloor, fixtureInstance, type, position);
        }
    }
}