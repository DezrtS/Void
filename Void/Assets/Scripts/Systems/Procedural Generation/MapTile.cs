public struct MapTile
{
    public enum MapTileType
    {
        None,
        Room,
        Hallway
    }

    public MapTileType Type;
    public TileCollection Collection;

    public MapTile(MapTileType type, TileCollection collection)
    {
        Type = type;
        Collection = collection;
    }
}