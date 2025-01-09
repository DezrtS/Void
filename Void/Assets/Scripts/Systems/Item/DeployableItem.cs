using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeployableItem : Item
{
    [SerializeField] private float deployRange = 5;
    [SerializeField] private LayerMask deployLayers;
    protected bool deployed;

    public bool CanDeploy()
    {
        return (!deployed);
    }

    public override void OnUse()
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

    public override void OnStopUsing()
    {

    }

    protected virtual void OnDeploy()
    {
        canPickUp = false;
        canDrop = false;
        deployed = true;
    }
}