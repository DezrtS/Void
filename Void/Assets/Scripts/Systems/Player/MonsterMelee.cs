using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class MonsterMelee : NetworkBehaviour
{
    public int meleeDamage = 15;
    public GameObject damageSpherePrefab;
    public Camera playerCamera;

    private float lastAttack = 0f;
    private const float attackTimer = 1.0f;

    private NetworkVariable<bool> canDamage = new NetworkVariable<bool>();

    void Update()
    {
        // Ensure only the owner can control the monster
        if (!IsOwner) return;

        HandleAttack();
    }

    private void HandleAttack()
    {
        lastAttack += Time.deltaTime;

        if (lastAttack > attackTimer)
        {
            SetCanDamageServerRpc(false);
        }

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            AttackServerRpc();
        }
        else if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            StopAttackServerRpc();
        }
    }

    [ServerRpc]
    private void AttackServerRpc()
    {
        SetCanDamageServerRpc(true);
        SpawnDamageSphereServerRpc();
        lastAttack = 0.0f;
    }

    [ServerRpc]
    private void StopAttackServerRpc()
    {
        SetCanDamageServerRpc(false);
    }

    [ServerRpc]
    private void SetCanDamageServerRpc(bool value)
    {
        canDamage.Value = value;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnDamageSphereServerRpc()
    {
        if (damageSpherePrefab == null || playerCamera == null)
        {
            Debug.LogError("Damage Sphere Prefab or Player Camera is not assigned!");
            return;
        }

        Vector3 spawnPosition = playerCamera.transform.position + playerCamera.transform.forward * 2f;

        GameObject damageSphere = Instantiate(damageSpherePrefab, spawnPosition, Quaternion.identity);

        NetworkObject networkObject = damageSphere.GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            networkObject.Spawn();
        }
        else
        {
            Debug.LogError("Damage Sphere does not have a NetworkObject component!");
        }

        Debug.Log($"Damage Sphere instantiated at {spawnPosition} in the direction of the camera.");
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (playerCamera != null)
        {
            Destroy(playerCamera.gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return; // Ensure collision logic is handled server-side

        if (other.CompareTag("Player"))
        {
            // Damage logic or deactivation logic
            gameObject.SetActive(false);
            Debug.Log($"Monster hit by player {other.name}!");
        }
    }
}
