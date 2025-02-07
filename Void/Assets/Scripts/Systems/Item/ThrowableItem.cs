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

    public override void OnUse()
    {
        Drop();
        Throw();
    }

    private void Throw()
    {
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

    /*public override void OnStopUsing()
    {
        throw new System.NotImplementedException();
    }*/
}