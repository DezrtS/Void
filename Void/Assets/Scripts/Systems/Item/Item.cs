using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Item : MonoBehaviour, INetworkUseable, IInteractable
{
    public event IUseable.UseHandler OnUsed;
    public delegate void ItemPickUpHandler(Item item, bool pickedUp);
    public event ItemPickUpHandler OnPickUp;

    [SerializeField] private ItemData itemData;
    [SerializeField] private bool canPickUp = true;
    [SerializeField] private bool canDrop = true;
    
    private NetworkItem networkItem;

    protected Rigidbody rig;
    private Collider col;
    private bool isPickedUp;
    private bool isUsing;

    public ItemData ItemData => itemData;
    public NetworkItem NetworkItem => networkItem;
    public bool IsPickedUp => isPickedUp;
    public bool IsUsing => isUsing;

    public bool CanPickUp() => !isPickedUp && canPickUp;
    public bool CanDrop() => isPickedUp && canDrop;
    public bool CanUse() => !isUsing;
    public bool CanStopUsing() => isUsing;

    private void Awake()
    {
        networkItem = GetComponent<NetworkItem>();
        rig = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    public void RequestUse() => networkItem.UseServerRpc();
    public void RequestStopUsing() => networkItem.StopUsingServerRpc();
    public void RequestPickUp() => networkItem.PickUpServerRpc();
    public void RequestDrop() => networkItem.DropServerRpc();

    public virtual void Use()
    {
        isUsing = true;
        OnUsed?.Invoke(this, isUsing);
    }

    public virtual void StopUsing()
    {
        isUsing = false;
        OnUsed?.Invoke(this, isUsing);
    }

    public virtual void PickUp()
    {
        isPickedUp = true;
        OnPickUp?.Invoke(this, isPickedUp);
        UpdateItemState(true);
    }

    public virtual void Drop()
    {
        isPickedUp = false;
        OnPickUp?.Invoke(this, isPickedUp);
        UpdateItemState(false);
    }

    private void UpdateItemState(bool isPickedUp)
    {
        rig.isKinematic = isPickedUp;
        col.enabled = !isPickedUp;
    }

    public void Interact(GameObject interactor)
    {
        if (interactor.TryGetComponent(out Hotbar hotbar))
        {
            hotbar.RequestPickUpItem(this);
        }
    }
}