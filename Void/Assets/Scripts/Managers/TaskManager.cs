using System;
using System.Collections.Generic;
using UnityEngine;

public class TaskManager : Singleton<TaskManager>
{
    public static event Action OnAllTasksCompleted;

    [SerializeField] private int taskCount = 1;
    private int tasksLeft;

    private NetworkTaskManager networkTaskManager;
    private bool isAllTasksCompleted;

    private List<Task> tasks = new();

    public int TaskCount => taskCount;
    public NetworkTaskManager NetworkTaskManager => networkTaskManager;
    public bool IsAllTasksCompleted => isAllTasksCompleted;
    public List<Task> Tasks => tasks;

    public void RequestSetTasksLeft(int tasksLeft) => networkTaskManager.SetTasksLeftServerRpc(tasksLeft);
    public void RequestSetAllTasksCompletedState(bool isCompleted) => networkTaskManager.SetAllTasksCompletedStateServerRpc(isCompleted);
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
            if (task == null) continue;
            task.OnTaskStateChanged += OnTaskStateChanged;
        }
    }

    private void OnDisable()
    {
        foreach (Task task in tasks)
        {
            if (task == null) continue;
            task.OnTaskStateChanged -= OnTaskStateChanged;
        }
    }

    public bool CheckAllTasksCompleted()
    {
        int tasksLeft = 0;
        for (int i = 0; i < tasks.Count; i++)
        {
            if (!tasks[i].IsCompleted)
            {
                tasksLeft++;
            }
        }

        RequestSetTasksLeft(tasksLeft);
        return tasksLeft == 0;
    }

    public void SetTasksLeft(int tasksLeft)
    {
        this.tasksLeft = tasksLeft;
        if (!networkTaskManager.IsServer) return;
        switch (tasksLeft)
        {
            case 5:
                AudioManager.RequestPlayDialogue(DialogueManager.Instance.FiveTasksLeft);
                break;
            case 4:
                AudioManager.RequestPlayDialogue(DialogueManager.Instance.FourTasksLeft);
                break;
            case 3:
                AudioManager.RequestPlayDialogue(DialogueManager.Instance.ThreeTasksLeft);
                break;
            case 2:
                AudioManager.RequestPlayDialogue(DialogueManager.Instance.TwoTasksLeft);
                break;
            case 1:
                AudioManager.RequestPlayDialogue(DialogueManager.Instance.OneTaskLeft);
                break;
        }
    }

    public void SetAllTasksCompleted(bool isAllTasksCompleted)
    {
        this.isAllTasksCompleted = isAllTasksCompleted;
        if (isAllTasksCompleted)
        {
            AudioManager.PlayDialogue(DialogueManager.Instance.AllTasksComplete);
            OnAllTasksCompleted?.Invoke();
        }
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
        task.NetworkTask.NetworkObject.Spawn(true);
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

        if (networkTaskManager.IsServer)
        {
            if (state)
            {
                RequestSetAllTasksCompletedState(CheckAllTasksCompleted());
            }
        }
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
        draggable.NetworkUseable.NetworkObject.Spawn(true);
        return draggable;
    }
}