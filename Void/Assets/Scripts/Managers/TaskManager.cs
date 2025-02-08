using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class TaskManager : NetworkSingleton<TaskManager>
{
    [SerializeField] private bool generateTasksOnSpawn;
    private NetworkVariable<int> totalTasks = new NetworkVariable<int>(0);
    private List<Task> tasks = new List<Task>();
    [SerializeField] private int tasksCount = 1;

    [SerializeField] private List<TaskObjectSpawnPoint> taskObjectSpawnPoints = new List<TaskObjectSpawnPoint>();

    protected override void OnEnable()
    {
        base.OnEnable();

        foreach (Task task in tasks)
        {
            task.OnTaskCompletion += OnTaskCompletion;
            task.OnUpdateTaskInstructions += RegenerateTaskInstructions;
        }
    }

    private void OnDisable()
    {
        foreach (Task task in tasks)
        {
            task.OnTaskCompletion -= OnTaskCompletion;
            task.OnUpdateTaskInstructions -= RegenerateTaskInstructions;
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer && generateTasksOnSpawn) GenerateTasks();
    }

    public void GenerateTasks()
    {
        List<TaskData> taskDatas = GameDataManager.Instance.Tasks;
        for (int i = 0; i < tasksCount; i++)
        {
            int randomTaskDataIndex = Random.Range(0, taskDatas.Count);
            TaskData selectedTaskData = taskDatas[randomTaskDataIndex];
            List<GameObject> taskPrefabs = selectedTaskData.TaskPrefabs;
            int randomTaskIndex = Random.Range(0, taskPrefabs.Count);
            SpawnTaskServerRpc(randomTaskDataIndex, randomTaskIndex);
        }
    }

    public void AddTaskObjectSpawnPoint(TaskObjectSpawnPoint taskObjectSpawnPoint)
    {
        //if (!taskObjectSpawnPoints.Contains(taskObjectSpawnPoint))
        //{
            taskObjectSpawnPoints.Add(taskObjectSpawnPoint);
        //}
    }

    public TaskObjectSpawnPoint GetAvailableTaskObjectSpawnPoint(TaskObjectSpawnPoint.TaskObjectType objectType)
    {
        List<TaskObjectSpawnPoint> possibleSpawnPoints = taskObjectSpawnPoints.FindAll(x => !x.IsOccupied && x.ObjectType == objectType);

        if (possibleSpawnPoints.Count > 0)
        {
            int index = Random.Range(0, possibleSpawnPoints.Count);
            return possibleSpawnPoints[index];
        }

        Debug.LogWarning("No Suitable Task Object Spawn Points were Found");
        return null;
    }

    public void AddTask(Task task)
    {
        if (!tasks.Contains(task))
        {
            tasks.Add(task);
            totalTasks.Value++;
            // OnTaskAdded Event
            task.OnTaskCompletion += OnTaskCompletion;
            task.OnUpdateTaskInstructions += RegenerateTaskInstructions;
        }
    }

    public void RemoveTask(Task task)
    {
        if (tasks.Contains(task))
        {
            tasks.Remove(task);
            totalTasks.Value--;
            // OnTaskRemoved Event
            task.OnTaskCompletion -= OnTaskCompletion;
            task.OnUpdateTaskInstructions -= RegenerateTaskInstructions;
        }
    }

    public void OnTaskCompletion(Task task)
    {
        Debug.Log($"{task.TaskData.TaskName} task was completed");
    }

    public void RegenerateTaskInstructions()
    {
        string instructions = "";
        foreach (Task task in tasks)
        {
            instructions += $"{task.GetInstructions()}\n";
        }
        UIManager.Instance.TaskText.text = instructions;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnTaskServerRpc(int taskDataIndex, int taskPrefabIndex)
    {
        TaskData selectedTaskData = GameDataManager.Instance.Tasks[taskDataIndex];
        GameObject taskPrefab = selectedTaskData.TaskPrefabs[taskPrefabIndex];

        GameObject spawnedTask = Instantiate(taskPrefab);
        Task task = spawnedTask.GetComponent<Task>();
        task.NetworkObject.Spawn();
        AddTask(task);

        SpawnTaskClientRpc(task.NetworkObjectId);
    }

    [ClientRpc(RequireOwnership = false)]
    public void SpawnTaskClientRpc(ulong networkObjectId)
    {
        NetworkObject taskObject = NetworkManager.SpawnManager.SpawnedObjects[networkObjectId];
        Task task = taskObject.GetComponent<Task>();
        AddTask(task);

        Debug.Log("Added Task");
    }
}