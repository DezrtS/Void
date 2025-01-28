using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Stat
{
    [SerializeField] private float baseValue; // The base value of the stat
    [SerializeField] private List<float> flatModifiers = new List<float>(); // Additive modifiers
    [SerializeField] private List<float> percentModifiers = new List<float>(); // Multiplicative modifiers

    public Stat(float baseValue)
    {
        this.baseValue = baseValue;
    }

    public float Value
    {
        get
        {
            float value = baseValue;

            // Add flat modifiers
            foreach (var modifier in flatModifiers)
                value += modifier;

            // Apply percent modifiers (multipliers)
            foreach (var modifier in percentModifiers)
                value *= 1 + modifier;

            return value;
        }
    }

    public void AddFlatModifier(float modifier)
    {
        flatModifiers.Add(modifier);
    }

    public void RemoveFlatModifier(float modifier)
    {
        flatModifiers.Remove(modifier);
    }

    public void AddPercentModifier(float modifier)
    {
        percentModifiers.Add(modifier);
    }

    public void RemovePercentModifier(float modifier)
    {
        percentModifiers.Remove(modifier);
    }

    public void SetBaseValue(float value)
    {
        baseValue = value;
    }
}


public class PlayerStats : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private Stat maxHealth = new Stat(100);
    [SerializeField] private Stat healthRegenerationRate = new Stat(5);

    [Header("Acceleration")]
    [SerializeField] private Stat timeToAccelerate = new Stat(0.25f);
    [SerializeField] private Stat timeToDeaccelerate = new Stat(0.25f);

    [Header("Speed")]
    [SerializeField] private Stat walkSpeed = new Stat(4);
    [SerializeField] private Stat sprintSpeed = new Stat(7);
    [SerializeField] private Stat crouchSpeed = new Stat(2);

    [Header("Jump")]
    [SerializeField] private Stat jumpPower = new Stat(7);

    [Header("Crouch")]
    [SerializeField] private Stat crouchHeight = new Stat(1);

    public Stat MaxHealth => maxHealth;
    public Stat HealthRegenerationRate => healthRegenerationRate;
    public Stat TimeToAccelerate => timeToAccelerate;
    public Stat TimeToDeacclerate => timeToDeaccelerate;

    public Stat WalkSpeed => walkSpeed;
    public Stat SprintSpeed => sprintSpeed;
    public Stat CrouchSpeed => crouchSpeed;

    public Stat JumpPower => jumpPower;

    public Stat CrouchHeight => crouchHeight;

    public void ApplyModifier(string statName, float modifier, bool isPercent = false)
    {
        var stat = GetStatByName(statName);
        if (stat == null) return;

        if (isPercent)
            stat.AddPercentModifier(modifier);
        else
            stat.AddFlatModifier(modifier);
    }

    public void RemoveModifier(string statName, float modifier, bool isPercent = false)
    {
        var stat = GetStatByName(statName);
        if (stat == null) return;

        if (isPercent)
            stat.RemovePercentModifier(modifier);
        else
            stat.RemoveFlatModifier(modifier);
    }

    private Stat GetStatByName(string statName)
    {
        // Use reflection or a dictionary for more dynamic access
        return statName switch
        {
            "maxHealth" => maxHealth,
            "healthRegenerationRate" => healthRegenerationRate,
            "timeToAccelerate" => timeToAccelerate,
            "timeToDeaccelerate" => timeToDeaccelerate,
            "walkSpeed" => walkSpeed,
            "sprintSpeed" => sprintSpeed,
            "crouchSpeed" => crouchSpeed,
            "jumpPower" => jumpPower,
            "crouchHeight" => crouchHeight,
            _ => null,
        };
    }
}
