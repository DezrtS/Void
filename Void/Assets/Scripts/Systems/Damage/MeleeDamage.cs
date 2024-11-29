using System.Collections;
using UnityEngine;
using Unity.Netcode;

public class MeleeDamage : NetworkBehaviour
{
    public int monsterMelee = 25;  
    public float despawnDelay = 0.1f;

    private void OnTriggerEnter(Collider other)
    {
        if (IsServer)
        {
            if (other.CompareTag("Player"))
            {
                ApplyDamage(other, monsterMelee);
            }
            DespawnSphere();
        }
    }

    private void ApplyDamage(Collider target, int damage)
    {
        IDamageable damageSystem = target.GetComponent<IDamageable>();
        if (damageSystem != null)
        {
            damageSystem.Damage(damage); 
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
