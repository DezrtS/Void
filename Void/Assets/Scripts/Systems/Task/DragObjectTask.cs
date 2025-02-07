using System;
using Unity.Netcode;
using UnityEngine;

public class DragObjectTask : Task
{
    [SerializeField] private GameObject draggablePrefab;

    public event Action<Draggable> OnDraggable;

    public GameObject DraggablePrefab => draggablePrefab;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsServer) return;
        SpawnDraggableServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnDraggableServerRpc()
    {
        Draggable draggable = Draggable.SpawnDraggable(draggablePrefab);
        TaskObjectSpawnPoint taskObjectSpawnPoint = TaskManager.Instance.GetAvailableTaskObjectSpawnPoint(TaskObjectSpawnPoint.TaskObjectType.Draggable);
        if (taskObjectSpawnPoint != null)
        {
            draggable.transform.position = taskObjectSpawnPoint.transform.position;
            taskObjectSpawnPoint.AddObject(draggable.gameObject);
        }
        OnDraggable?.Invoke(draggable);

        SpawnItemClientRpc(draggable.NetworkObjectId);
    }

    [ClientRpc(RequireOwnership = false)]
    public void SpawnItemClientRpc(ulong networkObjectId)
    {
        NetworkObject networkObject = NetworkManager.SpawnManager.SpawnedObjects[networkObjectId];
        Draggable draggable = networkObject.GetComponent<Draggable>();
        OnDraggable?.Invoke(draggable);
    }
}
