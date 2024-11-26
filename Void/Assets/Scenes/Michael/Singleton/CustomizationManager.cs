using System.Collections.Generic;
using UnityEngine;

public class CustomizationManager : MonoBehaviour
{
    public static CustomizationManager Instance { get; private set; }
    
    public List<GameObject> gameObjects;
    public int currentIndex = 0;
    [SerializeField] private SceneObjectManager sceneObjectManager;

    private void Awake()
    {
        // Ensure that there's only one instance of the class
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional: Keeps this object across scenes
        }
        else
        {
            Debug.LogWarning("Multiple CustomizationManager instances found. Destroying duplicate.");
            Destroy(gameObject); // Destroy duplicate instances
            return;
        }
    }

    void Start()
    {
        sceneObjectManager = GetComponent<SceneObjectManager>();
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