using Unity.Netcode;

public class NetworkItemDropOff : NetworkUseable
{
    private ItemDropOff itemDropOff;

    protected override void OnUseableInitialize()
    {
        base.OnUseableInitialize();
        OnItemDropOffInitialize();
    }

    private void OnItemDropOffInitialize()
    {
        itemDropOff = useable as ItemDropOff;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ProcessItemServerRpc(ulong itemNetworkObjectId)
    {
        NetworkObject itemNetworkObject = GetNetworkObject(itemNetworkObjectId);
        Item item = itemNetworkObject.GetComponent<Item>();

        if (!item.CanDrop()) return;
        item.RequestDrop();

        if (!item.CanPickUp()) return;
        item.RequestPickUp();

        ProcessItemClientRpc(itemNetworkObjectId);
        UseServerRpc();
    }

    [ClientRpc(RequireOwnership = false)]
    public void ProcessItemClientRpc(ulong itemNetworkObjectId)
    {
        if (itemDropOff.HasItem()) return;
        NetworkObject itemNetworkObject = GetNetworkObject(itemNetworkObjectId);
        Item item = itemNetworkObject.GetComponent<Item>();

        itemDropOff.ProcessItem(item);
    }

    [ServerRpc(RequireOwnership = false)]
    public void AcceptItemServerRpc()
    {
        if (!itemDropOff.HasItem()) return;
        AcceptItemClientRpc();
    }

    [ClientRpc(RequireOwnership = false)]
    public void AcceptItemClientRpc()
    {
        itemDropOff.AcceptItem();
    }

    [ServerRpc(RequireOwnership = false)]
    public void EjectItemServerRpc()
    {
        if (!itemDropOff.CanEjectItem()) return;
        
        if (!itemDropOff.HasItem())
        {
            StopUsingServerRpc();
            return;
        }
        
        EjectItemClientRpc();
    }

    [ClientRpc(RequireOwnership = false)]
    public void EjectItemClientRpc()
    {
        itemDropOff.EjectItem();
    }
}