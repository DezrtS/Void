using System;

public interface ISubtask : ICommand
{
    public delegate void SubtaskHandler(ISubtask subtask, bool completed);
    public event SubtaskHandler OnSubtaskCompletion;
    public event Action OnUpdateSubtaskInstructions;

    public Task Task { get; }
    public TaskData TaskData { get; }
    public SubtaskData SubtaskData { get; }
    public bool IsCompleted { get; }

    public abstract void Initialize(Task task);
    public abstract string GetSubtaskInstructions();
}