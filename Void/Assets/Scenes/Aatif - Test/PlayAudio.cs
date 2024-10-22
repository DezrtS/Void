using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAudio : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        AudioManager.Instance.PlayOneShot(FMODEventsManager.Instance.Sound1, Vector3.zero);
    }
}
