using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFactory : MonoBehaviour
{
    int fHeatlh;
    float fScale;
    float enemySpeed;

  

    public int setRandomHealth()
    {
        fHeatlh = Random.Range(1,5);
        return fHeatlh;
    }

    public float setRandomScale()
    {
        fScale = Random.Range(1f , 3f);
        return fScale;

        
    }
    public float setRandomSpeed()
    {
        enemySpeed = Random.Range(0.1f,1f);
        return enemySpeed;
    }
  

}
