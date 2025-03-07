using System.Collections.Generic;
using UnityEngine;

public class Trigger : MonoBehaviour
{
    public delegate void TriggerHandler(Trigger trigger, GameObject gameObject);
    public event TriggerHandler OnEnter;
    public event TriggerHandler OnExit;

    private List<GameObject> gameObjects = new List<GameObject>();

    private void OnTriggerEnter(Collider other)
    {
        OnEnter?.Invoke(this, other.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        if (gameObjects.Contains(other.gameObject)) OnExit?.Invoke(this, other.gameObject);
    }

    public void AddGameObject(GameObject gameObject)
    {
        if (!gameObjects.Contains(gameObject)) gameObjects.Add(gameObject);
    }

    public void RemoveGameObject(GameObject gameObject)
    {
        if (gameObjects.Contains(gameObject)) gameObjects.Remove(gameObject);
    }

    public void ClearGameObjects()
    {
        gameObjects.Clear();
    }
}