using Unity.Netcode;
using UnityEngine;

public abstract class Task : NetworkBehaviour
{
    [SerializeField] private TaskData taskData;

    protected ISubtask[] subtasks;

    public TaskData TaskData {  get { return taskData; } } 

    public delegate void TaskProgressHandler(Task task);

    public event TaskProgressHandler OnTaskCompletion;
    public event TaskProgressHandler OnSubTaskCompletion;

    private void Awake()
    {
        subtasks = new ISubtask[taskData.Subtasks.Count];
        for (int i = 0; i < subtasks.Length; i++)
        {
            subtasks[i] = taskData.Subtasks[i].CreateSubtaskInstance(this, taskData);
            subtasks[i].OnUpdateSubtaskInstructions += () =>
            {
                TaskList.Instance.AddTask(this);
            };
        }

        //completedSubtasks = new bool[taskData.Subtasks.Count];
    }

    private void Start()
    {
        TaskManager.Instance.AddTask(this);
    }

    public string GetInstructions()
    {
        string instructions = $"{taskData.TaskName} - {taskData.TaskInstructions}\n";
        foreach (ISubtask subtask in subtasks)
        {
            instructions += subtask.GetSubtaskInstructions() + "\n\n";
        }
        return instructions;
    }
}