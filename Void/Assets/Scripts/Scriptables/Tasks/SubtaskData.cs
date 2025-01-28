using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class SubtaskData : ScriptableObject
{
    [SerializeField] private string subtaskName;
    [SerializeField] private string subtaskInstructions;

    public string SubtaskName => subtaskName;
    public string SubtaskInstructions => subtaskInstructions;

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

    public event ISubtask.SubtaskHandler OnSubtaskCompletion;
    public event Action OnUpdateSubtaskInstructions;

    public virtual void Execute()
    {
        Debug.Log("TASK COMPLETED");
        isCompleted = true;
        OnSubtaskCompletion?.Invoke(this, isCompleted);
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
                instructions = instructions.Remove(index, count);
                i = index;

                keys.Add(i, data);
            }
        }

        return (instructions, keys);
    }

    protected void UpdateSubtaskInstructions()
    {
        OnUpdateSubtaskInstructions?.Invoke();
    }

    public abstract string GetSubtaskInstructions();

    public virtual void Undo()
    {
        isCompleted = false;
        OnSubtaskCompletion?.Invoke(this, isCompleted);
    }
}