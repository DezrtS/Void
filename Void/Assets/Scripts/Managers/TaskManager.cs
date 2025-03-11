using System;
using System.Collections.Generic;
using UnityEngine;

public class TaskManager : Singleton<TaskManager>
{
    public static event Action OnAllTasksCompleted;

    [SerializeField] private int taskCount = 1;

    private NetworkTaskManager networkTaskManager;
    private bool isAllTasksCompleted;

    private List<Task> tasks = new List<Task>();

    public int TaskCount => taskCount;
    public NetworkTaskManager NetworkTaskManager => networkTaskManager;
    public List<Task> Tasks => tasks;

    public void RequestSpawnTask(int taskDataIndex) => networkTaskManager.SpawnTaskServerRpc(taskDataIndex);
    public void RequestAddTask(Task task) => networkTaskManager.AddTaskServerRpc(task.NetworkTask.NetworkObjectId);
    public void RequestRemoveTask(Task task) => networkTaskManager.RemoveTaskServerRpc(task.NetworkTask.NetworkObjectId);

    private void Awake()
    {
        networkTaskManager = GetComponent<NetworkTaskManager>();
    }

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

    public void SetAllTasksCompleted(bool isAllTasksCompleted)
    {
        this.isAllTasksCompleted = isAllTasksCompleted;
        if (isAllTasksCompleted) OnAllTasksCompleted?.Invoke();
    }

    public void RequestSpawnRandomTasks()
    {
        List<TaskData> taskDatas = GameDataManager.Instance.Tasks;
        for (int i = 0; i < taskCount; i++)
        {
            int randomTaskDataIndex = UnityEngine.Random.Range(0, taskDatas.Count);
            RequestSpawnTask(randomTaskDataIndex);
        }
    }

    public Task SpawnTask(int taskDataIndex)
    {
        TaskData taskData = GameDataManager.Instance.Tasks[taskDataIndex];

        List<GameObject> taskPrefabs = taskData.TaskPrefabs;
        int randomTaskIndex = UnityEngine.Random.Range(0, taskPrefabs.Count);
        GameObject spawnedTask = Instantiate(taskPrefabs[randomTaskIndex]);
        Task task = spawnedTask.GetComponent<Task>();
        task.NetworkTask.NetworkObject.Spawn();
        return task;
    }

    public void AddTask(Task task)
    {
        tasks.Add(task);
        task.OnTaskStateChanged += OnTaskStateChanged;
        RegenerateTaskInstructions();
    }

    public void RemoveTask(Task task)
    {
        tasks.Remove(task);
        task.OnTaskStateChanged -= OnTaskStateChanged;
        RegenerateTaskInstructions();
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
        Debug.Log($"INSTRUCTIONS: {instructions}");
        UIManager.Instance.TaskText.text = instructions;
    }

    public static Draggable SpawnDraggable(GameObject draggablePrefab)
    {
        GameObject spawnedDraggable = Instantiate(draggablePrefab);
        Draggable draggable = spawnedDraggable.GetComponent<Draggable>();
        draggable.NetworkUseable.NetworkObject.Spawn();
        return draggable;
    }
}