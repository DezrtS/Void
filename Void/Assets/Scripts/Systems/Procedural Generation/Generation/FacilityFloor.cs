using System.Collections.Generic;
using UnityEngine;

public class FacilityFloor
{
    private readonly int seed;
    private Vector2Int size;
    private Vector3 offset;

    private readonly float tileSize;
    private readonly int interiorTilesPerMapTile;

    private readonly Grid2D<MapTile> floorMap;
    private readonly Grid2D<InteriorTile> interiorFloorMap;

    private readonly List<TileCollection> tileCollections;

    private readonly List<RoomInstance> roomInstances;
    private readonly List<FixtureInstance> fixtureInstances;

    public int Seed => seed;
    public Vector2Int Size => size;
    public Vector3 Offset => offset;

    public float TileSize => tileSize;
    public int InteriorTilesPerMapTile => interiorTilesPerMapTile;

    public Grid2D<MapTile> FloorMap => floorMap;
    public Grid2D<InteriorTile> InteriorFloorMap => interiorFloorMap;

    public List<TileCollection> TileCollections => tileCollections;

    public List <RoomInstance> RoomInstances => roomInstances;
    public List<FixtureInstance> FixtureInstances => fixtureInstances;

    public FacilityFloor(int seed, Vector2Int size, Vector3 offset, float tileSize, int interiorTilesPerMapTile)
    {
        this.seed = seed;
        this.size = size;
        this.offset = offset;

        this.tileSize = tileSize;
        this.interiorTilesPerMapTile = interiorTilesPerMapTile;

        floorMap = new Grid2D<MapTile>(size, Vector2Int.zero);
        interiorFloorMap = new Grid2D<InteriorTile> (size * interiorTilesPerMapTile, Vector2Int.zero);

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