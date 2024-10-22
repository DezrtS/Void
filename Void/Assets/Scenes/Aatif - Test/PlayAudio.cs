using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAudio : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        AudioManager.Instance.PlayOneShot(FMODEventManager.Instance.Sound1);
    }
}
