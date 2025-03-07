using UnityEngine;

public class Hotbar : MonoBehaviour
{
    public delegate void ItemEventHandler(int index, Item item);
    public delegate void ItemSwitchEventHandler(int fromIndex, int toIndex, Item fromItem, Item toItem);
    public event ItemEventHandler OnPickUpItem;
    public event ItemEventHandler OnDropItem;
    public event ItemSwitchEventHandler OnSwitchItem;

    [SerializeField] private Transform lookAt;
    [SerializeField] private Transform activeTransform;
    [SerializeField] private int hotbarCapacity;

    private NetworkHotbar networkHotbar;

    private Item[] hotbar;

    private int selectedIndex;
    private bool isDragging;
    private Draggable draggable;

    public int HotbarCapacity => hotbarCapacity;
    public NetworkHotbar NetworkHotbar => networkHotbar;
    public Item[] Items => hotbar;
    public int SelectedIndex => selectedIndex;
    public bool IsDragging => isDragging;
    public Draggable Draggable => draggable;

    public void RequestSwitchToItem(int index) => networkHotbar.SwitchToItemServerRpc(index);
    public void RequestPickUpItem(ulong itemNetworkObjectId) => networkHotbar.PickUpItemServerRpc(itemNetworkObjectId);
    public void RequestDropItem(int index) => networkHotbar.DropItemServerRpc(index);
    public void RequestStartDragging(ulong draggableNetworkObjectId) => networkHotbar.StartDraggingServerRpc(draggableNetworkObjectId);
    public void RequestStopDragging() => networkHotbar.StopDraggingServerRpc();

    private void Awake()
    {
        networkHotbar = GetComponent<NetworkHotbar>();
        hotbar = new Item[hotbarCapacity];
    }

    private void LateUpdate()
    {
        if (!networkHotbar.IsOwner) return;

        Item item = GetItem();
        if (item != null)
        {
            activeTransform.rotation = Quaternion.LookRotation((lookAt.position - activeTransform.position).normalized);
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

    public void SetEnabledItem(int index, bool enabled)
    {
        Item activeItem = hotbar[index];
        if (activeItem != null)
        {
            activeItem.gameObject.SetActive(enabled);
        }
    }

    public void SwitchToItem(int index)
    {
        OnSwitchItem?.Invoke(selectedIndex, index, hotbar[selectedIndex], hotbar[index]);
        selectedIndex = index;
    }

    public void RequestPickUpItem(ItemData itemData)
    {
        Item newItem = GameDataManager.SpawnItem(itemData);
        RequestPickUpItem(newItem);
    }

    public void RequestPickUpItem(Item item) => RequestPickUpItem(item.NetworkItem.NetworkObjectId);

    public void PickUpItem(Item item) => PickUpItem(item, selectedIndex);

    public void PickUpItem(Item item, int index)
    {
        hotbar[index] = item;
        OnPickUpItem?.Invoke(index, item);
    }

    public void RequestDropItem() => RequestDropItem(selectedIndex);

    public void RequestDropItem(Item item)
    {
        for (int i = 0; i < hotbar.Length; i++)
        {
            if (hotbar[i] == item)
            {
                RequestDropItem(i);
                return;
            }
        }
    }

    public void RequestDropEverything()
    {
        if (isDragging) RequestStopDragging();
        for (int i = 0; i < hotbarCapacity; i++)
        {
            if (hotbar[i] != null)
            {
                RequestDropItem(i);
            }
        }
    }

    public void DropItem(int index)
    {
        Item item = hotbar[index];
        hotbar[index] = null;
        OnDropItem?.Invoke(index, item);
    }

    public void StartDragging(Draggable draggable)
    {
        isDragging = true;
        this.draggable = draggable;
    }

    public void StopDragging()
    {
        draggable = null;
        isDragging = false;
    }
}