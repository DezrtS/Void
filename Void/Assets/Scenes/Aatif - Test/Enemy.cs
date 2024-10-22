using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int eHealth;
    public float scale;
    public float speed;

    EnemyFactory enemyFactory;

    GameObject target;

    public TextMeshPro enemyHealth;


    private void Awake()
    {
        enemyFactory = FindObjectOfType<EnemyFactory>();
        target = GameObject.FindGameObjectWithTag("Player");
    }

    void Start()
    {
        spawnEnemy();
        Invoke("destroyEnemy",7f);
    }

    
    void Update()
    {
        transform.position = Vector3.Lerp(transform.position, target.transform.position, speed * Time.deltaTime);
        UpdateHeatlh();
    }

    void spawnEnemy()
    {
        eHealth = enemyFactory.setRandomHealth();
        UpdateHeatlh();
        //transform.localScale = new Vector3(enemyFactory.setRandomScale(),enemyFactory.setRandomScale(),enemyFactory.setRandomScale());
        transform.localScale = new Vector3(eHealth, eHealth, eHealth);
        speed = enemyFactory.setRandomSpeed();
        
    }


    void destroyEnemy()
    {
        Destroy(gameObject);
    }

    void UpdateHeatlh()
    {
        enemyHealth.text = "Health: " + eHealth.ToString();
    }
}
