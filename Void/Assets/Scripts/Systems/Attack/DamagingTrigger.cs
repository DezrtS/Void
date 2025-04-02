using System.Collections.Generic;
using UnityEngine;

public class DamagingTrigger : MonoBehaviour
{
    [SerializeField] private GameManager.PlayerRole triggerRole;
    [SerializeField] private float damage;
    [SerializeField] private float damageRate;
    private readonly List<Health> healths = new List<Health>();

    private float timeAtLastDamage;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Health health))
        {
            if (!GameManager.CanDamage(other.gameObject, triggerRole)) return;
            healths.Add(health);
            health.OnDeathStateChanged += OnDeathStateChanged;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out Health health))
        {
            if (!healths.Contains(health)) return;

            healths.Remove(health);
            health.OnDeathStateChanged -= OnDeathStateChanged;
        }
    }

    private void FixedUpdate()
    {
        float timeSinceLevelLoad = Time.timeSinceLevelLoad;
        if (timeAtLastDamage + damageRate < timeSinceLevelLoad)
        {
            timeAtLastDamage = timeSinceLevelLoad;
            DealDamage();
        }
    }

    public void OnDeathStateChanged(Health health, bool isDead)
    {
        healths.Remove(health);
        health.OnDeathStateChanged -= OnDeathStateChanged;
    } 

    public void DealDamage()
    {
        for (int i = healths.Count - 1; i >= 0; i--)
        {
            Health health = healths[i];
            if (health == null) continue;
            if (health.NetworkHealth.IsServer) health.RequestDamage(damage);
        }
    }

    private void OnDestroy()
    {
        for (int i = healths.Count - 1; i >= 0; i--)
        {
            Health health = healths[i];
            if (health == null) continue;
            health.OnDeathStateChanged -= OnDeathStateChanged;
        }
    }
}