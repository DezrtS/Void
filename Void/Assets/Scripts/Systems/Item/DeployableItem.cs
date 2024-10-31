using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeployableItem : Item
{
    [SerializeField] private float deployRange = 5;
    [SerializeField] private LayerMask deployLayers;

    public override void Use()
    {
        Deploy();
    }

    public void Deploy()
    {
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, deployRange, deployLayers, QueryTriggerInteraction.Ignore))
        {
            transform.position = hit.point;
        }

        OnDeploy();
    }

    protected virtual void OnDeploy()
    {
        canUse = false;
        canPickUp = false;
        canDrop = false;
    }
}