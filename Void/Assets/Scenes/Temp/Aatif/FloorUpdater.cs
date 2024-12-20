using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorUpdater : MonoBehaviour
{
    /*
     * This Class is just used to increment the floor level, it also is only used for testing purposes and its 
     * functionality will change to soon change when tasks are complted. it will still need the gamemanager singleton because that will hold
     * all of the tasks to be completed before the level change but for now just interacting with the cube will increment the level.
     */
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            GameManagerAatifSingleton.instance.floorLevel = GameManagerAatifSingleton.instance.floorLevel + 1;
        }
    }
}
