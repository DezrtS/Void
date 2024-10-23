using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveGrenade : ThrowableItem
{
    [SerializeField] private float radius;
    [SerializeField] private float damage;
    [SerializeField] private float timeToActivation;
    [SerializeField] private float effectDuration;
    [SerializeField] private LayerMask effectableLayers;

    private bool isActive = false;
    private float timer = 0;

    private void FixedUpdate()
    {
        if (isActive)
        {
            if (timer <= 0)
            {
                Activate();
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
        Destroy(gameObject);
    }

    protected override void OnThrow()
    {
        base.OnThrow();
        isActive = true;
        timer = timeToActivation;
    }
}
