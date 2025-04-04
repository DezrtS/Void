using UnityEngine;

public class StatChangeMutation : Mutation
{
    [SerializeField] private StatChangesData statChangesData;
    private PlayerStats playerStats;

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
        Activate();
    }

    public void Activate()
    {
        cooldownTimer = mutationData.Cooldown;
        playerStats.ChangeStats(statChangesData);
        //if (networkUseable.IsServer) playerStats.RequestChangeStats(statChangesData);
    }
}