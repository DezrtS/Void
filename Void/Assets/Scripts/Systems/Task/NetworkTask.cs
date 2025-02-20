using Unity.Netcode;

public class NetworkTask : NetworkBehaviour
{
    protected Task task;
    private readonly NetworkVariable<bool> isCompleted = new();
    private readonly NetworkList<bool> subtaskStates = new();

    private void Awake()
    {
        OnTaskInitialize();
    }

    public virtual void OnTaskInitialize()
    {
        task = GetComponent<Task>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        OnTaskSpawn();
    }

    public virtual void OnTaskSpawn()
    {
        if (IsServer)
        {
            for (int i = 0; i < task.TaskData.Subtasks.Count; i++)
            {
                subtaskStates.Add(false);
            }
        }

        isCompleted.OnValueChanged += OnCompletionStateChanged;
        subtaskStates.OnListChanged += OnSubtaskStatesChanged;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        OnTaskDespawn();
    }

    public virtual void OnTaskDespawn()
    {
        isCompleted.OnValueChanged -= OnCompletionStateChanged;
        subtaskStates.OnListChanged -= OnSubtaskStatesChanged;
    }

    private void OnCompletionStateChanged(bool oldValue, bool newValue)
    {
        if (oldValue == newValue) return;
        task.UpdateTaskState(newValue);
    }

    private void OnSubtaskStatesChanged(NetworkListEvent<bool> networkListEvent)
    {

    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateTaskStateServerRpc(bool state)
    {
        if (isCompleted.Value == state) return;
        isCompleted.Value = state;
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateSubtaskStateServerRpc(int index, bool state)
    {
        if (subtaskStates[index] == state) return;
        subtaskStates[index] = state;
    }
}