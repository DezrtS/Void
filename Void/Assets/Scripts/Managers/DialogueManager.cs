using UnityEngine;

public class DialogueManager : Singleton<DialogueManager>
{
    [Header("Voicelines")]
    [SerializeField] public DialogueData Countdown;
    [SerializeField] public DialogueData FifteenSeconds;
    [SerializeField] public DialogueData ThirtySeconds;
    [SerializeField] public DialogueData OneMinute;
    [SerializeField] public DialogueData TwoMinutes;
    [SerializeField] public DialogueData ThreeMinutes;
    [SerializeField] public DialogueData FourMinutes;

    [SerializeField] public DialogueData AllTasksComplete;
    [SerializeField] public DialogueData OneTaskLeft;
    [SerializeField] public DialogueData TwoTasksLeft;
    [SerializeField] public DialogueData ThreeTasksLeft;
    [SerializeField] public DialogueData FourTasksLeft;
    [SerializeField] public DialogueData FiveTasksLeft;

    [SerializeField] public DialogueData OnePlayerSlayed;
    [SerializeField] public DialogueData TwoPlayersSlayed;
    [SerializeField] public DialogueData MonsterKilled;

    [SerializeField] public DialogueData RandomHumanVoicelines;
}
