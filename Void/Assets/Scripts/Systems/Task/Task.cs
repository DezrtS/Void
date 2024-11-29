using Unity.Netcode;
using UnityEngine;

public abstract class Task : NetworkBehaviour
{
    [SerializeField] private TaskData task;
    private bool[] completedSubtasks;

    public TaskData Data {  get { return task; } } 

    public delegate void TaskProgressHandler(Task task);

    public event TaskProgressHandler OnTaskCompletion;
    public event TaskProgressHandler OnSubTaskCompletion;

    private void Awake()
    {
        completedSubtasks = new bool[task.Subtasks.Count];
    }

    private void Start()
    {
        if (!IsServer) return;
        TaskManager.Instance.AddTask(this);
    }

    public virtual void CompleteTask()
    {
        OnTaskCompletion?.Invoke(this);
    }

    public virtual void CompleteSubtask(int id)
    {
        for (int i = 0; i < task.Subtasks.Count; i++)
        {
            if (task.Subtasks[i].Id == id)
            {
                if (completedSubtasks[i])
                {
                    return;
                }

                completedSubtasks[i] = true;
            }
        }
        if (IsTaskIsComplete())
        {
            CompleteTask();
        }
        else
        {
            OnSubTaskCompletion?.Invoke(this);
        }
    }

    public bool IsTaskIsComplete()
    {
        foreach (bool complete in completedSubtasks)
        {
            if (!complete)
            {
                return false;
            }
        }
        return true;
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestCompleteTaskServerRpc(ServerRpcParams rcpParams = default)
    {
        HandleCompleteTask();
        HandleCompleteTaskClientRpc();
    }

    public void HandleCompleteTask()
    {
        CompleteTask();
    }

    [ClientRpc]
    public void HandleCompleteTaskClientRpc()
    {
        HandleCompleteTask();
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestCompleteSubTaskServerRpc(int subtaskId, ServerRpcParams rcpParams = default)
    {
        HandleCompleteSubTask(subtaskId);
        HandleCompleteSubTaskClientRpc(subtaskId);
    }

    public void HandleCompleteSubTask(int subtaskId)
    {
        CompleteSubtask(subtaskId);
    }

    [ClientRpc]
    public void HandleCompleteSubTaskClientRpc(int subtaskId)
    {
        HandleCompleteSubTask(subtaskId);
    }
}