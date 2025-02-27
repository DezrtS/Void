using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StatChangeMutationData", menuName = "ScriptableObjects/Mutations/StatChangeMutationData")]
public class StatChangeMutationData : MutationData
{
    [SerializeField] private int key;
    [SerializeField] private float duration;
    [SerializeField] private List<StatChange> statChanges;

    public int Key => key; 
    public float Duration => duration;
    public List<StatChange> StatChanges => statChanges;
}
