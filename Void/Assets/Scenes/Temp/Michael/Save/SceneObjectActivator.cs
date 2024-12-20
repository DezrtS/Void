using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneObjectActivator : MonoBehaviour
{
    public SceneObjectsData sceneObjectsData;

    public void ApplyActiveObject()
    {
        foreach (string objectName in sceneObjectsData.objectNames)
        {
            GameObject obj = GameObject.Find(objectName);
            if (obj != null)
            {
                obj.SetActive(objectName == sceneObjectsData.activeObjectName);
            }
            else
            {
                Debug.LogWarning($"Object '{objectName}' not found in the scene.");
            }
        }
    }
}

