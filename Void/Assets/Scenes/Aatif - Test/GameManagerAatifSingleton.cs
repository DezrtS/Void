using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerAatifSingleton : MonoBehaviour
{
    public int floorLevel = 0;

    public static GameManagerAatifSingleton instance;
   
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }


    }

  



}
