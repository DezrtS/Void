using FMODUnity;
using UnityEngine;

public class FMODEventsManager : Singleton<FMODEventsManager>
{
    [field: Header("SFX")]
    [field: SerializeField] public EventReference Sound1 { get; private set; }
    [field: SerializeField] public EventReference Sound2 { get; private set; }
}