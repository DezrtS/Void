using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TaskManager : NetworkSingleton<TaskManager>
{
    [SerializeField] private bool generateTasksOnSpawn;
    private List<Task> tasks = new List<Task>();
    [SerializeField] private int tasksCount = 1;

    protected override void OnEnable()
    {
        base.OnEnable();

        foreach (Task task in tasks)
        {
            task.OnTaskStateChanged += OnTaskStateChanged;
        }
    }

    private void OnDisable()
    {
        foreach (Task task in tasks)
        {
            task.OnTaskStateChanged -= OnTaskStateChanged;
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

    public void AddTask(Task task)
    {
        if (!tasks.Contains(task))
        {
            tasks.Add(task);
            task.OnTaskStateChanged += OnTaskStateChanged;
        }
    }

    public void RemoveTask(Task task)
    {
        if (tasks.Contains(task))
        {
            tasks.Remove(task);
            task.OnTaskStateChanged -= OnTaskStateChanged;
        }
    }

    public void OnTaskStateChanged(Task task, bool state)
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
        task.NetworkTask.NetworkObject.Spawn();

        SpawnTaskClientRpc(task.NetworkTask.NetworkObjectId);
    }

    [ClientRpc(RequireOwnership = false)]
    public void SpawnTaskClientRpc(ulong networkObjectId)
    {
        NetworkObject taskObject = NetworkManager.SpawnManager.SpawnedObjects[networkObjectId];
        Task task = taskObject.GetComponent<Task>();
        AddTask(task);

        Debug.Log("Added Task");
    }

    public static Draggable SpawnDraggable(GameObject draggablePrefab)
    {
        GameObject spawnedDraggable = Instantiate(draggablePrefab);
        Draggable draggable = spawnedDraggable.GetComponent<Draggable>();
        draggable.NetworkUseable.NetworkObject.Spawn();
        return draggable;
    }
}