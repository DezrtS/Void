using System.Collections;
using UnityEngine;

public class ProjectileMutation : Mutation
{
    [SerializeField] protected ProjectileSpawner projectileSpawner;
    [SerializeField] private float spawnProjectileDelay;
    [SerializeField] private string spawnProjectileTrigger;
    private MutationHotbar mutationHotbar;

    public override void SetupMutation(GameObject player)
    {
        base.SetupMutation(player);
        player.TryGetComponent(out mutationHotbar);
    }

    public override void StopUsing()
    {
        base.StopUsing();
        StopAllCoroutines();
        StartCoroutine(SpawnProjectileCoroutine());
        cooldownTimer = mutationData.Cooldown;
        if (spawnProjectileTrigger != string.Empty) animationController.SetTrigger(spawnProjectileTrigger);
    }

    private IEnumerator SpawnProjectileCoroutine()
    {
        yield return new WaitForSeconds(spawnProjectileDelay);
        projectileSpawner.SpawnProjectile(mutationHotbar.ActiveTransform.position, mutationHotbar.ActiveTransform.rotation);
    }
}