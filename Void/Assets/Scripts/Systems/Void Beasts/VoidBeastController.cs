using Unity.Netcode;
using UnityEngine;

public class VoidBeastController : NetworkBehaviour
{
    private NavMeshMovement navMeshMovement;
    private Health health;

    public NavMeshMovement NavMeshMovement => navMeshMovement;
    public Health Health => health;

    private void Awake()
    {
        navMeshMovement = GetComponent<NavMeshMovement>();
        health = GetComponent<Health>();
        health.OnDeath += (Health health) =>
        {
            Die();
        };
    }

    private void FixedUpdate()
    {
        if (!IsServer) return;
    }

    public void Die()
    {
        if (!IsServer) return;
    }
}