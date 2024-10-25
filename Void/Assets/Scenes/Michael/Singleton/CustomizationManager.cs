using System.Collections.Generic;
using UnityEngine;

public class CustomizationManager : MonoBehaviour
{
    public List<GameObject> gameObjects;
    public int currentIndex = 0;

    void Start()
    {

    }

    
    void DeactivateAll()
    {
        foreach (GameObject obj in gameObjects)
        {
            obj.SetActive(false);
        }
    }

    
    public void ActivateByIndex(int index)
    {
        if (index < 0 || index >= gameObjects.Count)
        {
            Debug.LogWarning("Index out of range.");
            return;
        }

        DeactivateAll();

        
        gameObjects[index].SetActive(true);
        currentIndex = index;
    }
}