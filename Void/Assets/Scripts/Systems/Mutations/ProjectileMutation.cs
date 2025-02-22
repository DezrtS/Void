using UnityEngine;

public class ProjectileMutation : Mutation
{
    [SerializeField] protected ProjectileSpawner projectileSpawner;
    private MutationHotbar mutationHotbar;

    public override void SetupMutation(GameObject player)
    {
        player.TryGetComponent(out mutationHotbar);
    }

    public override void StopUsing()
    {
        base.StopUsing();
        projectileSpawner.SpawnProjectile(mutationHotbar.ActiveTransform.position, mutationHotbar.ActiveTransform.rotation);
        cooldownTimer = mutationData.Cooldown;
    }
}