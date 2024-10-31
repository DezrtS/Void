using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerAatifSingleton : MonoBehaviour
{

    /*
     * This class uses the singleton pattern with a minor change, instead of just destroying the script it destorys the object,
     * This script is static because it needs to be accessed by almost everything but not needs to change it so its good that it is static
     * 
     * For future purposes this class will need to generate a list of tasks and when they are complete then it will increment the floor level, that will
     * then change spawn rates and difficulty 
     */
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
