using Unity.Netcode;
using UnityEngine;

public abstract class Mutation : NetworkBehaviour, IUseable
{
    [SerializeField] protected MutationData mutationData;

    protected bool isUsing = false;
    protected float cooldownTimer;


    public event IUseable.UseHandler OnUsed;
    public bool IsUsing => isUsing;
    public MutationData MutationData => mutationData;

    public abstract void SetupMutation(GameObject player);

    public bool CanUse()
    {
        return !IsUsing && cooldownTimer <= 0;
    }

    public bool Use()
    {
        if (!CanUse()) return false;
        ForceUse();
        return true;
    }

    public void ForceUse()
    {
        if (!IsServer) UseClientSide();
        UseServerRpc();
    }

    protected virtual void UseClientSide()
    {
        UseClientServerSide();
    }

    protected virtual void UseServerSide()
    {
        UseClientServerSide();
    }

    protected virtual void UseClientServerSide()
    {
        isUsing = true;
    }

    [ServerRpc(RequireOwnership = false)]
    private void UseServerRpc(ServerRpcParams rpcParams = default)
    {
        if (!CanUse()) return;
        UseServerSide();
        ClientRpcParams clientRpcParams = GameMultiplayer.GenerateClientRpcParams(rpcParams);
        UseClientRpc(clientRpcParams);
    }

    [ClientRpc(RequireOwnership = false)]
    private void UseClientRpc(ClientRpcParams rpcParams = default)
    {
        UseClientSide();
    }

    public bool StopUsing()
    {
        if (!IsUsing) return false;
        ForceStopUsing();
        return true;
    }

    public void ForceStopUsing()
    {
        if (!IsServer) StopUsingClientSide();
        StopUsingServerRpc();
    }

    protected virtual void StopUsingClientSide()
    {
        StopUsingClientServerSide();
    }

    protected virtual void StopUsingServerSide()
    {
        StopUsingClientServerSide();
    }

    private void StopUsingClientServerSide()
    {
        isUsing = false;
    }

    [ServerRpc(RequireOwnership = false)]
    private void StopUsingServerRpc(ServerRpcParams rpcParams = default)
    {
        if (!IsUsing) return;
        StopUsingServerSide();
        ClientRpcParams clientRpcParams = GameMultiplayer.GenerateClientRpcParams(rpcParams);
        StopUsingClientRpc(clientRpcParams);
    }

    [ClientRpc(RequireOwnership = false)]
    private void StopUsingClientRpc(ClientRpcParams rpcParams = default)
    {
        StopUsingClientSide();
    }

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
