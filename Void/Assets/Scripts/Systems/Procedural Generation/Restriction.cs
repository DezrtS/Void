using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Restriction : ScriptableObject
{
    public enum RestrictionType
    {
        Manditory,
        Prohibitory
    }

    [SerializeField] private RestrictionType type;
    [SerializeField] private bool hasInteriorTileType;
    [SerializeField] private bool hasFixtureType;
    [SerializeField] private bool hasPathToWalkableTile;
    [SerializeField] private InteriorTile.InteriorTileType interiorTileType;
    [SerializeField] private FixtureData.FixtureType fixtureType;
    [SerializeField] private List<Vector2Int> positions = new List<Vector2Int>();

    public RestrictionType Type
    {
        get => type; 
        set => type = value;
    }
    public bool HasInteriorTileType
    {
        get => hasInteriorTileType;
        set => hasInteriorTileType = value;
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
    public InteriorTile.InteriorTileType InteriorTileType
    {
        get => interiorTileType;
        set => interiorTileType = value;
    }
    public FixtureData.FixtureType FixtureType
    {
        get => fixtureType; 
        set => fixtureType = value;
    }
    public List<Vector2Int> Positions
    {
        get => positions; 
        set => positions = value;
    }
}