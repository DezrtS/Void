using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class HotbarSlot
{
    public ItemData ItemData;
    public Item WorldItem;

    public HotbarSlot(ItemData itemData)
    {
        ItemData = itemData;
    }

    public HotbarSlot(Item item)
    {
        ItemData = item.ItemData;
        WorldItem = item;
    }
}

public class Inventory : NetworkBehaviour
{
    [SerializeField] private int hotbarCapacity;
    [SerializeField] private Transform activeTransform;
    private int selectedIndex;
    private HotbarSlot[] hotbar;
    private Dictionary<ResourceData, int> inventory;

    public delegate void ItemEventHandler(int index, Item item);
    public event ItemEventHandler OnPickUpItem;
    public event ItemEventHandler OnDropItem;
    public event ItemEventHandler OnSwitchItem;

    public delegate void ResourceEventHandler(ResourceData resource, int amount);
    public event ResourceEventHandler OnResourceEvent;

    public void Awake()
    {
        hotbar = new HotbarSlot[hotbarCapacity];
        inventory = new Dictionary<ResourceData, int>();
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            Item selectedItem = ActiveHotbarSlot()?.WorldItem;
            if (selectedItem)
            {
                selectedItem.StopUsing();
                RequestItemDropServerRpc(selectedItem.NetworkObjectId, new ServerRpcParams());
            }
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            if (!ActiveHotbarSlot()?.WorldItem)
            {
                Collider[] hits = Physics.OverlapSphere(transform.position, 1);
                foreach (Collider hit in hits)
                {
                    if (hit.transform.TryGetComponent(out Item item))
                    {
                        if (item.CanPickUp)
                        {
                            RequestItemPickUpServerRpc(item.NetworkObjectId, new ServerRpcParams());
                            return;
                        }
                    }
                }
            }
        }
    }

    public HotbarSlot ActiveHotbarSlot()
    {
        return hotbar[selectedIndex];
    }

    public void SwitchItem(int direction)
    {
        int newIndex = (selectedIndex + direction + hotbarCapacity) % hotbarCapacity;
        RequestSwitchItemServerRpc(selectedIndex, newIndex, new ServerRpcParams());
    }

    public void AddResource(ResourceData resource, int amount)
    {
        if (inventory.ContainsKey(resource))
        {
            inventory[resource] = inventory[resource] + amount;
        }
        else
        {
            inventory.Add(resource, amount);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestItemPickUpServerRpc(ulong itemNetworkObjectId, ServerRpcParams rcpParams = default)
    {
        NetworkObject itemNetworkObject = NetworkManager.SpawnManager.SpawnedObjects[itemNetworkObjectId];

        if (itemNetworkObject != null)
        {
            Item item = itemNetworkObject.GetComponent<Item>();
            if (item != null && item.CanPickUp && !item.IsPickedUp)
            {
                // Assign the ownership to the client who requested the pickup
                itemNetworkObject.ChangeOwnership(rcpParams.Receive.SenderClientId);
                ItemManager.Instance.CreateItemPickUpLog(rcpParams.Receive.SenderClientId, item);
                HandleItemPickUp(item);
                HandleItemPickUpClientRpc(itemNetworkObjectId);
            }
        }
    }

    [ClientRpc]
    public void HandleItemPickUpClientRpc(ulong itemNetworkObjectId)
    {
        NetworkObject itemNetworkObject = NetworkManager.SpawnManager.SpawnedObjects[itemNetworkObjectId];

        if (itemNetworkObject != null)
        {
            Item item = itemNetworkObject.GetComponent<Item>();
            if (item != null && item.CanPickUp)
            {
                HandleItemPickUp(item);
            }
        }
    }

    public void HandleItemPickUp(Item item)
    {
        item.PickUp();
        item.transform.parent = transform;
        item.transform.SetLocalPositionAndRotation(activeTransform.localPosition, Quaternion.identity);
        hotbar[selectedIndex] = new HotbarSlot(item);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestItemDropServerRpc(ulong itemNetworkObjectId, ServerRpcParams rcpParams = default)
    {
        NetworkObject itemNetworkObject = NetworkManager.SpawnManager.SpawnedObjects[itemNetworkObjectId];

        if (itemNetworkObject != null)
        {
            Item item = itemNetworkObject.GetComponent<Item>();
            if (item != null && item.CanDrop)
            {
                // Assign the ownership to the client who requested the pickup
                itemNetworkObject.ChangeOwnership(rcpParams.Receive.SenderClientId);
                ItemManager.Instance.CreateItemDropLog(rcpParams.Receive.SenderClientId, item);
                HandleItemDrop(item);
                HandleItemDropClientRpc(itemNetworkObjectId);
            }
        }
    }

    [ClientRpc]
    public void HandleItemDropClientRpc(ulong itemNetworkObjectId)
    {
        NetworkObject itemNetworkObject = NetworkManager.SpawnManager.SpawnedObjects[itemNetworkObjectId];

        if (itemNetworkObject != null)
        {
            Item item = itemNetworkObject.GetComponent<Item>();
            if (item != null && item.CanDrop && !item.IsDropped)
            {
                HandleItemDrop(item);
            }
        }
    }

    public void HandleItemDrop(Item item)
    {
        hotbar[selectedIndex] = null;
        item.Drop();
        item.transform.parent = null;
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestSwitchItemServerRpc(int fromIndex, int toIndex, ServerRpcParams rcpParams = default)
    {

        HandleSwitchItem(fromIndex, toIndex);
        HandleSwitchItemClientRpc(fromIndex, toIndex);
    }

    public void HandleSwitchItem(int fromIndex, int toIndex)
    {
        HotbarSlot from = hotbar[fromIndex];
        HotbarSlot to = hotbar[toIndex];

        if (from?.WorldItem)
        {
            from.WorldItem.gameObject.SetActive(false);
        }

        if (to?.WorldItem)
        {
            to.WorldItem.gameObject.SetActive(true);
        }

        selectedIndex = toIndex;
    }

    [ClientRpc]
    public void HandleSwitchItemClientRpc(int fromIndex, int toIndex)
    {
        HandleSwitchItem(fromIndex, toIndex);
    }
}