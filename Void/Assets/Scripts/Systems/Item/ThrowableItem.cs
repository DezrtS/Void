using UnityEngine;

public class ThrowableItem : Item
{
    [SerializeField] private float thowSpeed;
    [SerializeField] private float spinSpeed;

    public override void StopUsing()
    {
        base.StopUsing();
        Throw();
    }

    public virtual void Throw()
    {
        if (networkItem.IsOwner)
        {
            RequestDrop();
            Drop();
            rig.AddForce(thowSpeed * transform.forward, ForceMode.Impulse);
            rig.angularVelocity = spinSpeed * Vector3.right;
        }
    }
}