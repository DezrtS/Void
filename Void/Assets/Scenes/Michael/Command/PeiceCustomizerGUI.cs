using System.Collections.Generic;
using UnityEngine;
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
    public int customizationManagerIndex;
    void Start()
    {

        customizer = new PeiceCustomizer
        {
            character = character,
            peiceOptions = new List<GameObject>(peiceOptions)
        };
        
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

        
        
        UpdatePeice();
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
        onButtonClicked?.Invoke();
        
    }
}
