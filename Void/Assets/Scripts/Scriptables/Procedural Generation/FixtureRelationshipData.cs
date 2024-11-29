using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixtureRelationshipData : ScriptableObject
{
    [SerializeField] private FixtureRelationshipData otherRelationship;
    [SerializeField] private FixtureData otherFixture;
    [SerializeField] private bool enabled = true;
    [SerializeField] private float weight = 1;
    [SerializeField] private Vector2Int position;
    [SerializeField] private Matrix4x4 rotationMatrix = Matrix4x4.identity;
    [SerializeField] private RotationPreset rotationPreset;
    [SerializeField] private bool rotatable;
    [SerializeField] private Vector2Int rotationAxisPosition;
    [SerializeField] private bool mirrorable;
    [SerializeField] private Vector2Int mirrorAxis = Vector2Int.right;
    [SerializeField] private Vector2Int mirrorAxisPosition;

    public FixtureRelationshipData OtherRelationship
    {
        get => otherRelationship;
        set => otherRelationship = value;
    }
    public FixtureData OtherFixture
    {
        get => otherFixture;
        set => otherFixture = value;
    }
    public bool Enabled
    {
        get => enabled;
        set => enabled = value;
    }
    public float Weight
    {
        get => weight; 
        set => weight = value;
    }
    public Vector2Int Position
    {
        get => position; 
        set => position = value;
    }
    public Matrix4x4 RotationMatrix
    {
        get => rotationMatrix; 
        set => rotationMatrix = value;
    }
    public RotationPreset RotationPreset
    {
        get => rotationPreset; 
        set => rotationPreset = value;
    }
    public bool Rotatable
    {
        get => rotatable; 
        set => rotatable = value;
    }
    public Vector2Int RotationAxisPosition
    {
        get => rotationAxisPosition; 
        set => rotationAxisPosition = value;
    }
    public bool Mirrorable
    {
        get => mirrorable; 
        set => mirrorable = value;
    }
    public Vector2Int MirrorAxis
    {
        get => mirrorAxis;
        set => mirrorAxis = value;
    }
    public Vector2Int MirrorAxisPosition
    {
        get => mirrorAxisPosition; 
        set => mirrorAxisPosition = value;
    }
}