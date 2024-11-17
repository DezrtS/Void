using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixtureRelationshipData : ScriptableObject
{
    public FixtureData OtherFixture;
    public float Weight;
    public Vector2Int Position;
    public Vector2Int Forward = Vector2Int.one;
}