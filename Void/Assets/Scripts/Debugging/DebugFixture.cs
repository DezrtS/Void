using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugFixture : MonoBehaviour
{
    public FixtureData Data;
    public TileCollection ParentCollection;
    public Vector2Int Position;
    public Matrix4x4 RotationMatrix;
    public RotationPreset RotationPreset;

    public void SetupValues(FixtureInstance fixtureInstance)
    {
        Data = fixtureInstance.Data;
        ParentCollection = fixtureInstance.ParentCollection;
        RotationMatrix = fixtureInstance.RotationMatrix;
        RotationPreset = fixtureInstance.RotationPreset;
        Position = fixtureInstance.Position;
    }
}
