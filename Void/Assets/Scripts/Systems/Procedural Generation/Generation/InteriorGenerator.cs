using System.Collections.Generic;
using UnityEngine;
using Graphs;
using System.IO;
using UnityEngine.UIElements;

public class InteriorGenerator : MonoBehaviour
{
    [Header("Interior Tiles")]
    [SerializeField] private int interiorTilesPerMapTile = 5;
    [SerializeField] private float straightnessPenalty = 1f;

    [Header("Fixture Amount")]
    [Range(0, 100)]
    [SerializeField] private int emptySpacePercentage = 85;
    [SerializeField] private int maxFixturesPerTileCollection = 100;
    [SerializeField] private int maxFixtureAttempts = 100;

    [Header("Fixtures")]
    [SerializeField] private List<FixtureData> fixtureDatas;

    [Header("Debugging")]
    [SerializeField] private bool skipHallways;
    [SerializeField] private bool spawnFixturePositions;
    [SerializeField] private bool drawFixtureRelationships;
    [SerializeField] private bool drawWalkwayPaths;

    public int InteriorTilesPerMapTile => interiorTilesPerMapTile;

    public FixtureInstance EstablishNewFixture(FacilityFloor facilityFloor, TileCollection tileCollection)
    {
        for (int atttempt = 0; atttempt < maxFixtureAttempts; atttempt++)
        {
            int randomFixtureDataIndex = Random.Range(0, fixtureDatas.Count);
            FixtureInstance originFixture = FixtureGeneration.RandomlyPlaceFixture(facilityFloor, fixtureDatas[randomFixtureDataIndex], tileCollection);

            if (originFixture != null) return originFixture;
        }

        Debug.LogWarning($"Failed to Establish New Fixture Within {maxFixtureAttempts} Attempts");
        return null;
    }

    public void SetupInterior(FacilityFloor facilityFloor)
    {
        foreach (TileCollection tileCollection in facilityFloor.TileCollections)
        {
            foreach (Vector2Int position in tileCollection.mapTilePositions)
            {
                for (int y = 0; y < interiorTilesPerMapTile; y++)
                {
                    for (int x = 0; x < interiorTilesPerMapTile; x++)
                    {
                        facilityFloor.InteriorFloorMap[interiorTilesPerMapTile * position + new Vector2Int(x, y)] = new InteriorTile(InteriorTile.InteriorTileType.None, null);
                    }
                }
            }
        }
    }

    public void GenerateInteriors(FacilityFloor facilityFloor)
    {
        SetupInterior(facilityFloor);

        GenerateWalkways(facilityFloor);

        foreach (TileCollection tileCollection in facilityFloor.TileCollections)
        {
            if (skipHallways && tileCollection.Type == TileCollection.TileCollectionType.Hallway) continue;

            // TODO - Implement support for predefined rooms
            float maxRoomTileCount = tileCollection.mapTilePositions.Count * interiorTilesPerMapTile * interiorTilesPerMapTile;
            float roomTileCount = maxRoomTileCount;

            FixtureInstance originFixture = EstablishNewFixture(facilityFloor, tileCollection);
            if (originFixture == null) continue;

            roomTileCount -= originFixture.Data.TilePositions.Count;

            for (int i = 0; i < maxFixturesPerTileCollection; i++)
            {
                if (roomTileCount / maxRoomTileCount * 100 < emptySpacePercentage) break;

                int randomFixtureInstanceIndex = Random.Range(0, tileCollection.FixtureInstances.Count);
                bool success = false;

                for (int j = 0; j < tileCollection.FixtureInstances.Count; j++)
                {
                    // TODO - Implement optimization (Do not need to iterate over relationships that have already been looked at and confirmed unfulfillable)
                    FixtureInstance selectedFixture = tileCollection.FixtureInstances[(randomFixtureInstanceIndex + j) % tileCollection.FixtureInstances.Count];
                    FixtureInstance placedFixture = FixtureGeneration.PlaceRandomRelationship(facilityFloor, tileCollection, selectedFixture);

                    if (placedFixture == null) continue;
                    roomTileCount -= placedFixture.Data.TilePositions.Count;
                    success = true;

                    if (drawFixtureRelationships)
                    {

                    }
                    break;
                }

                if (!success)
                {
                    FixtureInstance newFixture = EstablishNewFixture(facilityFloor, tileCollection);
                    if (newFixture == null) break;
                    roomTileCount -= newFixture.Data.TilePositions.Count;
                }
            }
        }
    }

    private void GenerateWalkways(FacilityFloor facilityFloor)
    {
        foreach (TileCollection tileCollection in facilityFloor.TileCollections)
        {
            Pathfinder2D pathfinder2D = new Pathfinder2D(tileCollection.Bounds.upperRight * interiorTilesPerMapTile);

            for (int i = 0; i < tileCollection.connections.Count - 1; i++)
            {
                Vector2Int start = tileCollection.connections[i].from * interiorTilesPerMapTile;
                Vector2Int end = tileCollection.connections[i + 1].from * interiorTilesPerMapTile;

                List<Vector2Int> path = pathfinder2D.FindPath(start, (Pathfinder2D.Node from, Pathfinder2D.Node to) =>
                {
                    return CalculatePathCost(facilityFloor, tileCollection, end, from, to);
                },
                (Pathfinder2D.Node node) =>
                {
                    return node.Position == end;
                });

                if (path != null)
                {
                    InteriorGeneration.PlaceInteriorTiles(facilityFloor, null, InteriorTile.InteriorTileType.Walkway, path);

                    if (drawWalkwayPaths)
                    {
                        foreach (Vector2Int position in path)
                        {
                            Vector2 otherPos = (Vector2)(position) * facilityFloor.TileSize / interiorTilesPerMapTile;
                            Vector3 newPosition = new Vector3(position.x, 1, position.y);
                            Instantiate(FacilityGenerationManager.Instance.DebugMarker, newPosition, Quaternion.identity);
                        }
                    }
                }
            }
        }
    }

    private Pathfinder2D.PathInfo CalculatePathCost(FacilityFloor facilityFloor, TileCollection tileCollection, Vector2Int end, Pathfinder2D.Node from, Pathfinder2D.Node to)
    {
        Pathfinder2D.PathInfo pathInfo = new Pathfinder2D.PathInfo();

        Vector2Int scaledPosition = to.Position / facilityFloor.InteriorTilesPerMapTile;
        if (tileCollection != facilityFloor.FloorMap[scaledPosition].Collection)
        {
            pathInfo.traversable = false;
            return pathInfo;
        }

        pathInfo.cost = Vector2Int.Distance(to.Position, end);

        Pathfinder2D.Node previous = from.Previous;
        if (previous != null)
        {
            Vector2Int previousDifference = previous.Position - from.Position;
            Vector2Int difference = from.Position - to.Position;

            if (previousDifference != difference)
            {
                pathInfo.cost += straightnessPenalty;
            }
        }

        pathInfo.traversable = true;
        InteriorTile interiorTile = facilityFloor.InteriorFloorMap[to.Position];

        if (interiorTile.Type == InteriorTile.InteriorTileType.None)
        {
            pathInfo.cost += 10;
        }
        else if (interiorTile.Type == InteriorTile.InteriorTileType.Walkway)
        {
            pathInfo.cost += 5;
        }
        else
        {
            pathInfo.traversable = false;
        }

        return pathInfo;
    }
}