using System;
using UnityEngine;

public enum RotationPreset
{
    Zero,
    Ninety, 
    OneEighty, 
    TwoSeventy
}

[Serializable]
public class FixtureInstance
{
    public FixtureData Data;
    public TileCollection ParentCollection;
    public Vector2Int Position;
    public Matrix4x4 RotationMatrix;
    public RotationPreset RotationPreset;
}