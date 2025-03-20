using UnityEngine;

public class StatChangeMutation : Mutation
{
    private StatChangeMutationData statChangeMutationData;
    private PlayerStats playerStats;
    private float durationTimer;

    public bool CanActivate() => durationTimer <= 0;

    private void Start()
    {
        statChangeMutationData = mutationData as StatChangeMutationData;
    }

    public override void SetupMutation(GameObject player)
    {
        base.SetupMutation(player);
        if (player.TryGetComponent(out PlayerStats playerStats))
        {
            this.playerStats = playerStats;
        }
    }

    public override void Use()
    {
        base.Use();
        if (CanActivate()) Activate();
    }

    public void Activate()
    {
        durationTimer = statChangeMutationData.Duration;
        AddStats();
    }

    public override void UpdateTimers()
    {
        base.UpdateTimers();

        if (durationTimer > 0)
        {
            durationTimer -= Time.fixedDeltaTime;

            if (durationTimer <= 0)
            {
                durationTimer = 0;
                RemoveStats();
                cooldownTimer = mutationData.Cooldown;
            }
        }
    }

    private void AddStats()
    {
        for (int i = 0; i < statChangeMutationData.StatChanges.Count; i++)
        {
            StatChange statChange = statChangeMutationData.StatChanges[i];
            playerStats.ApplyModifier(statChange.StatName, statChangeMutationData.Key * 100 + i, statChange.Modifier, statChange.ModifierType);
        }
    }

    private void RemoveStats()
    {
        for (int i = 0; i < statChangeMutationData.StatChanges.Count; i++)
        {
            StatChange statChange = statChangeMutationData.StatChanges[i];
            playerStats.RemoveModifier(statChange.StatName, statChangeMutationData.Key * 100 + i);
        }
    }
}
