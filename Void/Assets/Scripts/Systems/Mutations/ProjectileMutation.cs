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
        AudioManager.PlayOneShot(spawnProjectileSound, gameObject);
        if (spawnAtCamera)
        {
            Transform cameraTransform = playerLook.CameraRootTransform;
            projectileSpawner.SpawnProjectile(cameraTransform.position, cameraTransform.rotation);
        }
        else
        {
            projectileSpawner.SpawnProjectile(transform.position, transform.rotation);
        }
    }
}