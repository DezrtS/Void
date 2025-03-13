using UnityEngine;

[CreateAssetMenu(fileName = "HoldingPositionData", menuName = "ScriptableObjects/Items/HoldingPositionData")]
public class HoldingPositionData : ScriptableObject
{
    [SerializeField] private Vector3 positionOffset;
    [SerializeField] private Vector3 rotationOffset;

    public Vector3 PositionOffset => positionOffset;
    public Vector3 RotationOffset => rotationOffset;
}