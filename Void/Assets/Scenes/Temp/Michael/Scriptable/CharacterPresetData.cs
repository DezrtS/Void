using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CharacterPresetData", menuName = "ScriptableObjects/CharacterPresetData", order = 1)]
public class CharacterPresetData : ScriptableObject
{
    public string presetName;
    public GameObject characterPrefab;
    public List<GameObject> bootOptions;
    public List<GameObject> hatOptions;
    
}