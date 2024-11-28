using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFactory : MonoBehaviour
{

    /*
     * This is the factory pattern used to spawn the enemies, for our design pattern so far we have a single smaller enemy, we dont plan on having more
     * so i've made the factory spawn random scale enemies with random health and speed for the purposes of not making all enemies seem so similar
     * In the future the enemy speed so be greater if they are smaller, the enemies health should be higher if they are bigger
     * 
     * Future note: this spawner will need to scale according to the floor level from the game manager, (floor level * 0.5) * the  health & speed
     * ^ scaling values subject to change after playtests
     */
    int fHeatlh;
    float fScale;
    float enemySpeed;
    

    GameManagerAatifSingleton gameManager;


    private void Start()
    {
        gameManager = FindObjectOfType<GameManagerAatifSingleton>();
    }

    public int setRandomHealth()
    {
        
        fHeatlh = gameManager.floorLevel;
        return fHeatlh;
    }

    public float setRandomScale()
    {
        fScale = gameManager.floorLevel;
        return fScale;

        
    }

    public float setRandomSpeed()
    {
        enemySpeed = Random.Range(0.1f,1f);
        return enemySpeed;
    }

    

}
