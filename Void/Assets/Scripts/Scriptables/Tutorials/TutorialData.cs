using UnityEngine;

[CreateAssetMenu(fileName = "TutorialData", menuName = "ScriptableObjects/Tutorials/TutorialData")]
public class TutorialData : ScriptableObject
{
    [SerializeField] private bool overrideTitle;
    [SerializeField] private string title;
    [TextArea(3, 5)]
    [SerializeField] private string description;

    public bool OverrideTitle => overrideTitle;
    public string Title => title;
    public string Description => description;
}