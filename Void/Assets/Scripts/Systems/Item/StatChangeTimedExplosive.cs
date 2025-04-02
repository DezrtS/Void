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
            if (!GameManager.CanDamage(raycastHit.collider.gameObject, explosiveRole)) return;
            playerStats.RequestChangeStats(statChangesData);
        }

        base.OnExplosiveHit(raycastHit);
    }
}