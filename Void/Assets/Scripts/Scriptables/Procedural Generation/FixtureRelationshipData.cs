using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixtureRelationshipData : ScriptableObject
{
    public FixtureRelationshipData OtherRelationship;
    public FixtureData OtherFixture;
    public float Weight = 1;
    public Vector2Int Position;
    public Matrix4x4 RotationMatrix = Matrix4x4.identity;
    public RotationPreset RotationPreset;
    public bool Rotatable;
    public bool Mirrorable;
    public Vector2Int MirrorAxis = Vector2Int.right;
    public Vector2Int MirrorAxisPosition;
}