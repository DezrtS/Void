using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StatChangeMutationData", menuName = "Scriptable Objects/StatChangeMutationData")]
public class StatChangeMutationData : MutationData
{
    [Serializable]
    public class StatChange
    {
        [Serializable]
        public struct Stat
        {
            public string statName;
            public float modifier;
            public bool isPercentage;
        }

        [SerializeField] private List<Stat> stats = new List<Stat>();
        public List<Stat> Stats => stats;
    };

    [SerializeField] private float duration;
    [SerializeField] private List<StatChange> statChanges;

    public float Duration => duration;
    public List<StatChange> StatChanges => statChanges;
}
