using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StatChangeItemData", menuName = "ScriptableObjects/Items/Data/StatChangeItemData")]
public class StatChangeItemData : ItemData
{
    [SerializeField] private int key;
    [SerializeField] private float duration;
    [SerializeField] private List<StatChange> statChanges;

    public int Key => key;
    public float Duration => duration;
    public List<StatChange> StatChanges => statChanges;
}