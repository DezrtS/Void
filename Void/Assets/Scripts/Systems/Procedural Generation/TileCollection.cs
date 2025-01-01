using System.Collections.Generic;
using UnityEngine;

public class TileCollection
{
    public enum TileCollectionType
    {
        None,
        Hallway,
        Storage,
        Office,
        Recreation,
        Wash,
        Lab,
        Room
    }

    public TileCollectionType Type;
    public int Id;
    public Vector2 AveragePosition;
    public (Vector2Int lowerLeft, Vector2Int upperRight) Bounds;

    public RoomData RoomData;
    
    public readonly List<Vector2Int> tilePositions;
    public readonly List<Connection> connections;

    private readonly List<FixtureInstance> fixtureInstances;
    public List<FixtureInstance> FixtureInstances => fixtureInstances;

    public TileCollection(TileCollectionType type, int id)
    {
        Type = type;
        Id = id;
        AveragePosition = Vector2.zero;
        Bounds = (new Vector2Int(int.MaxValue, int.MaxValue), new Vector2Int(int.MinValue, int.MinValue));
        tilePositions = new List<Vector2Int>();
        connections = new List<Connection>();

        fixtureInstances = new List<FixtureInstance>();
    }

    public void InitiatlizeRoom(RoomData roomData)
    {
        RoomData = roomData;
    }

    public void AddTilePosition(Vector2Int position)
    {
        Vector2 totalPosition = AveragePosition * tilePositions.Count;
        tilePositions.Add(position);
        totalPosition += position;
        AveragePosition = totalPosition / tilePositions.Count;
        Bounds = (new Vector2Int(Mathf.Min(position.x, Bounds.lowerLeft.x), Mathf.Min(position.y, Bounds.lowerLeft.y)),
            new Vector2Int(Mathf.Max(position.x + 1, Bounds.upperRight.x), Mathf.Max(position.y + 1, Bounds.upperRight.y)));
    }

    public void AddFixtureInstance(FixtureInstance fixtureInstance)
    {
        fixtureInstances.Add(fixtureInstance);
    }

    public Vector2Int GetRandomTilePosition()
    {
        return tilePositions[Random.Range(0, tilePositions.Count)];
    }

    public Vector2Int GetClosestTilePositionToAverage()
    {
        return GetClosestTileTowardsPosition(AveragePosition);
    }

    public Vector2Int GetClosestTileTowardsPosition(Vector2 setPosition)
    {
        Vector2Int closestPosition = Vector2Int.zero;
        float minDistance = float.MaxValue;
        foreach (Vector2Int position in tilePositions)
        {
            float distance = Vector2.Distance(position, setPosition);
            if (distance < minDistance)
            {
                closestPosition = position;
                minDistance = distance;
            }
        }
        return closestPosition;
    }

    public void AddConnection(Vector2Int from, Vector2Int to)
    {
        connections.Add(new Connection(from, to));
    }
}