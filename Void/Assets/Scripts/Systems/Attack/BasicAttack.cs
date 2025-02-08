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

    private bool isUsing = false;
    private List<Collider> hitColliders = new List<Collider>();

    public bool IsUsing => isUsing;

    public event IUseable.UseHandler OnUsed;

    public virtual bool CanUse()
    {
        return cooldownTimer <= 0 && durationTimer <= 0;
    }

    public bool Use()
    {
        if (!CanUse()) return false;
        if (!IsServer) UseClientSide();
        UseServerRpc();
        return true;
    }

    private void UseClientSide()
    {
        UseClientServerSide();
    }

    private void UseServerSide()
    {
        UseClientServerSide();
    }

    private void UseClientServerSide()
    {
        isUsing = true;
        durationTimer = duration;
    }

    [ServerRpc(RequireOwnership = false)]
    private void UseServerRpc(ServerRpcParams rpcParams = default)
    {
        if (!CanUse()) return;
        UseServerSide();
        ClientRpcParams clientRpcParams = GameMultiplayer.GenerateClientRpcParams(rpcParams);
        UseClientRpc(clientRpcParams);
    }

    [ClientRpc(RequireOwnership = false)]
    private void UseClientRpc(ClientRpcParams rpcParams = default)
    {
        UseClientSide();
    }

    public bool StopUsing()
    {
        return true;
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
                isUsing = false;
                hitColliders.Clear();
                cooldownTimer = cooldown;
            }
        }
    }

    private void Update()
    {
        UpdateTimers();

        if (isUsing)
        {
            Collider[] results = new Collider[10];
            Physics.OverlapSphereNonAlloc(transform.position + offset, size, results, layerMask, QueryTriggerInteraction.Ignore);

            if (results.Length > 0)
            {
                foreach (Collider collider in results)
                {
                    if (!collider || collider.gameObject == gameObject || hitColliders.Contains(collider)) continue;

                    hitColliders.Add(collider);
                    if (collider.TryGetComponent(out Health health)) {
                        if (IsServer) health.Damage(damage);
                    }
                }
            }
        }
    }
}