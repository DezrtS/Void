using FMODUnity;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Item : MonoBehaviour, INetworkUseable, IInteractable
{
    public event IUseable.UseHandler OnUsed;
    public delegate void ItemPickUpHandler(Item item, bool pickedUp);
    public event ItemPickUpHandler OnPickUp;

    [SerializeField] private ItemData itemData;
    [SerializeField] protected bool canPickUp = true;
    [SerializeField] protected bool canDrop = true;

    [SerializeField] private InteractableData pickUpInteractableData;
    [SerializeField] private TutorialData tutorialData;
    [SerializeField] private HoldingPositionData holdingPositionData;

    private NetworkItem networkItem;

    protected Rigidbody rig;
    private Collider col;
    private bool isPickedUp;
    private bool isUsing;

    public ItemData ItemData => itemData;
    public TutorialData TutorialData => tutorialData;
    public NetworkItem NetworkItem => networkItem;
    public bool IsPickedUp => isPickedUp;
    public bool IsUsing => isUsing;

    public bool CanPickUp() => !isPickedUp && canPickUp;
    public bool CanDrop() => isPickedUp && canDrop;
    public bool CanUse() => !isUsing;
    public bool CanStopUsing() => isUsing;

    private void Awake()
    {
        OnItemInitialize();
    }

    protected virtual void OnItemInitialize()
    {
        networkItem = GetComponent<NetworkItem>();
        rig = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    public void RequestUse() => networkItem.UseServerRpc();
    public void RequestStopUsing() => networkItem.StopUsingServerRpc();
    public void RequestPickUp() => networkItem.PickUpServerRpc();
    public void RequestDrop() => networkItem.DropServerRpc();

    public virtual void Use()
    {
        isUsing = true;
        AudioManager.PlayOneShot(itemData.UseSound, gameObject);
        OnUsed?.Invoke(this, isUsing);
    }

    public virtual void StopUsing()
    {
        isUsing = false;
        AudioManager.PlayOneShot(itemData.StopUsingSound, gameObject);
        OnUsed?.Invoke(this, isUsing);
    }

    public virtual void PickUp()
    {
        isPickedUp = true;
        AudioManager.PlayOneShot(itemData.PickUpSound, gameObject);
        OnPickUp?.Invoke(this, isPickedUp);
        UpdateItemState(true);
    }

    public virtual void Drop()
    {
        isPickedUp = false;
        AudioManager.PlayOneShot(itemData.DropSound, gameObject);
        OnPickUp?.Invoke(this, isPickedUp);
        UpdateItemState(false);
    }

    private void UpdateItemState(bool isPickedUp)
    {
        Debug.Log($"IsPickedUp: {isPickedUp}");
        rig.isKinematic = isPickedUp;
        col.enabled = !isPickedUp;
    }

    public InteractableData GetInteractableData()
    {
        return pickUpInteractableData;
    }

    public virtual void Interact(GameObject interactor)
    {
        if (interactor.TryGetComponent(out Hotbar hotbar))
        {
            hotbar.RequestPickUpItem(this);
        }
    }

    public void SetAtHoldingPosition(Vector3 worldPosition, Quaternion worldRotation)
    {
        if (holdingPositionData == null)
        {
            // No offsets, just set the world position and rotation
            transform.SetPositionAndRotation(worldPosition, worldRotation);
        }
        else
        {
            // Convert local offsets to world space
            Vector3 worldPositionOffset = worldRotation * holdingPositionData.PositionOffset;
            Quaternion worldRotationOffset = worldRotation * Quaternion.Euler(holdingPositionData.RotationOffset);

            // Apply the world-space offsets
            transform.SetPositionAndRotation(worldPosition + worldPositionOffset, worldRotationOffset);
        }
    }
}