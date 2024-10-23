using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ThrowableItem : Item
{
    [SerializeField] private float thowSpeed;
    [SerializeField] private float spinSpeed;

    private Rigidbody rig;

    private void Awake()
    {
        rig = GetComponent<Rigidbody>();
    }

    public override void Use()
    {
        Throw();
    }

    public override void OnPickUp()
    {
        base.OnPickUp();
        rig.isKinematic = true;
    }

    public override void OnDrop()
    {
        base.OnDrop();
        rig.isKinematic = false;
    }

    public void Throw()
    {
        rig.isKinematic = false;
        rig.AddForce(thowSpeed * transform.forward, ForceMode.Impulse);
        rig.angularVelocity = spinSpeed * Vector3.right;

        OnThrow();
    }

    protected virtual void OnThrow()
    {
        canUse = false;
        canPickUp = false;
        canDrop = false;
    }
}