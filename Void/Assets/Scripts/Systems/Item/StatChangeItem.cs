using UnityEngine;

public class StatChangeItem : Item
{
    private StatChangeItemData statChangeItemData;

    private void Start()
    {
        statChangeItemData = ItemData as StatChangeItemData;
    }

    public override void Use()
    {
        base.Use();
        Activate();
    }

    public void Activate()
    {
        PlayerStats playerStats = GetComponentInParent<PlayerStats>();
        if (playerStats == null) return;
        AddStats(playerStats);
        if (NetworkItem.IsServer) RequestDrop();
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