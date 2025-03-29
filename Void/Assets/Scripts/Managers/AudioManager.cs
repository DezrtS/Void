using System.Collections.Generic;
using UnityEngine;
using FMOD.Studio;
using FMODUnity;
using Unity.Netcode;

public class AudioManager : Singleton<AudioManager>
{
    [SerializeField] private EventReference defaultMusic;
    [SerializeField] private bool startMusicOnAwake;
    private static readonly List<EventInstance> eventInstances = new List<EventInstance>();
    private static EventInstance musicInstance;

    private void Awake()
    {
        if (startMusicOnAwake)
        {
            EventInstance musicInstance = CreateEventInstance(defaultMusic);
            musicInstance.start();
        }
    }

    public static void ChangeMusic(EventReference newMusicEventReference)
    {
        if (eventInstances.Contains(musicInstance))
        {
            eventInstances.Remove(musicInstance);

            musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            musicInstance.release();
        }

        musicInstance = CreateEventInstance(newMusicEventReference);
        musicInstance.start();
    }

    public static void StopMusic(FMOD.Studio.STOP_MODE stopMode)
    {
        musicInstance.stop(stopMode);
    }

    public static EventInstance CreateEventInstance(EventReference eventReference)
    {
        EventInstance eventInstance = RuntimeManager.CreateInstance(eventReference);
        eventInstances.Add(eventInstance);
        return eventInstance;
    }

    public static EventInstance CreateEventInstance(EventReference eventReference, GameObject attachTo)
    {
        EventInstance eventInstance = RuntimeManager.CreateInstance(eventReference);
        eventInstances.Add(eventInstance);
        RuntimeManager.AttachInstanceToGameObject(eventInstance, attachTo);
        return eventInstance;
    }

    public static void RemoveEventInstance(EventInstance eventInstance)
    {
        eventInstances.Remove(eventInstance);
        RuntimeManager.DetachInstanceFromGameObject(eventInstance);
    }

    public static void PlayOneShot(EventReference sound)
    {
        if (sound.IsNull) return;
        RuntimeManager.PlayOneShot(sound);
    }

    public static void PlayOneShot(EventReference sound, Vector3 worldPosition)
    {
        if (sound.IsNull) return;
        RuntimeManager.PlayOneShot(sound, worldPosition);
    }

    public static void PlayOneShot(EventReference sound, GameObject gameObject)
    {
        if (sound.IsNull) return;
        RuntimeManager.PlayOneShotAttached(sound, gameObject);
    }

    public static void RequestPlayOneShot(EventReference sound, Vector3 worldPosition) => PlayOneShotServerRpc(sound, worldPosition);

    [ServerRpc(RequireOwnership = false)]
    private static void PlayOneShotServerRpc(EventReference sound, Vector3 worldPosition)
    {
        PlayOneShotClientRpc(sound, worldPosition);
    }

    [ClientRpc(RequireOwnership = false)]
    private static void PlayOneShotClientRpc(EventReference sound, Vector3 worldPosition)
    {
        PlayOneShot(sound, worldPosition);
    }

    public static void PlayDialogue(DialogueData dialogueData)
    {
        if (dialogueData == null) return;
        UIManager.Instance.SetDialogueText(dialogueData);
        PlayOneShot(dialogueData.DialogueAudio);
    }

    public static void RequestPlayDialogue(DialogueData dialogueData) => PlayDialogueServerRpc(GameDataManager.Instance.GetDialogueDataIndex(dialogueData));

    [ServerRpc(RequireOwnership = false)]
    private static void PlayDialogueServerRpc(int index)
    {
        PlayDialogueClientRpc(index);
    }

    [ClientRpc(RequireOwnership = false)]
    private static void PlayDialogueClientRpc(int index)
    {
        PlayDialogue(GameDataManager.Instance.GetDialogueData(index));
    }

    public void CleanUp()
    {
        foreach (EventInstance eventInstance in eventInstances)
        {
            eventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            eventInstance.release();
        }

        eventInstances.Clear();
    }

    private void OnDestroy()
    {
        CleanUp();
    }
}