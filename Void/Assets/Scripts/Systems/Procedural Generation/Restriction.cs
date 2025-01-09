using System;
using System.Collections.Generic;
using UnityEngine;
using static FacilityGeneration;

[Serializable]
public class Restriction : ScriptableObject
{
    public enum RestrictionType
    {
        Manditory,
        Prohibitory
    }

    [SerializeField] private RestrictionType type;
    [SerializeField] private bool hasTileType;
    [SerializeField] private bool hasFixtureType;
    [SerializeField] private bool hasPathToWalkableTile;
    [SerializeField] private FacilityGeneration.TileType tileType;
    [SerializeField] private List<FixtureData.FixtureTag> fixtureTags = new List<FixtureData.FixtureTag>();
    [SerializeField] private List<Vector2Int> positions = new List<Vector2Int>();

    public RestrictionType Type
    {
        get => type; 
        set => type = value;
    }
    public bool HasTileType
    {
        get => hasTileType;
        set => hasTileType = value;
    }
    public bool HasFixtureType
    {
        get => hasFixtureType;
        set => hasFixtureType = value;
    }
    public bool HasPathToWalkableTile
    {
        get => hasPathToWalkableTile;
        set => hasPathToWalkableTile = value;
    }
    public FacilityGeneration.TileType TileType
    {
        get => tileType;
        set => tileType = value;
    }

    public List<FixtureData.FixtureTag> FixtureTags => fixtureTags;
    public List<Vector2Int> Positions => positions;
}