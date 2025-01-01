//using System.Collections.Generic;
//using UnityEngine;

//public static class MapGeneration
//{
//    public static bool CanPlaceMapTile(FacilityFloor facilityFloor, Vector2Int position)
//    {
//        if (!facilityFloor.FloorMap.InBounds(position)) return false;

//        return facilityFloor.FloorMap[position].Type == MapTile.MapTileType.None;
//    }

//    public static bool CanPlaceMapTiles(FacilityFloor facilityFloor, IEnumerable<Vector2Int> positions)
//    {
//        foreach (Vector2Int position in positions)
//        {
//            if (!CanPlaceMapTile(facilityFloor, position)) return false;
//        }
//        return true;
//    }

//    public static void PlaceMapTile(FacilityFloor facilityFloor, TileCollection tileCollection, MapTile.MapTileType type, Vector2Int position)
//    {
//        facilityFloor.FloorMap[position] = new MapTile(type, tileCollection);
//        tileCollection.AddMapTilePosition(position);
//    }

//    public static void PlaceMapTiles(FacilityFloor facilityFloor, TileCollection tileCollection, MapTile.MapTileType type, IEnumerable<Vector2Int> positions)
//    {
//        foreach (Vector2Int position in positions)
//        {
//            PlaceMapTile(facilityFloor, tileCollection, type, position);
//        }
//    }

//    public static bool TryPlaceMapTile(FacilityFloor facilityFloor, TileCollection tileCollection, MapTile.MapTileType type, Vector2Int position)
//    {
//        if (!CanPlaceMapTile(facilityFloor, position)) return false;

//        PlaceMapTile(facilityFloor, tileCollection, type, position);
//        return true;
//    }

//    public static bool TryPlaceMapTiles(FacilityFloor facilityFloor, TileCollection tileCollection, MapTile.MapTileType type, IEnumerable<Vector2Int> positions)
//    {
//        if (!CanPlaceMapTiles(facilityFloor, positions)) return false;

//        PlaceMapTiles(facilityFloor, tileCollection, type, positions);
//        return true;
//    }

//    public static void PlacePossibleMapTiles(FacilityFloor facilityFloor, TileCollection tileCollection, MapTile.MapTileType type, IEnumerable<Vector2Int> positions)
//    {
//        foreach (Vector2Int position in positions)
//        {
//            TryPlaceMapTile(facilityFloor, tileCollection, type, position);
//        }
//    }
//}
