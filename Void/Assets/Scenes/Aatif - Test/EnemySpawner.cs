using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    /*
     * This class will have future use in the game, the things that will need to be changed will be the prefab spawned
     * it will need to spawn our enemy with the navMeshAgent onto the procedurally generated map.
     * 
     * The enemy spawner will also need a cap, this means a change in the functions, if the spawner has spawnt
     * more than 7 enemies it needs to pause spawning, it also should spawn them in waves not 1 by 1 but for testing purposes this works
     */


    public GameObject enemyPrefab;

    void Start()
    {
        InvokeRepeating("SpawnEnemy",1f,3f);
    }

    
    void Update()
    {
        
    }


    void SpawnEnemy()
    {
        Instantiate(enemyPrefab);
    }

}
