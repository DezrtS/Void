using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StatChangesData", menuName = "ScriptableObjects/Stats/Data/StatChangesData")]
public class StatChangesData : ScriptableObject
{
    [SerializeField] private int key;
    [SerializeField] private float duration;
    [SerializeField] private List<StatChange> statChanges;

    public int Key => key;
    public float Duration => duration;
    public List<StatChange> StatChanges => statChanges;
}