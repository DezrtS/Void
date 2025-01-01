using System.Collections.Generic;
using UnityEngine;
using Graphs;
using System.IO;
using UnityEngine.UIElements;

public class InteriorGenerator : MonoBehaviour
{
    [Header("Interior Tiles")]
    [SerializeField] private int walkwaySize = 1;
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

    public void GenerateInteriors(FacilityFloor facilityFloor)
    {
        GenerateWalkways(facilityFloor);

        foreach (TileCollection tileCollection in facilityFloor.TileCollections)
        {
            if (skipHallways && tileCollection.Type == TileCollection.TileCollectionType.Hallway) continue;

            // TODO - Implement support for predefined rooms
            float maxRoomTileCount = tileCollection.tilePositions.Count;
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

    // Connections are connecting between rooms

    private void GenerateWalkways(FacilityFloor facilityFloor)
    {
        MultiNodePathfinder2D pathfinder2D = new MultiNodePathfinder2D(facilityFloor.Size);

        foreach (TileCollection tileCollection in facilityFloor.TileCollections)
        {
            for (int i = 0; i < tileCollection.connections.Count - 1; i++)
            {
                Vector2Int start = tileCollection.connections[i].from;
                Vector2Int end = tileCollection.connections[i + 1].from;

                List<Vector2Int> path = pathfinder2D.FindPath(start, walkwaySize, (MultiNodePathfinder2D.Node from, List<MultiNodePathfinder2D.Node> to) =>
                {
                    return CalculatePathCost(facilityFloor, tileCollection, end, from, to);
                },
                (MultiNodePathfinder2D.Node node) =>
                {
                    return node.Position == end;
                });

                if (path != null)
                {
                    if (path.Count <= 1)
                    {
                        Debug.LogWarning("Walkway Could Not Spawn");
                        continue;
                    }

                    List<Vector2Int> newPositions = MultiNodePathfinder2D.ExpandPath(path, walkwaySize);
                    //path.AddRange(newPositions);
                    //Debug.Log(path.Count);

                    FacilityGeneration.PlaceTiles(facilityFloor, tileCollection, newPositions, new FacilityGeneration.TilePlaceParams(FacilityGeneration.TileType.Walkway, true));
                }
            }
        }

        DebugManager.Instance.pathDebugging.Add((pathfinder2D.VisualNodeGrid, pathfinder2D.CommandInvoker));
    }

    private MultiNodePathfinder2D.EvaluationInfo CalculatePathCost(FacilityFloor facilityFloor, TileCollection tileCollection, Vector2Int end, MultiNodePathfinder2D.Node from, List<MultiNodePathfinder2D.Node> to)
    {
        MultiNodePathfinder2D.EvaluationInfo evaluationInfo = new MultiNodePathfinder2D.EvaluationInfo();

        evaluationInfo.Cost = Mathf.Pow(Vector2Int.Distance(to[0].Position, end), 2);

        MultiNodePathfinder2D.Node previous = from.Previous;
        if (previous != null)
        {
            Vector2Int previousDifference = previous.Position - from.Position;
            Vector2Int difference = from.Position - to[0].Position;

            if (previousDifference != difference)
            {
                evaluationInfo.Cost += straightnessPenalty;
            }
        }

        evaluationInfo.Traversable = true;

        FacilityGeneration.Tile tile = facilityFloor.TileMap[to[0].Position];

        if (tile.Type == FacilityGeneration.TileType.Room || tile.Type == FacilityGeneration.TileType.Hallway)
        {
            evaluationInfo.Cost += 25;
        }
        else if (tile.Type == FacilityGeneration.TileType.Walkway)
        {
            evaluationInfo.Cost += 5;
        }
        else
        {
            evaluationInfo.Traversable = false;
            return evaluationInfo;
        }

        for (int i = 1; i < to.Count; i++)
        {
            if (facilityFloor.TileMap[to[i].Position].Type != tile.Type)
            {
                evaluationInfo.Traversable = false;
                return evaluationInfo;
            }
        }

        return evaluationInfo;
    }
}