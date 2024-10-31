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
    
    public readonly List<Vector2Int> mapTilePositions;
    public readonly List<(Vector2Int from, Vector2Int to)> connections;

    public TileCollection(TileCollectionType type, int id)
    {
        Type = type;
        Id = id;
        AveragePosition = Vector2.zero;
        Bounds = (new Vector2Int(int.MaxValue, int.MaxValue), new Vector2Int(int.MinValue, int.MinValue));
        mapTilePositions = new List<Vector2Int>();
        connections = new List<(Vector2Int from, Vector2Int to)>();
    }

    public void AddMapTilePosition(Vector2Int position)
    {
        Vector2 totalPosition = AveragePosition * mapTilePositions.Count;
        mapTilePositions.Add(position);
        totalPosition += position;
        AveragePosition = totalPosition / mapTilePositions.Count;
        Bounds = (new Vector2Int(Mathf.Min(position.x, Bounds.lowerLeft.x), Mathf.Min(position.y, Bounds.lowerLeft.y)),
            new Vector2Int(Mathf.Max(position.x, Bounds.upperRight.x), Mathf.Max(position.y, Bounds.upperRight.y)));
    }

    public Vector2Int GetRandomMapTilePosition()
    {
        return mapTilePositions[Random.Range(0, mapTilePositions.Count)];
    }

    public Vector2Int GetClosestMapTilePositionToAverage()
    {
        Vector2Int closestPosition = Vector2Int.zero;
        float minDistance = float.MaxValue;
        foreach (Vector2Int position in mapTilePositions)
        {
            float distance = Vector2.Distance(position, AveragePosition);
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
        connections.Add((from, to));
    }

    public void PlaceFixtureAnywhere() // FixtureData
    {
        
    }

    public bool PlaceFixture(/*FixtureData data, */Vector2Int position, Vector2Int forward)
    {
        return false;
    }

    public bool CanPlaceFixture(List<Vector2Int> positions)
    {
        return false;
    }

    public void GenerateInterior()
    {

    }

    public void GenerateWalkways()
    {

    }
}