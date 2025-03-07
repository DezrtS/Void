using Unity.Netcode;
using UnityEngine;

public class NetworkHotbar : NetworkBehaviour
{
    private Hotbar hotbar;
    private readonly NetworkVariable<int> selectedIndex = new();
    private readonly NetworkVariable<bool> isDragging = new();

    private void Awake()
    {
        hotbar = GetComponent<Hotbar>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        selectedIndex.OnValueChanged += OnSelectedIndexChanged;
        isDragging.OnValueChanged += OnDraggingStateChanged;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        selectedIndex.OnValueChanged -= OnSelectedIndexChanged;
        isDragging.OnValueChanged -= OnDraggingStateChanged;
    }

    private void OnSelectedIndexChanged(int oldValue, int newValue)
    {
        if (oldValue == newValue) return;
        hotbar.SetEnabledItem(oldValue, false);
        hotbar.SwitchToItem(newValue);
        hotbar.SetEnabledItem(newValue, true);
    }

    private void OnDraggingStateChanged(bool oldValue, bool newValue)
    {
        if (oldValue == newValue) return;
        if (!newValue)
        {
            hotbar.Draggable.DetachRigidbody();
            hotbar.StopDragging();
        }
    }

    public void OnItemPickUpStateChanged(Item item, bool pickedUp)
    {
        if (!pickedUp)
        {
            for (int i = 0; i < hotbar.Items.Length; i++)
            {
                if (hotbar.Items[i] == item)
                {
                    item.transform.SetParent(null);
                    item.OnPickUp -= OnItemPickUpStateChanged;
                    DropItemClientRpc(i);
                    return;
                }
            }
        }
    }

    public void OnDraggableUsingStateChanged(IUseable useable, bool isUsing)
    {
        if (!isUsing)
        {
            Draggable draggable = useable as Draggable;
            draggable.OnUsed -= OnDraggableUsingStateChanged;
            draggable.DetachRigidbody();
            isDragging.Value = false;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SwitchToItemServerRpc(int index)
    {
        selectedIndex.Value = index;
    }

    [ServerRpc(RequireOwnership = false)]
    public void PickUpItemServerRpc(ulong itemNetworkObjectId)
    {
        NetworkObject itemNetworkObject = GetNetworkObject(itemNetworkObjectId);
        Item item = itemNetworkObject.GetComponent<Item>();

        if (item.CanPickUp())
        {
            if (itemNetworkObject.OwnerClientId != OwnerClientId)
            {
                Debug.Log("Changed Ownership");
                itemNetworkObject.ChangeOwnership(OwnerClientId);
            }

            item.RequestPickUp();
            item.OnPickUp += OnItemPickUpStateChanged;
            item.transform.SetParent(transform);

            if (hotbar.GetItem() == null)
            {
                PickUpItemClientRpc(itemNetworkObjectId, selectedIndex.Value);
                return;
            }

            for (int i = 0; i < hotbar.HotbarCapacity; i++)
            {
                if (hotbar.Items[i] == null)
                {
                    PickUpItemClientRpc(itemNetworkObjectId, i);
                    selectedIndex.Value = i;
                    return;
                }
            }

            DropItemServerRpc(hotbar.SelectedIndex);
            PickUpItemClientRpc(itemNetworkObjectId, selectedIndex.Value);
        }
    }

    [ClientRpc(RequireOwnership = false)]
    public void PickUpItemClientRpc(ulong itemNetworkObjectId, int index)
    {
        NetworkObject itemNetworkObject = GetNetworkObject(itemNetworkObjectId);
        Item item = itemNetworkObject.GetComponent<Item>();

        hotbar.PickUpItem(item, index);
    }

    [ServerRpc(RequireOwnership = false)]
    public void DropItemServerRpc(int index)
    {
        Item item = hotbar.Items[index];
        if (item == null) return;

        if (item.CanDrop())
        {
            item.transform.SetParent(null);
            item.OnPickUp -= OnItemPickUpStateChanged;
            item.RequestDrop();
            DropItemClientRpc(index);
        }
    }

    [ClientRpc(RequireOwnership = false)]
    public void DropItemClientRpc(int index)
    {
        hotbar.SetEnabledItem(index, true);
        hotbar.DropItem(index);
    }

    [ServerRpc(RequireOwnership = false)]
    public void StartDraggingServerRpc(ulong draggableNetworkObjectId)
    {
        if (TryGetComponent(out Rigidbody rig))
        {
            NetworkObject draggableNetworkObject = GetNetworkObject(draggableNetworkObjectId);
            Draggable draggable = draggableNetworkObject.GetComponent<Draggable>();

            if (!draggable.CanUse()) return;
            draggable.RequestUse();
            draggable.OnUsed += OnDraggableUsingStateChanged;
            StartDraggingClientRpc(draggableNetworkObjectId);
            isDragging.Value = true;
        }
    }

    [ClientRpc(RequireOwnership = false)]
    public void StartDraggingClientRpc(ulong draggableNetworkObjectId)
    {
        NetworkObject draggableNetworkObject = GetNetworkObject(draggableNetworkObjectId);
        Draggable draggable = draggableNetworkObject.GetComponent<Draggable>();

        Rigidbody rig = GetComponent<Rigidbody>();
        draggable.AttachRigidbody(rig);

        hotbar.StartDragging(draggable);
    }

    [ServerRpc(RequireOwnership = false)]
    public void StopDraggingServerRpc()
    {
        if (!isDragging.Value) return;

        Draggable draggable = hotbar.Draggable;
        if (draggable == null) return;

        if (!draggable.CanStopUsing()) return;
        draggable.OnUsed -= OnDraggableUsingStateChanged;
        draggable.RequestStopUsing();
        isDragging.Value = false;
    }
}