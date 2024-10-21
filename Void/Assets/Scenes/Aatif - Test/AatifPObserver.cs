using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

public class AatifPObserver : MonoBehaviour
{
    AatifPlayerMove player;

    public GameObject DeathScreen;
    public TextMeshProUGUI text;
    public TextMeshPro diaText;


    void Start()
    {
        player = GameObject.FindAnyObjectByType<AatifPlayerMove>();
        DeathScreen.SetActive(false);
    }

   
    void Update()
    {
        text.SetText("Health: " + player.health.ToString());
        diaText.SetText("Health: " + player.health.ToString());
        onDeath();
    }

    private void onDeath()
    {
        if (player.health == 0)
        {
            DeathScreen.SetActive(true);
        }
        
    }


}
