using UnityEngine;

public class StatChangeMutation : Mutation
{
    private StatChangeMutationData statChangeMutationData;
    private PlayerStats playerStats;
    private float durationTimer;

    private void Awake()
    {
        statChangeMutationData = mutationData as StatChangeMutationData;
    }

    public override void SetupMutation(GameObject player)
    {
        if (player.TryGetComponent(out PlayerStats playerStats))
        {
            this.playerStats = playerStats;
        }
    }

    public bool CanActivate()
    {
        return durationTimer <= 0;
    }

    public override void OnUse()
    {
        if (!CanActivate()) return;

        durationTimer = statChangeMutationData.Duration;
        AddStats();
    }

    public override void UpdateTimers()
    {
        base.UpdateTimers();

        if (durationTimer > 0)
        {
            durationTimer -= Time.deltaTime;

            if (durationTimer <= 0)
            {
                durationTimer = 0;
                RemoveStats();
                cooldownTimer = mutationData.Cooldown;
            }
        }
    }

    public override void OnStopUsing()
    {
        //throw new System.NotImplementedException();
    }

    private void AddStats()
    {
        for (int i = 0; i < statChangeMutationData.StatChanges.Count; i++)
        {
            StatChangeMutationData.StatChange statChange = statChangeMutationData.StatChanges[i];
            playerStats.ApplyModifier(statChange.StatName, statChangeMutationData.Key * 100 + i, statChange.Modifier, statChange.ModifierType);
        }
    }

    private void RemoveStats()
    {
        for (int i = 0; i < statChangeMutationData.StatChanges.Count; i++)
        {
            StatChangeMutationData.StatChange statChange = statChangeMutationData.StatChanges[i];
            playerStats.RemoveModifier(statChange.StatName, statChangeMutationData.Key * 100 + i);
        }
    }
}
