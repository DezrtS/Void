using Unity.Netcode;
using UnityEngine;

public abstract class Mutation : NetworkBehaviour, IUseable
{
    [SerializeField] protected MutationData mutationData;

    protected NetworkVariable<bool> isUsing = new NetworkVariable<bool>(false);
    protected float cooldownTimer;


    public event IUseable.UseHandler OnUsed;
    public bool IsUsing => isUsing.Value;
    public MutationData MutationData => mutationData;

    public abstract void SetupMutation(GameObject player);

    public bool CanUse()
    {
        return !IsUsing && cooldownTimer <= 0;
    }

    public void Use()
    {
        if (!CanUse()) return;
        isUsing.Value = true;
        OnUse();
    }

    public abstract void OnUse();

    public void StopUsing()
    {
        if (!IsUsing) return;
        isUsing.Value = false;
        OnStopUsing();
    }

    public abstract void OnStopUsing();

    private void FixedUpdate()
    {
        UpdateTimers();
    }

    public virtual void UpdateTimers()
    {
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;

            if (cooldownTimer <= 0)
            {
                cooldownTimer = 0;
            }
        }
    }
}
