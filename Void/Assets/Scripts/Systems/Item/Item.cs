using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Item : NetworkBehaviour, IUseable
{
    [SerializeField] private ItemData itemData;
    [SerializeField] protected bool canPickUp = true;
    [SerializeField] protected bool canDrop = true;

    protected bool isUsing;
    protected Rigidbody rig;

    public event IUseable.UseHandler OnUsed;

    public ItemData ItemData => itemData;
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
        RequestUseServerRpc(new ServerRpcParams());
    }

    protected virtual void OnUse() { }
    
    public void StopUsing()
    {
        RequestStopUsingServerRpc(new ServerRpcParams());
    }

    protected virtual void OnStopUsing() { }

    public void PickUp()
    {
        canPickUp = false;
        canDrop = true;
        rig.isKinematic = true;
        OnPickUp();
    }

    protected virtual void OnPickUp() { }

    public void Drop()
    {
        canDrop = false;
        canPickUp = true;
        rig.isKinematic = false;
        OnDrop();
    }

    protected virtual void OnDrop() { }

    private void HandleUse()
    {
        // TODO FIX USING GOING OUT OF SYNC [MAKE USE CHILD USE ACTIONS TRIGGER SERVER RPC]
        isUsing = true;
        OnUsed?.Invoke(true);
        OnUse();
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestUseServerRpc(ServerRpcParams rcpParams = default)
    {
        Item item = NetworkObject.GetComponent<Item>();
        if (item.CanUse())
        {
            HandleUse();
            HandleUseClientRpc(rcpParams.Receive.SenderClientId);
        }
    }

    [ClientRpc]
    public void HandleUseClientRpc(ulong clientId)
    {
        Item item = NetworkObject.GetComponent<Item>();
        if (item != null)
        {
            HandleUse();
        }
    }

    private void HandleStopUsing()
    {
        isUsing = false;
        OnUsed?.Invoke(false);
        OnStopUsing();
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestStopUsingServerRpc(ServerRpcParams rcpParams = default)
    {
        Item item = NetworkObject.GetComponent<Item>();
        if (item.IsUsing)
        {
            HandleStopUsing();
            HandleStopUsingClientRpc(rcpParams.Receive.SenderClientId);
        }
    }

    [ClientRpc]
    public void HandleStopUsingClientRpc(ulong clientId)
    {
        Item item = NetworkObject.GetComponent<Item>();
        if (item != null)
        {
            HandleStopUsing();
        }
    }
}