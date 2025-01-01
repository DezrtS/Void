using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class FacilityGeneration
{
    public enum TileType
    {
        None,
        Room,
        Hallway,
        Walkway,
        Fixture,
        Door,
        Window,
        Wall,
    }

    public class Tile
    {
        private TileCollection tileCollection;
        private TileType type = TileType.None;

        public TileCollection TileCollection { get { return tileCollection; } set { tileCollection = value; } }
        public TileType Type { get { return type; } set { type = value; } }

        public void Set(TileCollection tileCollection, TileType type)
        {
            this.tileCollection = tileCollection;
            this.type = type;
        }
    }

    public readonly struct TilePlaceParams
    {
        private readonly TileType placingType;
        private readonly TileType[] canOverride;
        private readonly bool overrideEverything;
        private readonly bool hasOverrides;

        public readonly TileType PlacingTile => placingType;

        public TilePlaceParams(TileType placingType, bool overrideEverything)
        {
            this.placingType = placingType;
            canOverride = null;
            this.overrideEverything = overrideEverything;
            hasOverrides = false;
        }

        public TilePlaceParams(TileType placingType, TileType[] canOverride)
        {
            this.placingType = placingType;
            this.canOverride = canOverride;
            overrideEverything = false;
            hasOverrides = true;
        }

        public readonly bool CanOverride(TileType tile)
        {
            if (overrideEverything) return true;

            if (hasOverrides) return canOverride.Contains(tile);

            return tile == TileType.None;
        }
    }

    public static bool CanPlaceTile(FacilityFloor facilityFloor, Vector2Int position, TilePlaceParams tilePlaceParams)
    {
        if (!facilityFloor.TileMap.InBounds(position)) return false;

        return tilePlaceParams.CanOverride(facilityFloor.TileMap[position].Type);
    }

    public static bool CanPlaceTiles(FacilityFloor facilityFloor, IEnumerable<Vector2Int> positions, TilePlaceParams tilePlaceParams)
    {
        foreach (Vector2Int position in positions)
        {
            if (!CanPlaceTile(facilityFloor, position, tilePlaceParams)) return false;
        }
        return true;
    }

    public static void PlaceTile(FacilityFloor facilityFloor, TileCollection tileCollection, Vector2Int position, TilePlaceParams tilePlaceParams)
    {
        facilityFloor.TileMap[position].Set(tileCollection, tilePlaceParams.PlacingTile);
        // TODO - Fix issue where overriding a position will add its duplicate to the tileCollection.
        tileCollection?.AddTilePosition(position);
    }

    public static void PlaceTiles(FacilityFloor facilityFloor, TileCollection tileCollection, IEnumerable<Vector2Int> positions, TilePlaceParams tilePlaceParams)
    {
        foreach (Vector2Int position in positions)
        {
            PlaceTile(facilityFloor, tileCollection, position, tilePlaceParams);
        }
    }

    public static bool TryPlaceTile(FacilityFloor facilityFloor, TileCollection tileCollection, Vector2Int position, TilePlaceParams tilePlaceParams)
    {
        if (!CanPlaceTile(facilityFloor, position, tilePlaceParams)) return false;

        PlaceTile(facilityFloor, tileCollection, position, tilePlaceParams);
        return true;
    }

    public static bool TryPlaceTiles(FacilityFloor facilityFloor, TileCollection tileCollection, IEnumerable<Vector2Int> positions, TilePlaceParams tilePlaceParams)
    {
        if (!CanPlaceTiles(facilityFloor, positions, tilePlaceParams)) return false;

        PlaceTiles(facilityFloor, tileCollection, positions, tilePlaceParams);
        return true;
    }

    public static void PlacePossibleTiles(FacilityFloor facilityFloor, TileCollection tileCollection, IEnumerable<Vector2Int> positions, TilePlaceParams tilePlaceParams)
    {
        foreach (Vector2Int position in positions)
        {
            TryPlaceTile(facilityFloor, tileCollection, position, tilePlaceParams);
        }
    }

    public static bool CheckTiles(FacilityFloor facilityFloor, IEnumerable<Vector2Int> positions, TileType type)
    {
        foreach (Vector2Int position in positions)
        {
            if (facilityFloor.TileMap[position].Type != type) return false;
        }
        return true;
    }

    public static List<Vector2Int> GenerateNeighbors(int radius)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();

        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                if (x == 0 && y == 0) continue; // Skip the center position
                neighbors.Add(new Vector2Int(x, y));
            }
        }

        return neighbors;
    }

    public static readonly Vector2Int[] DirectNeighbors = {
        new Vector2Int(0, 1),
        new Vector2Int(1, 0),
        new Vector2Int(0, -1),
        new Vector2Int(-1, 0),
    };

    public static readonly Vector2Int[] Neighbors = {
        new Vector2Int(0, 1),
        new Vector2Int(1, 1),
        new Vector2Int(1, 0),
        new Vector2Int(1, -1),
        new Vector2Int(0, -1),
        new Vector2Int(-1, -1),
        new Vector2Int(-1, 0),
        new Vector2Int(-1, 1),
    };

    public static readonly Vector2Int[] DiagonalNeighbors = {
        new Vector2Int(1, 1),
        new Vector2Int(1, -1),
        new Vector2Int(-1, -1),
        new Vector2Int(-1, 1),
    };

    public static Vector2Int[] SideNeighbors = {
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
    };

    public static Vector2Int[] ForwardNeighbors = {
        new Vector2Int(0, 1),
        new Vector2Int(0, -1),
    };
}