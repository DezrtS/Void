using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageSystem : MonoBehaviour
{
    [SerializeField] private int damage;
    [SerializeField] private int totalHealth;
    [SerializeField] private int currentHealth;

    void Start()
    {
        currentHealth = totalHealth;
    }

    public void Damage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Debug.Log("Player is Dead.");
        }
    }
}
