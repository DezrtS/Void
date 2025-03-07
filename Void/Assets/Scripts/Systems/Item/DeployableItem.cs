using UnityEngine;

public class DeployableItem : Item
{
    private ThrowableItemData throwableItemData;

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
        throwableItemData = ItemData as ThrowableItemData;
        networkDeployableItem = NetworkItem as NetworkDeployableItem;
    }

    public override void StopUsing()
    {
        base.StopUsing();
        Throw();
    }

    public override void PickUp()
    {
        base.PickUp();
        if (isDeployed) Undeploy();
    }

    public virtual void Throw()
    {
        canPickUp = false;
        if (NetworkItem.IsOwner)
        {
            RequestDrop();
            if (CanDrop()) Drop();
            rig.AddForce(throwableItemData.ThrowSpeed * transform.forward, ForceMode.Impulse);
            rig.angularVelocity = throwableItemData.SpinSpeed * Vector3.right;
        }
    }

    public virtual void Deploy()
    {
        isDeployed = true;

        if (NetworkItem.IsOwner)
        {
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, deployRange, deployLayers, QueryTriggerInteraction.Ignore))
            {
                RequestDrop();
                if (CanDrop()) Drop();
                transform.SetPositionAndRotation(hit.point, Quaternion.LookRotation(Vector3.forward, hit.normal));
                rig.isKinematic = true;
            }
        }
    }

    public virtual void Undeploy()
    {

    }
}