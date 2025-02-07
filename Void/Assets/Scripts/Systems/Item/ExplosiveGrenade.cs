using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveGrenade : ThrowableItem
{
    [SerializeField] private float radius;
    [SerializeField] private float damage;
    [SerializeField] private float timeToActivation;
    [SerializeField] private LayerMask effectableLayers;
    [SerializeField] private GameObject explosionEffect;

    [SerializeField]private float timer = 0;

    private void FixedUpdate()
    {
        if (thrown)
        {
            if (timer <= 0)
            {
                Activate();
                thrown = false;
            }
            else
            {
                timer -= Time.fixedDeltaTime;
            }
        }
    }

    public void Activate()
    {
        RaycastHit[] raycastHits = Physics.SphereCastAll(transform.position, radius, Vector3.forward, 10, effectableLayers);
        foreach (RaycastHit hit in raycastHits)
        {
            Debug.Log(hit.collider.gameObject.name);
        }
        Instantiate(explosionEffect, transform.position, Quaternion.identity);
        AudioManager.Instance.PlayOneShot(FMODEventManager.Instance.Sound2);
        Destroy(gameObject);
    }

    protected override void OnThrow()
    {
        timer = timeToActivation;
    }
}
