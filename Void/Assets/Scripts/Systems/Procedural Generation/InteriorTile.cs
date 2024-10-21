public struct InteriorTile
{
    public enum InteriorTileType
    {
        Wall,
        None,
        Walkway,
        Fixture
    }

    public InteriorTileType Type;
    public FixtureInstance Instance;

    public InteriorTile(InteriorTileType type, FixtureInstance instance)
    {
        Type = type;
        Instance = instance;
    }
}