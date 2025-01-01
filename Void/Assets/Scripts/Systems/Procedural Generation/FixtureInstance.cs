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
    public FixtureInstance(FixtureData fixtureData, TileCollection tileCollection, Vector2Int position, Matrix4x4 rotationMatrix, RotationPreset rotationPreset)
    {
        Data = fixtureData;
        ParentCollection = tileCollection;
        Position = position;
        RotationMatrix = rotationMatrix;
        RotationPreset = rotationPreset;
    }

    public FixtureData Data;
    public TileCollection ParentCollection;
    public Vector2Int Position;
    public Matrix4x4 RotationMatrix;
    public RotationPreset RotationPreset;
}