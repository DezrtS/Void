using UnityEngine;

public class StatChangeMutation : Mutation
{
    [SerializeField] private StatChangesData statChangesData;
    private PlayerStats playerStats;
    private float durationTimer;

    public bool CanActivate() => durationTimer <= 0;

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
        durationTimer = statChangesData.Duration;
        playerStats.RequestChangeStats(statChangesData);
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
                cooldownTimer = mutationData.Cooldown;
            }
        }
    }
}
