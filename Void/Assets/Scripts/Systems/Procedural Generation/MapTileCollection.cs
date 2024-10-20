using System.Collections.Generic;
using UnityEngine;

public enum MapTileCollectionType
{
    None,
    Room,
    Hallway
}

public class MapTileCollection
{
    public MapTileCollectionType MapTileCollectionType;
    public int Id;
    public Vector2Int CollectionOrigin;
    public Vector2 CollectionAveragePosition;
    public (Vector2Int bottomLeftBound, Vector2Int topRightBound) CollectionBounds;
    public List<MapTile> MapTiles;

    public MapTileCollection(MapTileCollectionType mapTileCollectionType, int id, Vector2Int collectionOrigin)
    {
        MapTileCollectionType = mapTileCollectionType;
        Id = id;
        CollectionOrigin = collectionOrigin;
        CollectionAveragePosition = Vector2.zero;
        CollectionBounds = (new Vector2Int(int.MaxValue, int.MaxValue), new Vector2Int(int.MinValue, int.MinValue));
        MapTiles = new List<MapTile>();
    }

    public void AddMapTile(MapTile mapTile)
    {
        Vector2 collectionTotalPosition = CollectionAveragePosition * MapTiles.Count;
        MapTiles.Add(mapTile);
        collectionTotalPosition += mapTile.Position;
        CollectionAveragePosition = collectionTotalPosition / MapTiles.Count;
        CollectionBounds = (new Vector2Int(Mathf.Min(mapTile.Position.x, CollectionBounds.bottomLeftBound.x), Mathf.Min(mapTile.Position.y, CollectionBounds.bottomLeftBound.y)),
            new Vector2Int(Mathf.Max(mapTile.Position.x, CollectionBounds.bottomLeftBound.x), Mathf.Max(mapTile.Position.y, CollectionBounds.bottomLeftBound.y)));
    }
}