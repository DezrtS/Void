using FMODUnity;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueData", menuName = "ScriptableObjects/Dialogue/Data/DialogueData", order = 1)]
public class DialogueData : ScriptableObject
{
    [SerializeField] private DialogueNameData dialogueNameData;
    [SerializeField] private EventReference dialogueAudio;
    [SerializeField] private List<DialogueLine> dialogueLines;

    public DialogueNameData DialogueNameData => dialogueNameData;
    public EventReference DialogueAudio => dialogueAudio;
    public List<DialogueLine> DialogueLines => dialogueLines;
}

[Serializable]
public class DialogueLine
{
    [SerializeField] private float duration;
    [TextArea(3, 5)]
    [SerializeField] private string dialogue;

    public float Duration => duration;
    public string Dialogue => dialogue;
}