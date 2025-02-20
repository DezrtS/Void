public interface ISubtask : ICommand
{
    public delegate void SubtaskStateHandler(ISubtask subtask, bool completed);
    public event SubtaskStateHandler OnSubtaskStateChanged;

    public Task Task { get; }
    public TaskData TaskData { get; }
    public SubtaskData SubtaskData { get; }
    public bool IsCompleted { get; }

    public abstract void Initialize(Task task);
    public abstract string GetSubtaskInstructions();
}