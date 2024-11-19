using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Restriction
{
    public enum RestrictionType
    {
        Manditory,
        Prohibitory
    }

    public RestrictionType Type;
    public bool HasInteriorTileType;
    public bool HasFixtureType;
    public bool HasPathToWalkableTile;
    public InteriorTile.InteriorTileType InteriorTileType;
    public FixtureData.FixtureType FixtureType;
    public List<Vector2Int> Positions;
}