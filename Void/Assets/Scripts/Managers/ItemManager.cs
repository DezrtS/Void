using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ItemManager : NetworkSingleton<ItemManager>
{
    [ServerRpc(RequireOwnership = false)]
    public void RequestItemPickUpServerRpc(ulong itemNetworkObjectId, ulong playerNetworkObjectId)
    {
        NetworkObject itemNetworkObject = NetworkManager.SpawnManager.SpawnedObjects[itemNetworkObjectId];
        NetworkObject playerNetworkObject = NetworkManager.SpawnManager.SpawnedObjects[playerNetworkObjectId];

        if (itemNetworkObject != null && playerNetworkObject != null)
        {
            Item item = itemNetworkObject.GetComponent<Item>();
            Inventory inventory = playerNetworkObject.GetComponent<Inventory>();
            if (item != null && item.CanPickUp)
            {
                // Assign the ownership to the client who requested the pickup
                itemNetworkObject.ChangeOwnership(playerNetworkObject.OwnerClientId);
                item.PickUp();
                item.transform.parent = playerNetworkObject.transform;
                item.transform.SetLocalPositionAndRotation(Vector3.forward, Quaternion.identity);
                inventory.PickUp(item);
                ItemPickUpClientRpc(itemNetworkObjectId, playerNetworkObjectId);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestItemDropServerRpc(ulong itemNetworkObjectId, ulong playerNetworkObjectId)
    {
        NetworkObject itemNetworkObject = NetworkManager.SpawnManager.SpawnedObjects[itemNetworkObjectId];
        NetworkObject playerNetworkObject = NetworkManager.SpawnManager.SpawnedObjects[playerNetworkObjectId];

        if (itemNetworkObject != null && playerNetworkObject != null)
        {
            Item item = itemNetworkObject.GetComponent<Item>();
            Inventory inventory = playerNetworkObject.GetComponent<Inventory>();
            if (item != null && item.CanDrop)
            {
                item.transform.parent = null;
                item.Drop();
                inventory.Drop();
                ItemDropClientRpc(itemNetworkObjectId, playerNetworkObjectId);
            }
        }
    }

    [ClientRpc]
    private void ItemPickUpClientRpc(ulong itemNetworkObjectId, ulong playerNetworkObjectId)
    {
        NetworkObject itemNetworkObject = NetworkManager.SpawnManager.SpawnedObjects[itemNetworkObjectId];
        NetworkObject playerNetworkObject = NetworkManager.SpawnManager.SpawnedObjects[playerNetworkObjectId];


        if (itemNetworkObject != null && playerNetworkObject != null)
        {
            Item item = itemNetworkObject.GetComponent<Item>();
            if (item != null)
            {
                item.PickUp();
                item.transform.parent = playerNetworkObject.transform;
                item.transform.SetLocalPositionAndRotation(Vector3.forward, Quaternion.identity);
            }
        }
    }

    [ClientRpc]
    private void ItemDropClientRpc(ulong itemNetworkObjectId, ulong playerNetworkObjectId)
    {
        NetworkObject itemNetworkObject = NetworkManager.SpawnManager.SpawnedObjects[itemNetworkObjectId];
        NetworkObject playerNetworkObject = NetworkManager.SpawnManager.SpawnedObjects[playerNetworkObjectId];


        if (itemNetworkObject != null && playerNetworkObject != null)
        {
            Item item = itemNetworkObject.GetComponent<Item>();
            if (item != null)
            {
                item.Drop();
                item.transform.parent = null;
            }
        }
    }
}