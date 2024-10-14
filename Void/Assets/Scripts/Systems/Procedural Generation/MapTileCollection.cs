using System.Collections.Generic;
using UnityEngine;

public enum MapTileCollectionType
{
    None,
    Room,
    Hallway
}

public struct MapTileCollection
{
    public MapTileCollectionType MapTileCollectionType { get; set; }
    public int Id;
    public Vector2 CollectionOrigin { get; set; }
    public List<MapTile> MapTiles;

    public MapTileCollection(MapTileCollectionType mapTileCollectionType, int id, Vector2 collectionOrigin)
    {
        MapTileCollectionType = mapTileCollectionType;
        Id = id;
        CollectionOrigin = collectionOrigin;
        MapTiles = new List<MapTile>();
    }
}