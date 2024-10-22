using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    
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
