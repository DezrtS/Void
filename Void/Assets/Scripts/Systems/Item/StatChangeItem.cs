using UnityEngine;

public class StatChangeItem : Item
{
    private StatChangeItemData statChangeItemData;

    private PlayerStats playerStats;
    private float durationTimer;

    public bool CanActivate() => durationTimer <= 0;

    private void Start()
    {
        statChangeItemData = ItemData as StatChangeItemData;
    }

    private void FixedUpdate()
    {
        UpdateTimers(Time.fixedDeltaTime);
    }

    public override void Use()
    {
        base.Use();
        if (CanActivate()) Activate();
    }

    public void Activate()
    {
        playerStats = GetComponentInParent<PlayerStats>();
        if (playerStats == null) return;
        durationTimer = statChangeItemData.Duration;
        AddStats();
        if (NetworkItem.IsServer) RequestDrop();
    }

    public void UpdateTimers(float deltaTime)
    {
        if (durationTimer > 0)
        {
            durationTimer -= deltaTime;

            if (durationTimer <= 0)
            {
                durationTimer = 0;
                RemoveStats();
            }
        }
    }

    private void AddStats()
    {
        for (int i = 0; i < statChangeItemData.StatChanges.Count; i++)
        {
            StatChange statChange = statChangeItemData.StatChanges[i];
            playerStats.ApplyModifier(statChange.StatName, statChangeItemData.Key * 100 + i, statChange.Modifier, statChange.ModifierType);
        }
    }

    private void RemoveStats()
    {
        for (int i = 0; i < statChangeItemData.StatChanges.Count; i++)
        {
            StatChange statChange = statChangeItemData.StatChanges[i];
            playerStats.RemoveModifier(statChange.StatName, statChangeItemData.Key * 100 + i);
        }
        playerStats = null;
    }
}