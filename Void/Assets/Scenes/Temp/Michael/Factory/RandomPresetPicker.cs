using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RandomPresetPicker : MonoBehaviour
{
    public List<CharacterPresetData> presetDataList; 
                                                     
    private PresetFactory presetFactory;
    private List<GameObject> createdCharacters = new List<GameObject>();
    public Button generatePresetButton;
    void Start()
    {
        
        presetFactory = new ConcretePresetFactory();
        generatePresetButton.onClick.AddListener(() => CreateRandomPreset());
    }
    
    
    public void CreateRandomPreset()
    {
        if (presetDataList.Count == 0)
        {
            Debug.LogWarning("No presets available.");
            return;
        }

        
        int randomIndex = Random.Range(0, presetDataList.Count);
        GameObject character = presetFactory.CreatePreset(presetDataList[randomIndex], null);
        createdCharacters.Add(character);
    }
}
