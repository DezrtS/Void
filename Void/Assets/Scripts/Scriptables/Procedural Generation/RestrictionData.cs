using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RestrictionData", menuName = "ScriptableObjects/Procedural Generation/RestrictionData", order = 1)]
public class RestrictionData : ScriptableObject
{
    [SerializeField] public List<Restriction> Restrictions;
}