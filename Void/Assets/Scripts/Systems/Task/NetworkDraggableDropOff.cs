using Unity.Netcode;

public class NetworkDraggableDropOff : NetworkBehaviour
{
    private DraggableDropOff draggableDropOff;

    private void Awake()
    {
        draggableDropOff = GetComponent<DraggableDropOff>();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ProcessDraggableServerRpc(ulong draggableNetworkObjectId)
    {
        ProcessDraggableClientRpc(draggableNetworkObjectId);
    }

    [ClientRpc(RequireOwnership = false)]
    public void ProcessDraggableClientRpc(ulong draggableNetworkObjectId)
    {
        NetworkObject draggableNetworkObject = GetNetworkObject(draggableNetworkObjectId);
        Draggable draggable = draggableNetworkObject.GetComponent<Draggable>();

        draggableDropOff.ProcessDraggable(draggable);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RecessDraggableServerRpc(ulong draggableNetworkObjectId)
    {
        RecessDraggableClientRpc(draggableNetworkObjectId);
    }

    [ClientRpc(RequireOwnership = false)]
    public void RecessDraggableClientRpc(ulong draggableNetworkObjectId)
    {
        NetworkObject draggableNetworkObject = GetNetworkObject(draggableNetworkObjectId);
        Draggable draggable = draggableNetworkObject.GetComponent<Draggable>();

        draggableDropOff.RecessDraggable(draggable);
    }

    [ServerRpc(RequireOwnership = false)]
    public void AcceptDraggableServerRpc(ulong draggableNetworkObjectId)
    {
        AcceptDraggableClientRpc(draggableNetworkObjectId);
    }

    [ClientRpc(RequireOwnership = false)]
    public void AcceptDraggableClientRpc(ulong draggableNetworkObjectId)
    {
        NetworkObject draggableNetworkObject = GetNetworkObject(draggableNetworkObjectId);
        Draggable draggable = draggableNetworkObject.GetComponent<Draggable>();

        draggableDropOff.AcceptDraggable(draggable);
    }
}