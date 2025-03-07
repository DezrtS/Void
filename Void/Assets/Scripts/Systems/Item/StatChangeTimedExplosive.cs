using System.Collections.Generic;
using UnityEngine;

public class StatChangeTimedExplosive : TimedExplosive
{
    [SerializeField] private StatChangeItemData statChangeItemData;

    public override void OnExplosiveHit(RaycastHit raycastHit)
    {
        if (raycastHit.collider == null) return;

        if (raycastHit.collider.TryGetComponent(out PlayerStats playerStats))
        {
            AddStats(playerStats);
        }

        base.OnExplosiveHit(raycastHit);
    }

    private void AddStats(PlayerStats playerStats)
    {
        for (int i = 0; i < statChangeItemData.StatChanges.Count; i++)
        {
            StatChange statChange = statChangeItemData.StatChanges[i];
            playerStats.ApplyModifier(statChange.StatName, statChangeItemData.Key * 100 + i, statChange.Modifier, statChange.ModifierType, statChangeItemData.Duration);
        }
    }
}