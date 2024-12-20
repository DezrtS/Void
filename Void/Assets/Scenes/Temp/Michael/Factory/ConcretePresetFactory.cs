using System.Collections.Generic;
using UnityEngine;

public abstract class PresetFactory
{
    public abstract GameObject CreatePreset(CharacterPresetData data, Transform parent);
}

public class ConcretePresetFactory : PresetFactory
{
    public override GameObject CreatePreset(CharacterPresetData data, Transform parent)
    {
        if (data.characterPrefab == null)
        {
            Debug.LogWarning("Character prefab is missing!");
            return null;
        }

        GameObject character = GameObject.Instantiate(data.characterPrefab, parent);
        character.name = data.presetName;

        
        ApplyRandomSelection(character, data.hatOptions);
        ApplyRandomSelection(character, data.bootOptions);
        
        

        return character;
    }

    private void ApplyRandomSelection(GameObject character, List<GameObject> options)
    {
        if (options == null || options.Count == 0)
            return;

        
        foreach (var option in options)
        {
            option.SetActive(false);
        }

        
        int randomIndex = Random.Range(0, options.Count);
        options[randomIndex].SetActive(true);
    }
}