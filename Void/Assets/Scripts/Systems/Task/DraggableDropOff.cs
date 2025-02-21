using UnityEngine;

public class DraggableDropOff : MonoBehaviour
{
    public delegate void DraggableDropOffHandler(Draggable draggable, DraggableDropOff draggableDropOff, bool isAdded);
    public static event DraggableDropOffHandler OnDropOff;

    private NetworkDraggableDropOff networkDraggableDropOff;

    public void RequestProcessDraggable(Draggable draggable) => networkDraggableDropOff.ProcessDraggableServerRpc(draggable.NetworkUseable.NetworkObjectId); 
    public void RequestRecessDraggable(Draggable draggable) => networkDraggableDropOff.RecessDraggableServerRpc(draggable.NetworkUseable.NetworkObjectId); 
    public void RequestAcceptDraggable(Draggable draggable) => networkDraggableDropOff.AcceptDraggableServerRpc(draggable.NetworkUseable.NetworkObjectId);

    private void Awake()
    {
        networkDraggableDropOff = GetComponent<NetworkDraggableDropOff>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (networkDraggableDropOff.IsServer)
        {
            if (other.TryGetComponent(out Draggable draggable))
            {
                RequestProcessDraggable(draggable);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (networkDraggableDropOff.IsServer)
        {
            if (other.TryGetComponent(out Draggable draggable))
            {
                RequestRecessDraggable(draggable);
            }
        }
    }

    public void ProcessDraggable(Draggable draggable)
    {
        OnDropOff?.Invoke(draggable, this, true);
    }

    public void RecessDraggable(Draggable draggable)
    {
        OnDropOff?.Invoke(draggable, this, false);
    }

    public void AcceptDraggable(Draggable draggable)
    {
        Debug.Log("Accepted");
    }
}