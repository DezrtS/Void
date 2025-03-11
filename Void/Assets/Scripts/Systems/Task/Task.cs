using UnityEngine;

public abstract class Task : MonoBehaviour
{
    public delegate void TaskStateHandler(Task task, bool completed);
    public event TaskStateHandler OnTaskStateChanged;

    [SerializeField] private TaskData taskData;

    private NetworkTask networkTask;

    private bool isCompleted;
    protected ISubtask[] subtasks;

    public TaskData TaskData => taskData;
    public NetworkTask NetworkTask => networkTask;
    public bool IsCompleted => isCompleted;

    public void RequestUpdateTaskState(bool state) => networkTask.UpdateTaskStateServerRpc(state);
    public void RequestUpdateSubtaskState(int index, bool state) => networkTask.UpdateSubtaskStateServerRpc(index, state);
    

    private void Awake()
    {
        networkTask = GetComponent<NetworkTask>();

        subtasks = new ISubtask[taskData.Subtasks.Count];
        for (int i = 0; i < subtasks.Length; i++)
        {
            subtasks[i] = taskData.Subtasks[i].CreateSubtaskInstance(this, taskData);
            subtasks[i].OnSubtaskStateChanged += OnSubtaskStateChanged;
        }
    }

    public string GetInstructions()
    {
        string instructions = $"{taskData.TaskName}\n";
        foreach (ISubtask subtask in subtasks)
        {
            instructions += $" - {subtask.GetSubtaskInstructions()}\n";
        }
        return instructions;
    }

    public void OnSubtaskStateChanged(ISubtask subtask, bool completed)
    {
        for (int i = 0; i < subtasks.Length; i++)
        {
            if (subtask == subtasks[i])
            {
                RequestUpdateSubtaskState(i, completed);
            }
        }

        if (networkTask.IsServer)
        {
            if (!isCompleted && completed) RequestUpdateTaskState(IsTaskComplete());
            else if (isCompleted && !completed) RequestUpdateTaskState(IsTaskComplete());
        }
    }

    public void UpdateTaskState(bool completed)
    {
        isCompleted = completed;
        OnTaskStateChanged?.Invoke(this, completed);
        TaskManager.Instance.RegenerateTaskInstructions();
    }

    public bool IsTaskComplete()
    {
        foreach (ISubtask subtask in subtasks)
        {
            if (!subtask.IsCompleted) return false;
        }
        return true;
    }
}