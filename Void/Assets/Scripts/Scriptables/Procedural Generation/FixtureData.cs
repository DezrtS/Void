using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FixtureData", menuName = "ScriptableObjects/Procedural Generation/FixtureData", order = 1)]
public class FixtureData : TileSection
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
    public RestrictionData RestrictionData;
    public List<FixtureRelationshipData> Relationships;
}