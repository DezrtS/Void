using UnityEngine;

[CreateAssetMenu(fileName = "InteractableData", menuName = "ScriptableObjects/Interactable/InteractableData")]
public class InteractableData : ScriptableObject
{
    [SerializeField] private string title;
    [TextArea(3, 5)]
    [SerializeField] private string description;

    public string Title => title;
    public string Description => description;
}