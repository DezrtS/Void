using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorUpdater : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            GameManagerAatifSingleton.instance.floorLevel = GameManagerAatifSingleton.instance.floorLevel + 1;
        }
    }
}
