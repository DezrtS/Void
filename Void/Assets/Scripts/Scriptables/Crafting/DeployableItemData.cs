using FMODUnity;
using UnityEngine;

[CreateAssetMenu(fileName = "DeployableItemData", menuName = "ScriptableObjects/Items/Data/DeployableItemData")]
public class DeployableItemData : ItemData
{
    [SerializeField] private float deployRange;
    [SerializeField] private LayerMask deployLayers;
    [SerializeField] private EventReference deploySound;

    public float DeployRange => deployRange;
    public LayerMask DeployLayers => deployLayers;
    public EventReference DeploySound => deploySound;
}