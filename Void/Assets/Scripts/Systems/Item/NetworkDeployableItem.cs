using Unity.Netcode;
using UnityEngine;

public class NetworkDeployableItem : NetworkItem
{
    private DeployableItem deployableItem;
    private readonly NetworkVariable<bool> isDeployed = new();

    protected override void OnItemInitialize()
    {
        base.OnItemInitialize();
        OnDeployableItemInitialize();
    }

    protected virtual void OnDeployableItemInitialize()
    {
        deployableItem = useable as DeployableItem;
    }

    protected override void OnItemSpawn()
    {
        base.OnItemSpawn();
        OnDeployableItemSpawn();
    }

    protected virtual void OnDeployableItemSpawn()
    {
        isDeployed.OnValueChanged += OnDeployedStateChanged;
    }

    protected override void OnItemDespawn()
    {
        base.OnItemDespawn();
        OnDeployableItemDespawn();
    }

    protected virtual void OnDeployableItemDespawn()
    {
        isDeployed.OnValueChanged -= OnDeployedStateChanged;
    }

    private void OnDeployedStateChanged(bool oldValue, bool newValue)
    {
        if (oldValue == newValue) return;
        if (newValue) deployableItem.Deploy();
        else deployableItem.Undeploy();
    }

    [ServerRpc(RequireOwnership = false)]
    public void DeployServerRpc()
    {
        if (!deployableItem.CanDeploy()) return;
        deployableItem.RequestDrop();
        isDeployed.Value = true;
    }

    [ServerRpc(RequireOwnership = false)]
    public void UndeployServerRpc()
    {
        if (!deployableItem.CanUndeploy()) return;
        isDeployed.Value = false;
    }
}