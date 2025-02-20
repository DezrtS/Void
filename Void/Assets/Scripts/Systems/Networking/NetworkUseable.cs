using Unity.Netcode;

public class NetworkUseable : NetworkBehaviour
{
    protected IUseable useable;
    private readonly NetworkVariable<bool> isUsing = new();

    private void Awake()
    {
        OnUseableInitialize();
    }

    protected virtual void OnUseableInitialize()
    {
        useable = GetComponent<IUseable>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        OnUseableSpawn();
    }

    protected virtual void OnUseableSpawn()
    {
        isUsing.OnValueChanged += OnUseStateChanged;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        OnUseableDespawn();
    }

    protected virtual void OnUseableDespawn()
    {
        isUsing.OnValueChanged -= OnUseStateChanged;
    }

    private void OnUseStateChanged(bool oldValue, bool newValue)
    {
        if (oldValue == newValue) return;
        if (newValue) useable.Use();
        else useable.StopUsing();
    }

    [ServerRpc(RequireOwnership = false)]
    public void UseServerRpc()
    {
        if (!useable.CanUse()) return;
        isUsing.Value = true;
    }

    [ServerRpc(RequireOwnership = false)]
    public void StopUsingServerRpc()
    {
        if (!useable.CanStopUsing()) return;
        isUsing.Value = false;
    }
}