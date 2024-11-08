using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterMelee : MonoBehaviour
{
    public int meleeDamage = 15;
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            gameObject.SetActive(false);
        }
    }
}
