using System.Collections.Generic;
using UnityEngine;
using FMOD.Studio;
using FMODUnity;

public class AudioManager : Singleton<AudioManager>
{
    private readonly List<EventInstance> eventInstances = new List<EventInstance>();

    public EventInstance CreateEventInstance(EventReference eventReference)
    {
        EventInstance eventInstance = RuntimeManager.CreateInstance(eventReference);
        eventInstances.Add(eventInstance);
        return eventInstance;
    }

    public EventInstance CreateEventInstance(EventReference eventReference, GameObject attachTo)
    {
        EventInstance eventInstance = RuntimeManager.CreateInstance(eventReference);
        eventInstances.Add(eventInstance);
        RuntimeManager.AttachInstanceToGameObject(eventInstance, attachTo);
        return eventInstance;
    }

    public void RemoveEventInstance(EventInstance eventInstance)
    {
        eventInstances.Remove(eventInstance);
        RuntimeManager.DetachInstanceFromGameObject(eventInstance);
    }

    public void PlayOneShot(EventReference sound)
    {
        RuntimeManager.PlayOneShot(sound);
    }

    public void PlayOneShot(EventReference sound, Vector3 worldPosition)
    {
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