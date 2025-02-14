using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Item : NetworkBehaviour, IUseable, IInteractable
{
    [SerializeField] private ItemData itemData;
    [SerializeField] protected bool isPickedUp = false;
    [SerializeField] protected bool isDropped = false;
    [SerializeField] protected bool canPickUp = true;
    [SerializeField] protected bool canDrop = true;

    protected bool isUsing;
    protected Rigidbody rig;
    protected Collider col;

    public delegate void ItemHandler(Item item);
    public event IUseable.UseHandler OnUsed;
    public event ItemHandler OnPickedUp;
    public event ItemHandler OnDropped;
    public event ItemHandler OnRequestForceDrop;

    public ItemData ItemData => itemData;
    public bool IsUsing => isUsing;
    public bool IsPickedUp => isPickedUp;
    public bool IsDropped => isDropped;

    private void Awake()
    {
        rig = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    public bool CanUse()
    {
        return !IsUsing;
    }

    public bool Use()
    {
        if (!CanUse()) return false;
        ForceUse();
        return true;
    }

    public void ForceUse()
    {
        if (!IsServer) UseClientSide();
        UseServerRpc();
    }

    protected virtual void UseClientSide()
    {
        UseClientServerSide();
    }

    protected virtual void UseServerSide()
    {
        UseClientServerSide();
    }

    protected virtual void UseClientServerSide()
    {
        isUsing = true;
    }

    [ServerRpc(RequireOwnership = false)]
    private void UseServerRpc(ServerRpcParams rpcParams = default)
    {
        if (!CanUse()) return;
        UseServerSide();
        ClientRpcParams clientRpcParams = GameMultiplayer.GenerateClientRpcParams(rpcParams);
        UseClientRpc(clientRpcParams);
    }

    [ClientRpc(RequireOwnership = false)]
    private void UseClientRpc(ClientRpcParams rpcParams = default)
    {
        UseClientSide();
    }

    public bool StopUsing()
    {
        if (!IsUsing) return false;
        ForceStopUsing();
        return true;
    }

    public void ForceStopUsing()
    {
        if (!IsServer) StopUsingClientSide();
        StopUsingServerRpc();
    }

    protected virtual void StopUsingClientSide()
    {
        StopUsingClientServerSide();
    }

    protected virtual void StopUsingServerSide()
    {
        StopUsingClientServerSide();
    }

    protected virtual void StopUsingClientServerSide()
    {
        isUsing = false;
    }

    [ServerRpc(RequireOwnership = false)]
    private void StopUsingServerRpc(ServerRpcParams rpcParams = default)
    {
        if (!IsUsing) return;
        StopUsingServerSide();
        ClientRpcParams clientRpcParams = GameMultiplayer.GenerateClientRpcParams(rpcParams);
        StopUsingClientRpc(clientRpcParams);
    }

    [ClientRpc(RequireOwnership = false)]
    private void StopUsingClientRpc(ClientRpcParams rpcParams = default)
    {
        StopUsingClientSide();
    }

    public bool CanPickUp()
    {
        return (canPickUp && !isPickedUp);
    }

    public bool PickUp()
    {
        if (!CanPickUp()) return false;
        ForcePickUp();
        return true;
    }

    public void ForcePickUp()
    {
        if (!IsServer) PickUpClientSide();
        PickUpServerRpc();
    }

    protected virtual void PickUpClientSide()
    {
        PickUpClientServerSide();
    }

    protected virtual void PickUpServerSide()
    {
        PickUpClientServerSide();
    }

    protected virtual void PickUpClientServerSide()
    {
        isDropped = false;
        isPickedUp = true;
        rig.isKinematic = true;
        col.enabled = false;
        OnPickedUp?.Invoke(this);
    }

    [ServerRpc(RequireOwnership = false)]
    private void PickUpServerRpc(ServerRpcParams rpcParams = default)
    {
        if (!CanPickUp()) return;
        PickUpServerSide();
        ClientRpcParams clientRpcParams = GameMultiplayer.GenerateClientRpcParams(rpcParams);
        PickUpClientRpc(clientRpcParams);
    }

    [ClientRpc(RequireOwnership = false)]
    private void PickUpClientRpc(ClientRpcParams rpcParams = default)
    {
        PickUpClientSide();
    }

    protected void DropItem()
    {
        OnRequestForceDrop?.Invoke(this);
    }

    public bool CanDrop()
    {
        return (canDrop && !isDropped);
    }

    public bool Drop()
    {
        if (!CanDrop()) return false;
        ForceDrop();
        return true;
    }

    public void ForceDrop()
    {
        if (!IsServer) DropClientSide();
        DropServerRpc();
    }

    protected virtual void DropClientSide()
    {
        DropClientServerSide();
    }

    protected virtual void DropServerSide()
    {
        DropClientServerSide();
    }

    protected virtual void DropClientServerSide()
    {
        isPickedUp = false;
        isDropped = true;
        col.enabled = true;
        rig.isKinematic = false;
        OnDropped?.Invoke(this);
    }

    [ServerRpc(RequireOwnership = false)]
    private void DropServerRpc(ServerRpcParams rpcParams = default)
    {
        if (!CanDrop()) return;
        DropServerSide();
        ClientRpcParams clientRpcParams = GameMultiplayer.GenerateClientRpcParams(rpcParams);
        DropClientRpc(clientRpcParams);
    }

    [ClientRpc(RequireOwnership = false)]
    private void DropClientRpc(ClientRpcParams rpcParams = default)
    {
        DropClientSide();
    }

    public void Interact(GameObject interactor)
    {
        if (interactor.TryGetComponent(out Hotbar hotbar))
        {
            hotbar.PickUpItem(this);
        }
    }
}