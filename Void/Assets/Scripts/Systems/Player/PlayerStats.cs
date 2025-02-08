using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StatModifier
{
    public enum StatModifierType
    {
        Add,
        Multiply,
        Power,
        Override
    }

    [SerializeField] private float modifierValue;
    [SerializeField] private StatModifierType modifierType;

    public float ModifierValue => modifierValue;
    public StatModifierType ModifierType => modifierType;

    public StatModifier(float modifierValue, StatModifierType modifierType)
    {
        this.modifierValue = modifierValue;
        this.modifierType = modifierType;
    }
}

[Serializable]
public class Stat
{
    [SerializeField] private float baseValue; // The base value of the stat
    [SerializeField] private Dictionary<int, StatModifier> modifiers = new Dictionary<int, StatModifier>();
    private float value;

    public Stat(float baseValue)
    {
        this.baseValue = baseValue;
        UpdateValue();
    }

    public float Value
    {
        get
        {
            return value;
        }
    }

    public void AddModifier(int key, StatModifier value)
    {
        if (!modifiers.TryAdd(key, value))
        {
            Debug.LogWarning("Duplicate Stat Modifier Key");
        }
        UpdateValue();
    }

    public void RemoveModifier(int key)
    {
        modifiers.Remove(key);
        UpdateValue();
    }

    public void SetBaseValue(float value)
    {
        baseValue = value;
        UpdateValue();
    }

    private void UpdateValue()
    {
        float value = baseValue;

        foreach (KeyValuePair<int, StatModifier> modifier in modifiers)
        {
            switch (modifier.Value.ModifierType)
            {
                case StatModifier.StatModifierType.Add:
                    value += modifier.Value.ModifierValue;
                    break;
                case StatModifier.StatModifierType.Multiply:
                    value *= modifier.Value.ModifierValue;
                    break;
                case StatModifier.StatModifierType.Power:
                    value = Mathf.Pow(value, modifier.Value.ModifierValue);
                    break;
                case StatModifier.StatModifierType.Override:
                    value = modifier.Value.ModifierValue;
                    break;
                default:
                    break;
            }
        }

        this.value = value;
    }
}


public class PlayerStats : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private Stat maxHealth = new Stat(100);
    [SerializeField] private Stat healthRegenerationRate = new Stat(5);
    [SerializeField] private Stat damageResistance = new Stat(0);

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
    public Stat DamageResistance => damageResistance;

    public Stat TimeToAccelerate => timeToAccelerate;
    public Stat TimeToDeacclerate => timeToDeaccelerate;

    public Stat WalkSpeed => walkSpeed;
    public Stat SprintSpeed => sprintSpeed;
    public Stat CrouchSpeed => crouchSpeed;

    public Stat JumpPower => jumpPower;

    public Stat CrouchHeight => crouchHeight;

    public void ApplyModifier(string statName, int key, float modifier, StatModifier.StatModifierType modifierType)
    {
        var stat = GetStatByName(statName);
        if (stat == null) return;

        StatModifier statModifier = new StatModifier(modifier, modifierType);
        stat.AddModifier(key, statModifier);
    }

    public void RemoveModifier(string statName, int key)
    {
        var stat = GetStatByName(statName);
        if (stat == null) return;

        stat.RemoveModifier(key);
    }

    protected virtual Stat GetStatByName(string statName)
    {
        // Use reflection or a dictionary for more dynamic access
        return statName switch
        {
            "maxHealth" => maxHealth,
            "healthRegenerationRate" => healthRegenerationRate,
            "damageResistance" => damageResistance,
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
