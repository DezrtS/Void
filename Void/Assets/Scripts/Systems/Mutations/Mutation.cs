using Unity.Netcode;
using UnityEngine;

public abstract class Mutation : NetworkBehaviour, INetworkUseable
{
    public event IUseable.UseHandler OnUsed;

    [SerializeField] protected MutationData mutationData;

    protected NetworkUseable networkUseable;

    protected float cooldownTimer;

    private bool isUsing;

    public bool IsUsing => isUsing;
    public MutationData MutationData => mutationData;

    public abstract void SetupMutation(GameObject player);

    public bool CanUse() => !isUsing && cooldownTimer <= 0;
    public bool CanStopUsing() => isUsing;

    public void RequestUse() => networkUseable.UseServerRpc();
    public void RequestStopUsing() => networkUseable.StopUsingServerRpc();

    private void Awake()
    {
        networkUseable = GetComponent<NetworkUseable>();
    }

    private void FixedUpdate()
    {
        UpdateTimers();
    }

    public virtual void Use()
    {
        isUsing = true;
    }

    public virtual void StopUsing()
    {
        isUsing = false;
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