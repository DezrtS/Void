using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Draggable : NetworkBehaviour, IInteractable
{
    private Rigidbody rig;
    private SpringJoint springJoint;
    private bool isDragging;

    public bool IsDragging => isDragging;

    public delegate void DraggableHandler(Draggable draggable);
    public event DraggableHandler OnStartDragging;
    public event DraggableHandler OnStopDragging;

    private void Awake()
    {
        rig = GetComponent<Rigidbody>();
        springJoint = GetComponent<SpringJoint>();
    }

    public void Interact(GameObject interactor)
    {
        if (isDragging) return;

        if (interactor.TryGetComponent(out Hotbar hotbar))
        {
            hotbar.StartDragging(this);
        }
    }

    public void Drag()
    {
        isDragging = true;
        OnStartDragging?.Invoke(this);
        //rig.isKinematic = true;
    }

    public void StopDragging()
    {
        isDragging = false;
        OnStopDragging?.Invoke(this);
        //rig.isKinematic = false;
    }

    public void AttachRigidbody(Rigidbody rig)
    {
        if (!springJoint) springJoint = transform.AddComponent<SpringJoint>();
        springJoint.connectedBody = rig;
    }

    public void DetachRigidbody()
    {
        springJoint.connectedBody = null;
        Destroy(springJoint);
        springJoint = null;
    }

    public static Draggable SpawnDraggable(GameObject gameObject)
    {
        GameObject spawnedObject = Instantiate(gameObject);
        Draggable draggable = spawnedObject.GetComponent<Draggable>();
        draggable.NetworkObject.Spawn();
        return draggable;
    }
}
