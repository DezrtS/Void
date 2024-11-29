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

    [SerializeField] private string fixtureName;
    [SerializeField] private GameObject fixturePrefab;
    [SerializeField] private bool isTaskFixture = false;
    [SerializeField] private Vector2Int gridSize = Vector2Int.one;
    [SerializeField] private List<Vector2Int> tilePositions = new List<Vector2Int>();
    [SerializeField] private List<RestrictionData> restrictions = new List<RestrictionData>();
    [SerializeField] private List<FixtureRelationshipData> relationships = new List<FixtureRelationshipData>();

    public string FixtureName
    {
        get => fixtureName;
        set => fixtureName = value;
    }
    public bool IsTaskFixture
    {
        get => isTaskFixture;
        set => isTaskFixture = value;
    }
    public Vector2Int GridSize
    {
        get => gridSize;
        set => gridSize = value;
    }
    public List<Vector2Int> TilePositions
    {
        get => tilePositions;
        set => tilePositions = value;
    }
    public List<RestrictionData> Restrictions
    {
        get => restrictions;
        set => restrictions = value;
    }
    public List<FixtureRelationshipData> Relationships
    {
        get => relationships;
        set => relationships = value;
    }
    public GameObject FixturePrefab => fixturePrefab;
}