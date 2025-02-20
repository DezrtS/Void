using System.Collections.Generic;
using UnityEngine;

public class BasicAttack : MonoBehaviour, INetworkUseable
{
    public event IUseable.UseHandler OnUsed;

    [SerializeField] private Animator animator;

    [SerializeField] private Vector3 offset;
    [SerializeField] private float size;
    [SerializeField] private LayerMask layerMask;

    [SerializeField] private float damage;

    [SerializeField] private float cooldown;
    [SerializeField] private float duration;

    protected NetworkUseable networkUseable;

    private float cooldownTimer;
    private float durationTimer;
    private List<Collider> hitColliders = new List<Collider>();

    private bool isAttacking;
    private bool isUsing;

    public bool IsAttacking => isAttacking;
    public bool IsUsing => isUsing;

    public bool CanUse() => !isUsing && !isAttacking && cooldownTimer <= 0 && durationTimer <= 0;
    public bool CanStopUsing() => isUsing;

    public void RequestUse() => networkUseable.UseServerRpc();
    public void RequestStopUsing() => networkUseable.StopUsingServerRpc();

    private void Awake()
    {
        networkUseable = GetComponent<NetworkUseable>();
    }

    private void Update()
    {
        UpdateTimers();

        if (networkUseable.IsServer && isAttacking)
        {
            Collider[] results = new Collider[10];
            Physics.OverlapSphereNonAlloc(transform.position + offset, size, results, layerMask, QueryTriggerInteraction.Ignore);

            if (results.Length > 0)
            {
                foreach (Collider collider in results)
                {
                    if (!collider || collider.gameObject == gameObject || hitColliders.Contains(collider)) continue;

                    hitColliders.Add(collider);
                    if (collider.TryGetComponent(out Health health))
                    {
                        health.RequestDamage(damage);
                    }
                }
            }
        }
    }

    public void Use()
    {
        isUsing = true;
        isAttacking = true;
        durationTimer = duration;

        if (networkUseable.IsOwner) animator.SetTrigger("Attack");
    }

    public void StopUsing()
    {
        isUsing = false;
    }

    private void UpdateTimers()
    {
        float deltaTime = Time.deltaTime;
        if (cooldownTimer > 0)
        {
            cooldownTimer -= deltaTime;
        }

        if (durationTimer > 0)
        {
            durationTimer -= deltaTime;

            if (durationTimer < 0)
            {
                isAttacking = false;
                hitColliders.Clear();
                cooldownTimer = cooldown;
            }
        }
    }
}