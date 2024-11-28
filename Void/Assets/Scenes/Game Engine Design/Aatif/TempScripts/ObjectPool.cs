using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
   
    private GameObject objectPrefab;
    private readonly Queue<GameObject> pool = new();
    private readonly List<GameObject> activePool = new();
    private bool spawnNewIfEmpty = false;
    private GameObject poolContainer;
    private int spawnCount = 3;

    public void InitializePool(GameObject objectPrefab, int objectCount, bool spawnNewIfEmpty)
    {
        this.objectPrefab = objectPrefab;
        poolContainer = new GameObject("Pool Container");

        for (int i = 0; i < objectCount; i++)
        {
            GameObject instance = Instantiate(objectPrefab, poolContainer.transform);
            instance.name = $"New Object {spawnCount}";
            spawnCount++;
            instance.SetActive(false);
            pool.Enqueue(instance);
        }

        this.spawnNewIfEmpty = spawnNewIfEmpty;
    }

    public GameObject GetObject()
    {
        if (pool.Count > 0)
        {
            GameObject activeObject = pool.Dequeue();
            activeObject.SetActive(true);
            activePool.Add(activeObject);
            if (pool.Contains(activeObject))
            {
                Debug.LogError("Object WAS NOT REMOVED");
            }
            return activeObject;
        }
        else if (spawnNewIfEmpty)
        {
            GameObject instance = Instantiate(objectPrefab, poolContainer.transform);
            instance.name = $"Chicken {spawnCount}";
            spawnCount++;
            activePool.Add(instance);
            return instance;
        }

        return null;
    }

    public void ReturnToPool(GameObject gameObject)
    {
        if (pool.Contains(gameObject))
        {
            Debug.LogError("Object Already In Pool");
        }
        activePool.Remove(gameObject);
        gameObject.SetActive(false);
        pool.Enqueue(gameObject);
    }

    public List<GameObject> GetActivePool()
    {
        return activePool;
    }
}