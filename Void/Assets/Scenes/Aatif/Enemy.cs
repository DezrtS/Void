using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Enemy : MonoBehaviour
{

    /*
     * This enemy script holds how the enemies work in the scene
     * 
     * Future Notes: The movement will need to change to work with unity's navMesh or A* 
     */
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
        UpdateHealth();
    }

    void spawnEnemy()
    {
        eHealth = enemyFactory.setRandomHealth();
        UpdateHealth();
        //transform.localScale = new Vector3(enemyFactory.setRandomScale(),enemyFactory.setRandomScale(),enemyFactory.setRandomScale());
        transform.localScale = new Vector3(eHealth, eHealth, eHealth);
        speed = enemyFactory.setRandomSpeed();
        
    }


    void destroyEnemy()
    {    
        Destroy(gameObject);
    }

    void UpdateHealth()
    {
        enemyHealth.text = "Health: " + eHealth.ToString();
    }
}
