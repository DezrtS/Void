using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ResourceData", menuName = "ScriptableObjects/Item", order = 1)]
public class ItemData : ScriptableObject
{
    public string Name;
    public string Description;
    public GameObject ItemPrefab;
}
