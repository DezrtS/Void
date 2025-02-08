using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StatChangeMutationData", menuName = "Scriptable Objects/StatChangeMutationData")]
public class StatChangeMutationData : MutationData
{
    [Serializable]
    public class StatChange
    {
        [SerializeField] private string statName;
        [SerializeField] private float modifier;
        [SerializeField] private StatModifier.StatModifierType modifierType;

        public string StatName => statName;
        public float Modifier => modifier;
        public StatModifier.StatModifierType ModifierType => modifierType;
    };

    [SerializeField] private int key;
    [SerializeField] private float duration;
    [SerializeField] private List<StatChange> statChanges;

    public int Key => key; 
    public float Duration => duration;
    public List<StatChange> StatChanges => statChanges;
}
