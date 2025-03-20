using UnityEngine;

public class ThrowableItem : Item
{
    private ThrowableItemData throwableItemData;

    protected virtual void Start()
    {
        throwableItemData = ItemData as ThrowableItemData;
    }

    public override void StopUsing()
    {
        base.StopUsing();
        Throw();
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
}