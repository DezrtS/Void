using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BasicAttack : NetworkBehaviour, IUseable
{
    [SerializeField] private Vector3 offset;
    [SerializeField] private float size;
    [SerializeField] private LayerMask layerMask;

    [SerializeField] private float damage;

    [SerializeField] private float cooldown;
    [SerializeField] private float duration;
    private float cooldownTimer;
    private float durationTimer;

    private bool isAttacking = false;
    private List<Collider> hitColliders = new List<Collider>();

    public bool IsUsing => throw new System.NotImplementedException();

    public event IUseable.UseHandler OnUsed;

    public virtual bool CanUse()
    {
        return cooldownTimer <= 0 && durationTimer <= 0;
    }

    public void Use()
    {
        RequestUseServerRpc(new ServerRpcParams());
    }

    public void StopUsing() { }

    private void HandleServerUse()
    {
        isAttacking = true;
        durationTimer = duration;
        //OnUsed?.Invoke(true);
    }

    private void HandleClientUse()
    {
        //Debug.Log("Attack Started");
        OnUsed?.Invoke(true);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestUseServerRpc(ServerRpcParams rcpParams = default)
    {
        if (CanUse())
        {
            HandleServerUse();
            HandleUseClientRpc(rcpParams.Receive.SenderClientId);
        }
    }

    [ClientRpc]
    public void HandleUseClientRpc(ulong clientId)
    {
        HandleClientUse();
    }

    private void Update()
    {
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }

        if (durationTimer > 0)
        {
            durationTimer -= Time.deltaTime;

            if (durationTimer < 0)
            {
                isAttacking = false;
                hitColliders.Clear();
                cooldownTimer = cooldown;
            }
        }

        if (isAttacking)
        {
            Collider[] results = new Collider[10];
            Physics.OverlapSphereNonAlloc(transform.position + offset, size, results, layerMask, QueryTriggerInteraction.Ignore);

            if (results.Length > 0)
            {
                foreach (Collider collider in results)
                {
                    if (!collider || hitColliders.Contains(collider)) continue;

                    hitColliders.Add(collider);
                    if (collider.TryGetComponent(out Health health)) {
                        health.Damage(damage);
                    }
                }
            }
        }
    }
}