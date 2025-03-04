using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeployableItem : Item
{
    [SerializeField] private float deployRange = 5;
    [SerializeField] private LayerMask deployLayers;
    
    private bool isDeployed;

    public bool CanDeploy() => !isDeployed;

    public override void StopUsing()
    {
        base.StopUsing();
        if (CanDeploy()) Deploy();
    }

    public virtual void Deploy()
    {
        isDeployed = true;

        if (NetworkItem.IsServer)
        {
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, deployRange, deployLayers, QueryTriggerInteraction.Ignore))
            {
                transform.position = hit.point;
            }
        }
    }
}