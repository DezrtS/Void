using FMOD.Studio;
using FMODUnity;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Draggable : MonoBehaviour, INetworkUseable, IInteractable
{
    public event IUseable.UseHandler OnUsed;

    [SerializeField] private InteractableData dragInteractableData;
    [SerializeField] private InteractableData dropInteractableData;

    [SerializeField] private EventReference dragSound;
    [SerializeField] private EventReference stopDraggingSound;
    [SerializeField] private EventReference draggingSound;

    private NetworkUseable networkUseable;

    private SpringJoint springJoint;
    private bool isUsing;

    private bool canDrag = true;
    private EventInstance draggingInstance;

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
        draggingInstance = AudioManager.CreateEventInstance(draggingSound, gameObject);
    }

    public void Use()
    {
        isUsing = true;
        OnUsed?.Invoke(this, isUsing);
        AudioManager.PlayOneShot(dragSound, gameObject);
        draggingInstance.start();
    }

    public void StopUsing()
    {
        isUsing = false;
        OnUsed?.Invoke(this, isUsing);
        AudioManager.PlayOneShot(stopDraggingSound, gameObject);
        draggingInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    public InteractableData GetInteractableData()
    {
        if (!canDrag) return null;
        return (isUsing) ? dropInteractableData : dragInteractableData;
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
        springJoint.connectedMassScale = 0.1f;
    }

    public void DetachRigidbody()
    {
        springJoint.connectedBody = null;
        Destroy(springJoint);
        springJoint = null;
    }
}