using System.Collections.Generic;
using UnityEngine;

public class FacilityFloor
{
    private readonly int seed;
    private Vector2Int size;
    private Vector3 offset;

    private readonly float tileSize;

    private readonly Grid2D<FacilityGeneration.Tile> tileMap;
    private readonly Grid2D<FixtureInstance> fixtureMap;

    private readonly List<TileCollection> tileCollections;

    private readonly List<RoomInstance> roomInstances;
    private readonly List<FixtureInstance> fixtureInstances;

    public int Seed => seed;
    public Vector2Int Size => size;
    public Vector3 Offset => offset;

    public float TileSize => tileSize;

    public Grid2D<FacilityGeneration.Tile> TileMap => tileMap;
    public Grid2D<FixtureInstance> FixtureMap => fixtureMap;

    public List<TileCollection> TileCollections => tileCollections;

    public List<RoomInstance> RoomInstances => roomInstances;
    public List<FixtureInstance> FixtureInstances => fixtureInstances;

    public FacilityFloor(int seed, Vector2Int size, Vector3 offset, float tileSize)
    {
        this.seed = seed;
        this.size = size;
        this.offset = offset;

        this.tileSize = tileSize;

        tileMap = new Grid2D<FacilityGeneration.Tile>(size);
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                tileMap[x, y] = new FacilityGeneration.Tile();
            }
        }

        fixtureMap = new Grid2D<FixtureInstance>(size);

        tileCollections = new List<TileCollection>();

        roomInstances = new List<RoomInstance>();
        fixtureInstances = new List<FixtureInstance>();
    }

    ~FacilityFloor()
    {
        tileCollections.Clear();
        roomInstances.Clear();
        fixtureInstances.Clear();
    }
}