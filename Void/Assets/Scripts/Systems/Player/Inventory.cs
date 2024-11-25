using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Inventory : NetworkBehaviour
{
    [SerializeField] private int hotbarCapacity;
    [SerializeField] private Transform activeTransform;
    [SerializeField] private Transform[] storageTransforms;
    private int selectedIndex;
    private Item[] hotbar;
    private Dictionary<ResourceData, int> inventory;

    public delegate void ItemEventHandler(int index, Item item);
    public event ItemEventHandler OnPickUpItem;
    public event ItemEventHandler OnDropItem;
    public event ItemEventHandler OnSwitchItem;

    public delegate void ResourceEventHandler(ResourceData resource, int amount);
    public event ResourceEventHandler OnResourceEvent;

    public void Awake()
    {
        hotbar = new Item[hotbarCapacity];
        inventory = new Dictionary<ResourceData, int>();
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            Item item = SelectedItem();
            if (item)
            {
                RequestItemDropServerRpc(item.NetworkObjectId, new ServerRpcParams());
            }
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            if (!SelectedItem())
            {
                RaycastHit[] hits = Physics.SphereCastAll(transform.position, 5, transform.forward);
                foreach (RaycastHit hit in hits)
                {
                    if (hit.transform.TryGetComponent(out Item item)) {
                        RequestItemPickUpServerRpc(item.NetworkObjectId, new ServerRpcParams());
                    }
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.F) && SelectedItem())
        {
            SelectedItem().Use();
        }
        else if (Input.GetKeyUp(KeyCode.F) && SelectedItem())
        {
            SelectedItem().StopUsing();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SwitchItem(true);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SwitchItem(false);
        }
    }

    public void PickUp(Item item)
    {
        hotbar[selectedIndex] = item;
    }

    public void Drop()
    {
        hotbar[selectedIndex] = null;
    }

    public Item SelectedItem()
    {
        return hotbar[selectedIndex];
    }

    public Item SwitchItem(bool left)
    {
        int newIndex = selectedIndex;
        newIndex += left ? -1 : 1;

        if (newIndex < 0)
        {
            newIndex = hotbarCapacity - 1;
        }
        else if (newIndex >= hotbarCapacity)
        {
            newIndex = 0;
        }

        SwapAndStore(selectedIndex, newIndex);
        selectedIndex = newIndex;

        return hotbar[selectedIndex];
    }

    public Item SwitchItem(int index)
    {
        if (index >= 0 && index < hotbarCapacity && index != selectedIndex)
        {
            SwapAndStore(selectedIndex, index);
            selectedIndex = index;
            return hotbar[selectedIndex];
        }
        return null;
    }

    public void SwapAndStore(int fromIndex, int toIndex)
    {
        Item from = hotbar[fromIndex];
        Item to = hotbar[toIndex];

        if (from)
        {
            if (storageTransforms.Length >= fromIndex)
            {
                from.transform.parent = storageTransforms[fromIndex];
                from.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            }
            else
            {
                from.GetComponent<MeshRenderer>().enabled = false;
            }
        }

        if (to)
        {
            if (storageTransforms.Length >= toIndex)
            {
                to.transform.parent = activeTransform;
                to.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            }
            else
            {
                to.GetComponent<MeshRenderer>().enabled = true;
            }
        }
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
            if (item != null && item.CanPickUp)
            {
                // Assign the ownership to the client who requested the pickup
                itemNetworkObject.ChangeOwnership(rcpParams.Receive.SenderClientId);
                ItemManager.Instance.CreateItemPickUpLog(rcpParams.Receive.SenderClientId, item);
                HandleItemPickUp(item);
                PickUp(item);
                HandleItemPickUpClientRpc(itemNetworkObjectId, rcpParams.Receive.SenderClientId);
            }
        }
    }

    [ClientRpc]
    public void HandleItemPickUpClientRpc(ulong itemNetworkObjectId, ulong clientId)
    {
        NetworkObject itemNetworkObject = NetworkManager.SpawnManager.SpawnedObjects[itemNetworkObjectId];

        if (itemNetworkObject != null)
        {
            Item item = itemNetworkObject.GetComponent<Item>();
            if (item != null && item.CanPickUp)
            {
                HandleItemPickUp(item);
                if (NetworkManager.Singleton.LocalClientId == clientId)
                {
                    PickUp(item);
                }
            }
        }
    }

    public void HandleItemPickUp(Item item)
    {
        item.PickUp();
        item.transform.parent = transform;
        item.transform.SetLocalPositionAndRotation(Vector3.forward, Quaternion.identity);
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
                Drop();
                HandleItemDropClientRpc(itemNetworkObjectId, rcpParams.Receive.SenderClientId);
            }
        }
    }

    [ClientRpc]
    public void HandleItemDropClientRpc(ulong itemNetworkObjectId, ulong clientId)
    {
        NetworkObject itemNetworkObject = NetworkManager.SpawnManager.SpawnedObjects[itemNetworkObjectId];

        if (itemNetworkObject != null)
        {
            Item item = itemNetworkObject.GetComponent<Item>();
            if (item != null && item.CanDrop)
            {
                HandleItemDrop(item);
                if (NetworkManager.Singleton.LocalClientId == clientId)
                {
                    Drop();
                }
            }
        }
    }

    public void HandleItemDrop(Item item)
    {
        item.Drop();
        item.transform.parent = null;
    }
}