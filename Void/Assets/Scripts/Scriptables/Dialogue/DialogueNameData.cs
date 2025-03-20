using UnityEngine;

[CreateAssetMenu(fileName = "DialogueNameData", menuName = "ScriptableObjects/Dialogue/Data/DialogueNameData")]
public class DialogueNameData : ScriptableObject
{
    [SerializeField] private string speakerName;
    public string SpeakerName => speakerName;
}