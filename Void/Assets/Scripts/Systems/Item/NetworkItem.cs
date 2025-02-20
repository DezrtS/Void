using Unity.Netcode;

public class NetworkItem : NetworkUseable
{
    private Item item;
    private readonly NetworkVariable<bool> isPickedUp = new();

    protected override void OnUseableInitialize()
    {
        base.OnUseableInitialize();
        OnItemInitialize();
    }

    protected virtual void OnItemInitialize()
    {
        item = useable as Item;
    }

    protected override void OnUseableSpawn()
    {
        base.OnUseableSpawn();
        OnItemSpawn();
    }

    protected virtual void OnItemSpawn()
    {
        isPickedUp.OnValueChanged += OnPickedUpStateChanged;
    }

    protected override void OnUseableDespawn()
    {
        base.OnUseableDespawn();
        OnItemDespawn();
    }

    protected virtual void OnItemDespawn()
    {
        isPickedUp.OnValueChanged -= OnPickedUpStateChanged;
    }

    private void OnPickedUpStateChanged(bool oldValue, bool newValue)
    {
        if (oldValue == newValue) return;
        if (newValue) item.PickUp();
        else item.Drop();
    }

    [ServerRpc(RequireOwnership = false)]
    public void PickUpServerRpc()
    {
        if (!item.CanPickUp()) return;
        isPickedUp.Value = true;
    }

    [ServerRpc(RequireOwnership = false)]
    public void DropServerRpc()
    {
        if (!item.CanDrop()) return;
        isPickedUp.Value = false;
    }
}