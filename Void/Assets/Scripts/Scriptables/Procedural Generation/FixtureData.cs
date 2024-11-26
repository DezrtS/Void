using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FixtureData", menuName = "ScriptableObjects/Procedural Generation/FixtureData", order = 1)]
public class FixtureData : ScriptableObject, IHoldTilePositions
{
    public enum FixtureType
    {
        None,
        Structural,
        Decorative,
        Task,
        Utility
    }

    public enum FixtureSubType
    {
        None,
        Seating,
        Plant,
        Storage,
        Surface
    }

    public string Name;
    public GameObject FixturePrefab;

    public Vector2Int GridSize { get; set; }
    [HideInInspector] public Vector2Int gridSize { get; set; }
    public List<Vector2Int> tilePositions1 = new List<Vector2Int>();
    public List<Vector2Int> tilePositions { get { return tilePositions1; } set { tilePositions1 = value; } }

    public List<RestrictionData> Restrictions = new List<RestrictionData>();
    public RestrictionData RestrictionData;
    public List<FixtureRelationshipData> Relationships = new List<FixtureRelationshipData>();
}