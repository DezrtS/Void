using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MutationHotbar : MonoBehaviour
{
    private List<Mutation> mutations = new List<Mutation>();
    private int selectedIndex;

    public Mutation GetActiveMutation()
    {
        if (mutations.Count <= 0) return null;

        return mutations[selectedIndex];
    }

    public void AddMutation(MutationData mutation)
    {
        AddMutationServerRpc(GameDataManager.Instance.GetMutationDataIndex(mutation));
    }

    public void AddMutationClientServerSide(Mutation mutation)
    {
        mutations.Add(mutation);
        mutation.SetupMutation(gameObject);
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddMutationServerRpc(int mutationIndex, ServerRpcParams rpcParams = default)
    {
        MutationData mutationData = GameDataManager.Instance.GetMutationData(mutationIndex);
        GameObject spawnedMutation = Instantiate(mutationData.MutationPrefab);
        Mutation mutation = spawnedMutation.GetComponent<Mutation>();
        mutation.NetworkObject.Spawn();
        mutation.NetworkObject.TrySetParent(transform);
        AddMutationClientServerSide(mutation);
        AddMutationClientRpc(mutation.NetworkObjectId);
    }

    public void AddMutationClientRpc(ulong mutationNetworkObjectId, ServerRpcParams rpcParams = default)
    {
        NetworkObject mutationNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[mutationNetworkObjectId];
        AddMutationClientServerSide(mutationNetworkObject.GetComponent<Mutation>());
    }

    public void SwitchMutation(int direction)
    {
        int newIndex = (Mathf.Abs(selectedIndex + direction)) % mutations.Count;
        SwitchToMutation(newIndex);
    }

    public void SwitchToMutation(int index)
    {
        SwitchToMutationClientServerSide(index);
        SwitchToMutationServerRpc(index);
    }

    public void SwitchToMutationClientServerSide(int index)
    {
        //OnSwitchItem?.Invoke(selectedIndex, index);
        selectedIndex = index;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SwitchToMutationServerRpc(int index, ServerRpcParams rpcParams = default)
    {
        SwitchToMutationClientServerSide(index);
        ClientRpcParams clientRpcParams = GameMultiplayer.GenerateClientRpcParams(rpcParams);
        SwitchToMutationClientRpc(index, clientRpcParams);
    }

    [ClientRpc(RequireOwnership = false)]
    public void SwitchToMutationClientRpc(int index, ClientRpcParams clientRpcParams = default)
    {
        SwitchToMutationClientServerSide(index);
    }
}
