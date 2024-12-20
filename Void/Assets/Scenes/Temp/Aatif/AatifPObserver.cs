using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

public class AatifPObserver : MonoBehaviour
{

    /* Explanation
     * This class observers the player stats then communicates that with UI elements
     * This allows for the player to not need to talk directly with the objects that need to
     * be affected by the player. This class helps deal with to many circular dependancies
     */

    AatifPlayerMove player;

    public GameObject DeathScreen;
    public TextMeshProUGUI text;
    public TextMeshPro diaText;

    public TextMeshProUGUI floorLevelTXT;

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
        floorLevelTXT.SetText("Floor: " + GameManagerAatifSingleton.instance.floorLevel.ToString());
    }

    private void onDeath()
    {
        if (player.health == 0)
        {
            DeathScreen.SetActive(true);
        }
        
    }


}
