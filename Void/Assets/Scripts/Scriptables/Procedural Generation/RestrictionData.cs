using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RestrictionData", menuName = "ScriptableObjects/Procedural Generation/RestrictionData", order = 1)]
public class RestrictionData : ScriptableObject
{
    [SerializeField] private bool enabled = true;
    [SerializeField] private List<TileCollection.TileCollectionType> tileCollectionTypes;
    [SerializeField] private List<Restriction> restrictions = new List<Restriction>();

    public bool Enabled
    {
        get => enabled; 
        set => enabled = value;
    }
    public List<TileCollection.TileCollectionType> TileCollectionTypes
    {
        get => tileCollectionTypes;
        set => tileCollectionTypes = value;
    }
    public List<Restriction> Restrictions
    {
        get => restrictions;
        set => restrictions = value;
    }
}