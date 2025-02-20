using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Draggable : MonoBehaviour, INetworkUseable, IInteractable
{
    public event IUseable.UseHandler OnUsed;

    private NetworkUseable networkUseable;

    private SpringJoint springJoint;
    private bool isUsing;

    public NetworkUseable NetworkUseable => networkUseable;
    public bool IsUsing => isUsing;

    public bool CanUse() => !isUsing;
    public bool CanStopUsing() => isUsing;

    public void RequestUse() => networkUseable.UseServerRpc();
    public void RequestStopUsing() => networkUseable.StopUsingServerRpc();

    private void Awake()
    {
        networkUseable = GetComponent<NetworkUseable>();
        springJoint = GetComponent<SpringJoint>();
    }

    public void Use()
    {
        isUsing = true;
        OnUsed?.Invoke(this, isUsing);
    }

    public void StopUsing()
    {
        isUsing = false;
        OnUsed?.Invoke(this, isUsing);
    }

    public void Interact(GameObject interactor)
    {
        if (!CanUse()) return;

        if (interactor.TryGetComponent(out Hotbar hotbar))
        {
            hotbar.RequestStartDragging(networkUseable.NetworkObjectId);
        }
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
}