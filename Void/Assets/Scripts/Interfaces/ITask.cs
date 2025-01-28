using System;
using UnityEngine;

public interface ITask : ICommand
{
    public delegate void SubtaskHandler(ITask task, bool completed);
    public event SubtaskHandler OnSubtaskCompletion;
    public event Action OnUpdateSubtaskInstructions;

    public Task Task { get; }
    public TaskData TaskData { get; }
    public SubtaskData SubtaskData { get; }
    public bool IsCompleted { get; }

    public abstract void Initialize(Task task);
    public abstract string GetSubtaskInstructions();
}