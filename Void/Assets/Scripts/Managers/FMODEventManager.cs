using UnityEngine;
using FMODUnity;

public class FMODEventManager : Singleton<FMODEventManager>
{
    [field: Header("Music")]
    [field: SerializeField] public EventReference TitleTheme;
    [field: SerializeField] public EventReference MainMenuTheme;
    [field: SerializeField] public EventReference LobbyTheme;
    [field: SerializeField] public EventReference ElevatorTheme;
    [field: SerializeField] public EventReference BackgroundTheme;
    [field: SerializeField] public EventReference AlarmTheme;

    [field: Header("Voicelines")]
    [field: SerializeField] public EventReference Countdown;
    [field: SerializeField] public EventReference FifteenSeconds;
    [field: SerializeField] public EventReference ThirtySeconds;
    [field: SerializeField] public EventReference OneMinute;
    [field: SerializeField] public EventReference TwoMinutes;
    [field: SerializeField] public EventReference ThreeMinutes;
    [field: SerializeField] public EventReference FourMinutes;

    [field: SerializeField] public EventReference AllTasksComplete;
    [field: SerializeField] public EventReference OneTaskLeft;
    [field: SerializeField] public EventReference TwoTasksLeft;
    [field: SerializeField] public EventReference ThreeTasksLeft;
    [field: SerializeField] public EventReference FourTasksLeft;
    [field: SerializeField] public EventReference FiveTasksLeft;

    [field: SerializeField] public EventReference OnePlayerSlayed;
    [field: SerializeField] public EventReference TwoPlayersSlayed;
    [field: SerializeField] public EventReference MonsterKilled;

    [field: SerializeField] public EventReference RandomHumanVoicelines;

    [field: Header("SFX")]
    [field: SerializeField] public EventReference Sound1;
    [field: SerializeField] public EventReference Sound2;

    [field: SerializeField] public EventReference SwitchItemSound;
    [field: SerializeField] public EventReference SelectMutationSound;
}