using System;
using Unity.Netcode;
using UnityEngine;

public class ItemRetrievalTask : Task
{
    [SerializeField] private ItemData itemData;

    public event Action<Item> OnItem;

    public ItemData ItemData => itemData;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsServer) return;
        SpawnItemServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnItemServerRpc()
    {
        Item item = ItemManager.SpawnItem(itemData);
        OnItem?.Invoke(item);

        SpawnItemClientRpc(item.NetworkObjectId);
    }

    [ClientRpc(RequireOwnership = false)]
    public void SpawnItemClientRpc(ulong networkObjectId)
    {
        NetworkObject networkObject = NetworkManager.SpawnManager.SpawnedObjects[networkObjectId];
        Item item = networkObject.GetComponent<Item>();
        OnItem?.Invoke(item);
    }



}
