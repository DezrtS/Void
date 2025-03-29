using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;

public class NavMeshTracking : NetworkBehaviour
{
    private NavMeshAgent agent;
    private Transform target;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (!agent.enabled)
        {
            Debug.LogWarning("NavMeshAgent was disabled! Enabling now...", this);
            agent.enabled = true;
        }

        if (!agent.isOnNavMesh)
        {
            TryPlaceOnNavMesh();
        }
    }

    private void Update()
    {
        if (!IsServer) return;

        if (target != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.SetDestination(target.position);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return; 

        if (other.CompareTag("Player"))
        {
            target = other.transform;

            if (agent.enabled && agent.isOnNavMesh)
            {
                agent.SetDestination(target.position);
            }
            else
            {
                Debug.LogWarning("Attempted to set destination but NavMeshAgent is not on a NavMesh.");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsServer) return;

        if (other.CompareTag("Player"))
        {
            target = null;

            if (agent.enabled && agent.isOnNavMesh)
            {
                agent.ResetPath();
            }
            else
            {
                Debug.LogWarning("Attempted to reset path but NavMeshAgent is not on a NavMesh.");
            }
        }
    }

    private void TryPlaceOnNavMesh()
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 5f, NavMesh.AllAreas))
        {
            transform.position = hit.position;
            agent.Warp(hit.position);
            Debug.Log("Successfully placed enemy on the NavMesh.", this);
        }
        else
        {
            Debug.LogError("Failed to place enemy on a NavMesh! Check its spawn position.", this);
        }
    }
}
