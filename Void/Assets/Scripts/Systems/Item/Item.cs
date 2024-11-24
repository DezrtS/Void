using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public abstract class Item : NetworkBehaviour, IUseable
{
    [SerializeField] private ItemData ItemData;
    [SerializeField] protected bool canPickUp = true;
    [SerializeField] protected bool canDrop = true;

    protected bool isUsing;
    protected Rigidbody rig;

    public event IUseable.UseHandler OnUsed;

    public bool IsUsing => isUsing;
    public bool CanPickUp => canPickUp;
    public bool CanDrop => canDrop;

    private void Awake()
    {
        rig = GetComponent<Rigidbody>();
    }

    public virtual bool CanUse()
    {
        return !isUsing;
    }

    public void Use()
    {
        if (CanUse())
        {
            isUsing = true;
            OnUsed?.Invoke(true);
            OnUse();
        }
    }

    protected virtual void OnUse() { }
    
    public void StopUsing()
    {
        if (isUsing)
        {
            isUsing = false;
            OnUsed?.Invoke(false);
            OnStopUsing();
        }
    }

    protected virtual void OnStopUsing() { }

    public void RequestPickUp(ulong playerNetworkObjectId)
    {
        if (canPickUp)
        {
            ItemManager.Instance.RequestItemPickUpServerRpc(NetworkObjectId, playerNetworkObjectId);
        }
    }

    public void PickUp()
    {
        canPickUp = false;
        canDrop = true;
        rig.isKinematic = true;
        OnPickUp();
    }

    protected virtual void OnPickUp() { }

    public void RequestDrop(ulong playerNetworkObjectId)
    {
        if (canPickUp)
        {
            ItemManager.Instance.RequestItemDropServerRpc(NetworkObjectId, playerNetworkObjectId);
        }
    }

    public void Drop()
    {
        canDrop = false;
        canPickUp = true;
        rig.isKinematic = false;
        OnDrop();
    }

    protected virtual void OnDrop() { }
}