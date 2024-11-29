using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{

    [SerializeField]
    GameObject playerTut;
    [SerializeField]
    GameObject monsterTut;
    [SerializeField]
    GameObject MainMenu;


    GameObject lastOpened;
    GameObject currOpened;
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

    public void Quit()
    {
        Application.Quit();
        Debug.Log("Quitted");
    }


    // Opens Player Tut
    public void LoadPlayerTut()
    {
        MainMenu.SetActive(false);
        monsterTut.SetActive(false);
        playerTut.SetActive(true);
    }

    //Opens Monster Tut
    public void LoadMonsterTut()
    {
        MainMenu.SetActive(false);
        monsterTut.SetActive(true);
        playerTut.SetActive(false);
    }
    //Opens Main Menu
    public void LoadMainMenu()
    {
        MainMenu.SetActive(true);
        monsterTut.SetActive(false);
        playerTut.SetActive(false);
    }

    public void OpenNeeded(GameObject target)
    {      
        target.SetActive(true);
        currOpened = target;
        CloseLast();
        lastOpened = target;
        
    }

    public void CloseLast()
    {
       
        if (lastOpened != null && lastOpened != currOpened)
        {
            lastOpened.SetActive(false);
        }
     
    }

    public void LoadFallExpo()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}
