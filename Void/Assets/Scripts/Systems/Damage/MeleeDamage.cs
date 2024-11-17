using System.Collections;
using UnityEngine;
using Unity.Netcode;

public class MeleeDamage : NetworkBehaviour
{
    public int monsterMelee = 25;
    public int playerMelee = 50;
    public float despawnDelay = 0.1f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            DamageSystem playerHealth = other.GetComponent<DamageSystem>();
            if (playerHealth != null)
            {
                playerHealth.Damage(monsterMelee);
            }

            if (IsServer)
            {
                DespawnSphere();
            }
        }
        if (other.CompareTag("Monster"))
        {
            DamageSystem playerHealth = other.GetComponent<DamageSystem>();
            if (playerHealth != null)
            {
                playerHealth.Damage(playerMelee);
            }

            if (IsServer)
            {
                DespawnSphere();
            }
        }
    }

    private void Start()
    {
        if (IsServer)
        {
            StartCoroutine(AutoDespawn());
        }
    }

    private IEnumerator AutoDespawn()
    {
        yield return new WaitForSeconds(despawnDelay);

        if (IsServer)
        {
            DespawnSphere();
        }
    }

    private void DespawnSphere()
    {
        NetworkObject networkObject = GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            networkObject.Despawn(); 
        }
    }
}
