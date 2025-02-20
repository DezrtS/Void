using System.Collections.Generic;
using UnityEngine;

public abstract class SubtaskData : ScriptableObject
{
    [SerializeField] private string subtaskName;
    [SerializeField] private string subtaskInstructions;
    [SerializeField] private bool displayCompletionState;

    public string SubtaskName => subtaskName;
    public string SubtaskInstructions => subtaskInstructions;
    public bool DisplayCompletionState => displayCompletionState;

    public abstract ISubtask CreateSubtaskInstance(Task task, TaskData taskData);
}

public abstract class Subtask : ISubtask
{
    protected Task task;
    protected TaskData taskData;
    protected SubtaskData subtaskData;
    protected bool isCompleted;

    public Task Task => task;
    public TaskData TaskData => taskData;
    public SubtaskData SubtaskData => subtaskData;
    public bool IsCompleted => isCompleted;

    public event ISubtask.SubtaskStateHandler OnSubtaskStateChanged;

    public virtual void Execute()
    {
        isCompleted = true;
        OnSubtaskStateChanged?.Invoke(this, isCompleted);
        RequestRegenerateTaskInstructions();
    }

    public Subtask(TaskData taskData, SubtaskData subtaskData)
    {
        this.taskData = taskData;
        this.subtaskData = subtaskData;
    }

    public virtual void Initialize(Task task)
    {
        this.task = task;
    }

    protected (string newInstructions, Dictionary<int, string> keys) GetFormatInstructions(string instructions)
    {
        if (subtaskData.DisplayCompletionState)
        {
            instructions = instructions.Insert(0, $"[{isCompleted}]");
        }

        int index = 0;
        string data = "";
        Dictionary<int, string> keys = new Dictionary<int, string>();

        for (int i = 0; i < instructions.Length; i++)
        {
            char c = instructions[i];
            if (c == '{')
            {
                index = i;
                i++;
                c = instructions[i];
                while (c != '}')
                {
                    data += c;
                    i++;
                    c = instructions[i];

                    if (i >= instructions.Length) break;
                }

                int count = i - index;
                instructions = instructions.Remove(index, count + 1);
                i = index;

                keys.Add(i, data);
            }
        }

        return (instructions, keys);
    }

    public abstract string GetSubtaskInstructions();

    public virtual void Undo()
    {
        isCompleted = false;
        OnSubtaskStateChanged?.Invoke(this, isCompleted);
        RequestRegenerateTaskInstructions();
    }

    public static void RequestRegenerateTaskInstructions()
    {
        if (TaskManager.Instance)
        {
            TaskManager.Instance.RegenerateTaskInstructions();
        }
        else
        {
            TaskManager.OnSingletonInitialized += (TaskManager taskManager) =>
            {
                taskManager.RegenerateTaskInstructions();
            };
        }
    }
}