using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct ResourceRequirement
{
    public ResourceData Resource;
    public int Amount;
}


[CreateAssetMenu(fileName = "RecipeData", menuName = "ScriptableObjects/Crafting Recipe", order = 1)]
public class RecipeData : ScriptableObject
{
    public List<ResourceRequirement> ResourceRequirements = new List<ResourceRequirement>();
    public ItemData Item;
}
