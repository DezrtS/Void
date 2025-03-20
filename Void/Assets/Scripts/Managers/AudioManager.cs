using System.Collections.Generic;
using UnityEngine;
using FMOD.Studio;
using FMODUnity;

public class AudioManager : Singleton<AudioManager>
{
    private static readonly List<EventInstance> eventInstances = new List<EventInstance>();

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

    public void CleanUp()
    {
        foreach (EventInstance eventInstance in eventInstances)
        {
            eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            eventInstance.release();
        }

        eventInstances.Clear();
    }

    private void OnDestroy()
    {
        CleanUp();
    }
}