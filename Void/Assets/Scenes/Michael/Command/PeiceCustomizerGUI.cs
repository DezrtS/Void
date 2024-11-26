using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PeiceCustomizerGUI :  MonoBehaviour
{
    public GameObject character;
    public GameObject[] peiceOptions;
    public string peiceName;
    private PeiceCustomizer customizer;
    private int currentPeiceIndex = 0;
    public CustomizationManager customizationManager;
    public List<ButtonData> buttonDataList;
    public delegate void OnButtonClicked();
    public static OnButtonClicked onButtonClicked;
    public static Action<String> activePeice;
    public int customizationManagerIndex;
    public SceneObjectsData sceneObjectsData;
    [SerializeField] private SceneObjectManager sceneObjectManager;
    [SerializeField] private Button saveButton;
    private bool isDirty;
    
    void Start()
    {

        customizer = new PeiceCustomizer
        {
            character = character,
            peiceOptions = new List<GameObject>(peiceOptions)
        };
        sceneObjectManager = GetComponent<SceneObjectManager>();
        if (sceneObjectsData.objectNames.Count == 0)
        {
            foreach (GameObject option in peiceOptions)
            {
                sceneObjectsData.objectNames.Add(option.name);
            }    
        }
        
        
        var customizationManagerIndex = this.customizationManagerIndex;
        var currentCustomizer = customizationManager.gameObjects.Count;
        customizationManager.ActivateByIndex(0);
        
        
        foreach (var buttonData in buttonDataList)
        {
            buttonData.button = GameObject.Find(buttonData.buttonName)?.GetComponent<Button>();
            buttonData.nextButton = GameObject.Find(buttonData.nextButtonName)?.GetComponent<Button>();
            buttonData.previousButton = GameObject.Find(buttonData.previousButtonName)?.GetComponent<Button>();

            
            
                
                buttonData.button.onClick.AddListener(() => customizationManager.ActivateByIndex((customizationManagerIndex + 1) % currentCustomizer ));
                
                buttonData.nextButton.onClick.AddListener(NextPeice);
                
            
            
                buttonData.previousButton.onClick.AddListener(PreviousPeice);
                
            
        }

        
        saveButton.onClick.AddListener(SaveClick);
        UpdatePeice();
    }

    private void SaveClick()
    {
        isDirty = true;
        Save(peiceOptions[currentPeiceIndex], peiceOptions[currentPeiceIndex].name);
    }

    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            PreviousPeice();
            
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            NextPeice();
        }

        if (isDirty)
        {
            Debug.Log("Saving changes...");
            isDirty = false;
        }
    }

    void NextPeice()
    {
        currentPeiceIndex = (currentPeiceIndex + 1) % peiceOptions.Length;
        UpdatePeice();
        
    }

    void PreviousPeice()
    {
        currentPeiceIndex = (currentPeiceIndex - 1 + peiceOptions.Length) % peiceOptions.Length;
        UpdatePeice();
    }

    void UpdatePeice()
    {
        var command = new ChangePeiceCommand(character, peiceOptions[currentPeiceIndex], new List<GameObject>(peiceOptions));
        customizer.AddCommand(command);
        customizer.ExecuteCommands();
        activePeice?.Invoke(peiceOptions[currentPeiceIndex].name);
        //Save(peiceOptions[currentPeiceIndex], peiceOptions[currentPeiceIndex].name);
        
    }

    public void Save(GameObject saveObject, string saveName)
    {
        sceneObjectsData.activeObjectName = saveName;
        sceneObjectManager.SaveActiveObject(saveObject);
    }
}
