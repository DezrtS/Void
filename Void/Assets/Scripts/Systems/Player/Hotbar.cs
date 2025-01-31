using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class Hotbar : NetworkBehaviour
{
    [SerializeField] private Transform activeTransform;
    [SerializeField] private int hotbarCapacity;

    public delegate void ItemEventHandler(int index, Item item);
    public delegate void ItemSwitchEventHandler(int fromIndex, int toIndex);
    public event ItemEventHandler OnPickUpItem;
    public event ItemEventHandler OnDropItem;
    public event ItemSwitchEventHandler OnSwitchItem;

    private Item[] hotbar;
    private int selectedIndex;

    private bool isDragging = false;
    private Dragable dragable;

    private void Awake()
    {
        hotbar = new Item[hotbarCapacity];
    }

    public Item GetActiveItem()
    {
        return hotbar[selectedIndex];
    }

    public void SetEnabledActiveItem(bool enabled)
    {
        Item activeItem = hotbar[selectedIndex];
        if (activeItem != null)
        {
            activeItem.gameObject.SetActive(enabled);
        }
    }

    public void SwitchItem(int direction)
    {
        int newIndex = (selectedIndex + direction + hotbarCapacity) % hotbarCapacity;
        SwitchToItem(newIndex);
    }

    public void SwitchToItem(int index)
    {
        OnSwitchItem?.Invoke(selectedIndex, index);
        SwitchToItemClientServerSide(index);
        SwitchToItemServerRpc(index);
    }

    public void SwitchToItemClientServerSide(int index)
    {
        SetEnabledActiveItem(false);
        selectedIndex = index;
        SetEnabledActiveItem(true);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SwitchToItemServerRpc(int index, ServerRpcParams rpcParams = default)
    {
        if (!IsHost) SwitchToItemClientServerSide(index);
        ClientRpcParams clientRpcParams = GameMultiplayer.GenerateClientRpcParams(rpcParams);
        SwitchToItemClientRpc(index, clientRpcParams);
    }

    [ClientRpc(RequireOwnership = false)]
    public void SwitchToItemClientRpc(int index, ClientRpcParams clientRpcParams = default)
    {
        SwitchToItemClientServerSide(index);
    }

    public void PickUpItem(ItemData itemData)
    {
        Item newItem = ItemManager.SpawnItem(itemData);
        PickUpItem(newItem);
    }

    public void PickUpItem(Item item)
    {
        if (!item.CanPickUp()) return;

        if (GetActiveItem() == null)
        {
            if (!IsServer) PickUpItemClientServerSide(selectedIndex, item);
            PickUpItemServerRpc(selectedIndex, item.NetworkObjectId);
            return;
        }

        for (int i = 0; i < hotbarCapacity; i++)
        {
            if (hotbar[i] == null)
            {
                if (!IsServer) PickUpItemClientServerSide(i, item);
                PickUpItemServerRpc(i, item.NetworkObjectId);
                return;
            }
        }

        DropItem();
        if (!IsServer) PickUpItemClientServerSide(selectedIndex, item);
        PickUpItemServerRpc(selectedIndex, item.NetworkObjectId);
    }

    public void PickUpItemClientServerSide(int index, Item item)
    {
        item.PickUp();
        item.transform.parent = transform;
        item.transform.SetLocalPositionAndRotation(activeTransform.localPosition, Quaternion.identity);
        hotbar[index] = item;
        OnPickUpItem?.Invoke(index, item);

        if (selectedIndex != index) SwitchToItem(index);
    }

    [ServerRpc(RequireOwnership = false)]
    public void PickUpItemServerRpc(int index, ulong itemNetworkObjectId, ServerRpcParams rpcParams = default)
    {
        NetworkObject itemNetworkObject = NetworkManager.SpawnManager.SpawnedObjects[itemNetworkObjectId];
        Item item = itemNetworkObject.GetComponent<Item>();
        if (item.CanPickUp())
        {
            if (itemNetworkObject.OwnerClientId != rpcParams.Receive.SenderClientId)
            {
                itemNetworkObject.ChangeOwnership(rpcParams.Receive.SenderClientId);
            }

            ItemManager.CreateItemPickUpLog(rpcParams.Receive.SenderClientId, item);
            PickUpItemClientServerSide(index, item);

            ClientRpcParams clientRpcParams = GameMultiplayer.GenerateClientRpcParams(rpcParams);
            PickUpItemClientRpc(index, item.NetworkObjectId, clientRpcParams);
        }
        //else
        //{
        //    ItemManager.CreateSimpleEventLog("ItemPickUpFailed", $"{item.ItemData.Name} - IsPickedUp: {item.IsPickedUp} - CanPickUp: {item.CanPickUp()}");
        //}
    }

    [ClientRpc(RequireOwnership = false)]
    public void PickUpItemClientRpc(int index, ulong itemNetworkObjectId, ClientRpcParams clientRpcParams = default)
    {
        NetworkObject itemNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[itemNetworkObjectId];
        PickUpItemClientServerSide(index, itemNetworkObject.GetComponent<Item>());
    }

    public void DropItem()
    {
        DropItem(selectedIndex);
    }

    public void DropAllItems()
    {
        for (int i = 0; i < hotbarCapacity; i++)
        {
            if (hotbar[i] != null)
            {
                DropItem(i);
            }
        }
    }

    public void DropItem(int index)
    {
        Item item = hotbar[index];

        if (item == null) return;
        if (!item.CanDrop()) return;

        if (!IsServer) DropItemClientServerSide(index);
        DropItemServerRpc(index);
    }

    public void DropItemClientServerSide(int index)
    {
        Item item = hotbar[index];

        hotbar[index] = null;
        item.Drop();
        item.transform.parent = null;
        OnDropItem?.Invoke(index, item);
    }

    [ServerRpc(RequireOwnership = false)]
    public void DropItemServerRpc(int index, ServerRpcParams rpcParams = default)
    {
        Item item = hotbar[index];

        if (item.CanDrop())
        {
            ItemManager.CreateItemDropLog(rpcParams.Receive.SenderClientId, item);
            DropItemClientServerSide(index);

            ClientRpcParams clientRpcParams = GameMultiplayer.GenerateClientRpcParams(rpcParams);
            DropItemClientRpc(index, clientRpcParams);
        }
    }

    [ClientRpc(RequireOwnership = false)]
    public void DropItemClientRpc(int index, ClientRpcParams clientRpcParams = default)
    {
        DropItemClientServerSide(index);
    }

    public void StartDragging(Dragable dragable)
    {
        if (isDragging) return;

        if (!IsServer) StartDraggingClientServerSide(dragable);
        StartDraggingServerRpc(dragable.NetworkObjectId);
    }

    public void StopDragging()
    {
        if (!isDragging) return;

    }

    public void StartDraggingClientServerSide(Dragable dragable)
    {
        isDragging = true;
        dragable.Drag();
        dragable.transform.SetParent(transform);
    }

    [ServerRpc(RequireOwnership = false)]
    public void StartDraggingServerRpc(ulong networkObjectId, ServerRpcParams rpcParams = default)
    {
        if (isDragging) return;

        if (!IsHost)
        {
            NetworkObject networkObject = NetworkManager.SpawnManager.SpawnedObjects[networkObjectId];
            StartDraggingClientServerSide(networkObject.GetComponent<Dragable>());
        }

        ClientRpcParams clientRpcParams = GameMultiplayer.GenerateClientRpcParams(rpcParams);
        StartDraggingClientRpc(networkObjectId, clientRpcParams);
    }

    [ClientRpc(RequireOwnership = false)]
    public void StartDraggingClientRpc(ulong networkObjectId, ClientRpcParams clientRpcParams = default)
    {
        NetworkObject networkObject = NetworkManager.SpawnManager.SpawnedObjects[networkObjectId];
        StartDraggingClientServerSide(networkObject.GetComponent<Dragable>());
    }
}