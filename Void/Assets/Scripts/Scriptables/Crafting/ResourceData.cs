using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "ResourceData", menuName = "ScriptableObjects/Resource", order = 1)]
public class ResourceData : ScriptableObject
{
    public int Id;
    public string Name;
    public Image Icon;
}