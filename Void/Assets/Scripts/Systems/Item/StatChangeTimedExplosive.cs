using System.Collections.Generic;
using UnityEngine;

public class StatChangeTimedExplosive : TimedExplosive
{
    [SerializeField] private StatChangesData statChangesData;

    public override void OnExplosiveHit(RaycastHit raycastHit)
    {
        if (raycastHit.collider == null) return;

        if (raycastHit.collider.TryGetComponent(out PlayerStats playerStats))
        {
            playerStats.RequestChangeStats(statChangesData);
        }

        base.OnExplosiveHit(raycastHit);
    }
}