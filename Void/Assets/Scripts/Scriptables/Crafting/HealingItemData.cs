using UnityEngine;

[CreateAssetMenu(fileName = "HealingItemData", menuName = "ScriptableObjects/Items/Data/HealingItemData")]
public class HealingItemData : ItemData
{
    [SerializeField] private float amount;
    [SerializeField] private float duration;

    public float Amount => amount;
    public float Duration => duration;
}