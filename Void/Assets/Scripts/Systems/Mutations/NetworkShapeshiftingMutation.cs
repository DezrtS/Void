using System.Reflection;
using Unity.Netcode;
using UnityEngine;

public class NetworkShapeshiftingMutation : NetworkUseable
{
    private ShapeshiftingMutation shapeshiftingMutation;
    private readonly NetworkVariable<bool> isActive = new();

    protected override void OnUseableInitialize()
    {
        base.OnUseableInitialize();
        shapeshiftingMutation = useable as ShapeshiftingMutation;
    }

    protected override void OnUseableSpawn()
    {
        base.OnUseableSpawn();
        isActive.OnValueChanged += OnActiveStateChanged;
    }

    protected override void OnUseableDespawn()
    {
        base.OnUseableSpawn();
        isActive.OnValueChanged -= OnActiveStateChanged;
    }

    private void OnActiveStateChanged(bool oldValue, bool newValue)
    {
        if (oldValue == newValue) return;
        if (newValue) shapeshiftingMutation.Activate();
        else shapeshiftingMutation.Deactivate();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ActivateShapeshiftingMutationServerRpc()
    {
        if (!shapeshiftingMutation.CanActivate()) return;
        isActive.Value = true;
    }

    [ServerRpc(RequireOwnership = false)]
    public void DeactivateShapeshiftingMutationServerRpc()
    {
        if (!shapeshiftingMutation.CanDeactivate()) return;
        isActive.Value = false;
    }
}