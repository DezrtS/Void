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
        draggable.transform.position = SpawnManager.Instance.GetRandomSpawnpointPosition(Spawnpoint.SpawnpointType.Draggable);
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
