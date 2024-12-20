using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SceneObjectsData", menuName = "ScriptableObjects/SceneObjectsData")]
public class SceneObjectsData : ScriptableObject
{
    public List<string> objectNames = new List<string>();
    public string activeObjectName;
}