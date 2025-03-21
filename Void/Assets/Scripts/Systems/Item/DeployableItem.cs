using UnityEngine;

public class DeployableItem : Item
{
    private DeployableItemData deployableItemData;

    private NetworkDeployableItem networkDeployableItem;
    private bool isDeployed;

    public bool IsDeployed => isDeployed;

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
        if (networkDeployableItem.IsServer) RequestUndeploy();
        base.PickUp();
    }

    public virtual void Deploy()
    {
        isDeployed = true;
        rig.isKinematic = true;
        Transform castTransform = CameraManager.Instance.transform;
        if (Physics.Raycast(castTransform.position, castTransform.forward, out RaycastHit hit,
            deployableItemData.DeployRange, deployableItemData.DeployLayers,
            QueryTriggerInteraction.Ignore))
        {
            if (NetworkItem.IsOwner)
            {
                // Preserve original forward direction while aligning to surface normal
                Vector3 surfaceAlignedForward = Vector3.ProjectOnPlane(castTransform.forward, hit.normal).normalized;

                // Ensure valid rotation when projecting (edge case handling)
                if (surfaceAlignedForward == Vector3.zero)
                    surfaceAlignedForward = Vector3.forward;  // Fallback axis

                Quaternion surfaceAlignment = Quaternion.LookRotation(surfaceAlignedForward, hit.normal);

                transform.SetPositionAndRotation(hit.point, surfaceAlignment);
            }
        }
        else
        {
            if (NetworkItem.IsOwner) transform.SetPositionAndRotation(castTransform.position + castTransform.forward * deployableItemData.DeployRange, transform.rotation);
            if (NetworkItem.IsServer) RequestUndeploy();
        }
    }

    public virtual void Undeploy()
    {
        isDeployed = false;
        rig.isKinematic = false;
    }
}