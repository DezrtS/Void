using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowableItem : Item
{
    [SerializeField] private float thowSpeed;
    [SerializeField] private float spinSpeed;

    protected bool thrown = false;

    public bool CanThrow()
    {
        return (!thrown);
    }

    protected override void StopUsingServerSide()
    {
        base.StopUsingServerSide();
        Throw();
    }

    private void Throw()
    {
        DropItem();
        rig.AddForce(thowSpeed * transform.forward, ForceMode.Impulse);
        rig.angularVelocity = spinSpeed * Vector3.right;

        OnThrow();
    }

    protected virtual void OnThrow()
    {
        canPickUp = false;
        canDrop = false;
        thrown = true;
    }
}