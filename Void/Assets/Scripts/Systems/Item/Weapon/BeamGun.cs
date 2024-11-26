using FMODUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class BeamGun : Item
{
    [SerializeField] private Beam beam;
    [SerializeField] private VisualEffect effect;
    [SerializeField] private float timeToWindUp;
    [SerializeField] private float timeToReachFullPower;
    [SerializeField] private float timeToOverheat;
    [SerializeField] private float cooldownMultiplier = 1;
    [SerializeField] private float overheatDelay;

    [SerializeField] private EventReference fireSound;

    protected bool isFiring;
    protected bool isActive;
    protected bool isOverheating;

    private float powerUpTimer = 0;
    private float activeTimer = 0;
    private float overheatTimer = 0;

    private void Start()
    {
        powerUpTimer = -timeToWindUp;
    }

    protected override void OnUse()
    {
        isFiring = true;
        effect.Play();
    }

    protected override void OnStopUsing()
    {
        isFiring = false;
    }

    public bool CanFire()
    {
        return (overheatTimer <= 0);
    }

    private void FixedUpdate()
    {
        if (isOverheating)
        {
            overheatTimer -= Time.fixedDeltaTime;

            if (overheatTimer <= 0 )
            {
                isOverheating = false;
            }
            else
            {
                return;
            }
        }

        if (isFiring)
        {
            if (isActive)
            {
                activeTimer += Time.fixedDeltaTime;
                if (activeTimer >= timeToOverheat)
                {
                    Overheat();
                }
            }
            else
            {
                powerUpTimer += Time.fixedDeltaTime;
                if (powerUpTimer >= timeToReachFullPower)
                {
                    isActive = true;
                    beam.SetPower(1);
                    powerUpTimer = timeToReachFullPower;
                }
                else
                {
                    beam.SetPower(Mathf.Max(powerUpTimer / timeToReachFullPower, 0));
                }
            }
        }
        else
        {
            isActive = false;
            if (activeTimer > 0)
            {
                activeTimer -= Time.fixedDeltaTime;

                if (activeTimer <= 0 )
                {
                    activeTimer = 0;
                }
            }

            if (powerUpTimer > -timeToWindUp)
            {
                powerUpTimer = powerUpTimer -= Time.fixedDeltaTime * cooldownMultiplier;
                if (powerUpTimer <= -timeToWindUp)
                {
                    powerUpTimer = -timeToWindUp;
                    beam.SetPower(0);
                }
                else
                {
                    beam.SetPower(Mathf.Max(powerUpTimer / timeToReachFullPower, 0));
                }
            }
        }
    }

    public void Overheat()
    {
        overheatTimer = overheatDelay;
        activeTimer = 0;
        powerUpTimer = timeToWindUp;
        isActive = false;
        isOverheating = true;
        beam.SetPower(0);
    }
}