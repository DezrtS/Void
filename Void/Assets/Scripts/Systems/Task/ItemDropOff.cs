using UnityEngine;

public class ItemDropOff : MonoBehaviour, INetworkUseable, IInteractable
{
    public event IUseable.UseHandler OnUsed;

    public delegate void ItemDropOffHandler(Item item, ItemDropOff itemDropOff, bool isAdded);
    public static event ItemDropOffHandler OnDropOff;

    [SerializeField] private float processingTime;
    [SerializeField] private float ejectionPower;

    private NetworkItemDropOff networkItemDropOff;
    private bool isUsing;

    private Item item;
    private float processingTimer;

    public bool IsUsing => isUsing;

    public bool CanUse() => !isUsing;
    public bool CanStopUsing() => isUsing;
    public bool HasItem() => item != null;
    public bool CanEjectItem() => processingTimer <= 0;

    public void RequestUse() => networkItemDropOff.UseServerRpc();
    public void RequestStopUsing() => networkItemDropOff.StopUsingServerRpc();
    public void RequestProcessItem(Item item) => networkItemDropOff.ProcessItemServerRpc(item.NetworkItem.NetworkObjectId);
    public void RequestAcceptItem() => networkItemDropOff.AcceptItemServerRpc();
    public void RequestEjectItem() => networkItemDropOff.EjectItemServerRpc();

    private void Awake()
    {
        networkItemDropOff = GetComponent<NetworkItemDropOff>();
    }

    private void FixedUpdate()
    {
        UpdateTimers();

        if (networkItemDropOff.IsServer && isUsing)
        {
            RequestEjectItem();
        }
    }

    public void Use()
    {
        isUsing = true;
        processingTimer = processingTime;
    }

    public void StopUsing()
    {
        isUsing = false;
        processingTimer = 0;
        item = null;
    }

    public void ProcessItem(Item item)
    {
        this.item = item;
        if (item.NetworkItem.IsOwner) item.transform.position = transform.position;
        OnDropOff?.Invoke(item, this, true);
    }

    public void AcceptItem()
    {
        Debug.Log("Item Accepted");
        StopUsing();
    }

    public void EjectItem()
    {
        OnDropOff?.Invoke(item, this, false);
        if (networkItemDropOff.IsServer)
        {
            item.RequestDrop();
            Rigidbody rig = item.GetComponent<Rigidbody>();
            rig.AddForce(transform.rotation * new Vector3(0, 1, 1) * ejectionPower, ForceMode.Impulse);
            RequestStopUsing();
        }
    }

    public void Interact(GameObject interactor)
    {
        if (HasItem()) return;

        if (interactor.TryGetComponent(out SurvivorController survivorController))
        {
            Item item = survivorController.Hotbar.GetItem();
            if (item != null) RequestProcessItem(item);
        }
    }

    private void UpdateTimers()
    {
        float fixedDeltaTime = Time.fixedDeltaTime;

        if (processingTimer > 0)
        {
            processingTimer -= fixedDeltaTime;
        }
    }
}
