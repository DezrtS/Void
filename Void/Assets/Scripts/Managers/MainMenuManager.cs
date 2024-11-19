using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField]
    GameObject lastOpened;

    void Start()
    {
        
    }

    
    void Update()
    {
        
    }

    public void StartGame()
    {
        //Change this however yall want to start the game.
        Debug.Log("Game Started");
    }

    public void OpenAsset(GameObject targetGO)
    {
        Debug.Log(targetGO.name);
        CloseAsset();
        targetGO.SetActive(true);
        lastOpened = targetGO;
    }

    public void CloseAsset()
    {
        if (lastOpened.name == "PlayerTutorial")
        {
            return;
        }
        else
        {
            lastOpened.SetActive(false);
        }
        
    }

    public void Quit()
    {
        Application.Quit();
        Debug.Log("Quitted");
    }

}
