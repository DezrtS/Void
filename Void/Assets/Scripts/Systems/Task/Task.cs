using UnityEngine;

public abstract class Task : MonoBehaviour
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
}