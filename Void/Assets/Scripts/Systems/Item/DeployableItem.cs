using UnityEngine;

public class DeployableItem : Item
{
    private DeployableItemData deployableItemData;

    private NetworkDeployableItem networkDeployableItem;
    private bool isDeployed;

    public DeployableItemData DeployableItemData => deployableItemData;
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
        if (NetworkItem.IsOwner)
        {
            Transform castTransform = CameraManager.Instance.transform;
            if (Physics.Raycast(castTransform.position, castTransform.forward, out RaycastHit hit, deployableItemData.DeployRange, deployableItemData.DeployLayers, QueryTriggerInteraction.Ignore))
            {
                Vector3 surfaceAlignedForward = Vector3.ProjectOnPlane(castTransform.forward, hit.normal).normalized;
                if (surfaceAlignedForward == Vector3.zero) surfaceAlignedForward = Vector3.forward;
                Quaternion surfaceAlignment = Quaternion.LookRotation(surfaceAlignedForward, hit.normal);

                transform.SetPositionAndRotation(hit.point, surfaceAlignment);
                AudioManager.RequestPlayOneShot(deployableItemData.DeploySound, transform.position);
            }
            else
            {
                transform.SetPositionAndRotation(castTransform.position + castTransform.forward * deployableItemData.DeployRange, transform.rotation);
                RequestUndeploy();
            }
        }
    }

    public virtual void Undeploy()
    {
        isDeployed = false;
        rig.isKinematic = false;
    }
}