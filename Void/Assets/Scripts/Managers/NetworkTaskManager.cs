using Unity.Netcode;
using UnityEngine;

public class NetworkTaskManager : NetworkBehaviour
{
    private TaskManager taskManager;
    private readonly NetworkVariable<bool> isAllTasksCompleted = new();

    private void Awake()
    {
        taskManager = GetComponent<TaskManager>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        isAllTasksCompleted.OnValueChanged += OnAllTasksCompletedStateChanged;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        isAllTasksCompleted.OnValueChanged -= OnAllTasksCompletedStateChanged;
    }

    private void OnAllTasksCompletedStateChanged(bool oldValue, bool newValue)
    {
        if (oldValue == newValue) return;
        taskManager.SetAllTasksCompleted(newValue);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetAllTasksCompletedStateServerRpc(bool isCompleted)
    {
        if (isAllTasksCompleted.Value == isCompleted) return;
        isAllTasksCompleted.Value = isCompleted;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnTaskServerRpc(int taskDataIndex)
    {
        Task task = taskManager.SpawnTask(taskDataIndex);
        taskManager.RequestAddTask(task);
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddTaskServerRpc(ulong taskNetworkObjectId)
    {
        NetworkObject taskNetworkObject = GetNetworkObject(taskNetworkObjectId);
        Task task = taskNetworkObject.GetComponent<Task>();
        if (taskManager.Tasks.Contains(task)) return;
        AddTaskClientRpc(taskNetworkObjectId);
    }

    [ClientRpc(RequireOwnership = false)]
    public void AddTaskClientRpc(ulong taskNetworkObjectId)
    {
        NetworkObject taskNetworkObject = GetNetworkObject(taskNetworkObjectId);
        Task task = taskNetworkObject.GetComponent<Task>();
        taskManager.AddTask(task);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RemoveTaskServerRpc(ulong taskNetworkObjectId)
    {
        NetworkObject taskNetworkObject = GetNetworkObject(taskNetworkObjectId);
        Task task = taskNetworkObject.GetComponent<Task>();
        if (!taskManager.Tasks.Contains(task)) return;
        RemoveTaskClientRpc(taskNetworkObjectId);
    }

    [ClientRpc(RequireOwnership = false)]
    public void RemoveTaskClientRpc(ulong taskNetworkObjectId)
    {
        NetworkObject taskNetworkObject = GetNetworkObject(taskNetworkObjectId);
        Task task = taskNetworkObject.GetComponent<Task>();
        taskManager.RemoveTask(task);
    }
}