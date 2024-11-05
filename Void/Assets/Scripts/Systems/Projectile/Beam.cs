using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Beam : MonoBehaviour
{
    [SerializeField] private float range;
    [SerializeField] private float damagePerFrame;
    [SerializeField] private LayerMask layerMask;
    private float power;
    private float maxPower = 1;

    public void SetPower(float power)
    {
        this.power = power;
        OnPowerChange();
    }

    private void OnPowerChange()
    {
        
    }

    private void FixedUpdate()
    {
        if (power > 0)
        {
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hitInfo, power * range, layerMask, QueryTriggerInteraction.Ignore))
            {
                maxPower = (hitInfo.distance / (power * range));
                OnBeamHit();
            }
            else
            {
                maxPower = 1;
            }
        }

        transform.localScale = Mathf.Min(power, maxPower) * range * Vector3.forward + new Vector3(1, 1, 0);
    }

    public void OnBeamHit()
    {

    }
}