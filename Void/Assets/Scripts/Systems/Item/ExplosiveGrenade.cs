using UnityEngine;

public class ExplosiveGrenade : ThrowableItem
{
    [SerializeField] private float radius;
    [SerializeField] private float damage;
    [SerializeField] private float timeToActivation;
    [SerializeField] private LayerMask effectableLayers;
    [SerializeField] private GameObject explosionEffect;

    private NetworkExplosiveGrenade networkExplosiveGrenade;
    private bool isActive;

    private float timer = 0;

    public bool CanActivateGrenade() => !isActive;
    public bool CanDeactivateGrenade() => isActive;
    public bool CanTriggerGrenade() => timer <= 0;

    public void RequestActivateGrenade() => networkExplosiveGrenade.ActivateGrenadeServerRpc();
    public void RequestDeactivateGrenade() => networkExplosiveGrenade.DeactivateGrenadeServerRpc();
    public void RequestTriggerGrenade() => networkExplosiveGrenade.TriggerGrenadeServerRpc();

    private void Start()
    {
        networkExplosiveGrenade = NetworkItem as NetworkExplosiveGrenade;
    }

    private void FixedUpdate()
    {
        UpdateTimers();

        if (networkExplosiveGrenade.IsServer && isActive)
        {
            RequestTriggerGrenade();
        }
    }

    public override void Throw()
    {
        base.Throw();
        if (networkExplosiveGrenade.IsServer) RequestActivateGrenade();
    }

    public void ActivateGrenade()
    {
        isActive = true;
        timer = timeToActivation;
    }

    public void DeactivateGrenade()
    {
        isActive = false;
        timer = 0;
    }

    public void TriggerGrenade()
    {
        Instantiate(explosionEffect, transform.position, Quaternion.identity);

        if (NetworkItem.IsServer)
        {
            RaycastHit[] raycastHits = Physics.SphereCastAll(transform.position, radius, Vector3.forward, radius, effectableLayers);
            foreach (RaycastHit hit in raycastHits)
            {
                if (hit.collider.TryGetComponent(out Health health))
                {
                    health.RequestDamage(damage);
                    Debug.Log(hit.collider.gameObject.name);
                }
            }
        }
    }

    private void UpdateTimers()
    {
        float fixedDeltaTime = Time.fixedDeltaTime;
        if (timer > 0)
        {
            timer -= fixedDeltaTime;
        }
    }
}