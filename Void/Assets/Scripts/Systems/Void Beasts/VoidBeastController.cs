using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class VoidBeastController : NetworkBehaviour
{
    private NavMeshMovement navMeshMovement;
    private Health health;

    bool hasTarget = false;
    private Transform targetTransform;

    public NavMeshMovement NavMeshMovement => navMeshMovement;
    public Health Health => health;

    private void Awake()
    {
        navMeshMovement = GetComponent<NavMeshMovement>();
        health = GetComponent<Health>();
        health.OnDeathStateChanged += OnDeathStateChanged;
    }

    public void Activate()
    {
        List<GameObject> playerObjects = GameManager.Instance.PlayerObjects;
        GameObject targetGameObject = GetClosestPlayer(playerObjects);
        if (targetGameObject == null) return;
        targetTransform = targetGameObject.transform;
        hasTarget = true;

        navMeshMovement.Pathfind(targetTransform.position);
    }

    private GameObject GetClosestPlayer(List<GameObject> players)
    {
        GameObject closestPlayer = null;
        float closestDistance = Mathf.Infinity;

        foreach (GameObject player in players)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPlayer = player;
            }
        }

        return closestPlayer;
    }

    private void FixedUpdate()
    {
        if (!IsServer) return;

        if (!hasTarget) return;

        navMeshMovement.PathfindingDestination = targetTransform.position;
    }

    public void OnDeathStateChanged(Health health, bool isDead)
    {
        if (!IsServer) return;

        if (isDead)
        {
            navMeshMovement.StopPathfinding();
            navMeshMovement.SetVelocity(Vector3.zero);
            hasTarget = false;
            targetTransform = null;
        }
    }
}