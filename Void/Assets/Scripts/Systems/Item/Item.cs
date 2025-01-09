using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Item : NetworkBehaviour, IUseable
{
    [SerializeField] private ItemData itemData;
    [SerializeField] protected bool isPickedUp = false;
    [SerializeField] protected bool isDropped = false;
    [SerializeField] protected bool canPickUp = true;
    [SerializeField] protected bool canDrop = true;

    protected NetworkVariable<bool> isUsing = new NetworkVariable<bool>(false);
    protected Rigidbody rig;

    public delegate void ItemHandler(Item item);
    public event IUseable.UseHandler OnUsed;
    public event ItemHandler OnPickedUp;
    public event ItemHandler OnDropped;

    public ItemData ItemData => itemData;
    public bool IsUsing => isUsing.Value;
    public bool IsPickedUp => isPickedUp;
    public bool IsDropped => isDropped;
    public bool CanPickUp => canPickUp;
    public bool CanDrop => canDrop;

    private void Awake()
    {
        rig = GetComponent<Rigidbody>();
    }

    public bool CanUse()
    {
        return !IsUsing;
    }

    public void Use()
    {
        if (!CanUse()) return;
        isUsing.Value = true;
        OnUse();
    }

    public virtual void OnUse() { }

    public void StopUsing()
    {
        if (!IsUsing) return;
        isUsing.Value = false;
        OnStopUsing();
    }

    public virtual void OnStopUsing() { }

    public virtual void PickUp()
    {
        isDropped = false;
        isPickedUp = true;
        rig.isKinematic = true;
        OnPickedUp?.Invoke(this);
    }

    public virtual void Drop()
    {
        isPickedUp = false;
        isDropped = true;
        rig.isKinematic = false;
        OnDropped?.Invoke(this);
    }
}