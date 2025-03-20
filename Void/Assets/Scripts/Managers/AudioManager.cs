using System.Collections.Generic;
using UnityEngine;
using FMOD.Studio;
using FMODUnity;

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