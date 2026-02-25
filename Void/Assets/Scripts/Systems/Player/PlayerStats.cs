using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
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
    [SerializeField] private float duration;
    private float creationTime;

    public float ModifierValue => modifierValue;
    public StatModifierType ModifierType => modifierType;
    public float Duration => duration;

    public StatModifier(float modifierValue, StatModifierType modifierType, float duration)
    {
        this.modifierValue = modifierValue;
        this.modifierType = modifierType;
        this.duration = duration;
        creationTime = Time.timeSinceLevelLoad;
    }

    public bool IsExpired(float timeSinceLevelLoad)
    {
        return timeSinceLevelLoad - creationTime > duration;
    }
}

[Serializable]
public class Stat
{
    public event Action Changed;
    
    [SerializeField] private float baseValue; // The base value of the stat
    [SerializeField] private Dictionary<int, StatModifier> modifiers = new Dictionary<int, StatModifier>();
    private float value;

    public Stat(float baseValue)
    {
        this.baseValue = baseValue;
        value = baseValue;
    }

    public float BaseValue => baseValue;

    public float Value
    {
        get
        {
            UpdateValue();
            return value;
        }
    }

    public void AddModifier(int key, StatModifier value)
    {
        if (!modifiers.TryAdd(key, value))
        {
            Debug.LogWarning("Duplicate Stat Modifier Key");
        }
        Debug.Log($"Added Modifier: {value.ModifierType}, {value.ModifierValue}, {value.Duration}");
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
        float timeSinceLevelLoad = Time.timeSinceLevelLoad;

        for (int i = modifiers.Count - 1; i >= 0; i--)
        {
            KeyValuePair<int, StatModifier> modifier = modifiers.ElementAt(i);

            if (modifier.Value.IsExpired(timeSinceLevelLoad))
            {
                modifiers.Remove(modifier.Key);
                Changed?.Invoke();
                continue;
            } 

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

[Serializable]
public class StatChange
{
    [SerializeField] private string statName;
    [SerializeField] private float modifier;
    [SerializeField] private StatModifier.StatModifierType modifierType;

    public string StatName => statName;
    public float Modifier => modifier;
    public StatModifier.StatModifierType ModifierType => modifierType;
};


public class PlayerStats : MonoBehaviour
{
    public event Action<PlayerStats> OnStatChanged;
    public event Action<PlayerStats> OnStatUpdated;
    
    [Header("Health")]
    [SerializeField] private Stat maxHealth = new Stat(100);
    [SerializeField] private Stat healthRegenerationRate = new Stat(5);
    [SerializeField] private Stat damageResistance = new Stat(0);

    [Header("Acceleration")]
    [SerializeField] private Stat acceleration = new Stat(4);
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

    public Stat Acceleration => acceleration;
    public Stat TimeToAccelerate => timeToAccelerate;
    public Stat TimeToDeacclerate => timeToDeaccelerate;

    public Stat WalkSpeed => walkSpeed;
    public Stat SprintSpeed => sprintSpeed;
    public Stat CrouchSpeed => crouchSpeed;

    public Stat JumpPower => jumpPower;

    public Stat CrouchHeight => crouchHeight;

    private void OnEnable()
    {
        MaxHealth.Changed += StatOnChanged;
        HealthRegenerationRate.Changed += StatOnChanged;
        DamageResistance.Changed += StatOnChanged;
        Acceleration.Changed += StatOnChanged;
        TimeToAccelerate.Changed += StatOnChanged;
        TimeToDeacclerate.Changed += StatOnChanged;
        WalkSpeed.Changed += StatOnChanged;
        SprintSpeed.Changed += StatOnChanged;
        CrouchSpeed.Changed += StatOnChanged;
        JumpPower.Changed += StatOnChanged;
        CrouchHeight.Changed += StatOnChanged;
    }

    private void OnDisable()
    {
        MaxHealth.Changed -= StatOnChanged;
        HealthRegenerationRate.Changed -= StatOnChanged;
        DamageResistance.Changed -= StatOnChanged;
        Acceleration.Changed -= StatOnChanged;
        TimeToAccelerate.Changed -= StatOnChanged;
        TimeToDeacclerate.Changed -= StatOnChanged;
        WalkSpeed.Changed -= StatOnChanged;
        SprintSpeed.Changed -= StatOnChanged;
        CrouchSpeed.Changed -= StatOnChanged;
        JumpPower.Changed -= StatOnChanged;
        CrouchHeight.Changed -= StatOnChanged;
    }

    private void StatOnChanged()
    {
        OnStatUpdated?.Invoke(this);
    }


    public void ChangeStats(StatChangesData statChangesData)
    {
        for (int i = 0; i < statChangesData.StatChanges.Count; i++)
        {
            StatChange statChange = statChangesData.StatChanges[i];
            ApplyModifier(statChange.StatName, statChangesData.Key * 100 + i, statChange.Modifier, statChange.ModifierType, statChangesData.Duration);
        }
        OnStatChanged?.Invoke(this);
    }

    public void RequestChangeStats(StatChangesData statChangesData) => ChangeStatsServerRpc(GameDataManager.Instance.GetStatChangesDataIndex(statChangesData));

    [ServerRpc(RequireOwnership = false)]
    public void ChangeStatsServerRpc(int index)
    {
        ChangeStatsClientRpc(index);
    }

    [ClientRpc(RequireOwnership = false)]
    public void ChangeStatsClientRpc(int index)
    {
        Debug.Log("CHaninging Stats");
        ChangeStats(GameDataManager.Instance.GetStatChangesData(index));
    }

    public void ApplyModifier(string statName, int key, float modifier, StatModifier.StatModifierType modifierType, float duration = 99999)
    {
        var stat = GetStatByName(statName);
        if (stat == null)
        {
            Debug.Log("Stat Name == NULL");
            return;
        }

        StatModifier statModifier = new StatModifier(modifier, modifierType, duration);
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
