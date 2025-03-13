using UnityEngine;

[CreateAssetMenu(fileName = "DeployableItemData", menuName = "ScriptableObjects/Items/Data/DeployableItemData")]
public class DeployableItemData : ItemData
{
    [SerializeField] private float deployRange;
    [SerializeField] private LayerMask deployLayers;

    public float DeployRange => deployRange;
    public LayerMask DeployLayers => deployLayers;
}