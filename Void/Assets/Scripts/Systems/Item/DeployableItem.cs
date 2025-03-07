using UnityEngine;

public class DeployableItem : Item
{
    private DeployableItemData deployableItemData;

    private NetworkDeployableItem networkDeployableItem;
    private bool isDeployed;

    public bool CanDeploy() => !isDeployed;
    public bool CanUndeploy() => isDeployed;

    protected override void OnItemInitialize()
    {
        base.OnItemInitialize();
        OnDeployableItemInitialize();
    }

    protected virtual void OnDeployableItemInitialize()
    {
        deployableItemData = ItemData as DeployableItemData;
        networkDeployableItem = NetworkItem as NetworkDeployableItem;
    }

    public void RequestDeploy() => networkDeployableItem.DeployServerRpc();
    public void RequestUndeploy() => networkDeployableItem.UndeployServerRpc();

    public override void StopUsing()
    {
        base.StopUsing();
        if (networkDeployableItem.IsServer) RequestDeploy();
    }

    public override void PickUp()
    {
        base.PickUp();
        if (networkDeployableItem.IsServer) RequestUndeploy();
    }

    public virtual void Deploy()
    {
        isDeployed = true;
        rig.isKinematic = true;
        if (NetworkItem.IsOwner)
        {
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit,
                deployableItemData.DeployRange, deployableItemData.DeployLayers,
                QueryTriggerInteraction.Ignore))
            {
                // Preserve original forward direction while aligning to surface normal
                Vector3 surfaceAlignedForward = Vector3.ProjectOnPlane(transform.forward, hit.normal).normalized;

                // Ensure valid rotation when projecting (edge case handling)
                if (surfaceAlignedForward == Vector3.zero)
                    surfaceAlignedForward = Vector3.forward;  // Fallback axis

                Quaternion surfaceAlignment = Quaternion.LookRotation(surfaceAlignedForward, hit.normal);

                transform.SetPositionAndRotation(hit.point, surfaceAlignment);
            }
            else
            {
                transform.SetPositionAndRotation(transform.position + transform.forward * deployableItemData.DeployRange, transform.rotation);
                RequestUndeploy();
            }
        }
    }

    public virtual void Undeploy()
    {
        rig.isKinematic = false;
        isDeployed = false;
    }
}