using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fixture : MonoBehaviour
{
    [SerializeField] private bool canSpawnTasks;
    [SerializeField] private List<TaskData> taskDatas;
    [SerializeField] private Vector3 spawnPosition;

    public bool SpawnTasks()
    {
        if (canSpawnTasks)
        {


            return true;
        }

        return false;
    }
}