using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Dragable : NetworkBehaviour, IInteractable
{
    private Rigidbody rig;
    private bool isDragging;

    private void Awake()
    {
        rig = GetComponent<Rigidbody>();
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
        rig.isKinematic = true;
    }

    public void StopDragging()
    {
        isDragging = false;
        rig.isKinematic = false;
    }
}
