using UnityEngine;
using FMODUnity;

public class FMODEventManager : Singleton<FMODEventManager>
{
    [field: Header("SFX")]
    [field: SerializeField] public EventReference Sound1;
    [field: SerializeField] public EventReference Sound2;
}