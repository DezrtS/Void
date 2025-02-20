using Unity.Netcode;

public class NetworkDragObjectTask : NetworkTask
{
    private DragObjectTask dragObjectTask;

    public override void OnTaskInitialize()
    {
        base.OnTaskInitialize();
        dragObjectTask = task as DragObjectTask;
    }

    public override void OnTaskSpawn()
    {
        base.OnTaskSpawn();
        if (IsServer) SpawnDraggableServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnDraggableServerRpc()
    {
        Draggable draggable = TaskManager.SpawnDraggable(dragObjectTask.DraggablePrefab);
        draggable.transform.position = SpawnManager.Instance.GetRandomSpawnpointPosition(Spawnpoint.SpawnpointType.Draggable);
        SpawnDraggableClientRpc(draggable.NetworkUseable.NetworkObjectId);
    }

    [ClientRpc(RequireOwnership = false)]
    public void SpawnDraggableClientRpc(ulong draggableNetworkObjectId)
    {
        NetworkObject draggableNetworkObject = NetworkManager.SpawnManager.SpawnedObjects[draggableNetworkObjectId];
        Draggable draggable = draggableNetworkObject.GetComponent<Draggable>();
        dragObjectTask.SpawnDraggable(draggable);
    }
}