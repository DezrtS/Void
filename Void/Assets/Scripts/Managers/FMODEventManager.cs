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

    [field: Header("SFX")]
    [field: SerializeField] public EventReference Sound1;
    [field: SerializeField] public EventReference Sound2;

    [field: SerializeField] public EventReference SwitchItemSound;
    [field: SerializeField] public EventReference SelectMutationSound;
}