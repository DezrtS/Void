using Unity.Netcode;
using UnityEngine;

public class NetworkMutationHotbar : NetworkBehaviour
{
    private MutationHotbar mutationHotbar;
    private readonly NetworkVariable<int> selectedIndex = new();

    private void Awake()
    {
        mutationHotbar = GetComponent<MutationHotbar>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        selectedIndex.OnValueChanged += OnSelectedIndexChanged;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        selectedIndex.OnValueChanged -= OnSelectedIndexChanged;
    }

    private void OnSelectedIndexChanged(int oldValue, int newValue)
    {
        if (oldValue == newValue) return;
        mutationHotbar.SwitchToMutation(newValue);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SwitchToMutationServerRpc(int index)
    {
        if (index >= mutationHotbar.MutationCount || index < 0) return;
        selectedIndex.Value = index;
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddMutationServerRpc(int mutationDataIndex)
    {
        Mutation mutation = GameDataManager.SpawnMutation(mutationDataIndex);
        mutation.transform.SetParent(transform);
        mutation.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        if (mutation.NetworkUseable.NetworkObject.OwnerClientId != OwnerClientId)
        {
            Debug.Log("Changed Ownership");
            mutation.NetworkUseable.NetworkObject.ChangeOwnership(OwnerClientId);
        }
        AddMutationClientRpc(mutation.NetworkUseable.NetworkObjectId);
    }

    [ClientRpc(RequireOwnership = false)]
    public void AddMutationClientRpc(ulong mutationNetworkObjectId)
    {
        NetworkObject mutationNetworkObject = GetNetworkObject(mutationNetworkObjectId);
        Mutation mutation = mutationNetworkObject.GetComponent<Mutation>();
        mutationHotbar.AddMutation(mutation);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RemoveMutationServerRpc(int index)
    {
        RemoveMutationClientRpc(index);
    }

    [ClientRpc(RequireOwnership = false)]
    public void RemoveMutationClientRpc(int index)
    {
        mutationHotbar.RemoveMutation(index);
    }
}