using FMODUnity;
using System.Collections;
using UnityEngine;

public class ProjectileMutation : Mutation
{
    [SerializeField] protected ProjectileSpawner projectileSpawner;
    [SerializeField] private float spawnProjectileDelay;
    [SerializeField] private string spawnProjectileTrigger;

    [SerializeField] private bool spawnAtCamera;

    [SerializeField] private EventReference spawnProjectileSound;
    private PlayerLook playerLook;

    public override void SetupMutation(GameObject player)
    {
        base.SetupMutation(player);
        player.TryGetComponent(out playerLook);
        projectileSpawner.OnHit += OnHit;
    }

    public override void StopUsing()
    {
        base.StopUsing();
        StopAllCoroutines();
        StartCoroutine(SpawnProjectileCoroutine());
        cooldownTimer = mutationData.Cooldown;
        if (spawnProjectileTrigger != string.Empty) animationController.SetTrigger(spawnProjectileTrigger);
    }

    private void OnHit(Projectile projectile, ProjectileSpawner projectileSpawner, RaycastHit raycastHit)
    {
        if (!networkUseable.IsOwner) return;
        if (raycastHit.collider.CompareTag("Monster") || raycastHit.collider.CompareTag("Player"))
        {
            UIManager.Instance.TriggerHit();
        }
    }

    private IEnumerator SpawnProjectileCoroutine()
    {
        yield return new WaitForSeconds(spawnProjectileDelay);
        AudioManager.PlayOneShot(spawnProjectileSound, gameObject);
        if (spawnAtCamera)
        {
            projectileSpawner.SpawnProjectile(playerLook.CameraRootTransform.position, playerLook.CameraRotationRootTransform.rotation);
        }
        else
        {
            projectileSpawner.SpawnProjectile(transform.position, transform.rotation);
        }
    }
}