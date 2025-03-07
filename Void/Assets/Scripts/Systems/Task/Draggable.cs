using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Draggable : MonoBehaviour, INetworkUseable, IInteractable
{
    public event IUseable.UseHandler OnUsed;

    private NetworkUseable networkUseable;

    private SpringJoint springJoint;
    private bool isUsing;

    private bool canDrag = true;

    public NetworkUseable NetworkUseable => networkUseable;
    public bool IsUsing => isUsing;
    public bool CanDrag {  get { return canDrag; } set { canDrag = value; } }

    public bool CanUse() => !isUsing && canDrag;
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
        springJoint.massScale = 5;
    }

    public void DetachRigidbody()
    {
        springJoint.connectedBody = null;
        Destroy(springJoint);
        springJoint = null;
    }
}