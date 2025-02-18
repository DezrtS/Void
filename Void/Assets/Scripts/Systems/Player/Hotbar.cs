using Unity.Netcode;
using UnityEngine;

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
    private Draggable draggable;

    private PlayerLook playerLook;

    public int SelectedIndex => selectedIndex;
    public bool IsDragging => isDragging;

    private void Awake()
    {
        hotbar = new Item[hotbarCapacity];

        TryGetComponent(out playerLook);
    }

    private void Update()
    {
        if (!IsOwner) return;
        Item item = GetItem();
        if (item != null)
        {
            item.transform.SetPositionAndRotation(activeTransform.position, activeTransform.rotation);
        }
    }

    public Item GetItem()
    {
        return GetItem(selectedIndex);
    }

    public Item GetItem(int index)
    {
        return hotbar[index];
    }

    public void SetEnabledItem(bool enabled)
    {
        SetEnabledItem(selectedIndex, enabled);
    }

    public void SetEnabledItem(int index, bool enabled)
    {
        Item activeItem = hotbar[index];
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
        SetEnabledItem(false);
        SetEnabledItem(index, true);
        selectedIndex = index;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SwitchToItemServerRpc(int index, ServerRpcParams rpcParams = default)
    {
        if (!IsServer) SwitchToItemClientServerSide(index);
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
        if (item.PickUp())
        {
            if (GetItem() == null)
            {
                if (!IsServer) PickUpItemClientSide(selectedIndex, item);
                PickUpItemServerRpc(selectedIndex, item.NetworkObjectId);
                return;
            }

            for (int i = 0; i < hotbarCapacity; i++)
            {
                if (hotbar[i] == null)
                {
                    if (!IsServer) PickUpItemClientSide(i, item);
                    PickUpItemServerRpc(i, item.NetworkObjectId);
                    return;
                }
            }

            DropItem();
            if (!IsServer) PickUpItemClientSide(selectedIndex, item);
            PickUpItemServerRpc(selectedIndex, item.NetworkObjectId);
        }
    }

    public void PickUpItemClientSide(int index, Item item)
    {
        PickUpItemClientServerSide(index, item);
    }

    public void PickUpItemServerSide(int index, Item item)
    {
        PickUpItemClientServerSide(index, item);
        item.OnRequestForceDrop += OnRequestForceDrop;
        //item.transform.parent = transform;
        item.transform.SetLocalPositionAndRotation(activeTransform.localPosition, Quaternion.identity);
    }

    public void PickUpItemClientServerSide(int index, Item item)
    {
        hotbar[index] = item;
        OnPickUpItem?.Invoke(index, item);
        if (selectedIndex != index) SwitchToItem(index);
    }

    [ServerRpc(RequireOwnership = false)]
    public void PickUpItemServerRpc(int index, ulong itemNetworkObjectId, ServerRpcParams rpcParams = default)
    {
        NetworkObject itemNetworkObject = NetworkManager.SpawnManager.SpawnedObjects[itemNetworkObjectId];
        Item item = itemNetworkObject.GetComponent<Item>();
        if (itemNetworkObject.OwnerClientId != rpcParams.Receive.SenderClientId)
        {
            Debug.Log("Changed Ownership");
            itemNetworkObject.ChangeOwnership(rpcParams.Receive.SenderClientId);
        }

        ItemManager.CreateItemPickUpLog(rpcParams.Receive.SenderClientId, item);
        PickUpItemServerSide(index, item);

        ClientRpcParams clientRpcParams = GameMultiplayer.GenerateClientRpcParams(rpcParams);
        PickUpItemClientRpc(index, item.NetworkObjectId, clientRpcParams);
    }

    [ClientRpc(RequireOwnership = false)]
    public void PickUpItemClientRpc(int index, ulong itemNetworkObjectId, ClientRpcParams clientRpcParams = default)
    {
        NetworkObject itemNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[itemNetworkObjectId];
        PickUpItemClientSide(index, itemNetworkObject.GetComponent<Item>());
    }

    private void OnRequestForceDrop(Item item)
    {
        item.OnRequestForceDrop -= OnRequestForceDrop;
        DropItem(item);
    }

    public void DropItem()
    {
        DropItem(selectedIndex);
    }

    public void DropItem(Item item)
    {
        for (int i = 0; i < hotbar.Length; i++)
        {
            if (hotbar[i] == item)
            {
                DropItem(i);
                return;
            }
        }
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
        
        if (item.Drop())
        {
            if (!IsServer) DropItemClientSide(index);
            DropItemServerRpc(index);
        }
    }

    public void DropItemClientSide(int index)
    {
        DropItemClientServerSide(index);
    }

    public void DropItemServerSide(int index)
    {
        Item item = hotbar[index];
        item.transform.parent = null;

        DropItemClientServerSide(index);
    }

    public void DropItemClientServerSide(int index)
    {
        Item item = hotbar[index];
        SetEnabledItem(index, true);

        hotbar[index] = null;
        
        OnDropItem?.Invoke(index, item);
    }

    [ServerRpc(RequireOwnership = false)]
    public void DropItemServerRpc(int index, ServerRpcParams rpcParams = default)
    {
        Item item = hotbar[index];

        ItemManager.CreateItemDropLog(rpcParams.Receive.SenderClientId, item);
        DropItemServerSide(index);

        ClientRpcParams clientRpcParams = GameMultiplayer.GenerateClientRpcParams(rpcParams);
        DropItemClientRpc(index, clientRpcParams);
    }

    [ClientRpc(RequireOwnership = false)]
    public void DropItemClientRpc(int index, ClientRpcParams clientRpcParams = default)
    {
        DropItemClientSide(index);
    }

    public void StartDragging(Draggable draggable)
    {
        if (isDragging) return;

        if (!IsServer) StartDraggingClientServerSide(draggable);
        StartDraggingServerRpc(draggable.NetworkObjectId);
    }

    public void StartDraggingClientServerSide(Draggable draggable)
    {
        isDragging = true;
        this.draggable = draggable;
        draggable.Drag();
        if (TryGetComponent(out Rigidbody rig)) 
        {
            draggable.AttachRigidbody(rig);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void StartDraggingServerRpc(ulong networkObjectId, ServerRpcParams rpcParams = default)
    {
        if (isDragging) return;

        NetworkObject networkObject = NetworkManager.SpawnManager.SpawnedObjects[networkObjectId];
        StartDraggingClientServerSide(networkObject.GetComponent<Draggable>());

        ClientRpcParams clientRpcParams = GameMultiplayer.GenerateClientRpcParams(rpcParams);
        StartDraggingClientRpc(networkObjectId, clientRpcParams);
    }

    [ClientRpc(RequireOwnership = false)]
    public void StartDraggingClientRpc(ulong networkObjectId, ClientRpcParams clientRpcParams = default)
    {
        NetworkObject networkObject = NetworkManager.SpawnManager.SpawnedObjects[networkObjectId];
        StartDraggingClientServerSide(networkObject.GetComponent<Draggable>());
    }

    public void StopDragging()
    {
        if (!isDragging) return;
        if (draggable == null) return;

        if (!IsServer) StopDraggingClientServerSide();
        StopDraggingServerRpc();
    }

    public void StopDraggingClientServerSide()
    {
        isDragging = false;
        draggable.StopDragging();
        draggable.DetachRigidbody();
        draggable = null;
    }

    [ServerRpc(RequireOwnership = false)]
    public void StopDraggingServerRpc(ServerRpcParams rpcParams = default)
    {
        if (!isDragging) return;
        if (draggable == null) return;

        StopDraggingClientServerSide();

        ClientRpcParams clientRpcParams = GameMultiplayer.GenerateClientRpcParams(rpcParams);
        StopDraggingClientRpc(clientRpcParams);
    }

    [ClientRpc(RequireOwnership = false)]
    public void StopDraggingClientRpc(ClientRpcParams clientRpcParams = default)
    {
        StopDraggingClientServerSide();
    }
}