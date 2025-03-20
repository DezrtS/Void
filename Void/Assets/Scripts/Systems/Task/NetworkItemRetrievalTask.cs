using Unity.Netcode;

public class NetworkItemRetrievalTask : NetworkTask
{
    private ItemRetrievalTask itemRetrievalTask;

    public override void OnTaskInitialize()
    {
        base.OnTaskInitialize();
        itemRetrievalTask = task as ItemRetrievalTask;
    }

    public override void OnTaskSpawn()
    {
        base.OnTaskSpawn();
        if (IsServer) SpawnItemServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnItemServerRpc()
    {
        Item item = GameDataManager.SpawnItem(itemRetrievalTask.ItemData);
        item.transform.position = SpawnManager.Instance.GetRandomSpawnpointPosition(Spawnpoint.SpawnpointType.Item);
        SpawnItemClientRpc(item.NetworkItem.NetworkObjectId);
    }

    [ClientRpc(RequireOwnership = false)]
    public void SpawnItemClientRpc(ulong itemNetworkObjectId)
    {
        NetworkObject itemNetworkObject = NetworkManager.SpawnManager.SpawnedObjects[itemNetworkObjectId];
        Item item = itemNetworkObject.GetComponent<Item>();
        itemRetrievalTask.SpawnItem(item);
    }
}