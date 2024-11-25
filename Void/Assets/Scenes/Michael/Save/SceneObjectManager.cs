using UnityEngine;
using System.IO;

public class SceneObjectManager: MonoBehaviour
{
    
    private string savePath;
    [SerializeField] public SceneObjectsData sceneObjectsData;
    [SerializeField] private string peiceType;
    private void Awake()
    {
        savePath = Application.persistentDataPath + $"/sceneObjects{peiceType}.json";
    }

    public void SaveActiveObject(GameObject activeObject)
    {
        
        sceneObjectsData.activeObjectName = activeObject.name;

        
        string json = JsonUtility.ToJson(sceneObjectsData);
        File.WriteAllText(savePath, json);
        Debug.Log("Scene objects data saved to " + savePath);
    }

    public void LoadActiveObject()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            JsonUtility.FromJsonOverwrite(json, sceneObjectsData);
            Debug.Log("Scene objects data loaded from " + savePath);
        }
        else
        {
            Debug.LogWarning("No saved data found.");
        }
    }
}